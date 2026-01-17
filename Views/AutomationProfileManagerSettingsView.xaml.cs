using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutomationProfileManager.Models;
using AutomationProfileManager.Services;

namespace AutomationProfileManager.Views
{
    public partial class AutomationProfileManagerSettingsView : UserControl
    {
        private readonly AutomationProfileManagerPlugin plugin;
        private ExtensionData extensionData;
        private AutomationProfile? selectedProfile;
        private GameAction? selectedAction;
        
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
            extensionData = plugin.GetExtensionData() ?? new ExtensionData();
        }

        private void InitializeUI()
        {
            ActionsDataGrid.ItemsSource = extensionData.ActionLibrary;
            ProfilesListBox.ItemsSource = extensionData.Profiles;
            AvailableActionsListBox.ItemsSource = extensionData.ActionLibrary;
        }

        private void ActionsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedAction = ActionsDataGrid.SelectedItem as GameAction;
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

            // Inizia il drag solo se il mouse si è mosso abbastanza
            if (Math.Abs(diff.X) > DragThreshold || Math.Abs(diff.Y) > DragThreshold)
            {
                if (!isDragging && selectedProfile != null && AvailableActionsListBox.SelectedItem != null)
                {
                    isDragging = true;
                    var action = AvailableActionsListBox.SelectedItem as GameAction;
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
                var action = e.Data.GetData("GameActionData") as GameAction;
                if (action != null)
                {
                    // Aggiunta nuova azione dalla libreria
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
                        WaitSeconds = action.WaitSeconds
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
            if (button?.DataContext is GameAction action)
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
            if (button?.DataContext is GameAction action)
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
            if (button?.DataContext is GameAction action)
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
                    WaitSeconds = action.WaitSeconds
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
    }
}
