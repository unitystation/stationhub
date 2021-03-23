using System;

namespace UnitystationLauncher.Models
{
    [Serializable]
    public class Prefs
    {
        public bool AutoRemove { get; set; }
        public string? LastLogin { get; set; }
    }
}
