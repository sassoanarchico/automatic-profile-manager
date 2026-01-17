using System;
using System.Collections.Generic;
using System.Diagnostics;
using AutomationProfileManager.Models;

namespace AutomationProfileManager.Services
{
    public class MirrorActionTracker
    {
        private readonly Dictionary<Guid, bool> actionStates = new Dictionary<Guid, bool>();

        public void TrackActionBeforeExecution(GameAction action)
        {
            if (action.ActionType == ActionType.CloseApp && action.IsMirrorAction)
            {
                var processName = System.IO.Path.GetFileNameWithoutExtension(action.Path);
                var isRunning = Process.GetProcessesByName(processName).Length > 0;
                actionStates[action.Id] = isRunning;
            }
        }

        public bool ShouldRestoreAction(GameAction action)
        {
            if (action.ActionType == ActionType.CloseApp && action.IsMirrorAction)
            {
                return actionStates.TryGetValue(action.Id, out var wasRunning) && wasRunning;
            }
            return false;
        }

        public void ClearTracking()
        {
            actionStates.Clear();
        }
    }
}
