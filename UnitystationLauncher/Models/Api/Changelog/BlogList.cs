using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UnitystationLauncher.Models.Api.Changelog;

[Serializable]
public class BlogList
{
    [JsonPropertyName("count")]
    public int? Count { get; set; }
    
    [JsonPropertyName("next")]
    public string? NextPage { get; set; }
    
    [JsonPropertyName("previous")]
    public string? PreviousPage { get; set; }

    [JsonPropertyName("results")] 
    public List<BlogPost> Posts { get; set; } = new();
}