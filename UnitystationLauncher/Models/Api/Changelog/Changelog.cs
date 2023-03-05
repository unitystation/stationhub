using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UnitystationLauncher.Models.Api.Changelog;

[Serializable]
public class Changelog
{
    [JsonPropertyName("count")]
    public long Count { get; set; }

    [JsonPropertyName("next")]
    public string? Next { get; set; }

    [JsonPropertyName("previous")]
    public string? Previous { get; set; }

    [JsonPropertyName("results")]
    public List<GameVersion>? Results { get; set; }
}