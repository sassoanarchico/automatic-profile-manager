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
                    Priority = 0
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Chiudi] Firefox",
                    ActionType = ActionType.CloseApp,
                    Path = "firefox",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Chiudi] Edge",
                    ActionType = ActionType.CloseApp,
                    Path = "msedge",
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
                    Priority = 0
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Chiudi] Telegram",
                    ActionType = ActionType.CloseApp,
                    Path = "Telegram",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Chiudi] Teams",
                    ActionType = ActionType.CloseApp,
                    Path = "ms-teams",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Chiudi] Slack",
                    ActionType = ActionType.CloseApp,
                    Path = "slack",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Chiudi] Spotify",
                    ActionType = ActionType.CloseApp,
                    Path = "Spotify",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Chiudi] OneDrive",
                    ActionType = ActionType.CloseApp,
                    Path = "OneDrive",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Chiudi] Dropbox",
                    ActionType = ActionType.CloseApp,
                    Path = "Dropbox",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Chiudi] Outlook",
                    ActionType = ActionType.CloseApp,
                    Path = "OUTLOOK",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Chiudi] Steam",
                    ActionType = ActionType.CloseApp,
                    Path = "steam",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Chiudi] Epic Games",
                    ActionType = ActionType.CloseApp,
                    Path = "EpicGamesLauncher",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Chiudi] GOG Galaxy",
                    ActionType = ActionType.CloseApp,
                    Path = "GalaxyClient",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0
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
                    Priority = 0
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Apri] Spotify",
                    ActionType = ActionType.StartApp,
                    Path = @"%APPDATA%\Spotify\Spotify.exe",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = false,
                    Priority = 0
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
                    Priority = 0
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
                    Priority = 0
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
                    Priority = 0
                },

                // === AZIONI DI ATTESA ===
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Attesa] 3 secondi",
                    ActionType = ActionType.Wait,
                    WaitSeconds = 3,
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    Priority = 0
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Attesa] 5 secondi",
                    ActionType = ActionType.Wait,
                    WaitSeconds = 5,
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    Priority = 0
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Attesa] 10 secondi",
                    ActionType = ActionType.Wait,
                    WaitSeconds = 10,
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    Priority = 0
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
                    Priority = 0
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Chiudi] OBS Studio",
                    ActionType = ActionType.CloseApp,
                    Path = "obs64",
                    ExecutionPhase = ExecutionPhase.AfterClosing,
                    IsMirrorAction = false,
                    Priority = 0
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
                    Priority = 0
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Chiudi] AMD Software",
                    ActionType = ActionType.CloseApp,
                    Path = "AMDRSServ",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0
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
                    Priority = 0
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Script] Svuota cache DNS",
                    ActionType = ActionType.PowerShellScript,
                    Path = "Clear-DnsClientCache",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = false,
                    Priority = 0
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Script] Pulisci cartella TEMP",
                    ActionType = ActionType.PowerShellScript,
                    Path = "Get-ChildItem $env:TEMP -Recurse -Force | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = false,
                    Priority = 0
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Script] Svuota Cestino",
                    ActionType = ActionType.PowerShellScript,
                    Path = "Clear-RecycleBin -Force -ErrorAction SilentlyContinue",
                    ExecutionPhase = ExecutionPhase.AfterClosing,
                    IsMirrorAction = false,
                    Priority = 0
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Script] Riavvia Esplora Risorse",
                    ActionType = ActionType.PowerShellScript,
                    Path = "Stop-Process -Name explorer -Force -ErrorAction SilentlyContinue; Start-Process explorer.exe",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = false,
                    Priority = 0
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Script] Modalita Gioco ON",
                    ActionType = ActionType.PowerShellScript,
                    Path = "Set-ItemProperty -Path \"HKCU:\\\\Software\\\\Microsoft\\\\GameBar\" -Name \"AutoGameModeEnabled\" -Value 1",
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0
                },
                new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = "[Script] Modalita Gioco OFF",
                    ActionType = ActionType.PowerShellScript,
                    Path = "Set-ItemProperty -Path \"HKCU:\\\\Software\\\\Microsoft\\\\GameBar\" -Name \"AutoGameModeEnabled\" -Value 0",
                    ExecutionPhase = ExecutionPhase.AfterClosing,
                    IsMirrorAction = true,
                    Priority = 0
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
