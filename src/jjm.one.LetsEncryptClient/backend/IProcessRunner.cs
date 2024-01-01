using System.Diagnostics;
using jjm.one.LetsEncryptClient.types;

namespace jjm.one.LetsEncryptClient.backend;

public interface IProcessRunner
{
    Task<ProcessResult> RunProcessAsync(ProcessStartInfo startInfo);
}