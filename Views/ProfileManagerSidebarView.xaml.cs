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
                    EmptyStateText.Visibility = Visibility.Visible;
                    ProfileCountText.Text = "0 profiles";
                    GameCountText.Text = "0 games assigned";
                    return;
                }

                EmptyStateText.Visibility = Visibility.Collapsed;
                int totalAssigned = 0;

                foreach (var profile in data.Profiles)
                {
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

                    profileItems.Add(new ProfileDisplayItem
                    {
                        ProfileId = profile.Id,
                        ProfileName = profile.Name,
                        ActionCount = profile.Actions?.Count.ToString() ?? "0",
                        AssignedGames = new ObservableCollection<GameAssignmentItem>(assignedGames),
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

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadProfiles();
        }

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

                // Let user pick a game from the library
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

                // Now pick a profile
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
                        // Remove assignment
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
    }

    public class ProfileDisplayItem
    {
        public Guid ProfileId { get; set; }
        public string ProfileName { get; set; } = "";
        public string ActionCount { get; set; } = "0";
        public ObservableCollection<GameAssignmentItem> AssignedGames { get; set; } = new ObservableCollection<GameAssignmentItem>();
        public Visibility NoGamesVisibility { get; set; } = Visibility.Visible;
    }

    public class GameAssignmentItem
    {
        public Guid GameId { get; set; }
        public string GameName { get; set; } = "";
    }
}
