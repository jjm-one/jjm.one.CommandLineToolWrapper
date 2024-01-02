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
    private readonly ToolSettings _toolSettings;
    private readonly WrapperSettings _wrapperSettings;
    private readonly ToolWrapper _toolWrapper;

    public ToolWrapperTests()
    {
        _mockProcessRunner = new Mock<IProcessRunner>();
        Mock<ILogger<ToolWrapper>> mockLogger = new();
        _toolSettings = new ToolSettings { ToolPath = "/path/to/tool", CommandTemplates = new Dictionary<string, string>() };
        _wrapperSettings = new WrapperSettings { RetryCount = 3, RetryIntervalInSeconds = 1, WorkingDirectory = "/working/directory", ErrorDialog = false };
        _toolWrapper = new ToolWrapper(_toolSettings, _wrapperSettings, _mockProcessRunner.Object, mockLogger.Object);
    }

    [Fact]
    public async Task RunCommandAsync_ShouldThrowArgumentException_WhenCommandNotFound()
    {
        // Arrange
        const string command = "nonexistent";

        // Act
        Func<Task> act = async () => await _toolWrapper.RunCommandAsync(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"Command '{command}' not found. (Parameter 'command')");
    }

    [Fact]
    public async Task RunCommandAsync_ShouldThrowArgumentException_WhenIncorrectNumberOfArguments()
    {
        // Arrange
        _toolSettings.CommandTemplates.Add("test", "{0}");
        const string command = "test";
        object?[] args = ["arg1"];

        // Act
        Func<Task> act = async () => await _toolWrapper.RunCommandAsync(command, args);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"Command '{command}' expects 1 arguments, but got {args.Length}. (Parameter 'args')");
    }

    [Fact]
    public async Task RunCommandAsync_ShouldRunProcessWithCorrectArguments()
    {
        // Arrange
        _toolSettings.CommandTemplates.Add("test", "{0}");
        const string command = "test";
        object?[] args = ["arg1"];
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
}