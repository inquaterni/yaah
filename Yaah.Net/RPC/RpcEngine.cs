using System.Text.Json;
using NLog;
using Yaah.Net.Models;

namespace Yaah.Net.RPC;

public class RpcEngine : IDisposable
{
    private const string BaseUrl = "https://aur.archlinux.org/rpc/v5/";
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly HttpClient _client;

    public RpcEngine()
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri(BaseUrl);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~RpcEngine()
    {
        Dispose(false);
    }

    /// <summary>
    ///     Helper for constructing url params for multi info search
    /// </summary>
    /// <param name="args">Arguments to be encoded</param>
    /// <returns>Encoded string</returns>
    private static string ConstructUrlArgs(IEnumerable<string> args)
    {
        var values = args as string[] ?? args.ToArray();
        Logger.Debug($"Input args: {string.Join(", ", values)}");
        var result = values.Aggregate("", (current, arg) => current + "arg[]=" + arg + "&");
        Logger.Debug($"Encoded args: {result[..^1]}");
        return result[..^1];
    }

    /// <summary>
    ///     Single-term search
    /// </summary>
    /// <param name="arg">Provide your search-term in the \p arg parameter.</param>
    /// <param name="by">
    ///     The by parameter lets you define the field that is used in the search query. If not defined, name-desc
    ///     is used. For name and name-desc a contains-like lookup is performed whereas all other fields require an exact
    ///     value.
    /// </param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Search for packages with a single term returning basic package information</returns>
    public async Task<SearchResult?> Search(string arg, string by = "name-desc", CancellationToken token = default)
    {
        Logger.Debug($"Sending GET request to URI: search/{arg}?by={by}");
        var response = await _client.GetAsync($"search/{arg}?by={by}", token);
        Logger.Debug($"Response status: {response.StatusCode} ({response.StatusCode:D})");
        response.EnsureSuccessStatusCode();
        return await JsonSerializer.DeserializeAsync<SearchResult>(await response.Content.ReadAsStreamAsync(token),
            cancellationToken: token);
    }

    /// <summary>
    ///     Package name search (starts-with)
    /// </summary>
    /// <param name="arg">Provide your search-term in the \p arg parameter.</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Returns a list of package-names starting with \p arg (max 20 results)</returns>
    public async Task<string[]?> Suggest(string arg, CancellationToken token = default)
    {
        Logger.Debug($"Sending GET request to URI: suggest/{arg}");
        var response = await _client.GetAsync($"suggest/{arg}", token);
        Logger.Debug($"Response status: {response.StatusCode} ({response.StatusCode:D})");
        response.EnsureSuccessStatusCode();
        return await JsonSerializer.DeserializeAsync<string[]>(await response.Content.ReadAsStreamAsync(token),
            cancellationToken: token);
    }

    /// <summary>
    ///     Package base search (starts-with)
    /// </summary>
    /// <param name="arg">Provide your search-term in the \p arg parameter.</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Returns a list of package-base-names starting with \p arg (max 20 results)</returns>
    public async Task<string[]?> SuggestPkgbase(string arg, CancellationToken token = default)
    {
        Logger.Debug($"Sending GET request to URI: suggest-pkgbase/{arg}");
        var response = await _client.GetAsync($"suggest-pkgbase/{arg}", token);
        Logger.Debug($"Response status: {response.StatusCode} ({response.StatusCode:D})");
        response.EnsureSuccessStatusCode();
        return await JsonSerializer.DeserializeAsync<string[]>(await response.Content.ReadAsStreamAsync(token),
            cancellationToken: token);
    }

    /// <summary>
    ///     Single package lookup
    /// </summary>
    /// <param name="arg">Provide a package name in the \p arg parameter.</param>
    /// <returns>Get detailed information for a single package</returns>
    /// <param name="token">Cancellation token</param>
    public async Task<InfoResult?> Info(string arg, CancellationToken token = default)
    {
        Logger.Debug($"Sending GET request to URI: info/{arg}");
        var response = await _client.GetAsync($"info/{arg}", token);
        Logger.Debug($"Response status: {response.StatusCode} ({response.StatusCode:D})");
        response.EnsureSuccessStatusCode();
        return await JsonSerializer.DeserializeAsync<InfoResult>(await response.Content.ReadAsStreamAsync(token),
            cancellationToken: token);
    }

    /// <summary>
    ///     Multi package lookup
    /// </summary>
    /// <param name="arg">Provide one or more package names in the \p arg parameter.</param>
    /// <returns>Get detailed information for multiple packages</returns>
    /// <param name="token">Cancellation token</param>
    public async Task<InfoResult?> Info(IEnumerable<string> arg, CancellationToken token = default)
    {
        var encodedArgs = ConstructUrlArgs(arg);
        Logger.Debug($"Sending GET request to URI: info?{encodedArgs}");
        var response = await _client.GetAsync($"info?{encodedArgs}", token);
        Logger.Debug($"Response status: {response.StatusCode} ({response.StatusCode:D})");
        response.EnsureSuccessStatusCode();
        return await JsonSerializer.DeserializeAsync<InfoResult>(await response.Content.ReadAsStreamAsync(token),
            cancellationToken: token);
    }

    private void Dispose(bool disposing)
    {
        if (disposing) _client.Dispose();
    }
}