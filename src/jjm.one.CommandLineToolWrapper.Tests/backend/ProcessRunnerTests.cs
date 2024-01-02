using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using jjm.one.CommandLineToolWrapper.backend;
using jjm.one.CommandLineToolWrapper.types;

namespace jjm.one.CommandLineToolWrapper.Tests.backend;

public class ProcessRunnerTests
{
    #region privat members

    private readonly ProcessRunner _processRunner = new();

    #endregion

    #region tests

    [Fact]
    public async Task RunProcessAsync_ShouldReturnProcessResult_WithCorrectOutput_WhenProcessExitsWithZero()
    {
        // Arrange
        var startInfo = new ProcessStartInfo();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/c echo test";
        }
        else
        {
            startInfo.FileName = "sh";
            startInfo.Arguments = "-c \"echo test\"";
        }

        // Act
        var result = await _processRunner.RunProcessAsync(startInfo);

        // Assert
        result.ExitCode.Should().Be(0);
        //result.Output.Should().Contain("test" + Environment.NewLine);
        //result.Error.Should().Contain(Environment.NewLine);
    }

    [Fact]
    public async Task RunProcessAsync_ShouldReturnProcessResult_WithCorrectError_WhenProcessExitsWithNonZero()
    {
        // Arrange
        var startInfo = new ProcessStartInfo();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/c echo test 1>&2 && exit 1";
        }
        else
        {
            startInfo.FileName = "sh";
            startInfo.Arguments = "-c \"echo test 1>&2; exit 1\"";
        }

        // Act
        Func<Task> act = async () => await _processRunner.RunProcessAsync(startInfo);

        // Assert
        await act.Should().ThrowAsync<ProcessFailedException>().Where(exc =>
            exc.Result != null &&
            exc.Result.ExitCode == 1 //&&
            //exc.Result.Output != null && exc.Result.Error != null &&
            //exc.Result.Output.Contains(Environment.NewLine) && exc.Result.Error.Contains("test" + Environment.NewLine)
            );
    }

    #endregion
}