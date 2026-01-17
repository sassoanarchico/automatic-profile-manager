using System;
using System.Collections.Generic;

namespace AutomationProfileManager.Models
{
    public class ProfileMapping
    {
        public Dictionary<Guid, Guid> GameToProfile { get; set; } = new Dictionary<Guid, Guid>();
    }
}
