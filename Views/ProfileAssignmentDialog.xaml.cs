using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AutomationProfileManager.Models;

namespace AutomationProfileManager.Views
{
    public partial class ProfileAssignmentDialog : Window
    {
        private List<AutomationProfile> profiles;
        private Guid? selectedProfileId;

        public ProfileAssignmentDialog(List<AutomationProfile> availableProfiles, string gameName)
        {
            InitializeComponent();
            profiles = availableProfiles;
            GameNameTextBlock.Text = $"Seleziona un profilo per:\n{gameName}";
            ProfilesListBox.ItemsSource = profiles;
            ProfilesListBox.SelectionChanged += ProfilesListBox_SelectionChanged;
        }

        private void ProfilesListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ProfilesListBox.SelectedItem is AutomationProfile profile)
            {
                selectedProfileId = profile.Id;
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void RemoveProfile_Click(object sender, RoutedEventArgs e)
        {
            selectedProfileId = null;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public Guid? GetSelectedProfileId()
        {
            return selectedProfileId;
        }
    }
}
