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

namespace AutomationProfileManager.Views
{
    public partial class AutomationProfileManagerSettingsView : UserControl
    {
        private readonly AutomationProfileManagerPlugin plugin;
        private ExtensionData? extensionData;
        private AutomationProfile? selectedProfile;
        private Models.GameAction? selectedAction;
        
        // Drag & drop state
        private Point? dragStartPoint;
        private bool isDragging = false;
        private const double DragThreshold = 10; // Pixel minimi per iniziare il drag

        public AutomationProfileManagerSettingsView(AutomationProfileManagerPlugin plugin)
        {
            InitializeComponent();
            this.plugin = plugin;
            LoadData();
            InitializeUI();
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
        }

        private void InitializeUI()
        {
            ActionsDataGrid.ItemsSource = extensionData.ActionLibrary;
            ProfilesListBox.ItemsSource = extensionData.Profiles;
            AvailableActionsListBox.ItemsSource = extensionData.ActionLibrary;
            
            // Initialize category filter
            RefreshCategoryFilter();
            
            // Initialize log viewer
            RefreshLog();
            
            // Initialize settings (always initialized in LoadData)
            ShowNotificationsCheckBox.IsChecked = extensionData.Settings.ShowNotifications;
            EnableDryRunCheckBox.IsChecked = extensionData.Settings.EnableDryRun;
            MaxLogEntriesTextBox.Text = extensionData.Settings.MaxLogEntries.ToString();
        }

        private void ActionsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedAction = ActionsDataGrid.SelectedItem as Models.GameAction;
            EditActionButton.IsEnabled = selectedAction != null;
            RemoveActionButton.IsEnabled = selectedAction != null;
        }

        private void AddAction_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ActionEditDialog();
            if (dialog.ShowDialog() == true)
            {
                var newAction = dialog.GetAction();
                extensionData.ActionLibrary.Add(newAction);
                RefreshActions();
                plugin.UpdateExtensionData(extensionData);
            }
        }

        private void EditAction_Click(object sender, RoutedEventArgs e)
        {
            if (selectedAction == null) return;

            var dialog = new ActionEditDialog(selectedAction);
            if (dialog.ShowDialog() == true)
            {
                var updatedAction = dialog.GetAction();
                var index = extensionData.ActionLibrary.IndexOf(selectedAction);
                if (index >= 0)
                {
                    extensionData.ActionLibrary[index] = updatedAction;
                }
                RefreshActions();
                plugin.UpdateExtensionData(extensionData);
            }
        }

        private void RemoveAction_Click(object sender, RoutedEventArgs e)
        {
            if (selectedAction == null) return;

            var result = MessageBox.Show(
                $"Rimuovere l'azione '{selectedAction.Name}'?\n\nQuesta azione verra rimossa anche da tutti i profili.",
                "Conferma Rimozione",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.Yes)
            {
                extensionData.ActionLibrary.Remove(selectedAction);
                
                foreach (var profile in extensionData.Profiles)
                {
                    profile.Actions.RemoveAll(a => a.Id == selectedAction.Id);
                }
                
                RefreshActions();
                RefreshProfiles();
                plugin.UpdateExtensionData(extensionData);
            }
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

        #region Available Actions - Drag Start

        private void AvailableActionsListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dragStartPoint = e.GetPosition(null);
            isDragging = false;
        }

        private void AvailableActionsListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || dragStartPoint == null)
                return;

            Point currentPoint = e.GetPosition(null);
            Vector diff = dragStartPoint.Value - currentPoint;

            // Inizia il drag solo se il mouse si � mosso abbastanza
            if (Math.Abs(diff.X) > DragThreshold || Math.Abs(diff.Y) > DragThreshold)
            {
                if (!isDragging && selectedProfile != null && AvailableActionsListBox.SelectedItem != null)
                {
                    isDragging = true;
                    var action = AvailableActionsListBox.SelectedItem as Models.GameAction;
                    if (action != null)
                    {
                        var data = new DataObject("GameActionData", action);
                        data.SetData("SourceType", "AvailableActions");
                        DragDrop.DoDragDrop(AvailableActionsListBox, data, DragDropEffects.Copy);
                    }
                    isDragging = false;
                    dragStartPoint = null;
                }
            }
        }

        private void AvailableActionsListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
                        Category = action.Category
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

        // Metodo per aggiungere azione con doppio click
        private void AvailableActionsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (selectedProfile == null)
            {
                MessageBox.Show("Seleziona prima un profilo dalla lista a sinistra.", "Nessun Profilo Selezionato", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var action = AvailableActionsListBox.SelectedItem as GameAction;
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
                    Category = action.Category
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
            AvailableActionsListBox.ItemsSource = null;
            AvailableActionsListBox.ItemsSource = extensionData.ActionLibrary;
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

        private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryFilterComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedCategory = selectedItem.Content.ToString();
                
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
            RefreshLog();
        }

        private void RefreshLog()
        {
            if (extensionData.ActionLog == null)
            {
                LogDataGrid.ItemsSource = null;
                return;
            }

            var logs = extensionData.ActionLog.AsEnumerable();

            if (ShowDryRunLogsCheckBox?.IsChecked == false)
            {
                logs = logs.Where(l => !l.IsDryRun);
            }

            LogDataGrid.ItemsSource = logs.OrderByDescending(l => l.Timestamp).ToList();
        }

        #endregion

        #region Settings

        private void ShowNotificationsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateNotificationSetting();
        }

        private void ShowNotificationsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateNotificationSetting();
        }

        private void UpdateNotificationSetting()
        {
            if (extensionData.Settings == null)
                extensionData.Settings = new ExtensionSettings();
            
            extensionData.Settings.ShowNotifications = ShowNotificationsCheckBox.IsChecked ?? true;
            plugin.UpdateExtensionData(extensionData);
        }

        private void EnableDryRunCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateDryRunSetting();
        }

        private void EnableDryRunCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateDryRunSetting();
        }

        private void UpdateDryRunSetting()
        {
            if (extensionData.Settings == null)
                extensionData.Settings = new ExtensionSettings();
            
            extensionData.Settings.EnableDryRun = EnableDryRunCheckBox.IsChecked ?? false;
            plugin.UpdateExtensionData(extensionData);
        }

        private void MaxLogEntriesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (extensionData.Settings == null)
                extensionData.Settings = new ExtensionSettings();
            
            if (int.TryParse(MaxLogEntriesTextBox.Text, out int maxEntries) && maxEntries > 0)
            {
                extensionData.Settings.MaxLogEntries = maxEntries;
                plugin.UpdateExtensionData(extensionData);
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
}
