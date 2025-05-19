using NLog;
using static Yaah.System.Database.LibAlpm;

namespace Yaah.System.Database;

public class DatabaseController: IDisposable
{
    private const string RootPath = "/";
    private const string DbPath = "/var/lib/pacman";

    private int _err;
    private readonly IntPtr _alpmHandle;
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public DatabaseController()
    {
        _alpmHandle = alpm_initialize(RootPath, DbPath, out _err);
        if (_err == 0) return;
        Logger.Fatal($"Could not initialize libalpm, error code: {_err}");
        throw new Exception("alpm_initialize failed");
    }

    /// <summary>
    /// Get pointer to local db struct
    /// </summary>
    /// <returns>Pointer to local db</returns>
    /// <exception cref="Exception">Occurs when alpm_get_localdb fails</exception>
    /// <see cref="https://man.archlinux.org/man/libalpm_databases.3.en"/>
    public IntPtr GetLocalDb()
    {
        var dbHandle = alpm_get_localdb(_alpmHandle);
        if (dbHandle == IntPtr.Zero) throw new Exception("alpm_get_localdb failed");
        return dbHandle;
    }

    /// <summary>
    /// Get pointer to package struct
    /// </summary>
    /// <param name="dbHandle">Pointer to local db</param>
    /// <param name="name">Package name</param>
    /// <returns>Pointer to package struct</returns>
    /// <exception cref="Exception">Occurs when alpm_db_get_pkg fails (e.g. attempt to get nonexisting package)</exception>
    public static IntPtr GetPackage(IntPtr dbHandle, string name)
    {
        var pkgPtr = alpm_db_get_pkg(dbHandle, name);
        if (pkgPtr == IntPtr.Zero) throw new Exception("alpm_db_get_pkg failed");
        return pkgPtr;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pkg">Pointer to package struct</param>
    /// <returns>Package name</returns>
    /// <exception cref="ArgumentNullException">Occurs when pointer is NULL</exception>
    public static string GetPackageName(IntPtr pkg)
    {
        if (pkg == IntPtr.Zero) throw new ArgumentNullException(nameof(pkg));
        return alpm_pkg_get_name(pkg);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pkg">Pointer to package struct</param>
    /// <returns>Package version</returns>
    /// <exception cref="ArgumentNullException">Occurs when pointer is NULL</exception>
    public static string GetPackageVersion(IntPtr pkg)
    {
        if (pkg == IntPtr.Zero) throw new ArgumentNullException(nameof(pkg));
        return alpm_pkg_get_version(pkg);
    }

    /// <summary>
    /// Compare two version strings and determine which one is 'newer'. Returns a value comparable to the way strcmp works. Returns 1 if a is newer than b, 0 if a and b are the same version, or -1 if b is newer than a.
    /// Different epoch values for version strings will override any further comparison. If no epoch is provided, 0 is assumed.
    /// Keep in mind that the pkgrel is only compared if it is available on both versions handed to this function. For example, comparing 1.5-1 and 1.5 will yield 0; comparing 1.5-1 and 1.5-2 will yield -1 as expected. This is mainly for supporting versioned dependencies that do not include the pkgrel.
    /// </summary>
    /// <see cref="https://man.archlinux.org/man/libalpm_packages.3.en"/>
    public static int VersionComparison(string a, string b)
    {
        return alpm_pkg_vercmp(a, b);
    }

    ~DatabaseController()
    {
        ReleaseUnmanagedResources();
    }

    private void ReleaseUnmanagedResources()
    {
        _err = alpm_release(_alpmHandle);
        if (_err == 0) return;
        Logger.Fatal($"Could not free memory, error code: {_err}");
        throw new Exception("alpm_release failed");
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }
}