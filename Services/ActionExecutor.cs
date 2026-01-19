using System;
using System.Diagnostics;
using System.IO;
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
    }

    public class ActionExecutor
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private ResolutionService? resolutionService;
        private ActionLogService? logService;

        public ActionExecutor(IPlayniteAPI api)
        {
        }

        public void SetLogService(ActionLogService service)
        {
            logService = service;
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

        public async Task<ActionExecutionResult> ExecuteActionAsync(GameAction action, bool dryRun = false)
        {
            var result = new ActionExecutionResult();

            try
            {
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
    }
}
