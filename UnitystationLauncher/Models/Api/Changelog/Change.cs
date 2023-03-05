using System;
using System.Text.Json.Serialization;

namespace UnitystationLauncher.Models.Api.Changelog;

[Serializable]
public class Change
{
    [JsonPropertyName("author_username")]
    public string? AuthorUsername { get; set; }

    [JsonPropertyName("author_url")]
    public string? AuthorUrl { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("pr_url")]
    public string? PrUrl { get; set; }

    [JsonPropertyName("pr_number")]
    public long PrNumber { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("build")]
    public string? Build { get; set; }

    [JsonPropertyName("date_added")]
    public DateTime DateAdded { get; set; }

}