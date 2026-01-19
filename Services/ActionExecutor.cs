using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutomationProfileManager.Models;
using Playnite.SDK;

namespace AutomationProfileManager.Services
{
    public class ActionExecutionResult
    {
        public bool Success { get; set; }
        public int ExitCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool SkippedByCondition { get; set; }
        public bool TimedOut { get; set; }
        public double ExecutionTimeMs { get; set; }
        public Guid ActionId { get; set; }
    }

    public class ActionExecutor
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private ResolutionService? resolutionService;
        private AudioService? audioService;
        private ConditionService? conditionService;
        private ActionLogService? logService;
        private StatisticsService? statisticsService;
        private Dictionary<Guid, ActionExecutionResult> executionResults = new Dictionary<Guid, ActionExecutionResult>();

        public ActionExecutor(IPlayniteAPI api)
        {
            conditionService = new ConditionService();
            audioService = new AudioService();
        }

        public void SetLogService(ActionLogService service)
        {
            logService = service;
        }

        public void SetStatisticsService(StatisticsService service)
        {
            statisticsService = service;
        }

        private ResolutionService GetResolutionService()
        {
            try
            {
                resolutionService ??= new ResolutionService();
                return resolutionService;
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "Failed to initialize ResolutionService, resolution changes disabled");
                throw;
            }
        }

        private AudioService GetAudioService()
        {
            audioService ??= new AudioService();
            return audioService;
        }

        public async Task<List<ActionExecutionResult>> ExecuteActionsAsync(List<GameAction> actions, bool dryRun = false)
        {
            var results = new List<ActionExecutionResult>();
            executionResults.Clear();

            var sequentialActions = actions.Where(a => !a.RunInParallel).ToList();
            var parallelActions = actions.Where(a => a.RunInParallel).ToList();

            // Execute parallel actions first
            if (parallelActions.Any())
            {
                logger.Info($"Executing {parallelActions.Count} actions in parallel...");
                var parallelTasks = parallelActions.Select(a => ExecuteActionWithTimeoutAsync(a, dryRun)).ToList();
                
                if (parallelActions.All(a => !a.WaitForCompletion))
                {
                    // Fire and forget
                    _ = Task.WhenAll(parallelTasks);
                }
                else
                {
                    var parallelResults = await Task.WhenAll(parallelTasks);
                    results.AddRange(parallelResults);
                }
            }

            // Execute sequential actions
            foreach (var action in sequentialActions)
            {
                // Check dependency
                if (action.RequiresPreviousSuccess && action.DependsOnActionId.HasValue)
                {
                    if (executionResults.TryGetValue(action.DependsOnActionId.Value, out var depResult))
                    {
                        if (!depResult.Success)
                        {
                            var skipResult = new ActionExecutionResult
                            {
                                ActionId = action.Id,
                                Success = false,
                                Message = $"Skipped: dependency action failed",
                                SkippedByCondition = true
                            };
                            results.Add(skipResult);
                            executionResults[action.Id] = skipResult;
                            continue;
                        }
                    }
                }

                var result = await ExecuteActionWithTimeoutAsync(action, dryRun);
                results.Add(result);
                executionResults[action.Id] = result;
            }

            return results;
        }

        private async Task<ActionExecutionResult> ExecuteActionWithTimeoutAsync(GameAction action, bool dryRun)
        {
            var stopwatch = Stopwatch.StartNew();
            var timeoutSeconds = action.TimeoutSeconds > 0 ? action.TimeoutSeconds : 60;

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                var result = await ExecuteActionAsync(action, dryRun);
                
                stopwatch.Stop();
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                result.ActionId = action.Id;

                statisticsService?.RecordActionExecution(action, result.Success, result.ExecutionTimeMs);

                return result;
            }
            catch (OperationCanceledException)
            {
                stopwatch.Stop();
                var result = new ActionExecutionResult
                {
                    ActionId = action.Id,
                    Success = false,
                    TimedOut = true,
                    Message = $"Action timed out after {timeoutSeconds} seconds",
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };
                
                logService?.Log(action, false, -1, result.Message, dryRun);
                statisticsService?.RecordActionExecution(action, false, result.ExecutionTimeMs);
                
                return result;
            }
        }

        public async Task<ActionExecutionResult> ExecuteActionAsync(GameAction action, bool dryRun = false)
        {
            var result = new ActionExecutionResult();

            try
            {
                // Check conditions first
                if (!dryRun && conditionService != null && !conditionService.EvaluateCondition(action.Condition))
                {
                    result.Success = true;
                    result.SkippedByCondition = true;
                    result.Message = $"Skipped: condition not met ({action.Condition?.Type})";
                    logService?.Log(action, true, 0, result.Message, false);
                    logger.Info($"Action '{action.Name}' skipped due to condition: {action.Condition?.Type}");
                    return result;
                }

                string dryRunPrefix = dryRun ? "[DRY-RUN] " : "";
                logger.Info($"{dryRunPrefix}Executing action: {action.Name} (Type: {action.ActionType}, Phase: {action.ExecutionPhase})");

                if (dryRun)
                {
                    result.Success = true;
                    result.Message = $"Would execute: {action.ActionType} - {action.Path} {action.Arguments}";
                    logService?.Log(action, true, 0, result.Message, true);
                    return result;
                }

                switch (action.ActionType)
                {
                    case ActionType.StartApp:
                        result = ExecuteStartApp(action);
                        break;
                    
                    case ActionType.CloseApp:
                        result = ExecuteCloseApp(action);
                        break;
                    
                    case ActionType.PowerShellScript:
                        result = await ExecutePowerShellScriptAsync(action);
                        break;
                    
                    case ActionType.SystemCommand:
                        result = ExecuteSystemCommand(action);
                        break;
                    
                    case ActionType.Wait:
                        await ExecuteWaitAsync(action);
                        result.Success = true;
                        result.Message = $"Waited {action.WaitSeconds} seconds";
                        break;

                    case ActionType.ChangeResolution:
                        result = ExecuteChangeResolution(action);
                        break;

                    case ActionType.SetVolume:
                        result = ExecuteSetVolume(action);
                        break;

                    case ActionType.MuteApp:
                        result = ExecuteMuteApp(action);
                        break;

                    case ActionType.UnmuteApp:
                        result = ExecuteUnmuteApp(action);
                        break;
                    
                    default:
                        logger.Warn($"Unknown action type: {action.ActionType}");
                        result.Success = false;
                        result.Message = $"Unknown action type: {action.ActionType}";
                        break;
                }

                logService?.Log(action, result.Success, result.ExitCode, result.Message, false);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to execute action: {action.Name}");
                result.Success = false;
                result.Message = ex.Message;
                logService?.Log(action, false, -1, ex.Message, dryRun);
            }

            return result;
        }

        public void SaveCurrentResolution()
        {
            GetResolutionService().SaveCurrentSettings();
        }

        public bool RestoreResolution()
        {
            return GetResolutionService().RestoreOriginalSettings();
        }

        private ActionExecutionResult ExecuteChangeResolution(GameAction action)
        {
            var result = new ActionExecutionResult();
            try
            {
                // Handle RESTORE command
                if (action.Path.Equals("RESTORE", StringComparison.OrdinalIgnoreCase))
                {
                    bool restored = GetResolutionService().RestoreOriginalSettings();
                    result.Success = restored;
                    result.Message = restored 
                        ? "Restored original resolution"
                        : "Failed to restore original resolution (fallback may have been used)";
                    return result;
                }

                var (width, height, refreshRate) = ResolutionService.ParseResolutionString(action.Path);
                bool changeSuccess = GetResolutionService().ChangeResolution(width, height, refreshRate);
                result.Success = changeSuccess;
                result.Message = changeSuccess 
                    ? $"Changed resolution to {width}x{height}@{refreshRate}Hz"
                    : $"Failed to change resolution to {width}x{height}@{refreshRate}Hz";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }

        private ActionExecutionResult ExecuteStartApp(GameAction action)
        {
            var result = new ActionExecutionResult();
            try
            {
                var expandedPath = Environment.ExpandEnvironmentVariables(action.Path);
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = expandedPath,
                    Arguments = action.Arguments,
                    UseShellExecute = true
                };

                Process.Start(processStartInfo);
                logger.Info($"Started application: {action.Path}");
                result.Success = true;
                result.Message = $"Started: {action.Path}";
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to start app: {action.Path}");
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }

        private ActionExecutionResult ExecuteCloseApp(GameAction action)
        {
            var result = new ActionExecutionResult();
            try
            {
                var processName = Path.GetFileNameWithoutExtension(action.Path);
                var processes = Process.GetProcessesByName(processName);

                if (processes.Length == 0)
                {
                    logger.Info($"Process not running: {processName}");
                    result.Success = true;
                    result.Message = $"Process not running: {processName}";
                    return result;
                }

                int closed = 0;
                foreach (var process in processes)
                {
                    try
                    {
                        process.Kill();
                        closed++;
                        logger.Info($"Closed process: {processName} (PID: {process.Id})");
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex, $"Failed to close process: {processName} (PID: {process.Id})");
                    }
                }

                result.Success = true;
                result.Message = $"Closed {closed} instance(s) of {processName}";
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to close app: {action.Path}");
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }

        private Task<ActionExecutionResult> ExecutePowerShellScriptAsync(GameAction action)
        {
            return Task.Run(() => ExecutePowerShellViaProcess(action));
        }

        private ActionExecutionResult ExecutePowerShellViaProcess(GameAction action)
        {
            try
            {
                var resolvedPath = string.IsNullOrWhiteSpace(action.Path)
                    ? string.Empty
                    : Environment.ExpandEnvironmentVariables(action.Path);

                bool isScriptFile = !string.IsNullOrWhiteSpace(resolvedPath) &&
                    (resolvedPath.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase) ||
                     File.Exists(resolvedPath));

                string arguments;
                if (isScriptFile)
                {
                    arguments = $"-ExecutionPolicy Bypass -File \"{resolvedPath}\" {action.Arguments}".Trim();
                }
                else
                {
                    var command = action.Path ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(action.Arguments))
                    {
                        command = $"{command} {action.Arguments}".Trim();
                    }

                    command = command.Replace("\"", "`\"");
                    arguments = $"-ExecutionPolicy Bypass -Command \"{command}\"";
                }

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                var result = new ActionExecutionResult();
                using (var process = Process.Start(processStartInfo))
                {
                    if (process != null)
                    {
                        process.WaitForExit();
                        result.ExitCode = process.ExitCode;
                        result.Success = process.ExitCode == 0;
                        result.Message = $"PowerShell {(isScriptFile ? "script" : "command")} completed with exit code {process.ExitCode}";
                        logger.Info($"Executed PowerShell {(isScriptFile ? "script" : "command")}: {action.Path}");
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to execute PowerShell script via process: {action.Path}");
                return new ActionExecutionResult { Success = false, Message = ex.Message };
            }
        }

        private ActionExecutionResult ExecuteSystemCommand(GameAction action)
        {
            var result = new ActionExecutionResult();
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {action.Path} {action.Arguments}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(processStartInfo))
                {
                    if (process != null)
                    {
                        process.WaitForExit();
                        result.ExitCode = process.ExitCode;
                        result.Success = process.ExitCode == 0;
                        result.Message = $"Command completed with exit code {process.ExitCode}";
                        logger.Info($"Executed system command: {action.Path}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to execute system command: {action.Path}");
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }

        private Task ExecuteWaitAsync(GameAction action)
        {
            var waitTime = action.WaitSeconds > 0 ? action.WaitSeconds : 1;
            logger.Info($"Waiting {waitTime} seconds...");
            return Task.Delay(waitTime * 1000);
        }

        private ActionExecutionResult ExecuteSetVolume(GameAction action)
        {
            var result = new ActionExecutionResult();
            try
            {
                if (int.TryParse(action.Path, out int volumePercent))
                {
                    bool success = GetAudioService().SetMasterVolume(volumePercent);
                    result.Success = success;
                    result.Message = success 
                        ? $"Set master volume to {volumePercent}%"
                        : "Failed to set master volume";
                }
                else if (action.Path.Equals("RESTORE", StringComparison.OrdinalIgnoreCase))
                {
                    bool success = GetAudioService().RestoreOriginalVolume();
                    result.Success = success;
                    result.Message = success 
                        ? "Restored original volume"
                        : "Failed to restore original volume";
                }
                else
                {
                    result.Success = false;
                    result.Message = $"Invalid volume value: {action.Path}. Use a number 0-100 or 'RESTORE'.";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }

        private ActionExecutionResult ExecuteMuteApp(GameAction action)
        {
            var result = new ActionExecutionResult();
            try
            {
                bool success = GetAudioService().MuteProcess(action.Path);
                result.Success = success;
                result.Message = success 
                    ? $"Muted process: {action.Path}"
                    : $"Failed to mute process: {action.Path}";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }

        private ActionExecutionResult ExecuteUnmuteApp(GameAction action)
        {
            var result = new ActionExecutionResult();
            try
            {
                bool success = GetAudioService().UnmuteProcess(action.Path);
                result.Success = success;
                result.Message = success 
                    ? $"Unmuted process: {action.Path}"
                    : $"Failed to unmute process: {action.Path}";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }

        public void SaveCurrentVolume()
        {
            GetAudioService().SaveCurrentVolume();
        }
    }
}
