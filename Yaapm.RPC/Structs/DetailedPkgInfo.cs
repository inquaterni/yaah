using System.Text.Json.Serialization;

namespace Yaapm.RPC.Structs;

public sealed class DetailedPkgInfo
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
    // [JsonPropertyName("OutOfDate")]
    // public string? OutOfDate { get; set; }
    [JsonPropertyName("Version")]
    public string Version { get; set; }
    [JsonPropertyName("URLPath")]
    public string UrlPath { get; set; }
    [JsonPropertyName("URL")]
    public string Url { get; set; }
    [JsonPropertyName("Submitter")]
    public string Submitter { get; set; }
    [JsonPropertyName("Licence")]
    public string[] Licence { get; set; }
    [JsonPropertyName("Depends")]
    public string[] Depends { get; set; }
    [JsonPropertyName("MakeDepends")]
    public string[] MakeDepends { get; set; }
    [JsonPropertyName("OptDepends")]
    public string[] OptDepends { get; set; }
    [JsonPropertyName("CheckDepends")]
    public string[] CheckDepends { get; set; }
    [JsonPropertyName("Provides")]
    public string[] Provides { get; set; }
    [JsonPropertyName("Conflicts")]
    public string[] Conflicts { get; set; }
    [JsonPropertyName("Replaces")]
    public string[] Replaces { get; set; }
    [JsonPropertyName("Groups")]
    public string[] Groups { get; set; }
    [JsonPropertyName("Keywords")]
    public string[] Keywords { get; set; }
    [JsonPropertyName("CoMaintainers")]
    public string[] CoMaintainers { get; set; }
}