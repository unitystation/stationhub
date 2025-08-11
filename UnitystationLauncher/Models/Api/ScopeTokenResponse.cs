using System.Text.Json.Serialization;

namespace UnitystationLauncher.Models.Api;

public class ScopeTokenResponse : JsonObject
{
    [JsonPropertyName("scope_token")]
    public string ScopeToken { get; set; } = string.Empty;
}