using System.Collections;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Yaapm.Net.Rpc;
using Yaapm.Net.Structs;
using Yaapm.System;
using Yaapm.System.Database;

namespace Yaapm.Net.InfoGathering;

public class PackageInspector(IntPtr db)
{
    private readonly RpcEngine _engine = new();

    /// <summary>
    ///  Gather and process nullable array of strings (packages)
    /// </summary>
    /// <param name="data"></param>
    /// <returns>Array of information about packages</returns>
    private async Task<DetailedPkgInfo[]> InfoNullable(string[]? data)
    {
        if (data == null || data.Length == 0) return [];
        var processedData = ProcessData(data);
        if (processedData.IsEmpty) return [];
        
        var response = await _engine.Info(processedData);
        if (response == null || response.ResultCount == 0) return [];
        
        return response.Results;
    }
    
    /// <summary>
    /// Version compare 
    /// </summary>
    /// <param name="pkg">Package info</param>
    /// <param name="op">Condition operator</param>
    /// <param name="version">Version string</param>
    /// <returns>Pair of booleans</returns>
    private (bool pass, bool toDownload) VersionCmp(DetailedPkgInfo pkg, string op, string version)
    {
        int databaseResult;
        try
        {
            var pkgPtr = DatabaseController.GetPackage(db, pkg.Name);
            var pkgName = DatabaseController.GetPackageName(pkgPtr);
            databaseResult = DatabaseController.VersionCompare(version, pkgName);
        }
        catch (Exception)
        {
            databaseResult = -2;
        }

        var remoteResult = DatabaseController.VersionCompare(version, pkg.Version);
        return op switch
        {
                ">=" => (remoteResult is -1 or 0 || databaseResult is -1 or 0, databaseResult is not (-1 or 0)),
            "<=" => (remoteResult is 1 or 0 || databaseResult is 1 or 0, databaseResult is not (1 or 0)),
            "="  => (remoteResult is 0 || databaseResult is 0, databaseResult is not 0),
            ">"  => (remoteResult is -1 || databaseResult is -1, databaseResult is not -1),
            "<"  => (remoteResult is 1 || databaseResult is 1, databaseResult is not 1),
            _ => (false, false)
        };
    }

    /// <summary>
    /// Match and check for version conditions
    /// </summary>
    /// <param name="data">Packages (with conditions inside e.g. 'python>=3.9' or 'python')</param>
    /// <returns>Bag of packages to download</returns>
    private ConcurrentBag<string> ProcessData(IEnumerable<string> data)
    {
        var result = new ConcurrentBag<string>();
        Parallel.ForEach(data, item =>
        {

            var match = VersionController.GetVersionMatch(item);
            if (match == null)
            {
                result.Add(item);
                return;
            }
            
            var infoResult = _engine.Info(match.Groups["name"].Value).Result;
            if (infoResult == null || infoResult.ResultCount == 0) return;

            var (pass, toDownload) = VersionCmp(infoResult.Results[0], match.Groups["op"].Value, match.Groups["version"].Value);
            if (pass)
            {
                if (toDownload)
                {
                    result.Add(infoResult.Results[0].Name);
                }
            }
            else
            {
                Console.WriteLine("Cannot resolve dependencies, manual intervention is required");
                Environment.Exit(4);
            }
        });
        return result;
    }

    /// <summary>
    /// Helper to add range of DetailedPkgInfo to the stack
    /// </summary>
    /// <param name="range"></param>
    /// <param name="stack"></param>
    private static void AddRange(IEnumerable<DetailedPkgInfo> range, Stack<DetailedPkgInfo> stack)
    {
        Parallel.ForEach(range, item =>
        {
            if (stack.Contains(item)) return;
            stack.Push(item);
        });
    }

    /// <summary>
    /// Gather package information
    /// </summary>
    /// <param name="packageName"></param>
    /// <returns></returns>
    public async Task<Hashtable> GatherPackageInfo(string packageName)
    {
        var packageInfo = await _engine.Info(packageName);
        var result = new Hashtable();
        
        if (packageInfo == null || packageInfo.ResultCount == 0) return result;
        
        Stack<DetailedPkgInfo> stack = new();
        stack.Push(packageInfo.Results[0]);

        while (stack.Count != 0)
        {
            var current = stack.Pop();
            
            if (result.ContainsKey(current.Name)) continue;
            result[current.Name] = current;
            
            var depends = await InfoNullable(current.Depends);
            var makeDepends = await InfoNullable(current.MakeDepends);
            var checkDepends = await InfoNullable(current.CheckDepends);
            
            AddRange(depends, stack);
            AddRange(makeDepends, stack);
            AddRange(checkDepends, stack);
        }
        
        return result;
    }
    
    /// <summary>
    /// Gather packages information
    /// </summary>
    /// <param name="packageName"></param>
    /// <returns></returns>
    public async Task<Hashtable> GatherPackageInfo(IEnumerable<string> packageName)
    {
        var packageInfo = await _engine.Info(packageName);
        var result = new Hashtable();
        
        
        if (packageInfo == null || packageInfo.ResultCount == 0) return result;
        
        Stack<DetailedPkgInfo> stack = new();
        AddRange(packageInfo.Results, stack);
        while (stack.Count != 0)
        {
            var current = stack.Pop();
            
            if (result.ContainsKey(current.Name)) continue;
            result[current.Name] = current;
            
            var depends = await InfoNullable(current.Depends);
            var makeDepends = await InfoNullable(current.MakeDepends);
            var checkDepends = await InfoNullable(current.CheckDepends);
            
            AddRange(depends, stack);
            AddRange(makeDepends, stack);
            AddRange(checkDepends, stack);
        }
        
        return result;
    }
}