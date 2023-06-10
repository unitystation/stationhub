using System;
using System.Text.Json.Serialization;

namespace UnitystationLauncher.Models;

[Serializable]
public class Installation
{
    public Guid InstallationId { get; set; }
    public string? ForkName { get; set; }
    public int BuildVersion { get; set; }

    public string? InstallationPath { get; set; }

    public DateTime LastPlayedDate { get; set; }

    [JsonIgnore]
    public bool RecentlyUsed => LastPlayedDate > DateTime.Now.AddDays(-7);
}
