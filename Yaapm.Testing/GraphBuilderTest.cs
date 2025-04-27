using QuikGraph.Algorithms;
using Yaapm.DReS;

namespace Testing;

public class GraphBuilderTest
{
    [Fact]
    public void GraphBuilder_ReturnsCorrectGraph()
    {
        var builder = new GraphBuilder();
        var res = builder.BuildFor("iup");
        Assert.NotNull(res);
        Assert.Equal(4, res.VertexCount);
        Assert.Equal(3, res.EdgeCount);
        Assert.True(res.IsDirectedAcyclicGraph());
        var succeeded = GraphBuilder.TryGetDepsInstallOrder(res, out var order);
        Assert.True(succeeded);
        Assert.Equal(["libim", "libcd", "iup", "pdflib-lite"], order);
    }
}