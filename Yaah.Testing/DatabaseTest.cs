using System.Runtime.InteropServices;
using Yaah.Infrastructure.Database;
using Yaah.Infrastructure.Errors.Exceptions;

namespace Testing;

public class DatabaseTest
{
    private static readonly DatabaseController Controller = new();

    [Theory(Timeout = 100)]
    [InlineData("gtk3")]
    [InlineData("yay")]
    public void GetExistingPackage_ReturnsRightPackage(string packageName)
    {
        var db = Controller.GetLocalDb();
        var pkg = DatabaseController.GetPackage(db, packageName);
        
        Assert.NotEqual(IntPtr.Zero, pkg);
        
        var name = DatabaseController.GetPackageName(pkg);
        Assert.NotNull(name);
        Assert.Equal(name, packageName);
    }

    [Fact(Timeout = 100)]
    public void RegisterDatabase_NoThrow()
    {
        Controller.RegisterSyncDb("multilib", -1);
    }

    [Fact(Timeout = 100)]
    public void GetNonExistingPackage_ThrowsException()
    {
        var db = Controller.GetLocalDb();
        Assert.Throws<DatabaseException>(() => DatabaseController.GetPackage(db, "non-existing-package"));
    }

    [Fact(Timeout = 100)]
    public void GetPackageNameNULL_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => DatabaseController.GetPackageName(IntPtr.Zero));
    }

    [Fact(Timeout = 100)]
    public void GetPackageVersionNULL_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => DatabaseController.GetPackageVersion(IntPtr.Zero));
    }
}