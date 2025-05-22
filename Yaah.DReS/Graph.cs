using System.Collections;
using NLog;
using QuikGraph;
using QuikGraph.Algorithms;
using Yaah.Infrastructure.Versioning;
using Yaah.Net.Models;

namespace Yaah.DReS;

public static class Graph
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
                yield return match.Groups["name"].Value;
            else
                yield return dep;
        }
    }

    private static void AddRange(IEnumerable<string> enumerable, Stack<string> stack)
    {
        foreach (var item in enumerable)
        {
            if (stack.Contains(item)) return;
            stack.Push(item);
        }
    }

    private static IEnumerable<T> GetPkgsFromTable<T>(IEnumerable<string> pkgs, Hashtable table)
    {
        foreach (var pkg in pkgs)
        {
            if (table[pkg] is not T t) continue;
            yield return t;
        }
    }

    /// <summary>
    ///     Build graph for given AUR package \p pkgExplicit
    /// </summary>
    /// <param name="pkgExplicit">AUR explicit package name</param>
    /// <param name="table">Hashtable with gathered packages data</param>
    /// <returns>
    ///     Dependency Graph
    ///     \dotfile graph-single.dot "Example graph for iup package"
    /// </returns>
    public static AdjacencyGraph<string, Edge<string>> BuildFor(string pkgExplicit, Hashtable table)
    {
        var pkgInfo = table[pkgExplicit] as DetailedPkgInfo;
        var result = new AdjacencyGraph<string, Edge<string>>();
        if (pkgInfo is null)
        {
            Logger.Error("PkgInfo is null, aborting.");
            return result;
        }

        var stack = new Stack<string>();
        stack.Push(pkgInfo.Name);
        while (stack.Count != 0)
            try
            {
                var current = (table[stack.Pop()] as DetailedPkgInfo)!;
                Logger.Debug($"Adding vertex {current.Name}");
                result.AddVertex(current.Name);
                var allDepends = GetRidOfVersionConditions(
                    ConcatNullable(ConcatNullable(current.Depends, current.MakeDepends), current.CheckDepends));
                foreach (var depend in allDepends)
                {
                    if (!table.ContainsKey(depend)) continue;

                    Logger.Debug($"Adding vertex {depend}");
                    result.AddVertex(depend);

                    if (result.ContainsEdge(current.Name, depend))
                    {
                        Logger.Warn($"Edge {current.Name}->{depend} already exists");
                        continue;
                    }

                    Logger.Debug($"Adding edge {current.Name}->{depend}");
                    result.AddEdge(new Edge<string>(current.Name, depend));

                    stack.Push(depend);
                }
            }
            catch (Exception e) when (e is ArgumentNullException)
            {
                Logger.Error($"{e.GetType().Name} was thrown while building graph ({e.Message}).");
            }

        return result;
    }

    /// <summary>
    ///     Build graph for given AUR packages \p pkgExplicit
    /// </summary>
    /// <param name="pkgExplicit">AUR explicit package names</param>
    /// <param name="table">Hashtable with gathered packages data</param>
    /// <returns>
    ///     Dependency Graph
    ///     \dotfile graph-multi.dot "Example graph for meta-package-manager, obs-studio-git and clion packages"
    /// </returns>
    public static AdjacencyGraph<string, Edge<string>> BuildFor(IEnumerable<string> pkgExplicit, Hashtable table)
    {
        var pkgs = GetPkgsFromTable<DetailedPkgInfo>(pkgExplicit, table);

        var result = new AdjacencyGraph<string, Edge<string>>();
        var detailedPkgInfos = pkgs as DetailedPkgInfo[] ?? pkgs.ToArray();
        if (detailedPkgInfos.Length == 0) return result;

        var stack = new Stack<string>();
        AddRange(detailedPkgInfos.Select(item => item.Name), stack);
        while (stack.Count != 0)
            try
            {
                var current = (table[stack.Pop()] as DetailedPkgInfo)!;
                Logger.Debug($"Adding vertex {current.Name}");
                result.AddVertex(current.Name);
                var allDepends = GetRidOfVersionConditions(
                    ConcatNullable(ConcatNullable(current.Depends, current.MakeDepends), current.CheckDepends));
                foreach (var depend in allDepends)
                {
                    if (!table.ContainsKey(depend)) continue;

                    Logger.Debug($"Adding vertex {depend}");
                    result.AddVertex(depend);

                    if (result.ContainsEdge(current.Name, depend))
                    {
                        Logger.Warn($"Edge {current.Name}->{depend} already exists");
                        continue;
                    }

                    Logger.Debug($"Adding edge {current.Name}->{depend}");
                    result.AddEdge(new Edge<string>(current.Name, depend));

                    stack.Push(depend);
                }
            }
            catch (Exception e) when (e is ArgumentNullException)
            {
                Logger.Error($"{e.GetType().Name} was thrown while building graph ({e.Message}).");
            }

        return result;
    }

    public static IEnumerable<string> GetInstallOrder(AdjacencyGraph<string, Edge<string>> graph)
    {
        return graph.SourceFirstTopologicalSort().Reverse();
    }
}