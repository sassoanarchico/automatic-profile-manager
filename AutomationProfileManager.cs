using System;
using System.Collections.Generic;
using System.Linq;
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
        
        private DataService dataService;
        private ActionExecutor actionExecutor;
        private MirrorActionTracker mirrorTracker;
        private ExtensionData extensionData;

        public override Guid Id { get; } = Guid.Parse("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");

        public AutomationProfileManagerPlugin(IPlayniteAPI api) : base(api)
        {
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            InitializeServices();
            LoadData();
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            SaveData();
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            ExecuteProfileActions(args.Game, ExecutionPhase.AfterStarting);
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            mirrorTracker?.ClearTracking();
            ExecuteProfileActions(args.Game, ExecutionPhase.BeforeStarting);
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            ExecuteProfileActions(args.Game, ExecutionPhase.AfterClosing);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return new AutomationProfileManagerSettings(this);
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new AutomationProfileManagerSettingsView(this);
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            yield return new GameMenuItem
            {
                Description = "Assegna Profilo di Automazione",
                Action = (menuArgs) => ShowProfileAssignmentMenu(args.Games.ToList())
            };
        }

        private void InitializeServices()
        {
            dataService = new DataService(PlayniteApi);
            actionExecutor = new ActionExecutor(PlayniteApi);
            mirrorTracker = new MirrorActionTracker();
        }

        private void LoadData()
        {
            extensionData = dataService.LoadData();
        }

        private void SaveData()
        {
            dataService?.SaveData(extensionData);
        }

        private async void ExecuteProfileActions(Game game, ExecutionPhase phase)
        {
            if (extensionData?.Mappings?.GameToProfile == null)
                return;

            if (!extensionData.Mappings.GameToProfile.TryGetValue(game.Id, out var profileId))
                return;

            var profile = extensionData.Profiles?.FirstOrDefault(p => p.Id == profileId);
            if (profile == null)
            {
                PlayniteApi.Notifications.Add(new NotificationMessage(
                    "AutomationProfileManager_ProfileNotFound",
                    $"Profile not found for game: {game.Name}",
                    NotificationType.Error
                ));
                return;
            }

            var actions = profile.Actions
                .Where(a => a.ExecutionPhase == phase)
                .OrderBy(a => a.Priority)
                .ToList();

            foreach (var action in actions)
            {
                if (phase == ExecutionPhase.BeforeStarting && action.IsMirrorAction)
                {
                    mirrorTracker.TrackActionBeforeExecution(action);
                }

                await actionExecutor.ExecuteActionAsync(action);

                if (phase == ExecutionPhase.AfterClosing && action.IsMirrorAction)
                {
                    if (mirrorTracker.ShouldRestoreAction(action))
                    {
                        var reverseAction = new Models.GameAction
                        {
                            Name = $"Restore: {action.Name}",
                            ActionType = ActionType.StartApp,
                            Path = action.Path,
                            Arguments = action.Arguments,
                            ExecutionPhase = ExecutionPhase.AfterClosing
                        };
                        await actionExecutor.ExecuteActionAsync(reverseAction);
                    }
                }
            }
        }

        private void ShowProfileAssignmentMenu(List<Game> games)
        {
            if (games == null || games.Count == 0)
                return;

            var game = games[0];
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
            extensionData.Mappings.GameToProfile[game.Id] = profileId;
            SaveData();
            PlayniteApi.Notifications.Add(new NotificationMessage(
                "AutomationProfileManager_ProfileAssigned",
                $"Profile assigned to {game.Name}",
                NotificationType.Info
            ));
        }

        private void RemoveProfileAssignment(Game game)
        {
            if (extensionData.Mappings.GameToProfile.ContainsKey(game.Id))
            {
                extensionData.Mappings.GameToProfile.Remove(game.Id);
                SaveData();
                PlayniteApi.Notifications.Add(new NotificationMessage(
                    "AutomationProfileManager_ProfileRemoved",
                    $"Profile removed from {game.Name}",
                    NotificationType.Info
                ));
            }
        }

        public ExtensionData GetExtensionData() => extensionData;
        
        public void UpdateExtensionData(ExtensionData data)
        {
            extensionData = data;
            SaveData();
        }
    }
}
