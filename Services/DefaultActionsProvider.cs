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
                // === APP COMUNI DA CHIUDERE ===
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Chiudi] Chrome",
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
                    Name = "[Chiudi] Firefox",
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
                    Name = "[Chiudi] Edge",
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
                    Name = "[Chiudi] Discord",
                    ActionType = ActionType.CloseApp,
                    Path = "Discord",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Comunicazione"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Chiudi] Telegram",
                    ActionType = ActionType.CloseApp,
                    Path = "Telegram",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Comunicazione"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Chiudi] Teams",
                    ActionType = ActionType.CloseApp,
                    Path = "ms-teams",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Comunicazione"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Chiudi] Slack",
                    ActionType = ActionType.CloseApp,
                    Path = "slack",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Comunicazione"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Chiudi] Spotify",
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
                    Name = "[Chiudi] OneDrive",
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
                    Name = "[Chiudi] Dropbox",
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
                    Name = "[Chiudi] Outlook",
                    ActionType = ActionType.CloseApp,
                    Path = "OUTLOOK",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Comunicazione"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Chiudi] Steam",
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
                    Name = "[Chiudi] Epic Games",
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
                    Name = "[Chiudi] GOG Galaxy",
                    ActionType = ActionType.CloseApp,
                    Path = "GalaxyClient",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Gaming"
                },

                // === APP COMUNI DA APRIRE ===
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Apri] Discord",
                    ActionType = ActionType.StartApp,
                    Path = @"%LOCALAPPDATA%\Discord\Update.exe",
                    Arguments = "--processStart Discord.exe",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = false,
                    Priority = 0,
                    Category = "Comunicazione"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Apri] Spotify",
                    ActionType = ActionType.StartApp,
                    Path = @"%APPDATA%\Spotify\Spotify.exe",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = false,
                    Priority = 0,
                    Category = "Multimedia"
                },

                // === COMANDI DI SISTEMA ===
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Sistema] Prestazioni Elevate ON",
                    ActionType = ActionType.SystemCommand,
                    Path = "powercfg",
                    Arguments = "/setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = false,
                    Priority = 0,
                    Category = "Sistema"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Sistema] Modalita Bilanciata",
                    ActionType = ActionType.SystemCommand,
                    Path = "powercfg",
                    Arguments = "/setactive 381b4222-f694-41f0-9685-ff5bb260df2e",
                    ExecutionPhase = ExecutionPhase.AfterClosing,
                    IsMirrorAction = false,
                    Priority = 0,
                    Category = "Sistema"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Sistema] Risparmio Energetico",
                    ActionType = ActionType.SystemCommand,
                    Path = "powercfg",
                    Arguments = "/setactive a1841308-3541-4fab-bc81-f71556f20b4a",
                    ExecutionPhase = ExecutionPhase.AfterClosing,
                    IsMirrorAction = false,
                    Priority = 0,
                    Category = "Sistema"
                },

                // === AZIONI DI ATTESA ===
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Attesa] 3 secondi",
                    ActionType = ActionType.Wait,
                    WaitSeconds = 3,
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    Priority = 0,
                    Category = "Utilita"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Attesa] 5 secondi",
                    ActionType = ActionType.Wait,
                    WaitSeconds = 5,
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    Priority = 0,
                    Category = "Utilita"
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Attesa] 10 secondi",
                    ActionType = ActionType.Wait,
                    WaitSeconds = 10,
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    Priority = 0,
                    Category = "Utilita"
                },

                // === STREAMING / RECORDING ===
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Apri] OBS Studio",
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
                    Name = "[Chiudi] OBS Studio",
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
                    Name = "[Chiudi] GeForce Experience",
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
                    Name = "[Chiudi] AMD Software",
                    ActionType = ActionType.CloseApp,
                    Path = "AMDRSServ",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0,
                    Category = "Hardware/Overlay"
                },

                // === SCRIPT POWERSHELL UTILI ===
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Script] Pulisci RAM",
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
                    Name = "[Script] Svuota cache DNS",
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
                    Name = "[Script] Pulisci cartella TEMP",
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
                    Name = "[Script] Svuota Cestino",
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
                    Name = "[Script] Riavvia Esplora Risorse",
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
                    Name = "[Script] Modalita Gioco ON",
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
                    Name = "[Script] Modalita Gioco OFF",
                    ActionType = ActionType.PowerShellScript,
                    Path = "Set-ItemProperty -Path \"HKCU:\\Software\\Microsoft\\GameBar\" -Name \"AutoGameModeEnabled\" -Value 0",
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
                    Name = "[Audio] Ripristina Volume",
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

                // === RISOLUZIONE ===
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Risoluzione] 1920x1080@60Hz",
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
                    Name = "[Risoluzione] 2560x1440@60Hz",
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
                    Name = "[Risoluzione] Ripristina",
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
                    Name = "Profilo Gaming Immersivo",
                    Actions = new List<GameAction>
                    {
                        new GameAction
                        {
                            Id = Guid.NewGuid(),
                            Name = "[Chiudi] Chrome",
                            ActionType = ActionType.CloseApp,
                            Path = "chrome",
                            ExecutionPhase = ExecutionPhase.BeforeStarting,
                            IsMirrorAction = true,
                            Priority = 0
                        },
                        new GameAction
                        {
                            Id = Guid.NewGuid(),
                            Name = "[Chiudi] Discord",
                            ActionType = ActionType.CloseApp,
                            Path = "Discord",
                            ExecutionPhase = ExecutionPhase.BeforeStarting,
                            IsMirrorAction = true,
                            Priority = 1
                        },
                        new GameAction
                        {
                            Id = Guid.NewGuid(),
                            Name = "[Sistema] Prestazioni Elevate ON",
                            ActionType = ActionType.SystemCommand,
                            Path = "powercfg",
                            Arguments = "/setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c",
                            ExecutionPhase = ExecutionPhase.BeforeStarting,
                            Priority = 2
                        },
                        new GameAction
                        {
                            Id = Guid.NewGuid(),
                            Name = "[Sistema] Modalita Bilanciata",
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
