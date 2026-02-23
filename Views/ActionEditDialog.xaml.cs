using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using AutomationProfileManager.Models;
using AutomationProfileManager.Services;
using Microsoft.Win32;

namespace AutomationProfileManager.Views
{
    public partial class ActionEditDialog : Window
    {
        private GameAction action;
        private ApplicationDiscoveryService appDiscovery;
        private List<string> existingCategories = new List<string>();

        public ActionEditDialog()
        {
            InitializeComponent();
            action = new GameAction();
            appDiscovery = new ApplicationDiscoveryService();
            ActionTypeComboBox.SelectedIndex = 0;
            InitializeCategories();
            UpdateUIForActionType();
        }

        public ActionEditDialog(GameAction? existingAction) : this()
        {
            action = existingAction ?? new GameAction();
            LoadAction();
        }

        public ActionEditDialog(GameAction? existingAction, List<string>? categories) : this()
        {
            action = existingAction ?? new GameAction();
            existingCategories = categories ?? new List<string>();
            InitializeCategories();
            if (existingAction != null)
            {
                LoadAction();
            }
        }

        private void InitializeCategories()
        {
            var defaultCategories = new[] { 
                "Browser", 
                "Communication", 
                "Multimedia", 
                "Gaming", 
                "Cloud/Sync", 
                "Hardware/Overlay", 
                "System", 
                "Script", 
                "Streaming", 
                "Audio", 
                "Display", 
                "Utility",
                "Emulators",
                "Other"
            };
            var allCategories = defaultCategories.Concat(existingCategories)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();
            CategoryComboBox.ItemsSource = allCategories;
            
            // Seleziona la prima categoria o "Altro" se vuota
            if (allCategories.Count > 0)
            {
                CategoryComboBox.SelectedIndex = 0;
            }
        }

        private void LoadAction()
        {
            NameTextBox.Text = action.Name;
            ActionTypeComboBox.SelectedItem = action.ActionType;
            PathTextBox.Text = action.Path;
            ArgumentsTextBox.Text = action.Arguments;
            MirrorActionCheckBox.IsChecked = action.IsMirrorAction;
            PriorityTextBox.Text = action.Priority.ToString();
            WaitSecondsTextBox.Text = action.WaitSeconds.ToString();
            CategoryComboBox.Text = action.Category ?? LocalizationService.GetString("LOC_APM_DefaultCategory");
            TagsTextBox.Text = action.Tags != null ? string.Join(", ", action.Tags) : "";
            UpdateUIForActionType();
        }

        private void ActionTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateUIForActionType();
        }

        private void UpdateUIForActionType()
        {
            if (ActionTypeComboBox.SelectedItem is ActionType selectedType)
            {
                bool isWait = selectedType == ActionType.Wait;
                bool isAppAction = selectedType == ActionType.StartApp || selectedType == ActionType.CloseApp;
                bool isResolution = selectedType == ActionType.ChangeResolution;

                // Show/hide wait seconds panel
                if (WaitSecondsPanel != null)
                    WaitSecondsPanel.Visibility = isWait ? Visibility.Visible : Visibility.Collapsed;
                
                // Show/hide app selection buttons
                if (SelectInstalledAppButton != null)
                    SelectInstalledAppButton.Visibility = isAppAction ? Visibility.Visible : Visibility.Collapsed;
                if (SelectRunningProcessButton != null)
                    SelectRunningProcessButton.Visibility = isAppAction ? Visibility.Visible : Visibility.Collapsed;
                
                // Update path label
                if (PathLabel != null)
                {
                    if (selectedType == ActionType.StartApp)
                        PathLabel.Content = LocalizationService.GetString("LOC_APM_PathLabel_StartApp");
                    else if (selectedType == ActionType.CloseApp)
                        PathLabel.Content = LocalizationService.GetString("LOC_APM_PathLabel_CloseApp");
                    else if (selectedType == ActionType.PowerShellScript)
                        PathLabel.Content = LocalizationService.GetString("LOC_APM_PathLabel_PowerShell");
                    else if (selectedType == ActionType.SystemCommand)
                        PathLabel.Content = LocalizationService.GetString("LOC_APM_PathLabel_SystemCommand");
                    else if (selectedType == ActionType.ChangeResolution)
                        PathLabel.Content = LocalizationService.GetString("LOC_APM_PathLabel_Resolution");
                    else if (isWait)
                        PathLabel.Content = LocalizationService.GetString("LOC_APM_PathLabel_Wait");
                    else
                        PathLabel.Content = LocalizationService.GetString("LOC_APM_PathLabel_Default");
                }
                
                // Enable/disable fields for Wait and Resolution actions
                if (PathTextBox != null)
                    PathTextBox.IsEnabled = !isWait;
                if (ArgumentsTextBox != null)
                    ArgumentsTextBox.IsEnabled = !isWait && !isResolution;
                    
                // Show placeholder for resolution
                if (isResolution && PathTextBox != null)
                {
                    PathTextBox.Text = PathTextBox.Text == action.Path ? action.Path : "1920x1080@60";
                }
            }
        }

        private void SelectInstalledApp_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ApplicationSelectionDialog(
                appDiscovery.GetInstalledApplications(), 
                LocalizationService.GetString("LOC_APM_SelectInstalledAppPrompt"));
            dialog.Owner = this;
            
            if (dialog.ShowDialog() == true)
            {
                var selectedApp = dialog.GetSelectedApplication();
                if (selectedApp != null)
                {
                    PathTextBox.Text = selectedApp.ExecutablePath;
                    if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                    {
                        NameTextBox.Text = selectedApp.Name;
                    }
                }
            }
        }

        private void SelectRunningProcess_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ApplicationSelectionDialog(
                appDiscovery.GetRunningProcesses(), 
                LocalizationService.GetString("LOC_APM_SelectRunningProcessPrompt"));
            dialog.Owner = this;
            
            if (dialog.ShowDialog() == true)
            {
                var selectedApp = dialog.GetSelectedApplication();
                if (selectedApp != null)
                {
                    if (ActionTypeComboBox.SelectedItem is ActionType selectedType)
                    {
                        if (selectedType == ActionType.CloseApp)
                        {
                            PathTextBox.Text = selectedApp.ProcessName;
                        }
                        else
                        {
                            PathTextBox.Text = selectedApp.ExecutablePath;
                        }
                    }
                    
                    if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                    {
                        NameTextBox.Text = selectedApp.Name;
                    }
                }
            }
        }

        private void BrowsePath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Executables (*.exe)|*.exe|PowerShell Scripts (*.ps1)|*.ps1|Batch Files (*.bat;*.cmd)|*.bat;*.cmd|All Files (*.*)|*.*",
                Title = LocalizationService.GetString("LOC_APM_SelectFile")
            };

            if (dialog.ShowDialog() == true)
            {
                PathTextBox.Text = dialog.FileName;
                if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    NameTextBox.Text = Path.GetFileNameWithoutExtension(dialog.FileName);
                }
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show(
                    LocalizationService.GetString("LOC_APM_ActionNameRequired"),
                    LocalizationService.GetString("LOC_APM_Error"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            action.Name = NameTextBox.Text.Trim();
            action.ActionType = (ActionType)ActionTypeComboBox.SelectedItem;
            action.Path = PathTextBox.Text?.Trim() ?? "";
            action.Arguments = ArgumentsTextBox.Text?.Trim() ?? "";
            action.IsMirrorAction = MirrorActionCheckBox.IsChecked ?? false;
            action.Category = CategoryComboBox.Text?.Trim() ?? LocalizationService.GetString("LOC_APM_DefaultCategory");
            
            // Salva i tag
            action.Tags = (TagsTextBox.Text ?? "")
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();
            
            if (int.TryParse(PriorityTextBox.Text, out int priority))
            {
                action.Priority = priority;
            }

            if (action.ActionType == ActionType.Wait)
            {
                if (int.TryParse(WaitSecondsTextBox.Text, out int waitSeconds))
                {
                    action.WaitSeconds = Math.Max(1, waitSeconds);
                }
                else
                {
                    action.WaitSeconds = 1;
                }
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public GameAction GetAction()
        {
            return action;
        }
    }
}

