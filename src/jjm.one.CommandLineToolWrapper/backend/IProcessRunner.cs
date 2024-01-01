using System.Diagnostics;
using jjm.one.CommandLineToolWrapper.types;

namespace jjm.one.CommandLineToolWrapper.backend;

public interface IProcessRunner
{
    Task<ProcessResult> RunProcessAsync(ProcessStartInfo startInfo);
}