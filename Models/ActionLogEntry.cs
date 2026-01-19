using System;

namespace AutomationProfileManager.Models
{
    public class ActionLogEntry
    {
        public Guid ActionId { get; set; }
        public string ActionName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool Success { get; set; }
        public int ExitCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsDryRun { get; set; }
    }
}
