using System.Text.Json.Serialization;

namespace Yaapm.RPC.Structs;

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