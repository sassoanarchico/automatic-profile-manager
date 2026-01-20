using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using AutomationProfileManager.Models;
using AutomationProfileManager.Services;
using Playnite.SDK;

namespace AutomationProfileManager.Views
{
    // Classe per raggruppamento azioni per categoria nel TreeView
    public class ActionCategory
    {
        public string CategoryName { get; set; } = string.Empty;
        public List<GameActionViewModel> Actions { get; set; } = new List<GameActionViewModel>();
    }

    // ViewModel per visualizzazione azioni con tag
    public class GameActionViewModel
    {
        public GameAction Action { get; set; }
        public string Name => Action?.Name ?? "";
        public string TagsDisplay => Action?.Tags?.Any() == true 
            ? $"[{string.Join(", ", Action.Tags)}]" 
            : "";
        public string DisplayText => Action?.Tags?.Any() == true 
            ? $"{Action.Name} [{string.Join(", ", Action.Tags)}]"
            : Action?.Name ?? "";
        
        public GameActionViewModel(GameAction action)
        {
            Action = action;
        }
    }

    public partial class AutomationProfileManagerSettingsView : UserControl
    {
        private readonly AutomationProfileManagerPlugin plugin = null!;
        private ExtensionData extensionData = new ExtensionData();
        private AutomationProfile? selectedProfile;
        private Models.GameAction? selectedAction;
        
        // Drag & drop state
        private Point? dragStartPoint;
        private bool isDragging = false;
        private const double DragThreshold = 10; // Pixel minimi per iniziare il drag
        
        // Flag per evitare eventi durante l'inizializzazione
        private bool isInitializing = true;

        public AutomationProfileManagerSettingsView(AutomationProfileManagerPlugin plugin)
        {
            try
            {
                isInitializing = true;
                InitializeComponent();
                this.plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
                LoadData();
                InitializeUI();
                isInitializing = false;
            }
            catch (Exception ex)
            {
                // Log error and show message
                Playnite.SDK.LogManager.GetLogger().Error(ex, "Failed to initialize settings view");
                System.Windows.MessageBox.Show(
                    $"Errore nel caricamento delle impostazioni: {ex.Message}\n\nDettagli: {ex.StackTrace}",
                    "Errore Impostazioni",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
            }
        }

        private void LoadData()
        {
            extensionData = plugin.GetExtensionData();
            
            // Ensure all properties are initialized
            if (extensionData == null)
            {
                extensionData = new ExtensionData();
            }
            
            if (extensionData.ActionLibrary == null)
            {
                extensionData.ActionLibrary = new List<Models.GameAction>();
            }
            
            if (extensionData.Profiles == null)
            {
                extensionData.Profiles = new List<AutomationProfile>();
            }
            
            if (extensionData.Mappings == null)
            {
                extensionData.Mappings = new ProfileMapping();
            }
            
            if (extensionData.Settings == null)
            {
                extensionData.Settings = new ExtensionSettings();
            }
            
            if (extensionData.ActionLog == null)
            {
                extensionData.ActionLog = new List<ActionLogEntry>();
            }

            if (extensionData.Statistics == null)
            {
                extensionData.Statistics = new List<ActionStatistics>();
            }

            if (extensionData.ProfileStats == null)
            {
                extensionData.ProfileStats = new List<ProfileStatistics>();
            }
        }

        private void InitializeUI()
        {
            try
            {
                if (extensionData == null)
                {
                    extensionData = new ExtensionData();
                }
                
                ActionsDataGrid.ItemsSource = extensionData.ActionLibrary ?? new List<Models.GameAction>();
                ProfilesListBox.ItemsSource = extensionData.Profiles ?? new List<AutomationProfile>();
                RefreshActionsTreeView();
                
                // Initialize category filter
                RefreshCategoryFilter();
                
                // Initialize log viewer
                RefreshLog();

                // Initialize statistics
                RefreshStatistics();
                
                // Initialize settings (always initialized in LoadData)
                if (extensionData.Settings != null)
                {
                    ShowNotificationsCheckBox.IsChecked = extensionData.Settings.ShowNotifications;
                    EnableDryRunCheckBox.IsChecked = extensionData.Settings.EnableDryRun;
                    MaxLogEntriesTextBox.Text = extensionData.Settings.MaxLogEntries.ToString();
                    AutoBackupCheckBox.IsChecked = extensionData.Settings.AutoBackupEnabled;
                    BackupIntervalTextBox.Text = extensionData.Settings.BackupIntervalDays.ToString();
                }
                else
                {
                    extensionData.Settings = new ExtensionSettings();
                    ShowNotificationsCheckBox.IsChecked = true;
                    EnableDryRunCheckBox.IsChecked = false;
                    MaxLogEntriesTextBox.Text = "100";
                    AutoBackupCheckBox.IsChecked = true;
                    BackupIntervalTextBox.Text = "7";
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex, "Failed to initialize UI");
                System.Windows.MessageBox.Show(
                    $"Errore nell'inizializzazione dell'interfaccia: {ex.Message}",
                    "Errore",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
            }
        }

        private void ActionsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedAction = ActionsDataGrid.SelectedItem as Models.GameAction;
            var selectedCount = ActionsDataGrid.SelectedItems.Count;
            EditActionButton.IsEnabled = selectedCount == 1;
            RemoveActionButton.IsEnabled = selectedCount > 0;
        }

        private void ActionsDataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && ActionsDataGrid.SelectedItems.Count > 0)
            {
                RemoveSelectedActions();
                e.Handled = true;
            }
        }

        private void RemoveSelectedActions()
        {
            var selectedActions = ActionsDataGrid.SelectedItems.Cast<Models.GameAction>().ToList();
            if (selectedActions.Count == 0) return;

            var message = selectedActions.Count == 1
                ? $"Rimuovere l'azione '{selectedActions[0].Name}'?\n\nQuesta azione verrà rimossa anche da tutti i profili."
                : $"Rimuovere {selectedActions.Count} azioni selezionate?\n\nQueste azioni verranno rimosse anche da tutti i profili.";

            var result = MessageBox.Show(
                message,
                "Conferma Rimozione",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                foreach (var action in selectedActions)
                {
                    extensionData.ActionLibrary.Remove(action);
                    
                    foreach (var profile in extensionData.Profiles)
                    {
                        profile.Actions.RemoveAll(a => a.Id == action.Id);
                    }
                }
                
                RefreshActions();
                RefreshProfiles();
                RefreshCategoryFilter();
                plugin.UpdateExtensionData(extensionData);
            }
        }

        private void AddAction_Click(object sender, RoutedEventArgs e)
        {
            var existingCategories = extensionData.ActionLibrary
                .Select(a => a.Category)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .ToList();
            
            var dialog = new ActionEditDialog(null, existingCategories);
            if (dialog.ShowDialog() == true)
            {
                var newAction = dialog.GetAction();
                extensionData.ActionLibrary.Add(newAction);
                RefreshActions();
                RefreshCategoryFilter();
                plugin.UpdateExtensionData(extensionData);
            }
        }

        private void EditAction_Click(object sender, RoutedEventArgs e)
        {
            if (selectedAction == null) return;

            var existingCategories = extensionData.ActionLibrary
                .Select(a => a.Category)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .ToList();

            var dialog = new ActionEditDialog(selectedAction, existingCategories);
            if (dialog.ShowDialog() == true)
            {
                var updatedAction = dialog.GetAction();
                var index = extensionData.ActionLibrary.IndexOf(selectedAction);
                if (index >= 0)
                {
                    extensionData.ActionLibrary[index] = updatedAction;
                }
                RefreshActions();
                RefreshCategoryFilter();
                plugin.UpdateExtensionData(extensionData);
            }
        }

        private void RemoveAction_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelectedActions();
        }

        private void ImportDefaultActions_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Vuoi importare le azioni predefinite?\n\n" +
                "Verranno aggiunte azioni comuni come:\n" +
                "- Chiudi/Apri applicazioni (Chrome, Discord, Spotify...)\n" +
                "- Comandi di sistema (Prestazioni elevate, Game Mode...)\n" +
                "- Azioni di attesa\n\n" +
                "Le azioni esistenti non verranno modificate.",
                "Importa Azioni Predefinite",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                var defaultActions = DefaultActionsProvider.GetDefaultActions();
                int importedCount = 0;

                foreach (var action in defaultActions)
                {
                    if (!extensionData.ActionLibrary.Any(a => a.Name == action.Name))
                    {
                        extensionData.ActionLibrary.Add(action);
                        importedCount++;
                    }
                }

                RefreshActions();
                plugin.UpdateExtensionData(extensionData);

                MessageBox.Show(
                    $"Importate {importedCount} azioni predefinite!\n\n" +
                    $"({defaultActions.Count - importedCount} azioni erano gia presenti)",
                    "Importazione Completata",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }

        private void AddProfile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TextInputDialog("Nuovo Profilo", "Inserisci il nome del profilo:");
            if (dialog.ShowDialog() == true)
            {
                var profileName = dialog.GetText();
                if (!string.IsNullOrWhiteSpace(profileName))
                {
                    var newProfile = new AutomationProfile { Name = profileName };
                    extensionData.Profiles.Add(newProfile);
                    RefreshProfiles();
                    plugin.UpdateExtensionData(extensionData);
                    ProfilesListBox.SelectedItem = newProfile;
                }
            }
        }

        private void RemoveProfile_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProfile == null) return;

            var result = MessageBox.Show(
                $"Rimuovere il profilo '{selectedProfile.Name}'?\n\nI giochi assegnati perderanno l'associazione.",
                "Conferma Rimozione",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                extensionData.Profiles.Remove(selectedProfile);
                
                var keysToRemove = extensionData.Mappings.GameToProfile
                    .Where(kvp => kvp.Value == selectedProfile.Id)
                    .Select(kvp => kvp.Key)
                    .ToList();
                
                foreach (var key in keysToRemove)
                {
                    extensionData.Mappings.GameToProfile.Remove(key);
                }
                
                selectedProfile = null;
                ProfileNameTextBlock.Text = "";
                ProfileActionsListBox.ItemsSource = null;
                
                RefreshProfiles();
                plugin.UpdateExtensionData(extensionData);
            }
        }

        private void ProfilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedProfile = ProfilesListBox.SelectedItem as AutomationProfile;
            RemoveProfileButton.IsEnabled = selectedProfile != null;
            DuplicateProfileButton.IsEnabled = selectedProfile != null;
            DryRunButton.IsEnabled = selectedProfile != null;
            FindReplaceButton.IsEnabled = selectedProfile != null;
            
            if (selectedProfile != null)
            {
                ProfileNameTextBlock.Text = "Profilo: " + selectedProfile.Name;
                ProfileActionsListBox.ItemsSource = null;
                ProfileActionsListBox.ItemsSource = selectedProfile.Actions;
            }
            else
            {
                ProfileNameTextBlock.Text = "Seleziona un profilo";
                ProfileActionsListBox.ItemsSource = null;
            }
        }

        #region Available Actions - TreeView Drag Start

        private void AvailableActionsTreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dragStartPoint = e.GetPosition(null);
            isDragging = false;
        }

        private void AvailableActionsTreeView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || dragStartPoint == null)
                return;

            Point currentPoint = e.GetPosition(null);
            Vector diff = dragStartPoint.Value - currentPoint;

            // Inizia il drag solo se il mouse si è mosso abbastanza
            if (Math.Abs(diff.X) > DragThreshold || Math.Abs(diff.Y) > DragThreshold)
            {
                if (!isDragging && selectedProfile != null)
                {
                    var selectedItem = AvailableActionsTreeView.SelectedItem;
                    GameAction? action = null;
                    
                    if (selectedItem is GameActionViewModel vm)
                    {
                        action = vm.Action;
                    }
                    
                    if (action != null)
                    {
                        isDragging = true;
                        var data = new DataObject("GameActionData", action);
                        data.SetData("SourceType", "AvailableActions");
                        DragDrop.DoDragDrop(AvailableActionsTreeView, data, DragDropEffects.Copy);
                        isDragging = false;
                        dragStartPoint = null;
                    }
                }
            }
        }

        private void AvailableActionsTreeView_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dragStartPoint = null;
            isDragging = false;
        }

        #endregion

        #region Profile Actions - Drag & Drop Target

        private void ProfileActionsListBox_DragOver(object sender, DragEventArgs e)
        {
            if (selectedProfile == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            if (e.Data.GetDataPresent("GameActionData"))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void ProfileActionsListBox_Drop(object sender, DragEventArgs e)
        {
            if (selectedProfile == null) return;

            if (e.Data.GetDataPresent("GameActionData"))
            {
                var action = e.Data.GetData("GameActionData") as Models.GameAction;
                if (action != null)
                {
                    // Aggiunta nuova azione dalla libreria
                    var profileAction = new Models.GameAction
                    {
                        Id = Guid.NewGuid(),
                        Name = action.Name,
                        ActionType = action.ActionType,
                        Path = action.Path,
                        Arguments = action.Arguments,
                        ExecutionPhase = ExecutionPhase.BeforeStarting,
                        IsMirrorAction = action.IsMirrorAction,
                        Priority = selectedProfile.Actions.Count,
                        WaitSeconds = action.WaitSeconds,
                        Category = action.Category,
                        Tags = action.Tags != null ? new List<string>(action.Tags) : new List<string>()
                    };
                    
                    selectedProfile.Actions.Add(profileAction);
                    UpdateActionPriorities();
                    RefreshProfileActions();
                    plugin.UpdateExtensionData(extensionData);
                }
            }
            
            e.Handled = true;
        }

        #endregion

        #region Profile Actions - Item Controls

        private void RemoveActionFromProfile_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProfile == null) return;

            var button = sender as Button;
            if (button?.DataContext is Models.GameAction action)
            {
                selectedProfile.Actions.Remove(action);
                UpdateActionPriorities();
                RefreshProfileActions();
                plugin.UpdateExtensionData(extensionData);
            }
            e.Handled = true;
        }

        private void ExecutionPhase_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (selectedProfile != null && e.AddedItems.Count > 0)
            {
                plugin.UpdateExtensionData(extensionData);
            }
        }

        private void MoveActionUp_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProfile == null) return;

            var button = sender as Button;
            if (button?.DataContext is Models.GameAction action)
            {
                int index = selectedProfile.Actions.IndexOf(action);
                if (index > 0)
                {
                    selectedProfile.Actions.RemoveAt(index);
                    selectedProfile.Actions.Insert(index - 1, action);
                    UpdateActionPriorities();
                    RefreshProfileActions();
                    plugin.UpdateExtensionData(extensionData);
                }
            }
            e.Handled = true;
        }

        private void MoveActionDown_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProfile == null) return;

            var button = sender as Button;
            if (button?.DataContext is Models.GameAction action)
            {
                int index = selectedProfile.Actions.IndexOf(action);
                if (index < selectedProfile.Actions.Count - 1)
                {
                    selectedProfile.Actions.RemoveAt(index);
                    selectedProfile.Actions.Insert(index + 1, action);
                    UpdateActionPriorities();
                    RefreshProfileActions();
                    plugin.UpdateExtensionData(extensionData);
                }
            }
            e.Handled = true;
        }

        #endregion

        // Metodo per aggiungere azione con doppio click nel TreeView
        private void AvailableActionsTreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (selectedProfile == null)
            {
                MessageBox.Show("Seleziona prima un profilo dalla lista a sinistra.", "Nessun Profilo Selezionato", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var selectedItem = AvailableActionsTreeView.SelectedItem;
            GameAction? action = null;
            
            if (selectedItem is GameActionViewModel vm)
            {
                action = vm.Action;
            }
            
            if (action != null)
            {
                var profileAction = new GameAction
                {
                    Id = Guid.NewGuid(),
                    Name = action.Name,
                    ActionType = action.ActionType,
                    Path = action.Path,
                    Arguments = action.Arguments,
                    ExecutionPhase = ExecutionPhase.BeforeStarting,
                    IsMirrorAction = action.IsMirrorAction,
                    Priority = selectedProfile.Actions.Count,
                    WaitSeconds = action.WaitSeconds,
                    Category = action.Category,
                    Tags = action.Tags != null ? new List<string>(action.Tags) : new List<string>()
                };

                selectedProfile.Actions.Add(profileAction);
                UpdateActionPriorities();
                RefreshProfileActions();
                plugin.UpdateExtensionData(extensionData);
            }
        }

        private void UpdateActionPriorities()
        {
            if (selectedProfile != null)
            {
                for (int i = 0; i < selectedProfile.Actions.Count; i++)
                {
                    selectedProfile.Actions[i].Priority = i;
                }
            }
        }

        private void RefreshActions()
        {
            ActionsDataGrid.ItemsSource = null;
            ActionsDataGrid.ItemsSource = extensionData.ActionLibrary;
            RefreshActionsTreeView();
        }

        // Aggiorna il TreeView con le azioni raggruppate per categoria
        private void RefreshActionsTreeView()
        {
            if (extensionData?.ActionLibrary == null)
            {
                AvailableActionsTreeView.ItemsSource = null;
                return;
            }
            
            var groupedActions = extensionData.ActionLibrary
                .GroupBy(a => a.Category ?? "Generale")
                .OrderBy(g => g.Key)
                .Select(g => new ActionCategory
                {
                    CategoryName = $"{g.Key} ({g.Count()})",
                    Actions = g.OrderBy(a => a.Name)
                               .Select(a => new GameActionViewModel(a))
                               .ToList()
                })
                .ToList();
            
            AvailableActionsTreeView.ItemsSource = null;
            AvailableActionsTreeView.ItemsSource = groupedActions;
        }

        private void RefreshProfiles()
        {
            ProfilesListBox.ItemsSource = null;
            ProfilesListBox.ItemsSource = extensionData.Profiles;
        }

        private void RefreshProfileActions()
        {
            if (selectedProfile != null)
            {
                ProfileActionsListBox.ItemsSource = null;
                ProfileActionsListBox.ItemsSource = selectedProfile.Actions;
            }
        }

        #region Category Filter

        private void RefreshCategoryFilter()
        {
            try
            {
                if (extensionData?.ActionLibrary == null)
                {
                    return;
                }
                
                var categories = extensionData.ActionLibrary
                    .Select(a => a.Category ?? "Generale")
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                CategoryFilterComboBox.Items.Clear();
                CategoryFilterComboBox.Items.Add(new ComboBoxItem { Content = "Tutte", IsSelected = true });
                
                foreach (var category in categories)
                {
                    CategoryFilterComboBox.Items.Add(new ComboBoxItem { Content = category });
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex, "Failed to refresh category filter");
            }
        }

        private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Skip during initialization
            if (isInitializing)
            {
                return;
            }
            
            if (extensionData?.ActionLibrary == null)
            {
                return;
            }

            if (CategoryFilterComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedCategory = selectedItem.Content?.ToString() ?? "Tutte";
                
                if (selectedCategory == "Tutte")
                {
                    ActionsDataGrid.ItemsSource = extensionData.ActionLibrary;
                }
                else
                {
                    ActionsDataGrid.ItemsSource = extensionData.ActionLibrary
                        .Where(a => (a.Category ?? "Generale") == selectedCategory)
                        .ToList();
                }
            }
        }

        #endregion

        #region Profile Management - New Features

        private void DuplicateProfile_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProfile == null) return;

            var dialog = new TextInputDialog("Duplica Profilo", "Inserisci il nome per il profilo duplicato:");
            if (dialog.ShowDialog() == true)
            {
                var newName = dialog.GetText();
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    var duplicatedProfile = new AutomationProfile
                    {
                        Id = Guid.NewGuid(),
                        Name = newName
                    };

                    foreach (var action in selectedProfile.Actions)
                    {
                        duplicatedProfile.Actions.Add(new Models.GameAction
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

                    extensionData.Profiles.Add(duplicatedProfile);
                    RefreshProfiles();
                    plugin.UpdateExtensionData(extensionData);
                    ProfilesListBox.SelectedItem = duplicatedProfile;
                }
            }
        }

        private void CreateFromTemplate_Click(object sender, RoutedEventArgs e)
        {
            var templates = ProfileTemplateService.GetEmulatorTemplates();
            var templateNames = templates.Select(t => t.Name).ToList();

            var dialog = new TextInputDialog("Crea da Template", "Seleziona un template:");
            // For simplicity, we'll use a simple selection dialog
            // In a real implementation, you might want a proper template selection dialog
            
            var templateDialog = new Window
            {
                Title = "Seleziona Template",
                Width = 400,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this)
            };

            var listBox = new ListBox
            {
                Margin = new Thickness(10),
                DisplayMemberPath = "Name"
            };
            
            foreach (var template in templates)
            {
                listBox.Items.Add(template);
            }

            var nameTextBox = new TextBox
            {
                Margin = new Thickness(10, 0, 10, 10),
                Height = 25
            };

            var okButton = new Button
            {
                Content = "Crea",
                Width = 100,
                Height = 30,
                Margin = new Thickness(0, 0, 10, 10),
                HorizontalAlignment = HorizontalAlignment.Right,
                IsDefault = true
            };

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(new TextBlock { Text = "Nome profilo:", Margin = new Thickness(10, 10, 10, 5) });
            stackPanel.Children.Add(nameTextBox);
            stackPanel.Children.Add(new TextBlock { Text = "Template:", Margin = new Thickness(10, 10, 10, 5) });
            stackPanel.Children.Add(listBox);
            stackPanel.Children.Add(okButton);

            templateDialog.Content = stackPanel;

            okButton.Click += (s, args) =>
            {
                if (listBox.SelectedItem is ProfileTemplate selectedTemplate && !string.IsNullOrWhiteSpace(nameTextBox.Text))
                {
                    var profile = ProfileTemplateService.CreateProfileFromTemplate(selectedTemplate, nameTextBox.Text);
                    extensionData.Profiles.Add(profile);
                    RefreshProfiles();
                    plugin.UpdateExtensionData(extensionData);
                    ProfilesListBox.SelectedItem = profile;
                    templateDialog.DialogResult = true;
                }
            };

            if (templateDialog.ShowDialog() == true)
            {
                // Profile created
            }
        }

        private async void DryRunProfile_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProfile == null) return;

            MessageBox.Show(
                "Esecuzione dry-run del profilo. Controlla il tab 'Log Azioni' per vedere i risultati.",
                "Dry-Run Avviato",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            await plugin.ExecuteProfileDryRunAsync(selectedProfile);
            RefreshLog();
        }

        private void FindReplacePath_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProfile == null) return;

            var dialog = new Window
            {
                Title = "Trova e Sostituisci Percorsi",
                Width = 500,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this)
            };

            var findTextBox = new TextBox { Margin = new Thickness(10), Height = 25 };
            var replaceTextBox = new TextBox { Margin = new Thickness(10), Height = 25 };
            var replaceButton = new Button
            {
                Content = "Sostituisci",
                Width = 120,
                Height = 30,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Right,
                IsDefault = true
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            grid.Children.Add(new TextBlock { Text = "Trova:", Margin = new Thickness(10, 10, 10, 5) });
            Grid.SetRow(findTextBox, 1);
            grid.Children.Add(findTextBox);
            grid.Children.Add(new TextBlock { Text = "Sostituisci con:", Margin = new Thickness(10, 10, 10, 5) });
            Grid.SetRow(replaceTextBox, 2);
            grid.Children.Add(replaceTextBox);
            Grid.SetRow(replaceButton, 3);
            grid.Children.Add(replaceButton);

            dialog.Content = grid;

            replaceButton.Click += (s, args) =>
            {
                string findText = findTextBox.Text;
                string replaceText = replaceTextBox.Text;

                if (string.IsNullOrWhiteSpace(findText))
                {
                    MessageBox.Show("Inserisci il testo da cercare.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int replacedCount = 0;
                foreach (var action in selectedProfile.Actions)
                {
                    if (action.Path.Contains(findText))
                    {
                        action.Path = action.Path.Replace(findText, replaceText);
                        replacedCount++;
                    }
                    if (action.Arguments.Contains(findText))
                    {
                        action.Arguments = action.Arguments.Replace(findText, replaceText);
                        replacedCount++;
                    }
                }

                RefreshProfileActions();
                plugin.UpdateExtensionData(extensionData);

                MessageBox.Show(
                    $"Sostituiti {replacedCount} occorrenze.",
                    "Sostituzione Completata",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                dialog.DialogResult = true;
            };

            dialog.ShowDialog();
        }

        private void ExportProfiles_Click(object sender, RoutedEventArgs e)
        {
            if (extensionData == null) return;

            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                DefaultExt = ".json",
                FileName = "automation_profiles_export.json"
            };

            if (saveDialog.ShowDialog() == true)
            {
                var exportService = new ProfileImportExportService();
                bool success = exportService.ExportProfiles(
                    extensionData.Profiles, 
                    extensionData.ActionLibrary, 
                    saveDialog.FileName
                );

                if (success)
                {
                    MessageBox.Show(
                        $"Esportati {extensionData.Profiles.Count} profili e {extensionData.ActionLibrary.Count} azioni.",
                        "Esportazione Completata",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                else
                {
                    MessageBox.Show(
                        "Errore durante l'esportazione.",
                        "Errore",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
        }

        private void ImportProfiles_Click(object sender, RoutedEventArgs e)
        {
            if (extensionData == null) return;

            var openDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                DefaultExt = ".json"
            };

            if (openDialog.ShowDialog() == true)
            {
                var exportService = new ProfileImportExportService();
                var importData = exportService.ImportFromFile(openDialog.FileName);

                if (importData == null)
                {
                    MessageBox.Show(
                        "Errore durante l'importazione. Verifica che il file sia valido.",
                        "Errore",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    return;
                }

                int profilesImported = 0;
                int actionsImported = 0;

                foreach (var profile in importData.Profiles)
                {
                    if (!extensionData.Profiles.Any(p => p.Name == profile.Name))
                    {
                        extensionData.Profiles.Add(profile);
                        profilesImported++;
                    }
                }

                foreach (var action in importData.Actions)
                {
                    if (!extensionData.ActionLibrary.Any(a => a.Name == action.Name))
                    {
                        extensionData.ActionLibrary.Add(action);
                        actionsImported++;
                    }
                }

                RefreshActions();
                RefreshProfiles();
                RefreshCategoryFilter();
                plugin.UpdateExtensionData(extensionData);

                MessageBox.Show(
                    $"Importati {profilesImported} profili e {actionsImported} azioni.\n" +
                    $"({importData.Profiles.Count - profilesImported} profili e {importData.Actions.Count - actionsImported} azioni erano gia presenti)",
                    "Importazione Completata",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }

        #endregion

        #region Log Viewer

        private void RefreshLog_Click(object sender, RoutedEventArgs e)
        {
            RefreshLog();
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Vuoi pulire tutti i log?",
                "Conferma",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                extensionData.ActionLog?.Clear();
                plugin.UpdateExtensionData(extensionData);
                RefreshLog();
            }
        }

        private void LogFilter_Changed(object sender, RoutedEventArgs e)
        {
            // Guard: skip if still initializing
            if (extensionData == null)
            {
                return;
            }
            RefreshLog();
        }

        private void RefreshLog()
        {
            try
            {
                if (extensionData?.ActionLog == null || LogDataGrid == null)
                {
                    return;
                }

                var logs = extensionData.ActionLog.AsEnumerable();

                if (ShowDryRunLogsCheckBox != null && ShowDryRunLogsCheckBox.IsChecked == false)
                {
                    logs = logs.Where(l => !l.IsDryRun);
                }

                LogDataGrid.ItemsSource = logs.OrderByDescending(l => l.Timestamp).ToList();
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex, "Failed to refresh log");
                if (LogDataGrid != null)
                {
                    LogDataGrid.ItemsSource = null;
                }
            }
        }

        #endregion

        #region Settings

        private void ShowNotificationsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (extensionData == null) return;
            UpdateNotificationSetting();
        }

        private void ShowNotificationsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (extensionData == null) return;
            UpdateNotificationSetting();
        }

        private void UpdateNotificationSetting()
        {
            if (extensionData == null) return;
            
            if (extensionData.Settings == null)
                extensionData.Settings = new ExtensionSettings();
            
            extensionData.Settings.ShowNotifications = ShowNotificationsCheckBox?.IsChecked ?? true;
            plugin.UpdateExtensionData(extensionData);
        }

        private void EnableDryRunCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (extensionData == null) return;
            UpdateDryRunSetting();
        }

        private void EnableDryRunCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (extensionData == null) return;
            UpdateDryRunSetting();
        }

        private void UpdateDryRunSetting()
        {
            if (extensionData == null) return;
            
            if (extensionData.Settings == null)
                extensionData.Settings = new ExtensionSettings();
            
            extensionData.Settings.EnableDryRun = EnableDryRunCheckBox?.IsChecked ?? false;
            plugin.UpdateExtensionData(extensionData);
        }

        private void MaxLogEntriesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (extensionData == null) return;
            
            if (extensionData.Settings == null)
                extensionData.Settings = new ExtensionSettings();
            
            if (int.TryParse(MaxLogEntriesTextBox?.Text, out int maxEntries) && maxEntries > 0)
            {
                extensionData.Settings.MaxLogEntries = maxEntries;
                plugin.UpdateExtensionData(extensionData);
            }
        }

        private void AutoBackupCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (extensionData == null) return;
            UpdateBackupSettings();
        }

        private void AutoBackupCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (extensionData == null) return;
            UpdateBackupSettings();
        }

        private void BackupIntervalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (extensionData == null) return;
            UpdateBackupSettings();
        }

        private void UpdateBackupSettings()
        {
            if (extensionData?.Settings == null) return;

            extensionData.Settings.AutoBackupEnabled = AutoBackupCheckBox?.IsChecked ?? true;
            
            if (int.TryParse(BackupIntervalTextBox?.Text, out int interval) && interval > 0)
            {
                extensionData.Settings.BackupIntervalDays = interval;
            }
            
            plugin.UpdateExtensionData(extensionData);
        }

        private void BackupNow_Click(object sender, RoutedEventArgs e)
        {
            if (extensionData == null) return;

            try
            {
                var backupService = new BackupService(plugin.PlayniteApi);
                var backupPath = backupService.CreateBackup(extensionData);
                
                if (backupPath != null)
                {
                    extensionData.Settings.LastBackupDate = DateTime.Now;
                    plugin.UpdateExtensionData(extensionData);
                    
                    MessageBox.Show(
                        $"Backup creato con successo:\n{backupPath}",
                        "Backup Completato",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Errore durante il backup: {ex.Message}",
                    "Errore",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void RestoreBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var backupService = new BackupService(plugin.PlayniteApi);
                var backups = backupService.GetAvailableBackups();

                if (backups.Length == 0)
                {
                    MessageBox.Show("Nessun backup disponibile.", "Backup", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    InitialDirectory = System.IO.Path.GetDirectoryName(backups[0]),
                    Filter = "JSON files (*.json)|*.json"
                };

                if (dialog.ShowDialog() == true)
                {
                    var restoredData = backupService.RestoreFromBackup(dialog.FileName);
                    if (restoredData != null)
                    {
                        plugin.UpdateExtensionData(restoredData);
                        extensionData = restoredData;
                        
                        RefreshActions();
                        RefreshProfiles();
                        RefreshLog();
                        RefreshStatistics();
                        
                        MessageBox.Show("Backup ripristinato con successo!", "Ripristino", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il ripristino: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RestartWizard_Click(object sender, RoutedEventArgs e)
        {
            if (extensionData?.Settings != null)
            {
                extensionData.Settings.WizardCompleted = false;
                plugin.UpdateExtensionData(extensionData);
                MessageBox.Show("Il wizard verrà mostrato al prossimo riavvio di Playnite.", "Wizard", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion

        #region Statistics

        private void RefreshStatistics_Click(object sender, RoutedEventArgs e)
        {
            RefreshStatistics();
        }

        private void RefreshStatistics()
        {
            try
            {
                if (extensionData?.Statistics == null) return;

                var statsService = new StatisticsService(extensionData.Statistics, extensionData.ProfileStats);
                var summary = statsService.GetSummary();

                TotalActionsText.Text = summary.TotalActionsExecuted.ToString();
                TimeSavedText.Text = summary.FormattedTimeSaved;
                SuccessRateText.Text = $"{summary.AverageSuccessRate:F0}%";
                
                if (summary.AverageSuccessRate >= 90)
                    SuccessRateText.Foreground = System.Windows.Media.Brushes.LimeGreen;
                else if (summary.AverageSuccessRate >= 70)
                    SuccessRateText.Foreground = System.Windows.Media.Brushes.Yellow;
                else
                    SuccessRateText.Foreground = System.Windows.Media.Brushes.Red;

                MostUsedActionsGrid.ItemsSource = summary.MostUsedActions;
                FailingActionsGrid.ItemsSource = summary.MostFailingActions;
            }
            catch (Exception ex)
            {
                LogManager.GetLogger().Error(ex, "Failed to refresh statistics");
            }
        }

        #endregion
    }

    // Simple converter for bool to status string
    public class BoolToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool success)
            {
                return success ? "OK" : "Errore";
            }
            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter for Tags list to comma-separated string
    public class TagsToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is List<string> tags && tags.Any())
            {
                return string.Join(", ", tags);
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

