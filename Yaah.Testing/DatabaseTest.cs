using Yaah.System.Database;

namespace Testing;

public class DatabaseTest
{
    private static readonly DatabaseController Controller = new();

    [Theory]
    [InlineData("gtk3")]
    [InlineData("yay")]
    public void GetExistingPackage_ReturnsRightPackage(string packageName)
    {
        var db = Controller.GetLocalDb();
        var pkg = DatabaseController.GetPackage(db, packageName);
        
        Assert.Equal(DatabaseController.GetPackageName(pkg), packageName);
    }

    [Fact]
    public void GetNonExistingPackage_ThrowsException()
    {
        var db = Controller.GetLocalDb();
        Assert.Throws<Exception>(() => DatabaseController.GetPackage(db, "non-existing-package"));
    }

    [Fact]
    public void GetPackageNameNULL_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => DatabaseController.GetPackageName(IntPtr.Zero));
    }
    
    [Fact]
    public void GetPackageVersionNULL_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => DatabaseController.GetPackageVersion(IntPtr.Zero));
    }

    [Theory]
    [InlineData("2.0.1-1")]
    [InlineData("2.0.3")]
    public void EqualVersionsVersionCmp_ReturnsZero(string version)
    {
        Assert.Equal(0, DatabaseController.VersionComparison(version, version));
    }
    
    [Theory]
    [InlineData("2.0.1-1", "2.0.3")]
    public void BIsNewerVersionCmp_ReturnsMinusOne(string a, string b)
    {
        Assert.Equal(-1, DatabaseController.VersionComparison(a, b));
    }
    
    [Theory]
    [InlineData("2.0.3", "2.0.1-1")]
    public void AIsNewerVersionCmp_ReturnsOne(string a, string b)
    {
        Assert.Equal(1, DatabaseController.VersionComparison(a, b));
    }
}