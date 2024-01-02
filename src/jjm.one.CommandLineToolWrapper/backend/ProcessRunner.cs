using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using jjm.one.CommandLineToolWrapper.types;

namespace jjm.one.CommandLineToolWrapper.backend;

/// <summary>
///     Implements <see cref="IProcessRunner" /> using <see cref="Process" />.
/// </summary>
internal class ProcessRunner : IProcessRunner
{
    #region interface implementation

    /// <inheritdoc />
    public async Task<ProcessResult> RunProcessAsync(ProcessStartInfo startInfo,
        bool captureOutput = true, bool captureError = true)
    {
        // local vars    
        string? output = null;
        string? error = null;

        // initialize string builders
        _outputBuilder.Clear();
        _errorBuilder.Clear();

        // Setup output capturing
        if (captureOutput)
            // Redirect the standard output so it can be read programmatically
            startInfo.RedirectStandardOutput = true;

        // Setup error capturing
        if (captureError)
            // Redirect the standard error so it can be read programmatically
            startInfo.RedirectStandardError = true;

        // Create a new process with the provided start info and enable raising events
        var process = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };

        // Register an event handler to capture the output data as it's received
        if (captureOutput) process.OutputDataReceived += (_, args) => _outputBuilder.AppendLine(args.Data);

        // Register an event handler to capture the error data as it's received
        if (captureError) process.ErrorDataReceived += (_, args) => _errorBuilder.AppendLine(args.Data);

        // Start the process and begin reading its output
        process.Start();

        // Begin reading the output
        if (captureOutput) process.BeginOutputReadLine();
        // Begin reading the error
        if (captureError) process.BeginErrorReadLine();

        // Wait for the process to exit asynchronously
        await process.WaitForExitAsync();

        // Get the output of the process
        if (captureOutput) output = _outputBuilder.ToString();

        // Get the error of the process
        if (captureError) error = _errorBuilder.ToString();

        // If the process exited with a non-zero code, throw a ProcessFailedException
        if (process.ExitCode != 0)
            throw new ProcessFailedException(startInfo.FileName, startInfo.Arguments,
                new ProcessResult(process.ExitCode, output, error));

        // Return the process result
        return new ProcessResult(process.ExitCode, output, error);
    }

    #endregion

    #region private members

    /// <summary>
    ///     A StringBuilder instance used to capture and build the output string
    ///     from the process that is run.
    /// </summary>
    private readonly StringBuilder _outputBuilder = new();

    /// <summary>
    ///     A StringBuilder instance used to capture and build the error string
    ///     from the process that is run.
    /// </summary>
    private readonly StringBuilder _errorBuilder = new();

    #endregion
}