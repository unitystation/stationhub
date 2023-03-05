using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UnitystationLauncher.Models.Api.Changelog;

[Serializable]
public class GameVersion
{
    [JsonPropertyName("version_number")]
    public string? VersionNumber { get; set; }

    [JsonPropertyName("date_created")]
    public DateTime DateCreated { get; set; }

    [JsonPropertyName("changes")]
    public List<Change>? Changes { get; set; }
}