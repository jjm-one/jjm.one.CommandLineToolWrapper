using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace jjm.one.CommandLineToolWrapper.settings;

/// <summary>
///     Represents the settings for a command line tool.
/// </summary>
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public class ToolSettings
{
    /// <summary>
    ///     Gets or sets the path to the tool.
    /// </summary>
    public string ToolPath { get; init; } = string.Empty;

    /// <summary>
    ///     Gets or sets the command templates for the tool.
    /// </summary>
    public Dictionary<string, string> CommandTemplates { get; init; } = new()
    {
        ["help"] = "--help",
        ["version"] = "--version"
    };

    /// <summary>
    ///     Gets or sets the exit codes that trigger a retry.
    /// </summary>
    public List<int> RetryExitCodes { get; init; } = [1];

    /// <summary>
    ///     Gets or sets the output strings that trigger a retry.
    /// </summary>
    public List<string> RetryOutputContains { get; init; } = ["network error", "timeout"];

    /// <summary>
    ///     Gets or sets the error strings that trigger a retry.
    /// </summary>
    public List<string> RetryErrorContains { get; init; } = ["network error", "timeout"];
}