using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using jjm.one.CommandLineToolWrapper.types;

namespace jjm.one.CommandLineToolWrapper.backend;

internal class ProcessRunner : IProcessRunner
{
    public async Task<ProcessResult> RunProcessAsync(ProcessStartInfo startInfo)
    {
        var process = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };

        var outputBuilder = new StringBuilder();
        process.OutputDataReceived += (sender, args) => outputBuilder.AppendLine(args.Data);

        process.Start();
        process.BeginOutputReadLine();

        await process.WaitForExitAsync();

        var output = outputBuilder.ToString();
        if (process.ExitCode != 0)
        {
            throw new ProcessFailedException(process.ExitCode, startInfo.FileName, startInfo.Arguments, output);
        }

        return new ProcessResult(process.ExitCode, output);
    }
}