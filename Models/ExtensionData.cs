using System.Collections.Generic;

namespace AutomationProfileManager.Models
{
    public class ExtensionData
    {
        public List<GameAction> ActionLibrary { get; set; } = new List<GameAction>();
        public List<AutomationProfile> Profiles { get; set; } = new List<AutomationProfile>();
        public ProfileMapping Mappings { get; set; } = new ProfileMapping();
    }
}
