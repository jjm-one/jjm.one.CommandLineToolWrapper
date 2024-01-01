namespace jjm.one.LetsEncryptClient.types;

public class ProcessResult(int exitCode, string output)
{
    public int ExitCode { get; } = exitCode;
    public string Output { get; } = output;
}