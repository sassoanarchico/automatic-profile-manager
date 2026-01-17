using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using AutomationProfileManager.Models;
using Playnite.SDK;

namespace AutomationProfileManager.Services
{
    public class ActionExecutor
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public ActionExecutor(IPlayniteAPI api)
        {
            // API stored for potential future use
        }

        public async Task<bool> ExecuteActionAsync(GameAction action)
        {
            try
            {
                logger.Info($"Executing action: {action.Name} (Type: {action.ActionType}, Phase: {action.ExecutionPhase})");

                switch (action.ActionType)
                {
                    case ActionType.StartApp:
                        return ExecuteStartApp(action);
                    
                    case ActionType.CloseApp:
                        return ExecuteCloseApp(action);
                    
                    case ActionType.PowerShellScript:
                        return await ExecutePowerShellScriptAsync(action);
                    
                    case ActionType.SystemCommand:
                        return ExecuteSystemCommand(action);
                    
                    case ActionType.Wait:
                        await ExecuteWaitAsync(action);
                        return true;
                    
                    default:
                        logger.Warn($"Unknown action type: {action.ActionType}");
                        return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to execute action: {action.Name}");
                return false;
            }
        }

        private bool ExecuteStartApp(GameAction action)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = action.Path,
                    Arguments = action.Arguments,
                    UseShellExecute = true
                };

                Process.Start(processStartInfo);
                logger.Info($"Started application: {action.Path}");
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to start app: {action.Path}");
                return false;
            }
        }

        private bool ExecuteCloseApp(GameAction action)
        {
            try
            {
                var processName = Path.GetFileNameWithoutExtension(action.Path);
                var processes = Process.GetProcessesByName(processName);

                if (processes.Length == 0)
                {
                    logger.Info($"Process not running: {processName}");
                    return true;
                }

                foreach (var process in processes)
                {
                    try
                    {
                        process.Kill();
                        logger.Info($"Closed process: {processName} (PID: {process.Id})");
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex, $"Failed to close process: {processName} (PID: {process.Id})");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to close app: {action.Path}");
                return false;
            }
        }

        private Task<bool> ExecutePowerShellScriptAsync(GameAction action)
        {
            return Task.Run(() => ExecutePowerShellViaProcess(action));
        }

        private bool ExecutePowerShellViaProcess(GameAction action)
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

                using (var process = Process.Start(processStartInfo))
                {
                    if (process != null)
                    {
                        process.WaitForExit();
                        logger.Info($"Executed PowerShell {(isScriptFile ? "script" : "command")}: {action.Path}");
                        return process.ExitCode == 0;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to execute PowerShell script via process: {action.Path}");
                return false;
            }
        }

        private bool ExecuteSystemCommand(GameAction action)
        {
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
                        logger.Info($"Executed system command: {action.Path}");
                        return process.ExitCode == 0;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to execute system command: {action.Path}");
                return false;
            }
        }

        private Task ExecuteWaitAsync(GameAction action)
        {
            var waitTime = action.WaitSeconds > 0 ? action.WaitSeconds : 1;
            logger.Info($"Waiting {waitTime} seconds...");
            return Task.Delay(waitTime * 1000);
        }
    }
}
