using System;
using System.Collections.Generic;
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
/// The default implementation of the <see cref="IToolWrapper"/> interface.
/// </summary>
public partial class ToolWrapper : IToolWrapper
{
    #region private static members

    /// <summary>
    /// Represents a regular expression used for pattern matching in the ToolWrapper class.
    /// </summary>
    [GeneratedRegex("{.*?}")]
    private static partial Regex MyRegex();

    #endregion

    #region private members

    /// <summary>
    /// The process runner used to execute commands.
    /// </summary>
    private readonly IProcessRunner _processRunner;

    /// <summary>
    /// The settings for the tool.
    /// </summary>
    private readonly ToolSettings _settings;

    /// <summary>
    /// The logger used to log information about the tool's operations.
    /// </summary>
    private readonly ILogger<ToolWrapper>? _logger;

    /// <summary>
    /// The dictionary mapping command names to their templates.
    /// </summary>
    private readonly Dictionary<string, string> _commandTemplates;

    #endregion
    
    #region ctor's

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolWrapper"/> class.
    /// </summary>
    /// <param name="settings">The settings for the tool.</param>
    /// <param name="processRunner">The process runner to use. If null, a new <see cref="ProcessRunner"/> will be created.</param>
    /// <param name="logger">The logger to use. If null, logging will be disabled.</param>
    public ToolWrapper(ToolSettings settings, IProcessRunner? processRunner = null, ILogger<ToolWrapper>? logger = null)
    {
        _processRunner = processRunner ?? new ProcessRunner();
        _settings = settings;
        _logger = logger;
        
        // initialize the command templates
        _commandTemplates = new Dictionary<string, string>
        {
            { "help", "--help" },
            { "version", "--version" },
        };
        
        // add the command templates from the settings
        foreach (var command in _settings.CommandTemplates)
        {
            _commandTemplates[command.Key] = command.Value;
        }
    }

    #endregion

    #region interface implementation

    /// <InheritDoc />
    public async Task<ProcessResult> RunCommandAsync(string command, params object?[] args)
    {
        // initialize the retry policy
        var retryPolicy = Policy
            .Handle<ProcessFailedException>(ex =>
                CheckExitCode(ex.ExitCode) || CheckOutput(ex.Output))
            .WaitAndRetryAsync(_settings.RetryCount, _ => 
                    TimeSpan.FromSeconds(_settings.RetryIntervalInSeconds),
                (exception, _, retryCount, _) =>
                {
                    _logger?.LogWarning(
                        "Retry {retryCount} for command '{command}' due to an error: {exception.Message}",
                        retryCount, command, exception.Message);
                });
        
        // execute the command wrapped in the retry policy
        return await retryPolicy.ExecuteAsync(async () =>
        {
            // check if the command exists
            if (!_commandTemplates.TryGetValue(command, out var commandTemplate))
            {
                _logger?.LogError("Command '{command}' not found.", command);
                throw new ArgumentException($"Command '{command}' not found.", nameof(command));
            }

            // check if the number of arguments is correct
            var expectedArgs = MyRegex().Matches(commandTemplate).Count;
            if (args.Length != expectedArgs)
            {
                _logger?.LogError(
                    "Command '{command}' expects {expectedArgs} arguments, but got {args.Length}.",
                    command, expectedArgs, args.Length);
                throw new ArgumentException(
                    $"Command '{command}' expects {expectedArgs} arguments, but got {args.Length}.", 
                    nameof(args));
            }

            // build the arguments
            var arguments = string.Format(commandTemplate, args);
            var startInfo = new ProcessStartInfo
            {
                FileName = _settings.ToolPath,
                Arguments = arguments,
                WorkingDirectory = _settings.WorkingDirectory,
                ErrorDialog = _settings.ErrorDialog
            };

            _logger?.LogDebug("Starting process with command: '{startInfo.FileName} {startInfo.Arguments}'",
                startInfo.FileName, startInfo.Arguments);

            // start the stopwatch
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // run the process
                var result = await _processRunner.RunProcessAsync(startInfo);

                if (_logger != null && _logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Process exited with code {result.ExitCode}. Output: {result.Output}",
                        result.ExitCode, result.Output);
                }

                return result;
            }
            catch (ProcessFailedException ex)
            {
                _logger?.LogError(ex, "An error occurred while running the '{command}' command.", command);
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "An unexpected error occurred while running the '{command}' command.", 
                    command);
                throw;
            }
            finally
            {
                // stop the stopwatch
                stopwatch.Stop();
                
                // log the execution time
                _logger?.LogTrace("Command '{command}' executed in {stopwatch.Elapsed.TotalSeconds} seconds.",
                    command, stopwatch.Elapsed.TotalSeconds);
            }
        });
    }

    #endregion
    
    #region private methods

    /// <summary>
    /// Checks if the exit code should trigger a retry.
    /// </summary>
    /// <param name="exitCode">The exit code to check.</param>
    /// <returns>True if the exit code should trigger a retry, false otherwise.</returns>
    private bool CheckExitCode(int exitCode)
    {
        return _settings.RetryUseExitCodeAnalysis && _settings.RetryExitCodes.Contains(exitCode);
    }
    
    /// <summary>
    /// Checks if the output should trigger a retry.
    /// </summary>
    /// <param name="output">The output to check.</param>
    /// <returns>True if the output should trigger a retry, false otherwise.</returns>
    private bool CheckOutput(string? output)
    {
        return output != null && _settings.RetryUseOutputAnalysis && _settings.RetryOutputContains.Any(output.Contains);
    }

    #endregion
}