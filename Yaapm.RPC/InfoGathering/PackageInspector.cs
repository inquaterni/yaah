using System.Collections;
using Yaapm.RPC.Structs;

namespace Yaapm.RPC.InfoGathering;

public class PackageInspector
{
    private readonly RpcEngine _engine = new();
    
    private async Task<DetailedPkgInfo[]> InfoNullable(string[]? data)
    {
        if (data == null || data.Length == 0) return [];
        var response = await _engine.Info(data);
        if (response == null || response.ResultCount == 0) return [];
        
        return response.Results;
    }

    private static void AddRange(IEnumerable<DetailedPkgInfo> enumerable, Stack<DetailedPkgInfo> stack)
    {
        foreach (var item in enumerable.Reverse())
        {
            if (stack.Contains(item)) continue;
            stack.Push(item);
        }
    }

    public async Task<Hashtable> GatherPackageMetadata(string packageName)
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
}