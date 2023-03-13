using System;
using System.Collections.Generic;

namespace UnitystationLauncher.Models.Api
{
    [Serializable]
    public class ServerList
    {
        public List<Server> Servers { get; set; } = new();
    }
}