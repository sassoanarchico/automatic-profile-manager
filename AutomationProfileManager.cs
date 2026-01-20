using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
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

        public override Guid Id { get; } = Guid.Parse("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");

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
                    $"Settings failed to load: {ex.Message}",
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
                    $"Settings view failed to load: {ex.Message}",
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
                        $"Profile not found for game: {game.Name}",
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
                    $"Profile assigned to {game.Name}",
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
                        $"Profile removed from {game.Name}",
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
