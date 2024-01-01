using System;
using System.Collections.Generic;

namespace jjm.one.CommandLineToolWrapper.settings;

/// <summary>
/// Represents the settings for a command line tool wrapper.
/// </summary>
public class WrapperSettings
{
    /// <summary>
    /// Gets or sets the working directory for the tool.
    /// </summary>
    public string WorkingDirectory { get; set; } = Environment.CurrentDirectory;

    /// <summary>
    /// Gets or sets a value indicating whether to show an error dialog.
    /// </summary>
    public bool ErrorDialog { get; set; } = false;

    /// <summary>
    /// Gets or sets the retry count for the tool.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the retry interval in seconds.
    /// </summary>
    public int RetryIntervalInSeconds { get; set; } = 10;

    /// <summary>
    /// Gets or sets a value indicating whether to use output analysis for retries.
    /// </summary>
    public bool RetryUseOutputAnalysis { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to use exit code analysis for retries.
    /// </summary>
    public bool RetryUseExitCodeAnalysis { get; set; } = true;
}