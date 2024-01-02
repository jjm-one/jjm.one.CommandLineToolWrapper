using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using jjm.one.CommandLineToolWrapper.backend;
using jjm.one.CommandLineToolWrapper.settings;
using jjm.one.CommandLineToolWrapper.types;
using Microsoft.Extensions.Logging;
using Polly;

namespace jjm.one.CommandLineToolWrapper;

/// <summary>
///     The default implementation of the <see cref="IToolWrapper" /> interface.
/// </summary>
public partial class ToolWrapper : IToolWrapper
{
    #region ctor's

    /// <summary>
    ///     Initializes a new instance of the <see cref="ToolWrapper" /> class.
    /// </summary>
    /// <param name="toolSettings">The settings for the command line tool.</param>
    /// <param name="wrapperSettings">The settings for the command line tool wrapper.</param>
    /// <param name="processRunner">The process runner to use. If null, a new <see cref="ProcessRunner" /> will be created.</param>
    /// <param name="logger">The logger to use. If null, logging will be disabled.</param>
    public ToolWrapper(ToolSettings toolSettings, WrapperSettings wrapperSettings,
        IProcessRunner? processRunner = null, ILogger<ToolWrapper>? logger = null)
    {
        _processRunner = processRunner ?? new ProcessRunner();
        _toolSettings = toolSettings;
        _wrapperSettings = wrapperSettings;
        _logger = logger;
    }

    #endregion

    #region interface implementation

    /// <InheritDoc />
    public async Task<ProcessResult> RunCommandAsync(string command, params object?[] args)
    {
        // Define a retry policy using Polly
        // This policy handles ProcessFailedException, which is thrown when the process fails
        // The policy checks if the exit code or the output of the process indicates a transient error that is worth retrying
        var retryPolicy = Policy
            .Handle<ProcessFailedException>(ex => ex.Result is not null &&
                                                  (CheckExitCode(ex.Result.ExitCode) || CheckOutput(ex.Result.Output) ||
                                                   CheckError(ex.Result
                                                       .Error))) // Check if the exception should be handled by the policy
            .WaitAndRetryAsync(_wrapperSettings.RetryCount, // The number of retries
                _ => TimeSpan.FromSeconds(_wrapperSettings.RetryIntervalInSeconds), // The delay between retries
                (exception, _, retryCount, _) =>
                {
                    // On retry, log a warning message
                    _logger?.LogWarning(
                        "Retry {RetryCount} for command '{Command}' due to an error: {ExceptionMessage}",
                        retryCount, command, exception.Message);
                });

        return await retryPolicy.ExecuteAsync(async () =>
        {
            // Check if the command exists in the command templates
            if (!_toolSettings.CommandTemplates.TryGetValue(command, out var commandTemplate))
            {
                _logger?.LogError("Command '{Command}' not found", command);
                throw new ArgumentException($"Command '{command}' not found.", nameof(command));
            }

            // Count the expected arguments for the command
            var expectedArgs = MyRegex().Matches(commandTemplate).Count;
            if (args.Length != expectedArgs)
            {
                _logger?.LogError(
                    "Command '{Command}' expects {ExpectedArgs} arguments, but got {ArgsLength}",
                    command, expectedArgs, args.Length);
                throw new ArgumentException(
                    $"Command '{command}' expects {expectedArgs} arguments, but got {args.Length}.",
                    nameof(args));
            }

            // Format the command arguments
            var arguments = string.Format(commandTemplate, args);
            // Create a new process start info with the tool path, arguments, and working directory
            var startInfo = new ProcessStartInfo
            {
                FileName = _toolSettings.ToolPath,
                Arguments = arguments,
                WorkingDirectory = _wrapperSettings.WorkingDirectory,
                ErrorDialog = _wrapperSettings.ErrorDialog
            };

            // Log the command that will be run
            _logger?.LogDebug("Starting process with command: '{FileName} {Arguments}'",
                startInfo.FileName, startInfo.Arguments);

            // Start a stopwatch to measure the execution time
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Run the process asynchronously and get the result
                var result = await _processRunner.RunProcessAsync(startInfo);

                if (_logger != null && _logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("Process exited with code {ExitCode}. Output: {Output}",
                        result.ExitCode, result.Output);

                // Return the process result
                return result;
            }
            catch (ProcessFailedException ex)
            {
                // Handle a <see cref="ProcessFailedException"/> by logging the error and throwing it again
                _logger?.LogError(ex, "An error occurred while running the '{Command}' command", command);
                throw;
            }
            catch (Exception ex)
            {
                // Handle any other exception by logging the error and throwing it again
                _logger?.LogError(ex, "An unexpected error occurred while running the '{Command}' command",
                    command);
                throw;
            }
            finally
            {
                // Stop the stopwatch and log the execution time
                stopwatch.Stop();
                _logger?.LogTrace("Command '{Command}' executed in {ElapsedTotalSeconds} seconds",
                    command, stopwatch.Elapsed.TotalSeconds);
            }
        });
    }

    #endregion

    #region private static members

    /// <summary>
    ///     Represents a regular expression used for pattern matching in the ToolWrapper class.
    /// </summary>
    [GeneratedRegex("{.*?}")]
    private static partial Regex MyRegex();

    #endregion

    #region private members

    /// <summary>
    ///     The process runner used to execute commands.
    /// </summary>
    private readonly IProcessRunner _processRunner;

    /// <summary>
    ///     The settings for the command line tool.
    /// </summary>
    private readonly ToolSettings _toolSettings;

    /// <summary>
    ///     The settings for the command line tool wrapper.
    /// </summary>
    private readonly WrapperSettings _wrapperSettings;

    /// <summary>
    ///     The logger used to log information about the tool's operations.
    /// </summary>
    private readonly ILogger<ToolWrapper>? _logger;

    #endregion

    #region internal methods

    /// <summary>
    ///     Checks if the exit code should trigger a retry.
    /// </summary>
    /// <param name="exitCode">The exit code to check.</param>
    /// <returns>True if the exit code should trigger a retry, false otherwise.</returns>
    internal bool CheckExitCode(int exitCode)
    {
        return _wrapperSettings.RetryUseExitCodeAnalysis && _toolSettings.RetryExitCodes.Contains(exitCode);
    }

    /// <summary>
    ///     Checks if the output should trigger a retry.
    /// </summary>
    /// <param name="output">The output to check.</param>
    /// <returns>True if the output should trigger a retry, false otherwise.</returns>
    internal bool CheckOutput(string? output)
    {
        return output != null && _wrapperSettings.RetryUseOutputAnalysis &&
               _toolSettings.RetryOutputContains.Exists(output.Contains);
    }

    /// <summary>
    ///     Checks if the error should trigger a retry.
    /// </summary>
    /// <param name="error">The output to check.</param>
    /// <returns>True if the output should trigger a retry, false otherwise.</returns>
    internal bool CheckError(string? error)
    {
        return error != null && _wrapperSettings.RetryUseErrorAnalysis &&
               _toolSettings.RetryErrorContains.Exists(error.Contains);
    }

    #endregion
}