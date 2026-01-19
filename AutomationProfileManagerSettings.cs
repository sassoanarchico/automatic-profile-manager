using System.Collections.Generic;
using Playnite.SDK;

namespace AutomationProfileManager
{
    public class AutomationProfileManagerSettings : ISettings
    {
        private AutomationProfileManagerPlugin? plugin;

        public AutomationProfileManagerSettings()
        {
        }

        public AutomationProfileManagerSettings(AutomationProfileManagerPlugin plugin)
        {
            this.plugin = plugin;
        }

        public void BeginEdit() { }
        public void CancelEdit() { }
        public void EndEdit() { }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }
}
