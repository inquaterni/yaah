using Yaah.Infrastructure.Versioning;

namespace Testing;

public class VersionControllerTests
{
    [Theory(Timeout = 100)]
    [InlineData("2.0.1-1")]
    [InlineData("2.0.3")]
    public void EqualVersionsVersionCmp_ReturnsZero(string version)
    {
        Assert.Equal(0, VersionController.VersionComparison(version, version));
    }

    [Theory(Timeout = 100)]
    [InlineData("2.0.1-1", "2.0.3")]
    public void BIsNewerVersionCmp_ReturnsMinusOne(string a, string b)
    {
        Assert.Equal(-1, VersionController.VersionComparison(a, b));
    }

    [Theory(Timeout = 100)]
    [InlineData("2.0.3", "2.0.1-1")]
    public void AIsNewerVersionCmp_ReturnsOne(string a, string b)
    {
        Assert.Equal(1, VersionController.VersionComparison(a, b));
    }
}