using System.Text.Json;
using Yaapm.Net.Structs;

namespace Yaapm.Net.Rpc;

public class RpcEngine: IDisposable
{
    private readonly HttpClient _client;
    
    private const string BaseUrl = "https://aur.archlinux.org/rpc/v5/";

    public RpcEngine()
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri(BaseUrl);
    }

    ~RpcEngine()
    {
        Dispose(false);
    }
    
    //=================[Helpers]=================//
    private static string ConstructUrlArgs(string[] args)
    {
        var result = args.Aggregate("", (current, arg) => current + "arg[]=" + arg + "&");
        return result[..^1];
    }

    //=================[Package Search]=================//
    
    /// <summary>
    /// Single-term search
    /// </summary>
    /// <param name="arg">Provide your search-term in the {arg} parameter.</param>
    /// <param name="by">The by parameter let's you define the field that is used in the search query. If not defined, name-desc is used. For name and name-desc a contains-like lookup is performed whereas all other fields require an exact value.</param>
    /// <returns>Search for packages with a single term returning basic package information</returns>
    public async Task<SearchResult?> Search(string arg, string by = "name-desc")
    {
        var response = await _client.GetAsync($"search/{arg}?by={by}");
        response.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<SearchResult>(await response.Content.ReadAsStreamAsync());
    }

    /// <summary>
    ///  Package name search (starts-with)
    /// </summary>
    /// <param name="arg">Provide your search-term in the {arg} parameter.</param>
    /// <returns>Returns a list of package-names starting with {arg} (max 20 results)</returns>
    public async Task<string[]?> Suggest(string arg)
    {
        var response = await _client.GetAsync($"suggest/{arg}");
        response.EnsureSuccessStatusCode();
        return await JsonSerializer.DeserializeAsync<string[]>(await response.Content.ReadAsStreamAsync());
    }

    /// <summary>
    /// Package base search (starts-with)
    /// </summary>
    /// <param name="arg">Provide your search-term in the {arg} parameter.</param>
    /// <returns>Returns a list of package-base-names starting with {arg} (max 20 results)</returns>
    public async Task<string[]?> SuggestPkgbase(string arg)
    {
        var response = await _client.GetAsync($"suggest-pkgbase/{arg}");
        response.EnsureSuccessStatusCode();
        return await JsonSerializer.DeserializeAsync<string[]>(await response.Content.ReadAsStreamAsync());
    }
    
    //=================[Package Details]=================//

    /// <summary>
    /// Single package lookup
    /// </summary>
    /// <param name="arg">Provide a package name in the {arg} parameter.</param>
    /// <returns>Get detailed information for a single package</returns>
    public async Task<InfoResult?> Info(string arg)
    {
        var response = await _client.GetAsync($"info/{arg}");
        response.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<InfoResult>(await response.Content.ReadAsStreamAsync());
    }

    /// <summary>
    /// Multi package lookup
    /// </summary>
    /// <param name="arg">Provide one or more package names in the {arg[]} parameter.</param>
    /// <returns>Get detailed information for multiple packages</returns>
    public async Task<InfoResult?> Info(string[] arg)
    {
        var response = await _client.GetAsync($"info?{ConstructUrlArgs(arg)}");
        response.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<InfoResult>(await response.Content.ReadAsStreamAsync());
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _client.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}