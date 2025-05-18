using Yaapm.Net.Rpc;
using Yaapm.Net.Structs;

namespace Testing;

public class RpcEngineTest
{
    
    private readonly RpcEngine _engine = new();
    
    [Theory]
    [InlineData("zen-browser")]
    public async Task Suggest_ReturnsStringArray(string suggestion)
    {
        var result = await _engine.Suggest(suggestion);
        Assert.NotNull(result);
        Assert.Equal(typeof(string[]), result.GetType());
    }

    [Theory]
    [InlineData("zen-browser")]
    public async Task Search_ReturnsSearchResult(string searchTerm)
    {
        var result = await _engine.Search(searchTerm);
        Assert.NotNull(result);
        Assert.Equal(typeof(SearchResult), result.GetType());
    }

    [Theory]
    [InlineData("zen-browser")]
    [InlineData("sent")]
    public async Task Info_ReturnsInfoResult(string searchTerm)
    {
        var result = await _engine.Info(searchTerm);
        Assert.NotNull(result);
        Assert.Equal(typeof(InfoResult), result.GetType());
    }

    [Fact]
    public async Task InfoMulti_ReturnsInfoResult()
    {
        var result = await _engine.Info([(string)"zen-browser", (string)"packettracer"]);
        Assert.NotNull(result);
        Assert.Equal(2, result.ResultCount);
        Assert.Equal(typeof(InfoResult), result.GetType());
    }
}