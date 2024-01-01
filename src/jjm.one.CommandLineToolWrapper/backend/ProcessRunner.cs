using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using jjm.one.CommandLineToolWrapper.types;

namespace jjm.one.CommandLineToolWrapper.backend;

/// <summary>
/// Implements <see cref="IProcessRunner"/> using <see cref="Process"/>.
/// </summary>
internal class ProcessRunner : IProcessRunner
{
    /// <inheritdoc/>
    public async Task<ProcessResult> RunProcessAsync(ProcessStartInfo startInfo)
    {
        // Redirect the standard output and error so they can be read programmatically
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        
        // Create a new process with the provided start info and enable raising events
        var process = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };

        // Create a StringBuilder to capture the process output
        var outputBuilder = new StringBuilder();
        // Register an event handler to capture the output data as it's received
        process.OutputDataReceived += (_, args) => outputBuilder.AppendLine(args.Data);

        // Start the process and begin reading its output
        process.Start();
        process.BeginOutputReadLine();

        // Wait for the process to exit asynchronously
        await process.WaitForExitAsync();

        // Get the output of the process
        var output = outputBuilder.ToString();

        // If the process exited with a non-zero code, throw a ProcessFailedException
        if (process.ExitCode != 0)
        {
            throw new ProcessFailedException(process.ExitCode, startInfo.FileName, startInfo.Arguments, output);
        }

        // Return the process result
        return new ProcessResult(process.ExitCode, output);
    }
}