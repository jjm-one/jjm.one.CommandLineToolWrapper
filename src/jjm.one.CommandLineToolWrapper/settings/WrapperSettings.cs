using System;
using System.Diagnostics.CodeAnalysis;

namespace jjm.one.CommandLineToolWrapper.settings;

/// <summary>
///     Represents the settings for a command line tool wrapper.
/// </summary>
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public class WrapperSettings
{
    /// <summary>
    ///     Gets or sets the working directory for the tool.
    /// </summary>
    public string WorkingDirectory { get; init; } = Environment.CurrentDirectory;

    /// <summary>
    ///     Gets or sets a value indicating whether to show an error dialog.
    /// </summary>
    public bool ErrorDialog { get; init; } = false;

    /// <summary>
    ///     Gets or sets the retry count for the tool.
    /// </summary>
    public int RetryCount { get; init; } = 3;

    /// <summary>
    ///     Gets or sets the retry interval in seconds.
    /// </summary>
    public int RetryIntervalInSeconds { get; init; } = 10;

    /// <summary>
    ///     Gets or sets a value indicating whether to use exit code analysis for retries.
    /// </summary>
    public bool RetryUseExitCodeAnalysis { get; init; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether to use output analysis for retries.
    /// </summary>
    public bool RetryUseOutputAnalysis { get; init; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether to use error analysis for retries.
    /// </summary>
    public bool RetryUseErrorAnalysis { get; init; } = true;
}