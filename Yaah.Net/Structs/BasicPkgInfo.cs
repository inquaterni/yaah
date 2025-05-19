using System.Text.Json.Serialization;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Yaah.Net.Structs;

public sealed class BasicPkgInfo
{
    [JsonPropertyName("ID")]
    public int Id { get; set; }
    [JsonPropertyName("Name")]
    public string Name { get; set; }
    [JsonPropertyName("Description")]
    public string Description { get; set; }
    [JsonPropertyName("PackageBaseID")]
    public int PackageBaseId { get; set; }
    [JsonPropertyName("PackageBase")]
    public string PackageBase { get; set; }
    [JsonPropertyName("Maintainer")]
    public string Maintainer { get; set; }
    [JsonPropertyName("NumVotes")]
    public int NumVotes { get; set; }
    [JsonPropertyName("Popularity")]
    public double Popularity { get; set; }
    [JsonPropertyName("FirstSubmitted")]
    public ulong FirstSubmitted { get; set; }
    [JsonPropertyName("LastModified")]
    public ulong LastModified { get; set; }
    [JsonPropertyName("OutOfDate")]
    public long? OutOfDate { get; set; }
    [JsonPropertyName("Version")]
    public string Version { get; set; }
    [JsonPropertyName("URLPath")]
    public string UrlPath { get; set; }
    [JsonPropertyName("URL")]
    public string Url { get; set; }
}