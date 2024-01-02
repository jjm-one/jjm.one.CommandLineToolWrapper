using System;

namespace jjm.one.CommandLineToolWrapper.types;

/// <summary>
/// Represents an exception that is thrown when a process run by the ToolWrapper fails.
/// </summary>
public class ProcessFailedException(string command, string arguments, ProcessResult? processResult = null)
    : Exception($"The process '{command} {arguments}' exited with code " +
                $"{(processResult is null? "unknown" : processResult.ExitCode)}.")
{
    #region public members

    /// <summary>
    /// Gets the command that was run when the process failed.
    /// </summary>
    public string Command { get; } = command;

    /// <summary>
    /// Gets the arguments that were used when the process failed.
    /// </summary>
    public string Arguments { get; } = arguments;

    /// <summary>
    /// Gets the result of the process, or null if the result is not available.
    /// </summary>
    public ProcessResult? Result { get; } = processResult;

    #endregion
}