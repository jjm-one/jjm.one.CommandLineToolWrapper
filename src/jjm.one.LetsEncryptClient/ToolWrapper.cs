using System.Diagnostics;
using System.Text.RegularExpressions;
using jjm.one.LetsEncryptClient.backend;
using jjm.one.LetsEncryptClient.settings;
using jjm.one.LetsEncryptClient.types;
using Microsoft.Extensions.Logging;
using Polly;

namespace jjm.one.LetsEncryptClient;

public partial class ToolWrapper : IToolWrapper
{
    private readonly IProcessRunner _processRunner;
    private readonly ToolSettings _settings;
    private readonly ILogger<ToolWrapper>? _logger;
    private readonly Dictionary<string, string> _commandTemplates;

    public ToolWrapper(ToolSettings settings, IProcessRunner? processRunner = null, ILogger<ToolWrapper>? logger = null)
    {
        _processRunner = processRunner ?? new ProcessRunner();
        _settings = settings;
        _logger = logger;
        
        _commandTemplates = new Dictionary<string, string>
        {
            { "help", "--help" },
            { "version", "--version" },
        };
        
        foreach (var command in _settings.CommandTemplates)
        {
            _commandTemplates[command.Key] = command.Value;
        }
    }

    public async Task<ProcessResult> RunCommandAsync(string command, params object?[] args)
    {
        var retryPolicy = Policy
            .Handle<ProcessFailedException>(ex =>
                CheckExitCode(ex.ExitCode) && CheckOutput(ex.Output))
            .WaitAndRetryAsync(_settings.RetryCount, retryAttempt => 
                    TimeSpan.FromSeconds(_settings.RetryIntervalInSeconds),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger?.LogWarning(
                        "Retry {retryCount} for command '{command}' due to an error: {exception.Message}",
                        retryCount, command, exception.Message);
                });
        
        return await retryPolicy.ExecuteAsync(async () =>
        {
            if (!_commandTemplates.TryGetValue(command, out var commandTemplate))
            {
                _logger?.LogError("Command '{command}' not found.", command);
                throw new ArgumentException($"Command '{command}' not found.", nameof(command));
            }

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

            var arguments = string.Format(commandTemplate, args);
            var startInfo = new ProcessStartInfo
            {
                FileName = _settings.ToolPath,
                Arguments = arguments,
                RedirectStandardOutput = _settings.RedirectStandardOutput,
                RedirectStandardError = _settings.RedirectStandardError,
                UseShellExecute = _settings.UseShellExecute,
                CreateNoWindow = _settings.CreateNoWindow,
                WorkingDirectory = _settings.WorkingDirectory,
                Verb = _settings.Verb,
                ErrorDialog = _settings.ErrorDialog
            };

            _logger?.LogDebug("Starting process with command: '{startInfo.FileName} {startInfo.Arguments}'",
                startInfo.FileName, startInfo.Arguments);

            var stopwatch = Stopwatch.StartNew();

            try
            {
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
                _logger?.LogError(ex, "An error occurred while running the '{command}' command.",
                    command);
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
                stopwatch.Stop();
                _logger?.LogTrace("Command '{command}' executed in {stopwatch.Elapsed.TotalSeconds} seconds.",
                    command, stopwatch.Elapsed.TotalSeconds);
            }
        });
    }

    [GeneratedRegex("{.*?}")]
    private static partial Regex MyRegex();
    
    private bool CheckExitCode(int exitCode)
    {
        return _settings.RetryUseExitCodeAnalysis && _settings.RetryExitCodes.Contains(exitCode);
    }
    
    private bool CheckOutput(string? output)
    {
        return output != null && _settings.RetryUseOutputAnalysis && _settings.RetryOutputContains.Any(output.Contains);
    }
}