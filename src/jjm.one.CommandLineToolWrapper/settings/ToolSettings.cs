using System;
using System.Collections.Generic;

namespace jjm.one.CommandLineToolWrapper.settings;

public class ToolSettings
{
    public string ToolPath { get; set; } = "certbot";
    public Dictionary<string, string> CommandTemplates { get; set; } = new()
    {
        ["help"] = "--help",
        ["version"] = "--version"
    };

    public bool RedirectStandardOutput { get; set; } = false;
    public bool RedirectStandardError { get; set; } = false;
    public bool UseShellExecute { get; set; } = false;
    public bool CreateNoWindow { get; set; } = true;
    public string WorkingDirectory { get; set; } = Environment.CurrentDirectory;
    public string Verb { get; set; } = "runas";
    public bool ErrorDialog { get; set; } = false;

    public int RetryCount { get; set; } = 3;
    public int RetryIntervalInSeconds { get; set; } = 10;
    public bool RetryUseOutputAnalysis { get; set; } = true;
    public bool RetryUseExitCodeAnalysis { get; set; } = true;
    public List<string> RetryOutputContains { get; set; } = ["network error", "timeout"];
    public List<int> RetryExitCodes { get; set; } = [1];
}