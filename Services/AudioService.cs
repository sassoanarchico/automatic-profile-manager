using System;
using System.Diagnostics;
using Playnite.SDK;

namespace AutomationProfileManager.Services
{
    public class AudioService
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private int? originalVolume;

        public void SaveCurrentVolume()
        {
            try
            {
                var result = ExecutePowerShell("(Get-AudioDevice -PlaybackVolume).Volume");
                if (int.TryParse(result?.Trim(), out int volume))
                {
                    originalVolume = volume;
                    logger.Info(LocalizationService.GetString("LOC_APM_Log_SavedOriginalVolume", volume));
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, LocalizationService.GetString("LOC_APM_Log_FailedToSaveVolume"));
            }
        }

        public bool SetMasterVolume(int volumePercent)
        {
            try
            {
                volumePercent = Math.Max(0, Math.Min(100, volumePercent));
                
                string script = $@"
                    $obj = New-Object -ComObject WScript.Shell
                    1..50 | ForEach-Object {{ $obj.SendKeys([char]174) }}
                    1..{volumePercent / 2} | ForEach-Object {{ $obj.SendKeys([char]175) }}
                ";
                
                ExecutePowerShell(script);
                logger.Info(LocalizationService.GetString("LOC_APM_Log_SetMasterVolume", volumePercent));
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, LocalizationService.GetString("LOC_APM_Log_FailedToSetMasterVolume"));
                return false;
            }
        }

        public bool RestoreOriginalVolume()
        {
            if (originalVolume.HasValue)
            {
                return SetMasterVolume(originalVolume.Value);
            }
            return false;
        }

        public bool MuteProcess(string processName)
        {
            try
            {
                string script = $@"
                    $processes = Get-Process -Name '{processName}' -ErrorAction SilentlyContinue
                    if ($processes) {{
                        foreach ($proc in $processes) {{
                            $audio = Get-AudioDevice -ID $proc.Id -ErrorAction SilentlyContinue
                            if ($audio) {{
                                Set-AudioDevice -ID $proc.Id -Mute $true -ErrorAction SilentlyContinue
                            }}
                        }}
                    }}
                ";
                
                ExecutePowerShell(script);
                logger.Info(LocalizationService.GetString("LOC_APM_Log_MutedProcess", processName));
                return true;
            }
            catch (Exception ex)
            {
                logger.Warn(ex, LocalizationService.GetString("LOC_APM_Log_FailedToMuteProcess", processName));
                return MuteProcessAlternative(processName);
            }
        }

        public bool UnmuteProcess(string processName)
        {
            try
            {
                string script = $@"
                    $processes = Get-Process -Name '{processName}' -ErrorAction SilentlyContinue
                    if ($processes) {{
                        foreach ($proc in $processes) {{
                            $audio = Get-AudioDevice -ID $proc.Id -ErrorAction SilentlyContinue
                            if ($audio) {{
                                Set-AudioDevice -ID $proc.Id -Mute $false -ErrorAction SilentlyContinue
                            }}
                        }}
                    }}
                ";
                
                ExecutePowerShell(script);
                logger.Info(LocalizationService.GetString("LOC_APM_Log_UnmutedProcess", processName));
                return true;
            }
            catch (Exception ex)
            {
                logger.Warn(ex, LocalizationService.GetString("LOC_APM_Log_FailedToUnmuteProcess", processName));
                return false;
            }
        }

        private bool MuteProcessAlternative(string processName)
        {
            try
            {
                string script = $@"
                    Add-Type -TypeDefinition @'
                    using System;
                    using System.Runtime.InteropServices;
                    public class AudioManager {{
                        [DllImport(""user32.dll"")]
                        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
                        public const int APPCOMMAND_VOLUME_MUTE = 0x80000;
                        public const int WM_APPCOMMAND = 0x319;
                    }}
'@
                    $proc = Get-Process -Name '{processName}' -ErrorAction SilentlyContinue | Select-Object -First 1
                    if ($proc -and $proc.MainWindowHandle) {{
                        [AudioManager]::SendMessage($proc.MainWindowHandle, [AudioManager]::WM_APPCOMMAND, $proc.MainWindowHandle, [IntPtr][AudioManager]::APPCOMMAND_VOLUME_MUTE)
                    }}
                ";
                
                ExecutePowerShell(script);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, LocalizationService.GetString("LOC_APM_Log_AlternativeMuteFailed", processName));
                return false;
            }
        }

        private string? ExecutePowerShell(string script)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -Command \"{script.Replace("\"", "`\"")}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(psi))
                {
                    if (process != null)
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();
                        return output;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, LocalizationService.GetString("LOC_APM_Log_FailedToExecutePowerShellAudio"));
            }
            return null;
        }
    }
}
