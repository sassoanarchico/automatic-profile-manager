using System;
using System.Collections.Generic;

namespace AutomationProfileManager.Models
{
    public class AutomationProfile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public List<GameAction> Actions { get; set; } = new List<GameAction>();
    }
}
