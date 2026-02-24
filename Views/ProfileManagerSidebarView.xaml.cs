using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AutomationProfileManager.Models;
using AutomationProfileManager.Services;
using Playnite.SDK;
using Playnite.SDK.Models;
using GameAction = AutomationProfileManager.Models.GameAction;

namespace AutomationProfileManager.Views
{
    public partial class ProfileManagerSidebarView : UserControl
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IPlayniteAPI playniteApi;
        private readonly AutomationProfileManagerPlugin plugin;
        private ObservableCollection<ProfileDisplayItem> profileItems;

        public ProfileManagerSidebarView(IPlayniteAPI api, AutomationProfileManagerPlugin plugin)
        {
            InitializeComponent();
            this.playniteApi = api;
            this.plugin = plugin;
            profileItems = new ObservableCollection<ProfileDisplayItem>();
            ProfilesItemsControl.ItemsSource = profileItems;
            LoadProfiles();
        }

        private void LoadProfiles()
        {
            try
            {
                profileItems.Clear();
                var data = plugin.GetExtensionData();

                if (data.Profiles == null || data.Profiles.Count == 0)
                {
                    EmptyStatePanel.Visibility = Visibility.Visible;
                    ProfileCountText.Text = "0 profiles";
                    GameCountText.Text = "0 games assigned";
                    return;
                }

                EmptyStatePanel.Visibility = Visibility.Collapsed;
                int totalAssigned = 0;

                foreach (var profile in data.Profiles)
                {
                    // Build assigned games list
                    var assignedGameIds = data.Mappings?.GameToProfile?
                        .Where(kvp => kvp.Value == profile.Id)
                        .Select(kvp => kvp.Key)
                        .ToList() ?? new List<Guid>();

                    var assignedGames = new List<GameAssignmentItem>();
                    foreach (var gameId in assignedGameIds)
                    {
                        var game = playniteApi.Database.Games?.FirstOrDefault(g => g.Id == gameId);
                        if (game != null)
                        {
                            assignedGames.Add(new GameAssignmentItem
                            {
                                GameId = game.Id,
                                GameName = game.Name
                            });
                        }
                    }
                    totalAssigned += assignedGames.Count;

                    // Build actions list
                    var actions = new List<ActionDisplayItem>();
                    if (profile.Actions != null)
                    {
                        foreach (var action in profile.Actions.OrderBy(a => a.Priority))
                        {
                            actions.Add(new ActionDisplayItem
                            {
                                ActionId = action.Id,
                                ProfileId = profile.Id,
                                ActionName = action.Name,
                                PhaseIcon = GetPhaseIcon(action.ExecutionPhase),
                                PhaseLabel = GetPhaseLabel(action.ExecutionPhase),
                                Action = action
                            });
                        }
                    }

                    profileItems.Add(new ProfileDisplayItem
                    {
                        ProfileId = profile.Id,
                        ProfileName = profile.Name,
                        ActionCount = (profile.Actions?.Count ?? 0).ToString(),
                        GameCount = assignedGames.Count.ToString(),
                        Actions = new ObservableCollection<ActionDisplayItem>(actions),
                        AssignedGames = new ObservableCollection<GameAssignmentItem>(assignedGames),
                        NoActionsVisibility = actions.Count == 0 ? Visibility.Visible : Visibility.Collapsed,
                        NoGamesVisibility = assignedGames.Count == 0 ? Visibility.Visible : Visibility.Collapsed
                    });
                }

                ProfileCountText.Text = $"{data.Profiles.Count} {(data.Profiles.Count == 1 ? "profile" : "profiles")}";
                GameCountText.Text = $"{totalAssigned} {(totalAssigned == 1 ? "game" : "games")} assigned";
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error loading profiles in sidebar");
            }
        }

        private string GetPhaseIcon(ExecutionPhase phase)
        {
            switch (phase)
            {
                case ExecutionPhase.BeforeStarting: return "\uE768";  // Play
                case ExecutionPhase.AfterStarting:  return "\uE769";  // Pause
                case ExecutionPhase.AfterClosing:   return "\uE71A";  // Stop
                default: return "\uE8B7";
            }
        }

        private string GetPhaseLabel(ExecutionPhase phase)
        {
            switch (phase)
            {
                case ExecutionPhase.BeforeStarting: return LocalizationService.GetString("LOC_APM_Phase_Before");
                case ExecutionPhase.AfterStarting:  return LocalizationService.GetString("LOC_APM_Phase_After");
                case ExecutionPhase.AfterClosing:   return LocalizationService.GetString("LOC_APM_Phase_Close");
                default: return "";
            }
        }

        // ── CREATE PROFILE ─────────────────────────────────────────

        private void CreateProfileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new TextInputDialog(
                    LocalizationService.GetString("LOC_APM_Sidebar_CreateProfile"),
                    LocalizationService.GetString("LOC_APM_Sidebar_ProfileName"),
                    "");

                if (dialog.ShowDialog() == true)
                {
                    var name = dialog.GetInput()?.Trim();
                    if (string.IsNullOrEmpty(name))
                    {
                        playniteApi.Dialogs.ShowMessage(
                            LocalizationService.GetString("LOC_APM_Sidebar_NameRequired"),
                            "Automation Profile Manager");
                        return;
                    }

                    var data = plugin.GetExtensionData();
                    if (data.Profiles == null) data.Profiles = new List<AutomationProfile>();

                    var newProfile = new AutomationProfile
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Actions = new List<GameAction>()
                    };

                    data.Profiles.Add(newProfile);
                    plugin.UpdateExtensionData(data);
                    LoadProfiles();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error creating profile from sidebar");
            }
        }

        // ── RENAME PROFILE ─────────────────────────────────────────

        private void RenameProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is Button btn) || !(btn.Tag is Guid profileId)) return;

                var data = plugin.GetExtensionData();
                var profile = data.Profiles?.FirstOrDefault(p => p.Id == profileId);
                if (profile == null) return;

                var dialog = new TextInputDialog(
                    LocalizationService.GetString("LOC_APM_Sidebar_RenameProfile"),
                    LocalizationService.GetString("LOC_APM_Sidebar_ProfileName"),
                    profile.Name);

                if (dialog.ShowDialog() == true)
                {
                    var name = dialog.GetInput()?.Trim();
                    if (!string.IsNullOrEmpty(name))
                    {
                        profile.Name = name;
                        plugin.UpdateExtensionData(data);
                        LoadProfiles();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error renaming profile from sidebar");
            }
        }

        // ── DELETE PROFILE ──────────────────────────────────────────

        private void DeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is Button btn) || !(btn.Tag is Guid profileId)) return;

                var data = plugin.GetExtensionData();
                var profile = data.Profiles?.FirstOrDefault(p => p.Id == profileId);
                if (profile == null) return;

                var result = playniteApi.Dialogs.ShowMessage(
                    string.Format(LocalizationService.GetString("LOC_APM_Sidebar_ConfirmDelete"), profile.Name),
                    "Automation Profile Manager",
                    MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    data.Profiles.Remove(profile);

                    // Remove all game mappings for this profile
                    if (data.Mappings?.GameToProfile != null)
                    {
                        var keysToRemove = data.Mappings.GameToProfile
                            .Where(kvp => kvp.Value == profileId)
                            .Select(kvp => kvp.Key)
                            .ToList();
                        foreach (var key in keysToRemove)
                        {
                            data.Mappings.GameToProfile.Remove(key);
                        }
                    }

                    plugin.UpdateExtensionData(data);
                    LoadProfiles();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error deleting profile from sidebar");
            }
        }

        // ── ADD ACTION TO PROFILE ───────────────────────────────────

        private void AddActionToProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is Button btn) || !(btn.Tag is Guid profileId)) return;

                var data = plugin.GetExtensionData();
                var profile = data.Profiles?.FirstOrDefault(p => p.Id == profileId);
                if (profile == null) return;

                // Get existing categories for the dialog
                var categories = data.Profiles
                    .SelectMany(p => p.Actions ?? new List<GameAction>())
                    .Select(a => a.Category)
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .ToList();

                var dialog = new ActionEditDialog(null, categories);
                if (dialog.ShowDialog() == true)
                {
                    var action = dialog.GetAction();
                    if (action != null)
                    {
                        if (profile.Actions == null) profile.Actions = new List<GameAction>();
                        profile.Actions.Add(action);
                        plugin.UpdateExtensionData(data);
                        LoadProfiles();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error adding action to profile from sidebar");
            }
        }

        // ── EDIT ACTION ─────────────────────────────────────────────

        private void EditAction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is Button btn) || !(btn.Tag is ActionDisplayItem actionItem)) return;

                var data = plugin.GetExtensionData();
                var profile = data.Profiles?.FirstOrDefault(p => p.Id == actionItem.ProfileId);
                if (profile == null) return;

                var existingAction = profile.Actions?.FirstOrDefault(a => a.Id == actionItem.ActionId);
                if (existingAction == null) return;

                var categories = data.Profiles
                    .SelectMany(p => p.Actions ?? new List<GameAction>())
                    .Select(a => a.Category)
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .ToList();

                var dialog = new ActionEditDialog(existingAction, categories);
                if (dialog.ShowDialog() == true)
                {
                    var updatedAction = dialog.GetAction();
                    if (updatedAction != null)
                    {
                        var index = profile.Actions.IndexOf(existingAction);
                        if (index >= 0)
                        {
                            profile.Actions[index] = updatedAction;
                        }
                        plugin.UpdateExtensionData(data);
                        LoadProfiles();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error editing action from sidebar");
            }
        }

        // ── REMOVE ACTION ───────────────────────────────────────────

        private void RemoveAction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is Button btn) || !(btn.Tag is ActionDisplayItem actionItem)) return;

                var data = plugin.GetExtensionData();
                var profile = data.Profiles?.FirstOrDefault(p => p.Id == actionItem.ProfileId);
                if (profile == null) return;

                var action = profile.Actions?.FirstOrDefault(a => a.Id == actionItem.ActionId);
                if (action == null) return;

                var result = playniteApi.Dialogs.ShowMessage(
                    string.Format(LocalizationService.GetString("LOC_APM_Sidebar_ConfirmRemoveAction"), action.Name),
                    "Automation Profile Manager",
                    MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    profile.Actions.Remove(action);
                    plugin.UpdateExtensionData(data);
                    LoadProfiles();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error removing action from sidebar");
            }
        }

        // ── ASSIGN GAME TO PROFILE ──────────────────────────────────

        private void AssignProfileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var data = plugin.GetExtensionData();
                if (data.Profiles == null || data.Profiles.Count == 0)
                {
                    playniteApi.Dialogs.ShowMessage(
                        LocalizationService.GetString("LOC_APM_Sidebar_NoProfilesToAssign"),
                        "Automation Profile Manager");
                    return;
                }

                // Pick a game
                var selectedGame = playniteApi.Dialogs.ChooseItemWithSearch(
                    new List<GenericItemOption>(
                        playniteApi.Database.Games
                            .OrderBy(g => g.Name)
                            .Select(g => new GenericItemOption(g.Name, g.Id.ToString()))
                    ),
                    (query) =>
                    {
                        if (string.IsNullOrWhiteSpace(query))
                        {
                            return new List<GenericItemOption>(
                                playniteApi.Database.Games
                                    .OrderBy(g => g.Name)
                                    .Select(g => new GenericItemOption(g.Name, g.Id.ToString()))
                            );
                        }
                        return new List<GenericItemOption>(
                            playniteApi.Database.Games
                                .Where(g => g.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                                .OrderBy(g => g.Name)
                                .Select(g => new GenericItemOption(g.Name, g.Id.ToString()))
                        );
                    },
                    LocalizationService.GetString("LOC_APM_Sidebar_SelectGame")
                );

                if (selectedGame == null) return;

                var gameId = Guid.Parse(selectedGame.Description);
                var game = playniteApi.Database.Games.FirstOrDefault(g => g.Id == gameId);
                if (game == null) return;

                // Pick a profile
                var dialog = new ProfileAssignmentDialog(data.Profiles, game.Name);
                if (dialog.ShowDialog() == true)
                {
                    var selectedProfileId = dialog.GetSelectedProfileId();
                    if (selectedProfileId.HasValue)
                    {
                        if (data.Mappings == null) data.Mappings = new ProfileMapping();
                        if (data.Mappings.GameToProfile == null) data.Mappings.GameToProfile = new Dictionary<Guid, Guid>();
                        data.Mappings.GameToProfile[game.Id] = selectedProfileId.Value;
                        plugin.UpdateExtensionData(data);
                    }
                    else
                    {
                        if (data.Mappings?.GameToProfile != null && data.Mappings.GameToProfile.ContainsKey(game.Id))
                        {
                            data.Mappings.GameToProfile.Remove(game.Id);
                            plugin.UpdateExtensionData(data);
                        }
                    }
                    LoadProfiles();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error assigning profile from sidebar");
            }
        }

        private void AssignToProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is Button btn) || !(btn.Tag is Guid profileId)) return;

                var data = plugin.GetExtensionData();
                var profile = data.Profiles?.FirstOrDefault(p => p.Id == profileId);
                if (profile == null) return;

                var selectedGame = playniteApi.Dialogs.ChooseItemWithSearch(
                    new List<GenericItemOption>(
                        playniteApi.Database.Games
                            .OrderBy(g => g.Name)
                            .Select(g => new GenericItemOption(g.Name, g.Id.ToString()))
                    ),
                    (query) =>
                    {
                        if (string.IsNullOrWhiteSpace(query))
                        {
                            return new List<GenericItemOption>(
                                playniteApi.Database.Games
                                    .OrderBy(g => g.Name)
                                    .Select(g => new GenericItemOption(g.Name, g.Id.ToString()))
                            );
                        }
                        return new List<GenericItemOption>(
                            playniteApi.Database.Games
                                .Where(g => g.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                                .OrderBy(g => g.Name)
                                .Select(g => new GenericItemOption(g.Name, g.Id.ToString()))
                        );
                    },
                    string.Format(LocalizationService.GetString("LOC_APM_Sidebar_AssignGameTo"), profile.Name)
                );

                if (selectedGame == null) return;

                var gameId = Guid.Parse(selectedGame.Description);
                if (data.Mappings == null) data.Mappings = new ProfileMapping();
                if (data.Mappings.GameToProfile == null) data.Mappings.GameToProfile = new Dictionary<Guid, Guid>();
                data.Mappings.GameToProfile[gameId] = profileId;
                plugin.UpdateExtensionData(data);
                LoadProfiles();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error assigning game to profile from sidebar");
            }
        }

        // ── REMOVE GAME FROM PROFILE ────────────────────────────────

        private void RemoveGameFromProfile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is Button btn) || !(btn.Tag is Guid gameId)) return;

                var data = plugin.GetExtensionData();
                if (data.Mappings?.GameToProfile != null && data.Mappings.GameToProfile.ContainsKey(gameId))
                {
                    data.Mappings.GameToProfile.Remove(gameId);
                    plugin.UpdateExtensionData(data);
                    LoadProfiles();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error removing game from profile in sidebar");
            }
        }

        // ── REFRESH ─────────────────────────────────────────────────

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadProfiles();
        }
    }

    // ── VIEW MODELS ─────────────────────────────────────────────────

    public class ProfileDisplayItem
    {
        public Guid ProfileId { get; set; }
        public string ProfileName { get; set; } = "";
        public string ActionCount { get; set; } = "0";
        public string GameCount { get; set; } = "0";
        public ObservableCollection<ActionDisplayItem> Actions { get; set; } = new ObservableCollection<ActionDisplayItem>();
        public ObservableCollection<GameAssignmentItem> AssignedGames { get; set; } = new ObservableCollection<GameAssignmentItem>();
        public Visibility NoActionsVisibility { get; set; } = Visibility.Visible;
        public Visibility NoGamesVisibility { get; set; } = Visibility.Visible;
    }

    public class ActionDisplayItem
    {
        public Guid ActionId { get; set; }
        public Guid ProfileId { get; set; }
        public string ActionName { get; set; } = "";
        public string PhaseIcon { get; set; } = "";
        public string PhaseLabel { get; set; } = "";
        public Models.GameAction Action { get; set; }
    }

    public class GameAssignmentItem
    {
        public Guid GameId { get; set; }
        public string GameName { get; set; } = "";
    }
}
