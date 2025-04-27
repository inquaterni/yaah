using QuikGraph;
using QuikGraph.Graphviz.Dot;

namespace Yaapm.CLI;
using DReS;

internal static class Program
{
    private static void Main(string[] args)
    {
        var builder = new GraphBuilder();
        
        Console.WriteLine("AUR Explicit(1): iup");
        var graph = builder.BuildFor("iup");
        

        if (graph != null && GraphBuilder.TryGetDepsInstallOrder(graph, out var deps))
        {
            Console.WriteLine($"AUR Dependencies ({deps.Count()}): " + string.Join(", ", deps));
        }

        var alg = new QuikGraph.Graphviz.GraphvizAlgorithm<string, Edge<string>>(graph)
        {
            ImageType = GraphvizImageType.PlainText
        };

        alg.Generate(new DReS.Optional.FileDotEngine(), "/home/mmatz/csharp/yaapm/doc/zbb.dot");
    }
}