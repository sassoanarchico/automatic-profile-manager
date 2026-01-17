using System;

namespace AutomationProfileManager.Models
{
    public class GameAction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public ActionType ActionType { get; set; }
        public string Path { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty;
        public ExecutionPhase ExecutionPhase { get; set; }
        public bool IsMirrorAction { get; set; }
        public int Priority { get; set; }
        public int WaitSeconds { get; set; } // For Wait action type
    }
}
