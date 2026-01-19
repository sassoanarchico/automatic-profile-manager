using System;
using System.Collections.Generic;
using AutomationProfileManager.Models;

namespace AutomationProfileManager.Services
{
    public class ProfileTemplate
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<GameAction> Actions { get; set; } = new List<GameAction>();
    }

    public class ProfileTemplateService
    {
        public static List<ProfileTemplate> GetEmulatorTemplates()
        {
            var templates = new List<ProfileTemplate>();

            // RetroArch Template
            templates.Add(new ProfileTemplate
            {
                Name = "RetroArch",
                Description = "Template per RetroArch con risoluzione, fullscreen e driver video",
                Actions = new List<GameAction>
                {
                    new GameAction
                    {
                        Name = "Cambia Risoluzione 1920x1080@60Hz",
                        ActionType = ActionType.ChangeResolution,
                        Path = "1920x1080@60",
                        ExecutionPhase = ExecutionPhase.BeforeStarting,
                        Priority = 0,
                        Category = "Risoluzione"
                    },
                    new GameAction
                    {
                        Name = "Avvia RetroArch Fullscreen",
                        ActionType = ActionType.StartApp,
                        Path = "%USERPROFILE%\\AppData\\Roaming\\RetroArch\\retroarch.exe",
                        Arguments = "-f",
                        ExecutionPhase = ExecutionPhase.BeforeStarting,
                        Priority = 1,
                        Category = "Emulatore"
                    },
                    new GameAction
                    {
                        Name = "Ripristina Risoluzione Originale",
                        ActionType = ActionType.ChangeResolution,
                        Path = "RESTORE",
                        ExecutionPhase = ExecutionPhase.AfterClosing,
                        Priority = 0,
                        Category = "Risoluzione"
                    }
                }
            });

            // Dolphin Template
            templates.Add(new ProfileTemplate
            {
                Name = "Dolphin",
                Description = "Template per Dolphin Emulator con risoluzione e fullscreen",
                Actions = new List<GameAction>
                {
                    new GameAction
                    {
                        Name = "Cambia Risoluzione 1920x1080@60Hz",
                        ActionType = ActionType.ChangeResolution,
                        Path = "1920x1080@60",
                        ExecutionPhase = ExecutionPhase.BeforeStarting,
                        Priority = 0,
                        Category = "Risoluzione"
                    },
                    new GameAction
                    {
                        Name = "Avvia Dolphin Fullscreen",
                        ActionType = ActionType.StartApp,
                        Path = "%PROGRAMFILES%\\Dolphin\\Dolphin.exe",
                        Arguments = "-e \"{GamePath}\" -b",
                        ExecutionPhase = ExecutionPhase.BeforeStarting,
                        Priority = 1,
                        Category = "Emulatore"
                    },
                    new GameAction
                    {
                        Name = "Ripristina Risoluzione Originale",
                        ActionType = ActionType.ChangeResolution,
                        Path = "RESTORE",
                        ExecutionPhase = ExecutionPhase.AfterClosing,
                        Priority = 0,
                        Category = "Risoluzione"
                    }
                }
            });

            // PCSX2 Template
            templates.Add(new ProfileTemplate
            {
                Name = "PCSX2",
                Description = "Template per PCSX2 con risoluzione e fullscreen",
                Actions = new List<GameAction>
                {
                    new GameAction
                    {
                        Name = "Cambia Risoluzione 1920x1080@60Hz",
                        ActionType = ActionType.ChangeResolution,
                        Path = "1920x1080@60",
                        ExecutionPhase = ExecutionPhase.BeforeStarting,
                        Priority = 0,
                        Category = "Risoluzione"
                    },
                    new GameAction
                    {
                        Name = "Avvia PCSX2 Fullscreen",
                        ActionType = ActionType.StartApp,
                        Path = "%PROGRAMFILES%\\PCSX2\\pcsx2.exe",
                        Arguments = "--fullscreen --nogui",
                        ExecutionPhase = ExecutionPhase.BeforeStarting,
                        Priority = 1,
                        Category = "Emulatore"
                    },
                    new GameAction
                    {
                        Name = "Ripristina Risoluzione Originale",
                        ActionType = ActionType.ChangeResolution,
                        Path = "RESTORE",
                        ExecutionPhase = ExecutionPhase.AfterClosing,
                        Priority = 0,
                        Category = "Risoluzione"
                    }
                }
            });

            // PPSSPP Template
            templates.Add(new ProfileTemplate
            {
                Name = "PPSSPP",
                Description = "Template per PPSSPP con risoluzione e fullscreen",
                Actions = new List<GameAction>
                {
                    new GameAction
                    {
                        Name = "Cambia Risoluzione 1920x1080@60Hz",
                        ActionType = ActionType.ChangeResolution,
                        Path = "1920x1080@60",
                        ExecutionPhase = ExecutionPhase.BeforeStarting,
                        Priority = 0,
                        Category = "Risoluzione"
                    },
                    new GameAction
                    {
                        Name = "Avvia PPSSPP Fullscreen",
                        ActionType = ActionType.StartApp,
                        Path = "%PROGRAMFILES%\\PPSSPP\\PPSSPPWindows.exe",
                        Arguments = "--fullscreen",
                        ExecutionPhase = ExecutionPhase.BeforeStarting,
                        Priority = 1,
                        Category = "Emulatore"
                    },
                    new GameAction
                    {
                        Name = "Ripristina Risoluzione Originale",
                        ActionType = ActionType.ChangeResolution,
                        Path = "RESTORE",
                        ExecutionPhase = ExecutionPhase.AfterClosing,
                        Priority = 0,
                        Category = "Risoluzione"
                    }
                }
            });

            // Cemu Template
            templates.Add(new ProfileTemplate
            {
                Name = "Cemu",
                Description = "Template per Cemu Wii U Emulator",
                Actions = new List<GameAction>
                {
                    new GameAction
                    {
                        Name = "Cambia Risoluzione 1920x1080@60Hz",
                        ActionType = ActionType.ChangeResolution,
                        Path = "1920x1080@60",
                        ExecutionPhase = ExecutionPhase.BeforeStarting,
                        Priority = 0,
                        Category = "Risoluzione"
                    },
                    new GameAction
                    {
                        Name = "Avvia Cemu",
                        ActionType = ActionType.StartApp,
                        Path = "%PROGRAMFILES%\\Cemu\\Cemu.exe",
                        Arguments = "-f -g \"{GamePath}\"",
                        ExecutionPhase = ExecutionPhase.BeforeStarting,
                        Priority = 1,
                        Category = "Emulatore"
                    },
                    new GameAction
                    {
                        Name = "Ripristina Risoluzione Originale",
                        ActionType = ActionType.ChangeResolution,
                        Path = "RESTORE",
                        ExecutionPhase = ExecutionPhase.AfterClosing,
                        Priority = 0,
                        Category = "Risoluzione"
                    }
                }
            });

            return templates;
        }

        public static AutomationProfile CreateProfileFromTemplate(ProfileTemplate template, string profileName)
        {
            var profile = new AutomationProfile
            {
                Name = profileName,
                Id = Guid.NewGuid()
            };

            foreach (var action in template.Actions)
            {
                profile.Actions.Add(new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = action.Name,
                    ActionType = action.ActionType,
                    Path = action.Path,
                    Arguments = action.Arguments,
                    ExecutionPhase = action.ExecutionPhase,
                    IsMirrorAction = action.IsMirrorAction,
                    Priority = action.Priority,
                    WaitSeconds = action.WaitSeconds,
                    Category = action.Category
                });
            }

            return profile;
        }
    }
}
