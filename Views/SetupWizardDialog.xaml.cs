using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AutomationProfileManager.Models;
using AutomationProfileManager.Services;

namespace AutomationProfileManager.Views
{
    public class DetectedApp
    {
        public string Name { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public string ExecutablePath { get; set; } = string.Empty;
        public string Category { get; set; } = "Other";
        public bool IsSelected { get; set; } = true;
    }

    public partial class SetupWizardDialog : Window
    {
        private readonly AutomationProfileManagerPlugin plugin;
        private ExtensionData? extensionData;
        private List<DetectedApp> allApps = new List<DetectedApp>();
        private List<DetectedApp> filteredApps = new List<DetectedApp>();

        public SetupWizardDialog(AutomationProfileManagerPlugin plugin)
        {
            InitializeComponent();
            this.plugin = plugin;
            this.extensionData = plugin.GetExtensionData();

            // Auto-scan on load
            ScanInstalledApps();
        }

        private void ScanInstalledApps()
        {
            try
            {
                var service = new InstalledAppsService();
                var installedApps = service.GetInstalledApps();

                allApps = installedApps.Select(a => new DetectedApp
                {
                    Name = a.Name,
                    ProcessName = a.ProcessName,
                    ExecutablePath = a.ExecutablePath,
                    Category = a.Category,
                    IsSelected = false // Default to not selected
                }).ToList();

                // Pre-select common apps
                var commonApps = new[] { "chrome", "firefox", "discord", "spotify", "steam", "obs", "nvidia" };
                foreach (var app in allApps)
                {
                    if (commonApps.Any(c => app.ProcessName.ToLowerInvariant().Contains(c)))
                    {
                        app.IsSelected = true;
                    }
                }

                ApplyFilter();
                UpdateAppCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    LocalizationService.GetString("LOC_APM_Error") + ": " + ex.Message,
                    LocalizationService.GetString("LOC_APM_Error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ApplyFilter()
        {
            var categoryFilter = (CategoryFilter.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? LocalizationService.GetString("LOC_APM_AllCategories");
            var searchText = SearchBox?.Text?.ToLowerInvariant() ?? "";

            filteredApps = allApps.Where(a =>
            {
                bool matchCategory = categoryFilter == LocalizationService.GetString("LOC_APM_AllCategories") || a.Category == categoryFilter;
                bool matchSearch = string.IsNullOrEmpty(searchText) ||
                                   a.Name.ToLowerInvariant().Contains(searchText) ||
                                   a.ProcessName.ToLowerInvariant().Contains(searchText);
                return matchCategory && matchSearch;
            }).ToList();

            DetectedAppsListBox.ItemsSource = null;
            DetectedAppsListBox.ItemsSource = filteredApps;
        }

        private void UpdateAppCount()
        {
            int selected = allApps.Count(a => a.IsSelected);
            int total = allApps.Count;
            AppCountText.Text = LocalizationService.GetString("LOC_APM_SelectedOfTotal", selected, total);
        }

        private void CategoryFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ScanInstalledApps_Click(object sender, RoutedEventArgs e)
        {
            ScanInstalledApps();
            MessageBox.Show(
                LocalizationService.GetString("LOC_APM_ScanCompleted", allApps.Count),
                LocalizationService.GetString("LOC_APM_ScanComplete"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void BrowseApp_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Applications (*.exe)|*.exe|Shortcuts (*.lnk)|*.lnk|All files (*.*)|*.*",
                Title = LocalizationService.GetString("LOC_APM_SelectApplicationTitle")
            };

            if (openDialog.ShowDialog() == true)
            {
                string filePath = openDialog.FileName;
                string appName = "";
                string processName = "";
                string exePath = filePath;

                try
                {
                    if (filePath.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
                    {
                        var targetPath = ResolveShortcut(filePath);
                        if (!string.IsNullOrEmpty(targetPath))
                        {
                            processName = System.IO.Path.GetFileNameWithoutExtension(targetPath);
                            appName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                            exePath = targetPath;
                        }
                    }
                    else
                    {
                        processName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                        appName = processName;
                    }

                    if (!string.IsNullOrEmpty(processName))
                    {
                        if (allApps.Any(a => a.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase)))
                        {
                            MessageBox.Show(
                                LocalizationService.GetString("LOC_APM_AppAlreadyExists", appName),
                                LocalizationService.GetString("LOC_APM_AppDuplicate"),
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }

                        allApps.Add(new DetectedApp
                        {
                            Name = appName,
                            ProcessName = processName,
                            ExecutablePath = exePath,
                            Category = LocalizationService.GetString("LOC_APM_CategoryManual"),
                            IsSelected = true
                        });

                        allApps = allApps.OrderBy(a => a.Name).ToList();
                        ApplyFilter();
                        UpdateAppCount();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        LocalizationService.GetString("LOC_APM_Error") + ": " + ex.Message,
                        LocalizationService.GetString("LOC_APM_Error"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private string ResolveShortcut(string shortcutPath)
        {
            try
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(shortcutPath);
                string content = System.Text.Encoding.Default.GetString(fileBytes);

                var match = System.Text.RegularExpressions.Regex.Match(content,
                    @"[A-Za-z]:\\[^\x00]+?\.exe",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    return match.Value;
                }
            }
            catch { }
            return "";
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var app in filteredApps)
            {
                app.IsSelected = true;
            }
            DetectedAppsListBox.ItemsSource = null;
            DetectedAppsListBox.ItemsSource = filteredApps;
            UpdateAppCount();
        }

        private void DeselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var app in filteredApps)
            {
                app.IsSelected = false;
            }
            DetectedAppsListBox.ItemsSource = null;
            DetectedAppsListBox.ItemsSource = filteredApps;
            UpdateAppCount();
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
                // Update summary when reaching last tab
                if (WizardTabs.SelectedIndex == WizardTabs.Items.Count - 2)
                {
                    UpdateSummary();
                }
                WizardTabs.SelectedIndex++;
                UpdateButtons();
            }
            else
            {
                CompleteWizard();
            }
        }

        private void UpdateSummary()
        {
            var selectedApps = allApps.Where(a => a.IsSelected).ToList();
            int closeActions = GenerateCloseActions.IsChecked == true ? selectedApps.Count : 0;
            int openActions = GenerateOpenActions.IsChecked == true ? selectedApps.Count : 0;

            SummaryText.Text = LocalizationService.GetString("LOC_APM_ActionsToCreate", closeActions + openActions, closeActions, openActions, selectedApps.Count);
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
            NextButton.Content = WizardTabs.SelectedIndex == WizardTabs.Items.Count - 1 ? LocalizationService.GetString("LOC_APM_WizardComplete") : LocalizationService.GetString("LOC_APM_WizardNext");
        }

        private void CompleteWizard()
        {
            try
            {
                if (extensionData == null)
                    extensionData = new ExtensionData();

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

                // Generate actions for selected apps
                var selectedApps = allApps.Where(a => a.IsSelected).ToList();
                bool addConditions = AddConditions.IsChecked == true;
                bool mirrorActions = GenerateMirrorActions.IsChecked == true;

                foreach (var app in selectedApps)
                {
                    // Generate CLOSE action
                    if (GenerateCloseActions.IsChecked == true)
                    {
                        var closeName = $"[Close] {app.Name}";
                        if (!extensionData.ActionLibrary.Any(a => a.Name == closeName))
                        {
                            var closeAction = new GameAction
                            {
                                Id = Guid.NewGuid(),
                                Name = closeName,
                                ActionType = ActionType.CloseApp,
                                Path = app.ProcessName,
                                ExecutionPhase = ExecutionPhase.BeforeStarting,
                                IsMirrorAction = mirrorActions,
                                Category = app.Category
                            };

                            if (addConditions)
                            {
                                closeAction.Condition = new ActionCondition
                                {
                                    Type = ConditionType.ProcessRunning,
                                    Value = app.ProcessName
                                };
                            }

                            extensionData.ActionLibrary.Add(closeAction);
                        }
                    }

                    // Generate OPEN action
                    if (GenerateOpenActions.IsChecked == true && !string.IsNullOrEmpty(app.ExecutablePath))
                    {
                        var openName = $"[Open] {app.Name}";
                        if (!extensionData.ActionLibrary.Any(a => a.Name == openName))
                        {
                            var openAction = new GameAction
                            {
                                Id = Guid.NewGuid(),
                                Name = openName,
                                ActionType = ActionType.StartApp,
                                Path = app.ExecutablePath,
                                ExecutionPhase = ExecutionPhase.BeforeStarting,
                                IsMirrorAction = false,
                                Category = app.Category
                            };

                            if (addConditions)
                            {
                                openAction.Condition = new ActionCondition
                                {
                                    Type = ConditionType.ProcessNotRunning,
                                    Value = app.ProcessName
                                };
                            }

                            extensionData.ActionLibrary.Add(openAction);
                        }
                    }
                }

                // Create profile based on selection
                if (ProfileGaming.IsChecked == true)
                {
                    CreateGamingProfile(selectedApps);
                }
                else if (ProfileStreaming.IsChecked == true)
                {
                    CreateStreamingProfile(selectedApps);
                }
                else if (ProfileEmulator.IsChecked == true)
                {
                    CreateEmulatorProfile(selectedApps);
                }

                plugin.UpdateExtensionData(extensionData);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(LocalizationService.GetString("LOC_APM_WizardConfigError"), ex.Message), LocalizationService.GetString("LOC_APM_Error"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateGamingProfile(List<DetectedApp> selectedApps)
        {
            var profile = new AutomationProfile
            {
                Id = Guid.NewGuid(),
                Name = "Immersive Gaming"
            };

            // Add close actions for browsers and communication apps
            var appsToClose = selectedApps.Where(a =>
                a.Category == "Browser" || a.Category == "Communication" || a.Category == "Cloud/Sync");

            foreach (var app in appsToClose)
            {
                var actionName = $"[Close] {app.Name}";
                var existingAction = extensionData?.ActionLibrary.FirstOrDefault(a => a.Name == actionName);
                if (existingAction != null)
                {
                    profile.Actions.Add(new GameAction
                    {
                        Id = existingAction.Id,
                        Name = existingAction.Name,
                        ActionType = existingAction.ActionType,
                        Path = existingAction.Path,
                        ExecutionPhase = ExecutionPhase.BeforeStarting,
                        IsMirrorAction = true,
                        Priority = profile.Actions.Count
                    });
                }
            }

            if (profile.Actions.Count > 0)
            {
                extensionData?.Profiles.Add(profile);
            }
        }

        private void CreateStreamingProfile(List<DetectedApp> selectedApps)
        {
            var profile = new AutomationProfile
            {
                Id = Guid.NewGuid(),
                Name = "Streaming/Recording"
            };

            // Find OBS
            var obsApp = selectedApps.FirstOrDefault(a =>
                a.ProcessName.ToLowerInvariant().Contains("obs"));

            if (obsApp != null)
            {
                var openActionName = $"[Open] {obsApp.Name}";
                var openAction = extensionData?.ActionLibrary.FirstOrDefault(a => a.Name == openActionName);
                if (openAction != null)
                {
                    profile.Actions.Add(new GameAction
                    {
                        Id = openAction.Id,
                        Name = openAction.Name,
                        ActionType = openAction.ActionType,
                        Path = openAction.Path,
                        ExecutionPhase = ExecutionPhase.BeforeStarting,
                        Priority = 0
                    });
                }
            }

            if (profile.Actions.Count > 0)
            {
                extensionData?.Profiles.Add(profile);
            }
        }

        private void CreateEmulatorProfile(List<DetectedApp> selectedApps)
        {
            var profile = new AutomationProfile
            {
                Id = Guid.NewGuid(),
                Name = "Emulators"
            };

            // Add resolution change from default actions
            var resAction = extensionData?.ActionLibrary.FirstOrDefault(a => a.Name.Contains("1920x1080"));
            if (resAction != null)
            {
                profile.Actions.Add(new GameAction
                {
                    Id = resAction.Id,
                    Name = resAction.Name,
                    ActionType = resAction.ActionType,
                    Path = resAction.Path,
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = true,
                    Priority = 0
                });
            }

            if (profile.Actions.Count > 0)
            {
                extensionData?.Profiles.Add(profile);
            }
        }
    }
}
