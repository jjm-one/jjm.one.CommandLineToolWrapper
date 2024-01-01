namespace jjm.one.CommandLineToolWrapper.types;

public class ProcessResult(int exitCode, string output)
{
    public ProcessResult() : this(0, string.Empty) { }

    public int ExitCode { get; } = exitCode;
    public string Output { get; } = output;
}