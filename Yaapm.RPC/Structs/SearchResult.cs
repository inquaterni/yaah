using System.Text.Json.Serialization;

namespace Yaapm.RPC.Structs;

public sealed class SearchResult
{
    [JsonPropertyName("resultcount")]
    public uint ResultCount { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("version")]
    public int Version { get; set; }
    [JsonPropertyName("results")]
    public BasicPkgInfo[] Results { get; set; }
}