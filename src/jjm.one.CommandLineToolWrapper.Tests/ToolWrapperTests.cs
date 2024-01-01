using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using jjm.one.CommandLineToolWrapper.backend;
using jjm.one.CommandLineToolWrapper.settings;
using jjm.one.CommandLineToolWrapper.types;
using Microsoft.Extensions.Logging;

namespace jjm.one.CommandLineToolWrapper.Tests;

public class ToolWrapperTests
{
    private readonly Mock<IProcessRunner> _mockProcessRunner;
    private readonly Mock<ILogger<ToolWrapper>> _mockLogger;
    private readonly ToolSettings _toolSettings;
    private readonly WrapperSettings _wrapperSettings;
    private readonly ToolWrapper _toolWrapper;

    public ToolWrapperTests()
    {
        _mockProcessRunner = new Mock<IProcessRunner>();
        _mockLogger = new Mock<ILogger<ToolWrapper>>();
        _toolSettings = new ToolSettings { ToolPath = "/path/to/tool", CommandTemplates = new Dictionary<string, string>() };
        _wrapperSettings = new WrapperSettings { RetryCount = 3, RetryIntervalInSeconds = 1, WorkingDirectory = "/working/directory", ErrorDialog = false };
        _toolWrapper = new ToolWrapper(_toolSettings, _wrapperSettings, _mockProcessRunner.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task RunCommandAsync_ShouldThrowArgumentException_WhenCommandNotFound()
    {
        // Arrange
        var command = "nonexistent";

        // Act
        Func<Task> act = async () => await _toolWrapper.RunCommandAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"Command '{command}' not found.");
    }

    [Fact]
    public async Task RunCommandAsync_ShouldThrowArgumentException_WhenIncorrectNumberOfArguments()
    {
        // Arrange
        _toolSettings.CommandTemplates.Add("test", "{0}");
        var command = "test";
        var args = new string[] { "arg1", "arg2" };

        // Act
        Func<Task> act = async () => await _toolWrapper.RunCommandAsync(command, args);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"Command '{command}' expects 1 arguments, but got {args.Length}.");
    }

    [Fact]
    public async Task RunCommandAsync_ShouldRunProcessWithCorrectArguments()
    {
        // Arrange
        _toolSettings.CommandTemplates.Add("test", "{0}");
        var command = "test";
        var args = new string[] { "arg1" };
        var processResult = new ProcessResult(0,"output");
        _mockProcessRunner.Setup(pr => pr.RunProcessAsync(It.IsAny<ProcessStartInfo>()))
            .ReturnsAsync(processResult);

        // Act
        var result = await _toolWrapper.RunCommandAsync(command, args);

        // Assert
        _mockProcessRunner.Verify(pr => pr.RunProcessAsync(It.Is<ProcessStartInfo>(psi =>
            psi.FileName == _toolSettings.ToolPath &&
            psi.Arguments == string.Format(_toolSettings.CommandTemplates[command], args) &&
            psi.WorkingDirectory == _wrapperSettings.WorkingDirectory &&
            psi.ErrorDialog == _wrapperSettings.ErrorDialog)), Times.Once);
        result.Should().Be(processResult);
    }

    [Fact]
    public async Task RunCommandAsync_ShouldRetryOnProcessFailedException()
    {
        // Arrange
        _toolSettings.CommandTemplates.Add("test", "{0}");
        var command = "test";
        var args = new string[] { "arg1" };
        var processResult = new ProcessResult(0,  "output" );
        _mockProcessRunner.SetupSequence(pr => pr.RunProcessAsync(It.IsAny<ProcessStartInfo>()))
            .Throws(new ProcessFailedException(0, "test", "output"))
            .ReturnsAsync(processResult);

        // Act
        var result = await _toolWrapper.RunCommandAsync(command, args);

        // Assert
        _mockProcessRunner.Verify(pr => pr.RunProcessAsync(It.IsAny<ProcessStartInfo>()), Times.Exactly(_wrapperSettings.RetryCount + 1));
        result.Should().Be(processResult);
    }
}