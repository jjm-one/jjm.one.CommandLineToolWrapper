using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using jjm.one.CommandLineToolWrapper.backend;
using jjm.one.CommandLineToolWrapper.types;

namespace jjm.one.CommandLineToolWrapper.Tests.backend;

public class ProcessRunnerTests
{
    [Fact]
    public async Task RunProcessAsync_ShouldReturnProcessResult_WhenProcessExitsWithZero()
    {
        // Arrange
        var startInfo = new ProcessStartInfo
        {
            FileName = "echo",
            Arguments = "Hello World"
        };

        var processRunner = new ProcessRunner();

        // Act
        var result = await processRunner.RunProcessAsync(startInfo);

        // Assert
        result.ExitCode.Should().Be(0);
    }

    [Fact]
    public async Task RunProcessAsync_ShouldThrowProcessFailedException_WhenProcessExitsWithNonZero()
    {
        // Arrange
        var startInfo = new ProcessStartInfo();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/c exit 1";
        }
        else
        {
            startInfo.FileName = "sh";
            startInfo.Arguments = "-c \"exit 1\"";
        }

        var processRunner = new ProcessRunner();

        // Act
        Func<Task> act = async () => await processRunner.RunProcessAsync(startInfo);

        // Assert
        await act.Should().ThrowAsync<ProcessFailedException>();
    }
}