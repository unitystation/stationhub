using System.Text.Json.Serialization;

namespace UnitystationLauncher.Models.Api;

public class VersionModel
{
    [JsonPropertyName("version")]
    public required string Version { get; set; }
}