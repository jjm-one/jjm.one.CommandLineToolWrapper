using jjm.one.CommandLineToolWrapper.types;

namespace jjm.one.CommandLineToolWrapper;
/// <summary>
/// Represents a wrapper for a command line tool.
/// </summary>
public interface IToolWrapper
{
    /// <summary>
    /// Runs a command asynchronously.
    /// </summary>
    /// <param name="command">The command to run.</param>
    /// <param name="args">The arguments for the command.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, with a <see cref="ProcessResult"/> as the result.</returns>
    Task<ProcessResult> RunCommandAsync(string command, params object?[] args);
}