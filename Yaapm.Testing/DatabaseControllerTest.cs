using Database;

namespace Testing;

public class DatabaseControllerTest
{
    [Theory]
    [InlineData("bash")]
    public void TryGetPackageVersion_ReturnsValidPkgInfo(string packageName)
    {
        var succeeded = DatabaseController.TryGetPkgInfo(packageName, out var pkgInfo);
        Assert.True(succeeded);
        Assert.NotNull(pkgInfo);
        Assert.IsType<long>(pkgInfo.InstalledSize);
        Assert.IsType<DateTime>(pkgInfo.InstallDate);
        Assert.IsType<DateTime>(pkgInfo.BuildDate);
    }

    [Theory]
    [InlineData("pkg")]
    [InlineData("")]
    public void TryGetPackageVersion_ReturnsFalse(string packageName)
    {
        var succeeded = DatabaseController.TryGetPackageVersion(packageName, out var packageVersion);
        Assert.False(succeeded);
        Assert.Null(packageVersion);
    }
}