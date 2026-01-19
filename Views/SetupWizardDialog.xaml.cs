using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AutomationProfileManager.Models;
using AutomationProfileManager.Services;

namespace AutomationProfileManager.Views
{
    public class DetectedApp
    {
        public string Name { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public bool IsSelected { get; set; } = true;
    }

    public partial class SetupWizardDialog : Window
    {
        private readonly AutomationProfileManagerPlugin plugin;
        private ExtensionData? extensionData;
        private List<DetectedApp> detectedApps = new List<DetectedApp>();

        public SetupWizardDialog(AutomationProfileManagerPlugin plugin)
        {
            InitializeComponent();
            this.plugin = plugin;
            this.extensionData = plugin.GetExtensionData();
            
            DetectApps();
        }

        private void DetectApps()
        {
            detectedApps = new List<DetectedApp>();

            // Process names are case-insensitive on Windows, but GetProcessesByName needs exact match
            // These are the actual process names as they appear in Task Manager
            var commonApps = new[]
            {
                ("Chrome", new[] { "chrome", "Chrome" }),
                ("Firefox", new[] { "firefox", "Firefox" }),
                ("Edge", new[] { "msedge", "MicrosoftEdge" }),
                ("Discord", new[] { "Discord", "discord" }),
                ("Spotify", new[] { "Spotify", "spotify" }),
                ("Steam", new[] { "steam", "Steam", "steamwebhelper" }),
                ("Epic Games", new[] { "EpicGamesLauncher", "EpicWebHelper" }),
                ("GOG Galaxy", new[] { "GalaxyClient", "GOG Galaxy" }),
                ("Telegram", new[] { "Telegram", "telegram" }),
                ("Teams", new[] { "Teams", "ms-teams", "msteams" }),
                ("Slack", new[] { "slack", "Slack" }),
                ("OneDrive", new[] { "OneDrive", "onedrive" }),
                ("Dropbox", new[] { "Dropbox", "dropbox" }),
                ("Brave", new[] { "brave", "Brave" }),
                ("Opera", new[] { "opera", "Opera" }),
                ("Vivaldi", new[] { "vivaldi", "Vivaldi" }),
                ("WhatsApp", new[] { "WhatsApp", "whatsapp" }),
                ("Zoom", new[] { "Zoom", "zoom" }),
                ("Skype", new[] { "Skype", "skype" }),
                ("VLC", new[] { "vlc", "VLC" }),
                ("Nvidia Overlay", new[] { "nvcontainer", "NVIDIA Share" }),
                ("AMD Software", new[] { "RadeonSoftware", "AMDRSServ" }),
                ("Wallpaper Engine", new[] { "wallpaper32", "wallpaper64" }),
                ("Rainmeter", new[] { "Rainmeter", "rainmeter" }),
                ("MSI Afterburner", new[] { "MSIAfterburner" }),
                ("OBS Studio", new[] { "obs64", "obs32" }),
                ("Xbox Game Bar", new[] { "GameBar", "XboxGameBar" })
            };

            foreach (var (name, processNames) in commonApps)
            {
                try
                {
                    bool found = false;
                    string foundProcess = "";
                    
                    foreach (var processName in processNames)
                    {
                        var processes = System.Diagnostics.Process.GetProcessesByName(processName);
                        if (processes.Length > 0)
                        {
                            found = true;
                            foundProcess = processName;
                            break;
                        }
                    }
                    
                    if (found)
                    {
                        detectedApps.Add(new DetectedApp
                        {
                            Name = name,
                            ProcessName = foundProcess,
                            IsSelected = true
                        });
                    }
                }
                catch { }
            }

            DetectedAppsListBox.ItemsSource = detectedApps;
        }

        private void DetectApps_Click(object sender, RoutedEventArgs e)
        {
            DetectApps();
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            if (WizardTabs.SelectedIndex > 0)
            {
                WizardTabs.SelectedIndex--;
                UpdateButtons();
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (WizardTabs.SelectedIndex < WizardTabs.Items.Count - 1)
            {
                WizardTabs.SelectedIndex++;
                UpdateButtons();
            }
            else
            {
                CompleteWizard();
            }
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            if (extensionData?.Settings != null)
            {
                extensionData.Settings.WizardCompleted = true;
                plugin.UpdateExtensionData(extensionData);
            }
            DialogResult = false;
            Close();
        }

        private void UpdateButtons()
        {
            PrevButton.IsEnabled = WizardTabs.SelectedIndex > 0;
            NextButton.Content = WizardTabs.SelectedIndex == WizardTabs.Items.Count - 1 ? "Completa" : "Avanti";
        }

        private void CompleteWizard()
        {
            try
            {
                if (extensionData == null)
                    extensionData = new ExtensionData();

                // Apply settings
                if (extensionData.Settings == null)
                    extensionData.Settings = new ExtensionSettings();

                extensionData.Settings.ShowNotifications = EnableNotifications.IsChecked ?? true;
                extensionData.Settings.AutoBackupEnabled = EnableAutoBackup.IsChecked ?? true;
                extensionData.Settings.WizardCompleted = true;

                // Import default actions if selected
                if (ImportDefaultActions.IsChecked == true)
                {
                    var defaultActions = DefaultActionsProvider.GetDefaultActions();
                    foreach (var action in defaultActions)
                    {
                        if (!extensionData.ActionLibrary.Any(a => a.Name == action.Name))
                        {
                            extensionData.ActionLibrary.Add(action);
                        }
                    }
                }

                // Add detected apps as close actions
                foreach (var app in detectedApps.Where(a => a.IsSelected))
                {
                    var actionName = $"[Chiudi] {app.Name}";
                    if (!extensionData.ActionLibrary.Any(a => a.Name == actionName))
                    {
                        extensionData.ActionLibrary.Add(new GameAction
                        {
                            Id = Guid.NewGuid(),
                            Name = actionName,
                            ActionType = ActionType.CloseApp,
                            Path = app.ProcessName,
                            ExecutionPhase = ExecutionPhase.BeforeStarting,
                            IsMirrorAction = true,
                            Category = "App Rilevate"
                        });
                    }
                }

                // Create profile based on selection
                if (ProfileGaming.IsChecked == true)
                {
                    CreateGamingProfile();
                }
                else if (ProfileStreaming.IsChecked == true)
                {
                    CreateStreamingProfile();
                }
                else if (ProfileEmulator.IsChecked == true)
                {
                    CreateEmulatorProfile();
                }

                plugin.UpdateExtensionData(extensionData);
                
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante la configurazione: {ex.Message}", "Errore", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateGamingProfile()
        {
            var profile = new AutomationProfile
            {
                Id = Guid.NewGuid(),
                Name = "Gaming Immersivo"
            };

            foreach (var app in detectedApps.Where(a => a.IsSelected))
            {
                profile.Actions.Add(new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = $"[Chiudi] {app.Name}",
                    ActionType = ActionType.CloseApp,
                    Path = app.ProcessName,
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = profile.Actions.Count
                });
            }

            profile.Actions.Add(new GameAction
            {
                Id = Guid.NewGuid(),
                Name = "[Sistema] Prestazioni Elevate",
                ActionType = ActionType.SystemCommand,
                Path = "powercfg",
                Arguments = "/setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c",
                ExecutionPhase = ExecutionPhase.BeforeStarting,
                Priority = profile.Actions.Count
            });

            extensionData?.Profiles.Add(profile);
        }

        private void CreateStreamingProfile()
        {
            var profile = new AutomationProfile
            {
                Id = Guid.NewGuid(),
                Name = "Streaming/Recording"
            };

            profile.Actions.Add(new GameAction
            {
                Id = Guid.NewGuid(),
                Name = "[Apri] OBS Studio",
                ActionType = ActionType.StartApp,
                Path = @"C:\Program Files\obs-studio\bin\64bit\obs64.exe",
                ExecutionPhase = ExecutionPhase.BeforeStarting,
                Priority = 0
            });

            profile.Actions.Add(new GameAction
            {
                Id = Guid.NewGuid(),
                Name = "[Chiudi] OBS Studio",
                ActionType = ActionType.CloseApp,
                Path = "obs64",
                ExecutionPhase = ExecutionPhase.AfterClosing,
                Priority = 1
            });

            extensionData?.Profiles.Add(profile);
        }

        private void CreateEmulatorProfile()
        {
            var profile = new AutomationProfile
            {
                Id = Guid.NewGuid(),
                Name = "Emulatori"
            };

            profile.Actions.Add(new GameAction
            {
                Id = Guid.NewGuid(),
                Name = "[Risoluzione] 1920x1080@60Hz",
                ActionType = ActionType.ChangeResolution,
                Path = "1920x1080@60",
                ExecutionPhase = ExecutionPhase.BeforeStarting,
                IsMirrorAction = true,
                Priority = 0
            });

            profile.Actions.Add(new GameAction
            {
                Id = Guid.NewGuid(),
                Name = "[Risoluzione] Ripristina",
                ActionType = ActionType.ChangeResolution,
                Path = "RESTORE",
                ExecutionPhase = ExecutionPhase.AfterClosing,
                Priority = 1
            });

            extensionData?.Profiles.Add(profile);
        }
    }
}
