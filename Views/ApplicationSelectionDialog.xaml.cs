using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AutomationProfileManager.Services;

namespace AutomationProfileManager.Views
{
    public partial class ApplicationSelectionDialog : Window
    {
        private List<InstalledApplication> allApplications;
        private List<InstalledApplication> filteredApplications;

        public ApplicationSelectionDialog(List<InstalledApplication> apps, string prompt)
        {
            InitializeComponent();
            allApplications = apps ?? new List<InstalledApplication>();
            filteredApplications = allApplications;
            PromptTextBlock.Text = prompt;
            ApplicationsDataGrid.ItemsSource = filteredApplications;
            ApplicationsDataGrid.MouseDoubleClick += ApplicationsDataGrid_MouseDoubleClick;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text?.ToLowerInvariant() ?? "";

            if (string.IsNullOrWhiteSpace(searchText))
            {
                filteredApplications = allApplications;
            }
            else
            {
                filteredApplications = allApplications
                    .Where(a =>
                        (a.Name?.ToLowerInvariant().Contains(searchText) ?? false) ||
                        (a.ProcessName?.ToLowerInvariant().Contains(searchText) ?? false) ||
                        (a.ExecutablePath?.ToLowerInvariant().Contains(searchText) ?? false))
                    .ToList();
            }

            ApplicationsDataGrid.ItemsSource = filteredApplications;
        }

        private void ApplicationsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ApplicationsDataGrid.SelectedItem != null)
            {
                OK_Click(sender, e);
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public InstalledApplication? GetSelectedApplication()
        {
            return ApplicationsDataGrid.SelectedItem as InstalledApplication;
        }
    }
}
