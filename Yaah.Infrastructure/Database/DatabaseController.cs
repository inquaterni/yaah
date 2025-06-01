using NLog;
using Yaah.Infrastructure.Errors.Exceptions;
using static Yaah.Infrastructure.Errors.ErrorTranslator;
using static Yaah.Infrastructure.LibAlpm;

namespace Yaah.Infrastructure.Database;

public class DatabaseController : IDisposable
{
    private const string RootPath = "/";
    private const string DbPath = "/var/lib/pacman";
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly IntPtr _alpmHandle;

    private int _err;

    public DatabaseController()
    {
        _alpmHandle = alpm_initialize(RootPath, DbPath, out _err);
        if (_err != 0)
        {
            Logger.Fatal($"Could not initialize libalpm: {TranslateAlpmError(_err)}");
            throw new DatabaseException(_err);
        }

        Logger.Debug("Registering core db");
        alpm_register_syncdb(_alpmHandle, "core", -1);
        Logger.Debug("Registering extra db");
        alpm_register_syncdb(_alpmHandle, "extra", -1);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Get pointer to local db struct
    /// </summary>
    /// <returns>Pointer to local db</returns>
    /// <exception cref="DatabaseException">Occurs when alpm_get_localdb fails</exception>
    /// <see cref="https://man.archlinux.org/man/libalpm_databases.3.en" />
    public IntPtr GetLocalDb()
    {
        var dbHandle = alpm_get_localdb(_alpmHandle);
        if (dbHandle == IntPtr.Zero)
        {
            throw new DatabaseException(_err);
        }

        return dbHandle;
    }

    public IntPtr RegisterSyncDb(string treename, int siglevel)
    {
        if (string.IsNullOrEmpty(treename))
        {
            throw new ArgumentNullException(nameof(treename));
        }
        var result = alpm_register_syncdb(_alpmHandle, treename, siglevel);
        return result;
    }

    /// <summary>
    ///     Get pointer to package struct
    /// </summary>
    /// <param name="dbHandle">Pointer to local db</param>
    /// <param name="name">Package name</param>
    /// <returns>Pointer to package struct</returns>
    /// <exception cref="DatabaseException">Occurs when alpm_db_get_pkg fails (e.g. attempt to get nonexisting package)</exception>
    /// <exception cref="ArgumentNullException">Occurs when \p dbHandle is NULL</exception>
    public static IntPtr GetPackage(IntPtr dbHandle, string name)
    {
        if (dbHandle == IntPtr.Zero)
        {
            throw new ArgumentNullException(nameof(dbHandle));
        }

        var pkgPtr = alpm_db_get_pkg(dbHandle, name);
        if (pkgPtr == IntPtr.Zero)
        {
            throw new DatabaseException(dbHandle);
        }

        return pkgPtr;
    }


    public static IntPtr GetDbFromPackage(IntPtr pkg)
    {
        if (pkg == IntPtr.Zero)
        {
            throw new ArgumentNullException(nameof(pkg));
        }
        var result = alpm_pkg_get_db(pkg);
        if (result == IntPtr.Zero)
        {
            throw new DatabaseException(pkg);
        }
        return result;
    }

    /// <summary>
    /// </summary>
    /// <param name="pkg">Pointer to package struct</param>
    /// <returns>Package name</returns>
    /// <exception cref="ArgumentNullException">Occurs when pointer is NULL</exception>
    public static string GetPackageName(IntPtr pkg)
    {
        if (pkg == IntPtr.Zero)
        {
            throw new ArgumentNullException(nameof(pkg));
        }
        var name = alpm_pkg_get_name(pkg);
        return name;
    }

    /// <summary>
    /// </summary>
    /// <param name="pkg">Pointer to package struct</param>
    /// <returns>Package version</returns>
    /// <exception cref="ArgumentNullException">Occurs when pointer is NULL</exception>
    public static string GetPackageVersion(IntPtr pkg)
    {
        if (pkg == IntPtr.Zero)
        {
            throw new ArgumentNullException(nameof(pkg));
        }
        var result = alpm_pkg_get_version(pkg);
        return result;
    }


    ~DatabaseController()
    {
        ReleaseUnmanagedResources();
    }

    private void ReleaseUnmanagedResources()
    {
        _err = alpm_release(_alpmHandle);
        if (_err == 0)
        {
            return;
        }
        Logger.Fatal($"Could not free memory: {TranslateAlpmError(_err)}");
        throw new DatabaseException(_err);
    }
}