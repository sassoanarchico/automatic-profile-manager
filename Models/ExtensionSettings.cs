namespace AutomationProfileManager.Models
{
    public class ExtensionSettings
    {
        public bool ShowNotifications { get; set; } = true;
        public bool LogActionsToFile { get; set; } = false;
        public int MaxLogEntries { get; set; } = 100;
    }
}
