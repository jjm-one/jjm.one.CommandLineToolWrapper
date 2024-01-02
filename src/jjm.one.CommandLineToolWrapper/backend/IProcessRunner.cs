using System.Diagnostics;
using System.Threading.Tasks;
using jjm.one.CommandLineToolWrapper.types;

namespace jjm.one.CommandLineToolWrapper.backend;

/// <summary>
/// Represents a class that is responsible for running processes.
/// </summary>
public interface IProcessRunner
{
    /// <summary>
    /// Runs a process asynchronously using the provided start info.
    /// </summary>
    /// <param name="startInfo">The process start info.</param>
    /// <param name="captureOutput">Whether to capture the process output.</param>
    /// <param name="captureError">Whether to capture the process error.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the process result.</returns>
    /// <exception cref="ProcessFailedException">Thrown when the process exits with a non-zero code.</exception>
    Task<ProcessResult> RunProcessAsync(ProcessStartInfo startInfo, 
    bool captureOutput = true, bool captureError = true);
}