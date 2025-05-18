using static Yaapm.System.Database.LibAlpm;

namespace Yaapm.System.Database;

public class DatabaseController
{
    private const string RootPath = "/";
    private const string DbPath = "/var/lib/pacman";

    private int _err;
    private readonly IntPtr _alpmHandle;

    public DatabaseController()
    {
        _alpmHandle = alpm_initialize(RootPath, DbPath, out _err);
        if (_err != 0)
        {
            throw new Exception("alpm_initialize failed");
        }
    }

    public IntPtr GetLocalDb()
    {
        var dbHandle = alpm_get_localdb(_alpmHandle);
        if (dbHandle == IntPtr.Zero) throw new Exception("alpm_get_localdb failed");
        return dbHandle;
    }

    public static IntPtr GetPackage(IntPtr dbHandle, string name)
    {
        var pkgPtr = alpm_db_get_pkg(dbHandle, name);
        if (pkgPtr == IntPtr.Zero) throw new Exception("alpm_db_get_pkg failed");
        return pkgPtr;
    }

    // Test incorrect data
    public static string GetPackageName(IntPtr pkg)
    {
        return alpm_pkg_get_name(pkg);
    }

    // Test incorrect data
    public static string GetPackageVersion(IntPtr pkg)
    {
        return alpm_pkg_get_version(pkg);
    }

    /// <summary>
    /// Compare two version strings and determine which one is 'newer'. Returns a value comparable to the way strcmp works. Returns 1 if a is newer than b, 0 if a and b are the same version, or -1 if b is newer than a.
    /// Different epoch values for version strings will override any further comparison. If no epoch is provided, 0 is assumed.
    /// Keep in mind that the pkgrel is only compared if it is available on both versions handed to this function. For example, comparing 1.5-1 and 1.5 will yield 0; comparing 1.5-1 and 1.5-2 will yield -1 as expected. This is mainly for supporting versioned dependencies that do not include the pkgrel.
    /// </summary>
    public static int VersionCompare(string a, string b)
    {
        return alpm_pkg_vercmp(a, b);
    }

    ~DatabaseController()
    {
        _ = alpm_release(_alpmHandle);
    }
}