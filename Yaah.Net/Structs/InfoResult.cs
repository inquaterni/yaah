using System.Text.Json.Serialization;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Yaah.Net.Structs;

public sealed class InfoResult
{
    [JsonPropertyName("resultcount")]
    public int ResultCount { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("version")]
    public int Version { get; set; }
    [JsonPropertyName("results")]
    public DetailedPkgInfo[] Results { get; set; }
}