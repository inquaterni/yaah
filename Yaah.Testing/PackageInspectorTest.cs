using Yaah.Infrastructure.Database;
using Yaah.Infrastructure.InfoGathering;

namespace Testing;

public class PackageInspectorTest
{
    private static readonly DatabaseController Database = new();
    private readonly PackageInspector _inspector = new(Database.GetLocalDb());

    [Theory]
    [InlineData("iup", new[]
    {
        "iup",
        "lua-cd",
        "lua51-cd",
        "lua52-cd",
        "lua53-cd",
        "libcd",
        "pdflib-lite",
        "lua-im",
        "lua51-im",
        "lua52-im",
        "lua53-im",
        "libim"
    })]
    [InlineData("clion", new[] { "clion" })]
    public async Task GetPackageInfo_ReturnsRightPackages(string pkg, IEnumerable<string> expected)
    {
        var result = await _inspector.GatherPackageInfo(pkg);

        var keysSet = new HashSet<string>(result.Keys.Cast<string>());
        var expectedSet = new HashSet<string>(expected);
        Assert.True(expectedSet.SetEquals(keysSet));
    }

    [Theory]
    [InlineData("some-nonexisting-package1")]
    [InlineData("some-nonexisting-package2")] // Why?
    [InlineData("some-nonexisting-package3")]
    public async Task GetPackageInfo_BadData_ReturnsEmptyTable(string pkg)
    {
        var result = await _inspector.GatherPackageInfo(pkg);

        Assert.Equal(0, result.Keys.Count);
    }
}