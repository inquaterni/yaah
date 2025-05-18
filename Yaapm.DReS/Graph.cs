using System.Collections;
using System.Text.RegularExpressions;
using QuikGraph;
using QuikGraph.Algorithms;
using Yaapm.Net.Structs;
using Yaapm.System;

namespace Yaapm.DReS;

public static class Graph
{
    private static IEnumerable<string> ConcatNullable(IEnumerable<string>? first, IEnumerable<string>? second)
    {
        return first switch
        {
            null when second is null => [],
            not null when second is null => first,
            null => second,
            not null => first.Concat(second)
        };
    }

    private static IEnumerable<string> GetRidOfVersionConditions(IEnumerable<string> depends)
    {
        foreach (var dep in depends)
        {
            var match = VersionController.GetVersionMatch(dep);
            if (match != null)
            {
                yield return match.Groups["name"].Value;
            }
            else
            {
                yield return dep;
            }
        }
    }
    
    private static void AddRange(IEnumerable<string> enumerable, Stack<string> stack)
    {
        Parallel.ForEach(enumerable, item =>
        {
            if (stack.Contains(item)) return;
            stack.Push(item);
        });
    }

    public static AdjacencyGraph<string, Edge<string>> BuildFor(string pkgExplicit, Hashtable table)
    {
        
        var pkgInfo = table[pkgExplicit] as DetailedPkgInfo;
        var result = new AdjacencyGraph<string, Edge<string>>();
        if (pkgInfo is null) return result;

        var stack = new Stack<string>();
        stack.Push(pkgInfo.Name);
        while (stack.Count != 0)
        {
            var current = (table[stack.Pop()] as DetailedPkgInfo)!;
            result.AddVertex(current.Name);
            var allDepends = GetRidOfVersionConditions(ConcatNullable(ConcatNullable(current.Depends, current.MakeDepends), current.CheckDepends));
            foreach (var depend in allDepends)
            {
                if (!table.ContainsKey(depend)) continue;
                result.AddVertex(depend);
                
                if (result.ContainsEdge(current.Name, depend)) continue;
                result.AddEdge(new Edge<string>(current.Name, depend));
                
                stack.Push(depend);
            }
        }
        return result;
    }
    
    public static AdjacencyGraph<string, Edge<string>> BuildFor(IEnumerable<string> pkgExplicit, Hashtable table)
    {
        List<DetailedPkgInfo> pkgs = [];
        foreach (var pkg in pkgExplicit)
        {
            if (table[pkg] is not DetailedPkgInfo pkgInfo) continue;
            pkgs.Add(pkgInfo);
        }
        
        var result = new AdjacencyGraph<string, Edge<string>>();
        if (pkgs.Count == 0) return result;

        var stack = new Stack<string>();
        AddRange(pkgs.Select(item => item.Name), stack);
        while (stack.Count != 0)
        {
            var current = (table[stack.Pop()] as DetailedPkgInfo)!;
            result.AddVertex(current.Name);
            var allDepends = GetRidOfVersionConditions(ConcatNullable(ConcatNullable(current.Depends, current.MakeDepends), current.CheckDepends));
            foreach (var depend in allDepends)
            {
                if (!table.ContainsKey(depend)) continue;
                result.AddVertex(depend);
                
                if (result.ContainsEdge(current.Name, depend)) continue;
                result.AddEdge(new Edge<string>(current.Name, depend));
                
                stack.Push(depend);
            }
        }
        return result;
    }

    public static IEnumerable<string> GetInstallOrder(AdjacencyGraph<string, Edge<string>> graph)
    {
        return graph.SourceFirstTopologicalSort().Reverse();
    }
}