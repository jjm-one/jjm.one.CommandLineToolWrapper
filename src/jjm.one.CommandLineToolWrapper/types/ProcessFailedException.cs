using System;

namespace jjm.one.CommandLineToolWrapper.types;

public class ProcessFailedException(string command, string arguments, ProcessResult? processResult = null)
    : Exception($"The process '{command} {arguments}' exited with code " +
                $"{(processResult is null? "unknown" : processResult.ExitCode)}.")
{
    public string Command { get; } = command;
    public string Arguments { get; } = arguments;
    public ProcessResult? Result { get; } = processResult;
}