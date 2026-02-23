using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AutomationProfileManager.Models;
using AutomationProfileManager.Services;
using AutomationProfileManager.Views;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;

namespace AutomationProfileManager
{
    public class AutomationProfileManagerPlugin : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        
        private DataService? dataService;
        private ActionExecutor? actionExecutor;
        private MirrorActionTracker? mirrorTracker;
        private NotificationService? notificationService;
        private BackupService? backupService;
        private StatisticsService? statisticsService;
        private ExtensionData? extensionData;

        public override Guid Id { get; } = Guid.Parse("d2e3f4a5-b6c7-8901-defa-2345678901bc");

        private void EnsureInitialized()
        {
            dataService ??= new DataService(PlayniteApi);
            notificationService ??= new NotificationService(PlayniteApi);
            backupService ??= new BackupService(PlayniteApi, notificationService);
            actionExecutor ??= new ActionExecutor(PlayniteApi);
            mirrorTracker ??= new MirrorActionTracker();
            extensionData ??= dataService.LoadData();

            if (extensionData != null)
            {
                statisticsService ??= new StatisticsService(
                    extensionData.Statistics ?? new List<ActionStatistics>(),
                    extensionData.ProfileStats ?? new List<ProfileStatistics>()
                );
                actionExecutor?.SetStatisticsService(statisticsService);
                notificationService?.SetShowNotifications(extensionData.Settings?.ShowNotifications ?? true);
            }
        }

        public AutomationProfileManagerPlugin(IPlayniteAPI api) : base(api)
        {
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
            EnsureInitialized();
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            EnsureInitialized();
            
            // Show wizard if not completed
            if (extensionData?.Settings != null && !extensionData.Settings.WizardCompleted)
            {
                ShowSetupWizard();
            }

            // Check for backup
            if (extensionData?.Settings != null && backupService != null)
            {
                if (backupService.ShouldBackup(extensionData.Settings))
                {
                    var backupPath = backupService.CreateBackup(extensionData);
                    if (backupPath != null)
                    {
                        extensionData.Settings.LastBackupDate = DateTime.Now;
                        SaveData();
                    }
                }
            }
        }

        private void ShowSetupWizard()
        {
            try
            {
                var wizard = new SetupWizardDialog(this);
                wizard.ShowDialog();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to show setup wizard");
            }
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            SaveData();
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            EnsureInitialized();
            ExecuteProfileActions(args.Game, ExecutionPhase.AfterStarting);
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            EnsureInitialized();
            mirrorTracker?.ClearTracking();
            // Save current resolution and volume before any changes
            if (actionExecutor != null)
            {
                actionExecutor.SaveCurrentResolution();
                actionExecutor.SaveCurrentVolume();
            }
            ExecuteProfileActions(args.Game, ExecutionPhase.BeforeStarting);
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            EnsureInitialized();
            ExecuteProfileActions(args.Game, ExecutionPhase.AfterClosing);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            try
            {
                EnsureInitialized();
                return new AutomationProfileManagerSettings(this);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "GetSettings failed");
                    PlayniteApi.Notifications.Add(new NotificationMessage(
                    "AutomationProfileManager_SettingsError",
                    string.Format(LocalizationService.GetString("LOC_APM_SettingsLoadFailed"), ex.Message),
                    NotificationType.Error
                ));
                return new AutomationProfileManagerSettings();
            }
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            try
            {
                EnsureInitialized();
                return new AutomationProfileManagerSettingsView(this);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "GetSettingsView failed");
                PlayniteApi.Notifications.Add(new NotificationMessage(
                    "AutomationProfileManager_SettingsViewError",
                    string.Format(LocalizationService.GetString("LOC_APM_SettingsViewLoadFailed"), ex.Message),
                    NotificationType.Error
                ));
                throw;
            }
        }

        private void InitializeServices()
        {
            dataService ??= new DataService(PlayniteApi);
            actionExecutor ??= new ActionExecutor(PlayniteApi);
            mirrorTracker ??= new MirrorActionTracker();
        }

        private void LoadData()
        {
            extensionData = dataService?.LoadData() ?? new ExtensionData();
        }

        private void SaveData()
        {
            dataService?.SaveData(extensionData ?? new ExtensionData());
        }

        private async void ExecuteProfileActions(Game game, ExecutionPhase phase)
        {
            try
            {
                if (extensionData == null || extensionData.Mappings == null || extensionData.Mappings.GameToProfile == null)
                    return;

                if (!extensionData.Mappings.GameToProfile.TryGetValue(game.Id, out var profileId))
                    return;

                var profile = extensionData.Profiles?.FirstOrDefault(p => p.Id == profileId);
                if (profile == null)
                {
                    if (extensionData.Settings?.ShowNotifications ?? true)
                    {
                        PlayniteApi.Notifications.Add(new NotificationMessage(
                            "AutomationProfileManager_ProfileNotFound",
                            string.Format(LocalizationService.GetString("LOC_APM_ProfileNotFound"), game.Name),
                            NotificationType.Error
                        ));
                    }
                    return;
                }
                
                // Check if dry-run is enabled
                bool dryRun = extensionData.Settings?.EnableDryRun ?? false;

                var actions = profile.Actions
                    .Where(a => a.ExecutionPhase == phase)
                    .OrderBy(a => a.Priority)
                    .ToList();

                if (actions.Count == 0) return;

                // Show notification
                notificationService?.ShowProfileStarted(profile.Name, game.Name, actions.Count);
                var stopwatch = Stopwatch.StartNew();
                int successCount = 0;
                int failCount = 0;

                // Use new batch execution with parallel support
                if (actionExecutor != null)
                {
                    var results = await actionExecutor.ExecuteActionsAsync(actions, dryRun);
                    successCount = results.Count(r => r.Success);
                    failCount = results.Count(r => !r.Success);
                }

                // Handle mirror actions for AfterClosing phase
                if (phase == ExecutionPhase.AfterClosing)
                {
                    foreach (var action in actions.Where(a => a.IsMirrorAction))
                    {
                        if (mirrorTracker != null && mirrorTracker.ShouldRestoreAction(action))
                        {
                            var reverseAction = new Models.GameAction
                            {
                                Name = $"Restore: {action.Name}",
                                ActionType = ActionType.StartApp,
                                Path = action.Path,
                                Arguments = action.Arguments,
                                ExecutionPhase = ExecutionPhase.AfterClosing
                            };
                            if (actionExecutor != null)
                            {
                                await actionExecutor.ExecuteActionAsync(reverseAction, dryRun);
                            }
                        }
                    }
                }
                else if (phase == ExecutionPhase.BeforeStarting)
                {
                    foreach (var action in actions.Where(a => a.IsMirrorAction))
                    {
                        mirrorTracker?.TrackActionBeforeExecution(action);
                    }
                }

                stopwatch.Stop();

                // Record statistics
                statisticsService?.RecordProfileExecution(profile, stopwatch.Elapsed.TotalSeconds);

                // Show completion notification
                notificationService?.ShowProfileCompleted(profile.Name, successCount, failCount, stopwatch.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error executing profile actions for game '{game?.Name ?? "(unknown)"}' in phase {phase}");
                if (extensionData?.Settings?.ShowNotifications ?? true)
                {
                    PlayniteApi.Notifications.Add(new NotificationMessage(
                        "AutomationProfileManager_ExecutionError",
                        string.Format(LocalizationService.GetString("LOC_APM_ExecutionError"), ex.Message),
                        NotificationType.Error
                    ));
                }
            }
        }

        private void ShowProfileAssignmentMenu(List<Game> games)
        {
            if (games == null || games.Count == 0)
                return;

            var game = games[0];
            if (extensionData?.Profiles == null)
            {
                extensionData = GetExtensionData();
            }
            var dialog = new ProfileAssignmentDialog(extensionData.Profiles, game.Name);
            
            if (dialog.ShowDialog() == true)
            {
                var selectedProfileId = dialog.GetSelectedProfileId();
                if (selectedProfileId.HasValue)
                {
                    AssignProfileToGame(game, selectedProfileId.Value);
                }
                else
                {
                    RemoveProfileAssignment(game);
                }
            }
        }

        private void AssignProfileToGame(Game game, Guid profileId)
        {
            if (extensionData?.Mappings?.GameToProfile == null) return;
            
            extensionData.Mappings.GameToProfile[game.Id] = profileId;
            SaveData();
            if (extensionData.Settings?.ShowNotifications ?? true)
            {
                PlayniteApi.Notifications.Add(new NotificationMessage(
                    "AutomationProfileManager_ProfileAssigned",
                    string.Format(LocalizationService.GetString("LOC_APM_ProfileAssigned"), game.Name),
                    NotificationType.Info
                ));
            }
        }

        private void RemoveProfileAssignment(Game game)
        {
            if (extensionData?.Mappings?.GameToProfile == null) return;
            
            if (extensionData.Mappings.GameToProfile.ContainsKey(game.Id))
            {
                extensionData.Mappings.GameToProfile.Remove(game.Id);
                SaveData();
                if (extensionData.Settings?.ShowNotifications ?? true)
                {
                    PlayniteApi.Notifications.Add(new NotificationMessage(
                        "AutomationProfileManager_ProfileRemoved",
                        string.Format(LocalizationService.GetString("LOC_APM_ProfileRemovedFrom"), game.Name),
                        NotificationType.Info
                    ));
                }
            }
        }
        
        public async Task ExecuteProfileDryRunAsync(AutomationProfile profile)
        {
            if (profile == null || actionExecutor == null) return;
            
            var actions = profile.Actions.OrderBy(a => a.Priority).ToList();
            foreach (var action in actions)
            {
                await actionExecutor.ExecuteActionAsync(action, dryRun: true);
            }
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs menuArgs)
        {
            EnsureInitialized();
            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = LocalizationService.GetString("LOC_APM_Menu_ManageProfiles"),
                    MenuSection = "@Automation Profile Manager",
                    Icon = "\uE77B",
                    Action = args =>
                    {
                        try
                        {
                            var settingsView = new AutomationProfileManagerSettingsView(this);
                            var window = new Window
                            {
                                Title = "Automation Profile Manager",
                                Content = settingsView,
                                Width = 900,
                                Height = 600,
                                WindowStartupLocation = WindowStartupLocation.CenterScreen
                            };
                            window.ShowDialog();
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "Error opening profile manager");
                        }
                    }
                },
                new MainMenuItem
                {
                    Description = LocalizationService.GetString("LOC_APM_Menu_CreateBackup"),
                    MenuSection = "@Automation Profile Manager",
                    Icon = "\uE78C",
                    Action = args =>
                    {
                        EnsureInitialized();
                        if (backupService != null && extensionData != null)
                        {
                            var path = backupService.CreateBackup(extensionData);
                            if (path != null)
                            {
                                extensionData.Settings.LastBackupDate = DateTime.Now;
                                SaveData();
                                PlayniteApi.Dialogs.ShowMessage(
                                    string.Format(LocalizationService.GetString("LOC_APM_BackupCreated"), path),
                                    "Automation Profile Manager");
                            }
                        }
                    }
                }
            };
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs menuArgs)
        {
            EnsureInitialized();
            return new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = LocalizationService.GetString("LOC_APM_Menu_AssignProfile"),
                    MenuSection = "Automation Profile Manager",
                    Icon = "\uE77B",
                    Action = args =>
                    {
                        if (args.Games != null && args.Games.Count > 0)
                        {
                            ShowProfileAssignmentMenu(args.Games.ToList());
                        }
                    }
                },
                new GameMenuItem
                {
                    Description = LocalizationService.GetString("LOC_APM_Menu_RemoveProfile"),
                    MenuSection = "Automation Profile Manager",
                    Icon = "\uE74D",
                    Action = args =>
                    {
                        if (args.Games != null)
                        {
                            foreach (var game in args.Games)
                            {
                                RemoveProfileAssignment(game);
                            }
                        }
                    }
                },
                new GameMenuItem
                {
                    Description = LocalizationService.GetString("LOC_APM_Menu_ViewProfile"),
                    MenuSection = "Automation Profile Manager",
                    Icon = "\uE946",
                    Action = args =>
                    {
                        if (args.Games != null && args.Games.Count > 0)
                        {
                            var game = args.Games[0];
                            var data = GetExtensionData();
                            if (data.Mappings?.GameToProfile != null && 
                                data.Mappings.GameToProfile.TryGetValue(game.Id, out var profileId))
                            {
                                var profile = data.Profiles?.FirstOrDefault(p => p.Id == profileId);
                                if (profile != null)
                                {
                                    var actionsText = string.Join("\n", profile.Actions.Select(a => $"  - {a.Name} ({a.ExecutionPhase})"));
                                    PlayniteApi.Dialogs.ShowMessage(
                                        $"{LocalizationService.GetString("LOC_APM_Menu_ProfileInfo")}: {profile.Name}\n{LocalizationService.GetString("LOC_APM_Sidebar_Actions")}: {profile.Actions.Count}\n\n{actionsText}",
                                        "Automation Profile Manager");
                                }
                                else
                                {
                                    PlayniteApi.Dialogs.ShowMessage(
                                        LocalizationService.GetString("LOC_APM_Menu_NoProfileAssigned"),
                                        "Automation Profile Manager");
                                }
                            }
                            else
                            {
                                PlayniteApi.Dialogs.ShowMessage(
                                    LocalizationService.GetString("LOC_APM_Menu_NoProfileAssigned"),
                                    "Automation Profile Manager");
                            }
                        }
                    }
                }
            };
        }

        public override IEnumerable<SidebarItem> GetSidebarItems()
        {
            return new List<SidebarItem>
            {
                new SidebarItem
                {
                    Title = "Profile Manager",
                    Type = SiderbarItemType.View,
                    Icon = new TextBlock
                    {
                        Text = "\uE77B",
                        FontFamily = new FontFamily("Segoe MDL2 Assets"),
                        FontSize = 20
                    },
                    Opened = () =>
                    {
                        EnsureInitialized();
                        return new ProfileManagerSidebarView(PlayniteApi, this);
                    }
                }
            };
        }

        public ExtensionData GetExtensionData()
        {
            if (extensionData == null)
            {
                LoadData();
            }
            return extensionData ?? new ExtensionData();
        }
        
        public void UpdateExtensionData(ExtensionData data)
        {
            if (data == null)
            {
                data = new ExtensionData();
            }
            
            // Ensure all properties are initialized
            if (data.ActionLibrary == null)
            {
                data.ActionLibrary = new List<Models.GameAction>();
            }
            
            if (data.Profiles == null)
            {
                data.Profiles = new List<AutomationProfile>();
            }
            
            if (data.Mappings == null)
            {
                data.Mappings = new ProfileMapping();
            }
            
            if (data.Settings == null)
            {
                data.Settings = new ExtensionSettings();
            }
            
            if (data.ActionLog == null)
            {
                data.ActionLog = new List<ActionLogEntry>();
            }
            
            extensionData = data;
            SaveData();
        }
    }
}
