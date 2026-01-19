using System;
using System.Collections.Generic;

namespace AutomationProfileManager.Models
{
    public class ExtensionSettings
    {
        public bool ShowNotifications { get; set; } = true;
        public bool LogActionsToFile { get; set; } = false;
        public int MaxLogEntries { get; set; } = 100;
        public bool EnableDryRun { get; set; } = false;

        // Backup
        public bool AutoBackupEnabled { get; set; } = true;
        public int BackupIntervalDays { get; set; } = 7;
        public int MaxBackupCount { get; set; } = 5;
        public DateTime LastBackupDate { get; set; } = DateTime.MinValue;

        // Wizard
        public bool WizardCompleted { get; set; } = false;

        // Hotkeys
        public Dictionary<Guid, string> ProfileHotkeys { get; set; } = new Dictionary<Guid, string>();
    }

    public class ActionStatistics
    {
        public Guid ActionId { get; set; }
        public string ActionName { get; set; } = string.Empty;
        public int ExecutionCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public double TotalExecutionTimeMs { get; set; }
        public DateTime FirstExecution { get; set; }
        public DateTime LastExecution { get; set; }
    }

    public class ProfileStatistics
    {
        public Guid ProfileId { get; set; }
        public string ProfileName { get; set; } = string.Empty;
        public int ExecutionCount { get; set; }
        public double TotalTimeSavedSeconds { get; set; }
        public DateTime LastUsed { get; set; }
    }
}
