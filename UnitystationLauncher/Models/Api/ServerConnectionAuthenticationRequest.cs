using System.Text.Json.Serialization;

namespace UnitystationLauncher.Models.Api;

public class ServerConnectionAuthenticationRequest
{
    [JsonPropertyName("shared_secret")]
    public string SharedSecret { get; set; } = string.Empty;

    [JsonPropertyName("unique_identifier")]
    public string UniqueIdentifier { get; set; } = string.Empty;
    
    [JsonPropertyName("auth_realm")]
    public string? AuthRealm { get; set; }
}