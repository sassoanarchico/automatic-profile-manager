using System;
using System.Collections.Generic;
using AutomationProfileManager.Models;

namespace AutomationProfileManager.Services
{
    public static class DefaultActionsProvider
    {
        public static List<GameAction> GetDefaultActions()
        {
            return new List<GameAction>
            {
                // === CLOSE COMMON APPS ===
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Close] Chrome",
                    ActionType = ActionType.CloseApp,
                    Path = "chrome",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Browser"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Close] Firefox",
                    ActionType = ActionType.CloseApp,
                    Path = "firefox",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Browser"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Close] Edge",
                    ActionType = ActionType.CloseApp,
                    Path = "msedge",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Browser"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Close] Brave",
                    ActionType = ActionType.CloseApp,
                    Path = "brave",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Browser"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Close] Discord",
                    ActionType = ActionType.CloseApp,
                    Path = "Discord",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Communication"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Close] Telegram",
                    ActionType = ActionType.CloseApp,
                    Path = "Telegram",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Communication"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Close] Teams",
                    ActionType = ActionType.CloseApp,
                    Path = "ms-teams",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Communication"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Close] Slack",
                    ActionType = ActionType.CloseApp,
                    Path = "slack",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Communication"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Close] Zoom",
                    ActionType = ActionType.CloseApp,
                    Path = "Zoom",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Communication"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Close] WhatsApp",
                    ActionType = ActionType.CloseApp,
                    Path = "WhatsApp",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Communication"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Close] Signal",
                    ActionType = ActionType.CloseApp,
                    Path = "Signal",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Communication"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Close] Spotify",
                    ActionType = ActionType.CloseApp,
                    Path = "Spotify",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Multimedia"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Close] OneDrive",
                    ActionType = ActionType.CloseApp,
                    Path = "OneDrive",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Cloud/Sync"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Close] Dropbox",
                    ActionType = ActionType.CloseApp,
                    Path = "Dropbox",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Cloud/Sync"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Close] Outlook",
                    ActionType = ActionType.CloseApp,
                    Path = "OUTLOOK",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Communication"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Close] Notion",
                    ActionType = ActionType.CloseApp,
                    Path = "Notion",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Utility"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Close] Steam",
                    ActionType = ActionType.CloseApp,
                    Path = "steam",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Gaming"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Close] Epic Games",
                    ActionType = ActionType.CloseApp,
                    Path = "EpicGamesLauncher",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Gaming"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Close] GOG Galaxy",
                    ActionType = ActionType.CloseApp,
                    Path = "GalaxyClient",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Gaming"
                },

                // === OPEN COMMON APPS ===
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Open] Discord",
                    ActionType = ActionType.StartApp,
                    Path = @"%LOCALAPPDATA%\Discord\Update.exe",
                    Arguments = "--processStart Discord.exe",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = false,
                    Priority = 0,
                    Category = "Communication"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Open] Spotify",
                    ActionType = ActionType.StartApp,
                    Path = @"%APPDATA%\Spotify\Spotify.exe",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = false,
                    Priority = 0,
                    Category = "Multimedia"
                },

                // === SYSTEM COMMANDS ===
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[System] High Performance ON",
                    ActionType = ActionType.SystemCommand,
                    Path = "powercfg",
                    Arguments = "/setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = false,
                    Priority = 0,
                    Category = "System"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[System] Balanced Mode",
                    ActionType = ActionType.SystemCommand,
                    Path = "powercfg",
                    Arguments = "/setactive 381b4222-f694-41f0-9685-ff5bb260df2e",
                    ExecutionPhase = ExecutionPhase.AfterClosing,
                    IsMirrorAction = false,
                    Priority = 0,
                    Category = "System"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[System] Power Saver",
                    ActionType = ActionType.SystemCommand,
                    Path = "powercfg",
                    Arguments = "/setactive a1841308-3541-4fab-bc81-f71556f20b4a",
                    ExecutionPhase = ExecutionPhase.AfterClosing,
                    IsMirrorAction = false,
                    Priority = 0,
                    Category = "System"
                },

                // === WAIT ACTIONS ===
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Wait] 3 seconds",
                    ActionType = ActionType.Wait,
                    WaitSeconds = 3,
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    Priority = 0,
                    Category = "Utility"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Wait] 5 seconds",
                    ActionType = ActionType.Wait,
                    WaitSeconds = 5,
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    Priority = 0,
                    Category = "Utility"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Wait] 10 seconds",
                    ActionType = ActionType.Wait,
                    WaitSeconds = 10,
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    Priority = 0,
                    Category = "Utility"
                },

                // === STREAMING / RECORDING ===
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Open] OBS Studio",
                    ActionType = ActionType.StartApp,
                    Path = @"C:\Program Files\obs-studio\bin\64bit\obs64.exe",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = false,
                    Priority = 0,
                    Category = "Streaming"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Close] OBS Studio",
                    ActionType = ActionType.CloseApp,
                    Path = "obs64",
                    ExecutionPhase = ExecutionPhase.AfterClosing,
                    IsMirrorAction = false,
                    Priority = 0,
                    Category = "Streaming"
                },

                // === NVIDIA / AMD ===
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Close] GeForce Experience",
                    ActionType = ActionType.CloseApp,
                    Path = "NVIDIA GeForce Experience",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Hardware/Overlay"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Close] AMD Software",
                    ActionType = ActionType.CloseApp,
                    Path = "AMDRSServ",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Hardware/Overlay"
                },

                // === POWERSHELL SCRIPTS ===
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Script] Clean RAM",
                    ActionType = ActionType.PowerShellScript,
                    Path = "[System.GC]::Collect(); [System.GC]::WaitForPendingFinalizers()",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = false,
                    Priority = 0,
                    Category = "Script"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Script] Flush DNS",
                    ActionType = ActionType.PowerShellScript,
                    Path = "Clear-DnsClientCache",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = false,
                    Priority = 0,
                    Category = "Script"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Script] Clean TEMP folder",
                    ActionType = ActionType.PowerShellScript,
                    Path = "Get-ChildItem $env:TEMP -Recurse -Force | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = false,
                    Priority = 0,
                    Category = "Script"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Script] Empty Recycle Bin",
                    ActionType = ActionType.PowerShellScript,
                    Path = "Clear-RecycleBin -Force -ErrorAction SilentlyContinue",
                    ExecutionPhase = ExecutionPhase.AfterClosing,
                    IsMirrorAction = false,
                    Priority = 0,
                    Category = "Script"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Script] Restart Explorer",
                    ActionType = ActionType.PowerShellScript,
                    Path = "Stop-Process -Name explorer -Force -ErrorAction SilentlyContinue; Start-Process explorer.exe",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = false,
                    Priority = 0,
                    Category = "Script"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Script] Game Mode ON",
                    ActionType = ActionType.PowerShellScript,
                    Path = "Set-ItemProperty -Path \"HKCU:\\Software\\Microsoft\\GameBar\" -Name \"AutoGameModeEnabled\" -Value 1",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Script"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Script] Game Mode OFF",
                    ActionType = ActionType.PowerShellScript,
                    Path = "Set-ItemProperty -Path \"HKCU:\\Software\\Microsoft\\GameBar\" -Name \"AutoGameModeEnabled\" -Value 0",
                    ExecutionPhase = ExecutionPhase.AfterClosing,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Script"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Script] Focus Assist ON",
                    ActionType = ActionType.PowerShellScript,
                    Path = "Set-ItemProperty -Path \"HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Notifications\\Settings\" -Name \"NOC_GLOBAL_SETTING_TOASTS_ENABLED\" -Value 0 -Type DWord -Force",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Script"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Script] Focus Assist OFF",
                    ActionType = ActionType.PowerShellScript,
                    Path = "Set-ItemProperty -Path \"HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Notifications\\Settings\" -Name \"NOC_GLOBAL_SETTING_TOASTS_ENABLED\" -Value 1 -Type DWord -Force",
                    ExecutionPhase = ExecutionPhase.AfterClosing,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Script"
                },

                // === AUDIO ===
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Audio] Volume 50%",
                    ActionType = ActionType.SetVolume,
                    Path = "50",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Audio"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Audio] Volume 75%",
                    ActionType = ActionType.SetVolume,
                    Path = "75",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Audio"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Audio] Volume 100%",
                    ActionType = ActionType.SetVolume,
                    Path = "100",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Audio"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Audio] Restore Volume",
                    ActionType = ActionType.SetVolume,
                    Path = "RESTORE",
                    ExecutionPhase = ExecutionPhase.AfterClosing,
                    IsMirrorAction = false,
                    Priority = 0,
                    Category = "Audio"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Audio] Mute Discord",
                    ActionType = ActionType.MuteApp,
                    Path = "Discord",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Audio"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Audio] Mute Spotify",
                    ActionType = ActionType.MuteApp,
                    Path = "Spotify",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Audio"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Audio] Mute Chrome",
                    ActionType = ActionType.MuteApp,
                    Path = "chrome",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Audio"
                },

                // === RESOLUTION ===
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Resolution] 1920x1080@60Hz",
                    ActionType = ActionType.ChangeResolution,
                    Path = "1920x1080@60",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Display"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Resolution] 2560x1440@60Hz",
                    ActionType = ActionType.ChangeResolution,
                    Path = "2560x1440@60",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Display"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Resolution] 3840x2160@60Hz",
                    ActionType = ActionType.ChangeResolution,
                    Path = "3840x2160@60",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Display"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Resolution] 1280x720@60Hz",
                    ActionType = ActionType.ChangeResolution,
                    Path = "1280x720@60",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Display"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Resolution] Restore",
                    ActionType = ActionType.ChangeResolution,
                    Path = "RESTORE",
                    ExecutionPhase = ExecutionPhase.AfterClosing,
                    IsMirrorAction = false,
                    Priority = 0,
                    Category = "Display"
                }
            };
        }

        public static List<AutomationProfile> GetDefaultProfiles()
        {
            return new List<AutomationProfile>
            {
                new AutomationProfile
                {
                    Id = Guid.NewGuid(),
                    Name = "Immersive Gaming",
                    Actions = new List<GameAction>
                    {
                        new GameAction
                        {
                            Id = Guid.NewGuid(),
                            Name = "[Close] Chrome",
                            ActionType = ActionType.CloseApp,
                            Path = "chrome",
                            ExecutionPhase = ExecutionPhase.BeforeStarting,
                            IsMirrorAction = true,
                            Priority = 0
                        },
                        new GameAction
                        {
                            Id = Guid.NewGuid(),
                            Name = "[Close] Discord",
                            ActionType = ActionType.CloseApp,
                            Path = "Discord",
                            ExecutionPhase = ExecutionPhase.BeforeStarting,
                            IsMirrorAction = true,
                            Priority = 1
                        },
                        new GameAction
                        {
                            Id = Guid.NewGuid(),
                            Name = "[System] High Performance ON",
                            ActionType = ActionType.SystemCommand,
                            Path = "powercfg",
                            Arguments = "/setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c",
                            ExecutionPhase = ExecutionPhase.BeforeStarting,
                            Priority = 2
                        },
                        new GameAction
                        {
                            Id = Guid.NewGuid(),
                            Name = "[System] Balanced Mode",
                            ActionType = ActionType.SystemCommand,
                            Path = "powercfg",
                            Arguments = "/setactive 381b4222-f694-41f0-9685-ff5bb260df2e",
                            ExecutionPhase = ExecutionPhase.AfterClosing,
                            Priority = 0
                        }
                    }
                }
            };
        }
    }
}
