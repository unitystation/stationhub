using System;
using System.Text.Json.Serialization;

namespace UnitystationLauncher.Models.Api.Changelog;

[Serializable]
public class BlogSection
{
    [JsonPropertyName("heading")]
    public string? Heading { get; set; }

    [JsonPropertyName("body")]
    public string? Body { get; set; }
}