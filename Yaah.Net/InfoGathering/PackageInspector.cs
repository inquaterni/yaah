using System.Collections;
using System.Collections.Concurrent;
using NLog;
using Yaah.Net.Rpc;
using Yaah.Net.Structs;
using Yaah.Infrastructure;
using Yaah.Infrastructure.Database;

namespace Yaah.Net.InfoGathering;

public class PackageInspector
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly IntPtr _db;
    private readonly RpcEngine _engine = new();

    // ReSharper disable once ConvertToPrimaryConstructor
    public PackageInspector(IntPtr db)
    {
        _db = db;
    }

    /// <summary>
    ///     Gather and process nullable array of strings (packages)
    /// </summary>
    /// <param name="data"></param>
    /// <returns>Array of information about packages</returns>
    private async Task<DetailedPkgInfo[]> InfoNullable(string[]? data)
    {
        Logger.Debug($"Data: {string.Join(", ", data ?? ["NULL"])}");
        if (data == null || data.Length == 0) return [];
        var processedData = ProcessData(data);
        Logger.Debug($"Processed data: {string.Join(", ", !processedData.IsEmpty ? processedData : ["EMPTY"])}");
        if (processedData.IsEmpty) return [];

        var response = await _engine.Info(processedData);
        if (response == null || response.ResultCount == 0) return [];

        return response.Results;
    }

    /// <summary>
    ///     Version comparison
    /// </summary>
    /// <param name="pkg">Package info</param>
    /// <param name="op">Condition operator</param>
    /// <param name="version">Version string</param>
    /// <returns>Pair of booleans first indicating if package passed the needed condition, second if it is required to download</returns>
    private (bool pass, bool toDownload) VersionCmp(DetailedPkgInfo pkg, string op, string version)
    {
        int databaseResult;
        try
        {
            Logger.Debug("Getting package information from DB");
            var pkgPtr = DatabaseController.GetPackage(_db, pkg.Name);
            var pkgVersion = DatabaseController.GetPackageVersion(pkgPtr);
            Logger.Debug("Compairing requred version & DB version");
            databaseResult = VersionController.VersionComparison(version, pkgVersion);
            Logger.Debug($"Package version: {pkgVersion}, cmp result: {databaseResult}");
        }
        catch (Exception e)
        {
            Logger.Warn($"Getting package information from DB failed: {e}, cmp result: -2");
            databaseResult = -2;
        }

        Logger.Debug("Compairing requred version & remote version");
        var remoteResult = VersionController.VersionComparison(version, pkg.Version);
        Logger.Debug($"Package version: {pkg.Version}, cmp result: {remoteResult}");
        return op switch
        {
            ">=" => (remoteResult is -1 or 0 || databaseResult is -1 or 0, databaseResult is not (-1 or 0)),
            "<=" => (remoteResult is 1 or 0 || databaseResult is 1 or 0, databaseResult is not (1 or 0)),
            "=" => (remoteResult is 0 || databaseResult is 0, databaseResult is not 0),
            ">" => (remoteResult is -1 || databaseResult is -1, databaseResult is not -1),
            "<" => (remoteResult is 1 || databaseResult is 1, databaseResult is not 1),
            _ => (false, false)
        };
    }

    /// <summary>
    ///     Match and check for version conditions
    /// </summary>
    /// <param name="data">Packages (with conditions inside e.g. 'python>=3.9' or 'python')</param>
    /// <returns>Bag of packages to download</returns>
    private ConcurrentBag<string> ProcessData(IEnumerable<string> data)
    {
        var result = new ConcurrentBag<string>();
        Parallel.ForEach(data, item =>
        {
            Logger.Debug("Matching version conditions");
            var match = VersionController.GetVersionMatch(item);
            if (match == null)
            {
                Logger.Debug($"No version conditions match '{item}'");
                result.Add(item);
                return;
            }

            Logger.Debug(
                $"Found version conditions match '{item}' (name: '{match.Groups["name"]}', op: {match.Groups["op"]}, version: {match.Groups["version"]})");

            Logger.Debug($"Fetching '{match.Groups["name"]}' package information");
            var infoResult = _engine.Info(match.Groups["name"].Value).Result;

            if (infoResult == null || infoResult.ResultCount == 0)
            {
                Logger.Warn($"No package information match '{item}'");
                return;
            }

            var (pass, toDownload) = VersionCmp(infoResult.Results[0], match.Groups["op"].Value,
                match.Groups["version"].Value);
            if (pass)
            {
                Logger.Debug("Version comparison passed");
                if (!toDownload)
                {
                    Logger.Debug($"Package '{infoResult.Results[0].Name}' is not required to download");
                    return;
                }

                Logger.Debug($"Adding '{infoResult.Results[0].Name}' to download bag'");
                result.Add(infoResult.Results[0].Name);
            }
            else
            {
                Logger.Fatal(
                    $"Version comparison failed: (required: {match.Groups["version"]}, got: {infoResult.Results[0].Version})");
                Console.WriteLine("Cannot resolve dependencies, manual intervention is required");
                Environment.Exit(4);
            }
        });
        return result;
    }

    /// <summary>
    ///     Helper to add range of DetailedPkgInfo to the stack
    /// </summary>
    private static void AddRange(IEnumerable<DetailedPkgInfo> range, Stack<DetailedPkgInfo> stack)
    {
        foreach (var item in range)
        {
            if (stack.Contains(item)) continue;
            stack.Push(item);
        }
    }

    /// <summary>
    ///     Gather package information
    /// </summary>
    /// <param name="aurExplicit">Explicit AUR package name</param>
    public async Task<Hashtable> GatherPackageInfo(string aurExplicit)
    {
        var packageInfo = await _engine.Info(aurExplicit);
        var result = new Hashtable();

        if (packageInfo == null || packageInfo.ResultCount == 0)
        {
            Logger.Error($"No AUR package '{aurExplicit}'.");
            return result;
        }

        Stack<DetailedPkgInfo> stack = new();
        stack.Push(packageInfo.Results[0]);

        while (stack.Count > 0)
            try
            {
                var current = stack.Pop();
                Logger.Debug($"Processing '{current.Name}'");

                if (result.ContainsKey(current.Name))
                {
                    Logger.Warn($"Package '{current.Name}' already in table");
                    continue;
                }

                result[current.Name] = current;

                var depends = await InfoNullable(current.Depends);
                var makeDepends = await InfoNullable(current.MakeDepends);
                var checkDepends = await InfoNullable(current.CheckDepends);

                AddRange(depends, stack);
                AddRange(makeDepends, stack);
                AddRange(checkDepends, stack);
            }
            catch (Exception e) when (e is NullReferenceException)
            {
                Logger.Error($"{e.GetType().Name} was thrown while gathering package information ({e.Message}).");
            }

        return result;
    }

    /// <summary>
    ///     Gather packages information
    /// </summary>
    /// <param name="aurExplicit">Explicit AUR packages names</param>
    public async Task<Hashtable> GatherPackageInfo(IEnumerable<string> aurExplicit)
    {
        var values = aurExplicit as string[] ?? aurExplicit.ToArray();
        var packageInfo = await _engine.Info(values);
        var result = new Hashtable();


        if (packageInfo == null || packageInfo.ResultCount == 0)
        {
            Logger.Error($"No packages for [{string.Join(", ", values)}].");
            return result;
        }

        Stack<DetailedPkgInfo> stack = new();
        AddRange(packageInfo.Results, stack);
        while (stack.Count > 0)
            try
            {
                var current = stack.Pop();
                Logger.Debug($"Processing '{current.Name}'");

                if (result.ContainsKey(current.Name))
                {
                    Logger.Warn($"Package '{current.Name}' already in table");
                    continue;
                }

                result[current.Name] = current;

                var depends = await InfoNullable(current.Depends);
                var makeDepends = await InfoNullable(current.MakeDepends);
                var checkDepends = await InfoNullable(current.CheckDepends);

                AddRange(depends, stack);
                AddRange(makeDepends, stack);
                AddRange(checkDepends, stack);
            }
            catch (Exception e) when (e is NullReferenceException)
            {
                Logger.Error($"{e.GetType().Name} was thrown while gathering package information ({e.Message}).");
            }

        return result;
    }
}