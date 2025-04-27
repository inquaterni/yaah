using QuikGraph;
using QuikGraph.Algorithms;
using Yaapm.RPC;
using Yaapm.RPC.Structs;

namespace Yaapm.DReS;

public class GraphBuilder
{
    private readonly RpcEngine _engine = new();

    private InfoResult FetchDeps(string[] deps)
    {
        var res = _engine.Info(deps).Result;
        return res ?? new InfoResult();
    }

    private static T[] ArrayConcat<T>(T[]? a, T[]? b)
    {
        if (a is null && b is null)
        {
            throw new ArgumentException("Both arrays are null.");
        }
        
        if (a is null && b is not null)
        {
            return b;
        }

        if (b is null && a is not null)
        {
            return a;
        }

        return a!.Concat(b!).ToArray();
    }

    private void BuildGraph(DetailedPkgInfo pkg, InfoResult deps, ref List<string> cache, ref AdjacencyGraph<string, Edge<string>> graph)
    {
        if (deps.ResultCount == 0) return; 
        Console.WriteLine($"\nFetching deps of {pkg.Name}");
        Console.WriteLine($"Deps are: {string.Join(", ", deps!.Results.Select(x => x.Name))}");
        
        foreach (var dep in deps.Results)
        {
            Console.WriteLine($"Processing dep {dep.Name}");
            Console.WriteLine(string.Join(", ", cache));
            if (cache.Contains(dep.Name)) continue;
            cache.Add(dep.Name);
            Console.WriteLine($"Adding vertexes {pkg.Name} & {dep.Name}");
            graph.AddVertex(pkg.Name);
            graph.AddVertex(dep.Name);
            Console.WriteLine($"Adding edge between {pkg.Name} & {dep.Name}");
            graph.AddEdge(new Edge<string>(pkg.Name, dep.Name));
            
            Console.WriteLine($"Fetching deps for {dep.Name}");
            BuildGraph(dep, FetchDeps(ArrayConcat(dep.Depends, dep.MakeDepends)), ref cache, ref graph);
        }
    }

    public AdjacencyGraph<string, Edge<string>>? BuildFor(string name)
    {
        List<string> cache = [];
        var info = _engine.Info(name).Result;

        var explicitResult = info?.Results.FirstOrDefault(pkgInfo => pkgInfo.Name == name);
        if (explicitResult == null) return null;
        
        var graph = new AdjacencyGraph<string, Edge<string>>();
        graph.AddVertex(explicitResult.Name);
        var deps = FetchDeps(ArrayConcat(explicitResult.Depends, explicitResult.MakeDepends));
        if (deps == null) return null;
        
        BuildGraph(explicitResult, deps, ref cache, ref graph);
        
        return graph;
    }

    public static bool TryGetDepsInstallOrder(AdjacencyGraph<string, Edge<string>> graph, out IEnumerable<string>? order)
    {
        order = graph.IsDirectedAcyclicGraph() ? graph.TopologicalSort() : null;
        
        return graph.IsDirectedAcyclicGraph();
    }
}