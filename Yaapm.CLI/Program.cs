using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Graphviz.Dot;
using Yaapm.RPC.InfoGathering;

namespace Yaapm.CLI;
using DReS;

internal static class Program
{
    private static void Main(string[] args)
    {
        var builder = new GraphBuilder();
        var inspector = new PackageInspector();

        var table = inspector.GatherPackageMetadata("iup").Result;
        var adjacencyGraph = builder.BuildFor("iup", table);
        
        Console.WriteLine("Correct order: " + string.Join(", ", adjacencyGraph.TopologicalSort().Reverse()) + ";");

        var alg = new QuikGraph.Graphviz.GraphvizAlgorithm<string, Edge<string>>(adjacencyGraph)
        {
            ImageType = GraphvizImageType.PlainText
        };
        
        alg.FormatVertex += (_, eventArgs) =>
        {
            eventArgs.VertexFormat.Label = eventArgs.Vertex;
        };

        alg.Generate(new DReS.Optional.FileDotEngine(), "/home/mmatz/csharp/yaapm/doc/iup_depends.dot");
    }
}