namespace jjm.one.LetsEncryptClient.types;

public class ProcessFailedException(int exitCode, string command, string arguments, string? output = null)
    : Exception($"The process '{command} {arguments}' exited with code {exitCode}.")
{
    public int ExitCode { get; } = exitCode;
    public string Command { get; } = command;
    public string Arguments { get; } = arguments;
    public string? Output { get; } = output;
}