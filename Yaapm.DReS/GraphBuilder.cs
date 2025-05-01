using System.Collections;
using QuikGraph;
using Yaapm.RPC.Structs;

namespace Yaapm.DReS;

public class GraphBuilder
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
    
    public AdjacencyGraph<string, Edge<string>> BuildFor(string pkgExplicit, Hashtable table)
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
            
            var temp = ConcatNullable(current.Depends, current.MakeDepends);
            var allDepends = ConcatNullable(temp, current.CheckDepends);
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
}