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
        public int WaitSeconds { get; set; }
        public string Category { get; set; } = "Generale";

        // Condizioni per l'esecuzione
        public ActionCondition? Condition { get; set; }

        // Esecuzione parallela e timeout
        public bool RunInParallel { get; set; } = false;
        public bool WaitForCompletion { get; set; } = true;
        public int TimeoutSeconds { get; set; } = 60;

        // Dipendenze
        public Guid? DependsOnActionId { get; set; }
        public bool RequiresPreviousSuccess { get; set; } = false;
    }

    public class ActionCondition
    {
        public ConditionType Type { get; set; } = ConditionType.None;
        public string Value { get; set; } = string.Empty;
        public string TimeStart { get; set; } = string.Empty;
        public string TimeEnd { get; set; } = string.Empty;
    }

    public enum ConditionType
    {
        None,
        ProcessRunning,
        ProcessNotRunning,
        FileExists,
        FileNotExists,
        TimeRange
    }
}
