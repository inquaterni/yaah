using System.Text.Json;
using NLog;
using NLog.Config;
using Yaah.Net.Structs;

namespace Yaah.Net.Rpc;

public class RpcEngine: IDisposable
{
    private const string BaseUrl = "https://aur.archlinux.org/rpc/v5/";
    private readonly HttpClient _client;
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
    /// <summary>
    ///  Helper for constructing url params for multi info search
    /// </summary>
    /// <param name="args">Arguments to be encoded</param>
    /// <returns>Encoded string</returns>
    private static string ConstructUrlArgs(IEnumerable<string> args)
    {
        Logger.Debug($"Input args: {string.Join(", ", args)}");
        var result = args.Aggregate("", (current, arg) => current + "arg[]=" + arg + "&");
        Logger.Debug($"Encoded args: {result[..^1]}");
        return result[..^1];
    }

    //=================[Package Search]=================//
    /// <summary>
    /// Single-term search
    /// </summary>
    /// <param name="arg">Provide your search-term in the {arg} parameter.</param>
    /// <param name="by">The by parameter lets you define the field that is used in the search query. If not defined, name-desc is used. For name and name-desc a contains-like lookup is performed whereas all other fields require an exact value.</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Search for packages with a single term returning basic package information</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="JsonException"></exception>
    public async Task<SearchResult?> Search(string arg, string by = "name-desc", CancellationToken token = default)
    {
        Logger.Debug($"Sending GET request to URI: search/{arg}?by={by}");
        var response = await _client.GetAsync($"search/{arg}?by={by}", token);
        Logger.Debug($"Response status: {response.StatusCode} ({response.StatusCode:D})");
        response.EnsureSuccessStatusCode();
        return await JsonSerializer.DeserializeAsync<SearchResult>(await response.Content.ReadAsStreamAsync(token), cancellationToken: token);
    }

    /// <summary>
    ///  Package name search (starts-with)
    /// </summary>
    /// <param name="arg">Provide your search-term in the {arg} parameter.</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Returns a list of package-names starting with {arg} (max 20 results)</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="JsonException"></exception>
    public async Task<string[]?> Suggest(string arg, CancellationToken token = default)
    {
        Logger.Debug($"Sending GET request to URI: suggest/{arg}");
        var response = await _client.GetAsync($"suggest/{arg}", token);
        Logger.Debug($"Response status: {response.StatusCode} ({response.StatusCode:D})");
        response.EnsureSuccessStatusCode();
        return await JsonSerializer.DeserializeAsync<string[]>(await response.Content.ReadAsStreamAsync(token), cancellationToken: token);
    }

    /// <summary>
    /// Package base search (starts-with)
    /// </summary>
    /// <param name="arg">Provide your search-term in the {arg} parameter.</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Returns a list of package-base-names starting with {arg} (max 20 results)</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="JsonException"></exception>
    public async Task<string[]?> SuggestPkgbase(string arg, CancellationToken token = default)
    {
        Logger.Debug($"Sending GET request to URI: suggest-pkgbase/{arg}");
        var response = await _client.GetAsync($"suggest-pkgbase/{arg}", token);
        Logger.Debug($"Response status: {response.StatusCode} ({response.StatusCode:D})");
        response.EnsureSuccessStatusCode();
        return await JsonSerializer.DeserializeAsync<string[]>(await response.Content.ReadAsStreamAsync(token), cancellationToken: token);
    }
    
    //=================[Package Details]=================//
    /// <summary>
    /// Single package lookup
    /// </summary>
    /// <param name="arg">Provide a package name in the {arg} parameter.</param>
    /// <returns>Get detailed information for a single package</returns>
    /// <param name="token">Cancellation token</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="JsonException"></exception>
    public async Task<InfoResult?> Info(string arg, CancellationToken token = default)
    {
        Logger.Debug($"Sending GET request to URI: info/{arg}");
        var response = await _client.GetAsync($"info/{arg}", token);
        Logger.Debug($"Response status: {response.StatusCode} ({response.StatusCode:D})");
        response.EnsureSuccessStatusCode();
        return await JsonSerializer.DeserializeAsync<InfoResult>(await response.Content.ReadAsStreamAsync(token), cancellationToken: token);
    }

    /// <summary>
    /// Multi package lookup
    /// </summary>
    /// <param name="arg">Provide one or more package names in the {arg[]} parameter.</param>
    /// <returns>Get detailed information for multiple packages</returns>
    /// <param name="token">Cancellation token</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="JsonException"></exception>
    public async Task<InfoResult?> Info(IEnumerable<string> arg, CancellationToken token = default)
    {
        var encodedArgs = ConstructUrlArgs(arg);
        Logger.Debug($"Sending GET request to URI: info?{encodedArgs}");
        var response = await _client.GetAsync($"info?{encodedArgs}", token);
        Logger.Debug($"Response status: {response.StatusCode} ({response.StatusCode:D})");
        response.EnsureSuccessStatusCode();
        return await JsonSerializer.DeserializeAsync<InfoResult>(await response.Content.ReadAsStreamAsync(token), cancellationToken: token);
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