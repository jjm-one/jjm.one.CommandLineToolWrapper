namespace jjm.one.CommandLineToolWrapper.types;

public class ProcessResult(int exitCode, string? output = null, string? error = null)
{
    public ProcessResult() : this(0) { }

    public int ExitCode { get; set; } = exitCode;
    public string? Output { get; set; } = output;
    public string? Error { get; set; } = error;
}