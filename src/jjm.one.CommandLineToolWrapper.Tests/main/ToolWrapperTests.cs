using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using jjm.one.CommandLineToolWrapper.backend;
using jjm.one.CommandLineToolWrapper.settings;
using jjm.one.CommandLineToolWrapper.types;
using Microsoft.Extensions.Logging;

namespace jjm.one.CommandLineToolWrapper.Tests.main;

public class ToolWrapperTests
{
    private readonly Mock<ILogger<ToolWrapper>> _mockLogger;
    private readonly Mock<IProcessRunner> _mockProcessRunner;
    private readonly ToolWrapper _toolWrapper;

    public ToolWrapperTests()
    {
        _mockProcessRunner = new Mock<IProcessRunner>();
        _mockLogger = new Mock<ILogger<ToolWrapper>>();
        var toolSettings = new ToolSettings
        {
            ToolPath = "/usr/bin/test",
            CommandTemplates = new Dictionary<string, string> { { "test", "test {0}" } }
        };
        var wrapperSettings = new WrapperSettings
        {
            RetryCount = 3,
            RetryIntervalInSeconds = 1,
            WorkingDirectory = "/tmp",
            ErrorDialog = false
        };
        _toolWrapper = new ToolWrapper(toolSettings, wrapperSettings, _mockProcessRunner.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task RunCommandAsync_ShouldReturnProcessResult_WhenProcessExitsWithZero()
    {
        // Arrange
        _mockProcessRunner.Setup(p => p.RunProcessAsync(It.IsAny<ProcessStartInfo>(),
                true, true))
            .ReturnsAsync(new ProcessResult { ExitCode = 0, Output = "test output", Error = "" });

        // Act
        var result = await _toolWrapper.RunCommandAsync("test", "arg");

        // Assert
        result.ExitCode.Should().Be(0);
        result.Output.Should().Be("test output");
        result.Error.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task RunCommandAsync_ShouldThrowArgumentException_WhenCommandNotFound()
    {
        // Arrange
        const string command = "nonexistent";

        // Act
        Func<Task> act = async () => await _toolWrapper.RunCommandAsync(command, "arg");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"Command '{command}' not found. (Parameter 'command')");
    }

    [Fact]
    public async Task RunCommandAsync_ShouldThrowArgumentException_WhenIncorrectNumberOfArguments()
    {
        // Arrange
        const string command = "test";

        // Act
        Func<Task> act = async () => await _toolWrapper.RunCommandAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"Command '{command}' expects 1 arguments, but got 0. (Parameter 'args')");
    }

    [Fact]
    public async Task RunCommandAsync_ShouldRetry_WhenProcessExitsWithNonZero()
    {
        // Arrange
        _mockProcessRunner.Setup(p => p.RunProcessAsync(It.IsAny<ProcessStartInfo>(),
            true, true)).ThrowsAsync(
            new ProcessFailedException("test", "args",
                new ProcessResult { ExitCode = 1, Output = "", Error = "test error" }));

        // Act
        Func<Task> act = async () => await _toolWrapper.RunCommandAsync("test", "arg");

        // Assert
        await act.Should().ThrowAsync<ProcessFailedException>().Where(exc =>
            exc.Command.Equals("test") && exc.Arguments.Equals("args") &&
            exc.Result != null &&
            exc.Result.ExitCode == 1 //&&
            //exc.Result.Output != null && exc.Result.Output.Equals("") &&
            //exc.Result.Error != null && exc.Result.Error.Equals("test error")
            );
        _mockProcessRunner.Verify(p =>
            p.RunProcessAsync(It.IsAny<ProcessStartInfo>(), true, true), Times.Exactly(4));
    }

    [Fact]
    public async Task RunCommandAsync_ShouldLogError_WhenProcessExitsWithNonZero()
    {
        // Arrange
        _mockProcessRunner.Setup(p => p.RunProcessAsync(It.IsAny<ProcessStartInfo>(),
            true, true)).ThrowsAsync(
            new ProcessFailedException("test", "args",
                new ProcessResult { ExitCode = 1, Output = "", Error = "test error" }));

        // Act
        Func<Task> act = async () => await _toolWrapper.RunCommandAsync("test", "arg");

        // Assert
        await act.Should().ThrowAsync<ProcessFailedException>();
        _mockLogger.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!), Times.Exactly(4));
    }
    
    [Fact]
    public void CheckExitCode_ReturnsTrue_WhenExitCodeIsInRetryExitCodes()
    {
        // Arrange
        var wrapperSettings = new WrapperSettings
        {
            RetryUseExitCodeAnalysis = true
        };
        var toolSettings = new ToolSettings
        {
            RetryExitCodes = [1]
        };
        var toolWrapper = new ToolWrapper(toolSettings, wrapperSettings);

        // Act
        var result = toolWrapper.CheckExitCode(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CheckExitCode_ReturnsFalse_WhenExitCodeIsNotInRetryExitCodes()
    {
        // Arrange
        var wrapperSettings = new WrapperSettings
        {
            RetryUseExitCodeAnalysis = true
        };
        var toolSettings = new ToolSettings
        {
            RetryExitCodes = [1]
        };
        var toolWrapper = new ToolWrapper(toolSettings, wrapperSettings);

        // Act
        var result = toolWrapper.CheckExitCode(2);

        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public void CheckOutput_ReturnsTrue_WhenOutputContainsRetryOutputString()
    {
        // Arrange
        var wrapperSettings = new WrapperSettings
        {
            RetryUseOutputAnalysis = true
        };
        var toolSettings = new ToolSettings
        {
            RetryOutputContains = ["network error"]
        };
        var toolWrapper = new ToolWrapper(toolSettings, wrapperSettings);

        // Act
        var result = toolWrapper.CheckOutput("A network error occurred");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CheckOutput_ReturnsFalse_WhenOutputDoesNotContainRetryOutputString()
    {
        // Arrange
        var wrapperSettings = new WrapperSettings
        {
            RetryUseOutputAnalysis = true
        };
        var toolSettings = new ToolSettings
        {
            RetryOutputContains = ["network error"]
        };
        var toolWrapper = new ToolWrapper(toolSettings, wrapperSettings);

        // Act
        var result = toolWrapper.CheckOutput("An unrelated error occurred");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CheckOutput_ReturnsFalse_WhenOutputIsNull()
    {
        // Arrange
        var wrapperSettings = new WrapperSettings
        {
            RetryUseOutputAnalysis = true
        };
        var toolSettings = new ToolSettings
        {
            RetryOutputContains = ["network error"]
        };
        var toolWrapper = new ToolWrapper(toolSettings, wrapperSettings);

        // Act
        var result = toolWrapper.CheckOutput(null);

        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public void CheckOutput_ReturnsTrue_WhenErrorContainsRetryErrorString()
    {
        // Arrange
        var wrapperSettings = new WrapperSettings
        {
            RetryUseErrorAnalysis = true
        };
        var toolSettings = new ToolSettings
        {
            RetryErrorContains = ["network error"]
        };
        var toolWrapper = new ToolWrapper(toolSettings, wrapperSettings);

        // Act
        var result = toolWrapper.CheckError("A network error occurred");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CheckOutput_ReturnsFalse_WhenErrorDoesNotContainRetryErrorString()
    {
        // Arrange
        var wrapperSettings = new WrapperSettings
        {
            RetryUseErrorAnalysis = true
        };
        var toolSettings = new ToolSettings
        {
            RetryErrorContains = ["network error"]
        };
        var toolWrapper = new ToolWrapper(toolSettings, wrapperSettings);

        // Act
        var result = toolWrapper.CheckError("An unrelated error occurred");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CheckOutput_ReturnsFalse_WhenErrorIsNull()
    {
        // Arrange
        var wrapperSettings = new WrapperSettings
        {
            RetryUseErrorAnalysis = true
        };
        var toolSettings = new ToolSettings
        {
            RetryErrorContains = ["network error"]
        };
        var toolWrapper = new ToolWrapper(toolSettings, wrapperSettings);

        // Act
        var result = toolWrapper.CheckError(null);

        // Assert
        result.Should().BeFalse();
    }
    
}