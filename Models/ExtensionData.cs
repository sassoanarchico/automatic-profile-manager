using System.Collections.Generic;

namespace AutomationProfileManager.Models
{
    public class ExtensionData
    {
        public List<GameAction> ActionLibrary { get; set; } = new List<GameAction>();
        public List<AutomationProfile> Profiles { get; set; } = new List<AutomationProfile>();
        public ProfileMapping Mappings { get; set; } = new ProfileMapping();
        public ExtensionSettings Settings { get; set; } = new ExtensionSettings();
        public List<ActionLogEntry> ActionLog { get; set; } = new List<ActionLogEntry>();
        public List<ActionStatistics> Statistics { get; set; } = new List<ActionStatistics>();
        public List<ProfileStatistics> ProfileStats { get; set; } = new List<ProfileStatistics>();
    }
}
