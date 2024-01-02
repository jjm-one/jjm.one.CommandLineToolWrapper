namespace jjm.one.CommandLineToolWrapper.types;

/// <summary>
/// Represents the result of a process run by the ToolWrapper.
/// </summary>
public class ProcessResult(int exitCode, string? output = null, string? error = null)
{
    #region public members

    /// <summary>
    /// Gets or sets the exit code of the process.
    /// </summary>
    public int ExitCode { get; init; } = exitCode;

    /// <summary>
    /// Gets or sets the output of the process, or null if there was no output.
    /// </summary>
    public string? Output { get; init; } = output;

    /// <summary>
    /// Gets or sets the error message of the process, or null if there was no error.
    /// </summary>
    public string? Error { get; init; } = error;

    #endregion

    #region ctor's

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessResult"/> class with a default exit code of 0.
    /// </summary>
    public ProcessResult() : this(0) { }

    #endregion
}