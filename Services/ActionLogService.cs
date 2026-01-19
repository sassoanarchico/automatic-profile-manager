using System;
using System.Collections.Generic;
using System.Linq;
using AutomationProfileManager.Models;

namespace AutomationProfileManager.Services
{
    public class ActionLogService
    {
        private readonly List<ActionLogEntry> logEntries;
        private readonly int maxEntries;

        public ActionLogService(List<ActionLogEntry> entries, int maxEntries = 100)
        {
            logEntries = entries ?? new List<ActionLogEntry>();
            this.maxEntries = maxEntries;
        }

        public void Log(GameAction action, bool success, int exitCode, string message, bool isDryRun = false)
        {
            var entry = new ActionLogEntry
            {
                ActionId = action.Id,
                ActionName = action.Name,
                Timestamp = DateTime.Now,
                Success = success,
                ExitCode = exitCode,
                Message = message,
                IsDryRun = isDryRun
            };

            logEntries.Insert(0, entry);

            while (logEntries.Count > maxEntries)
            {
                logEntries.RemoveAt(logEntries.Count - 1);
            }
        }

        public List<ActionLogEntry> GetRecentLogs(int count = 50)
        {
            return logEntries.Take(count).ToList();
        }

        public ActionLogEntry? GetLastLogForAction(Guid actionId)
        {
            return logEntries.FirstOrDefault(e => e.ActionId == actionId);
        }

        public void Clear()
        {
            logEntries.Clear();
        }
    }
}
