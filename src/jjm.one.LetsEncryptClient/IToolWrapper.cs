using jjm.one.LetsEncryptClient.types;

namespace jjm.one.LetsEncryptClient;

public interface IToolWrapper
{
    Task<ProcessResult> RunCommandAsync(string command, params object?[] args);
}