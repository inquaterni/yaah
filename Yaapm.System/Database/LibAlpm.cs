using System.Runtime.InteropServices;

namespace Yaapm.System.Database;

public partial class LibAlpm
{
    private const string Lib = "libalpm";

    [LibraryImport(Lib)]
    [UnmanagedCallConv(CallConvs = [typeof(global::System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr alpm_initialize(
        [MarshalAs(UnmanagedType.LPStr)] string root,
        [MarshalAs(UnmanagedType.LPStr)] string dbpath,
        out int err
        );
    
    [LibraryImport(Lib)]
    [UnmanagedCallConv(CallConvs = [typeof(global::System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int alpm_release(IntPtr handle);
    
    // Local dbs
    [DllImport(Lib)]
    [UnmanagedCallConv(CallConvs = [typeof(global::System.Runtime.CompilerServices.CallConvCdecl)])]
    public static extern IntPtr alpm_get_localdb(IntPtr handle);

    [LibraryImport(Lib)]
    [UnmanagedCallConv(CallConvs = [typeof(global::System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr alpm_get_syncdbs(IntPtr handle);
    
    [LibraryImport(Lib)]
    [UnmanagedCallConv(CallConvs = [typeof(global::System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr alpm_db_get_pkgcache(IntPtr db);
    
    /// <summary>
    /// Compare two version strings and determine which one is 'newer'. Returns a value comparable to the way strcmp works. Returns 1 if a is newer than b, 0 if a and b are the same version, or -1 if b is newer than a.
    /// Different epoch values for version strings will override any further comparison. If no epoch is provided, 0 is assumed.
    /// Keep in mind that the pkgrel is only compared if it is available on both versions handed to this function. For example, comparing 1.5-1 and 1.5 will yield 0; comparing 1.5-1 and 1.5-2 will yield -1 as expected. This is mainly for supporting versioned dependencies that do not include the pkgrel.
    /// </summary>
    [LibraryImport(Lib)]
    [UnmanagedCallConv(CallConvs = [typeof(global::System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial int alpm_pkg_vercmp(
        [MarshalAs(UnmanagedType.LPStr)] string a,
        [MarshalAs(UnmanagedType.LPStr)] string b);
    
    [LibraryImport(Lib)]
    [UnmanagedCallConv(CallConvs = [typeof(global::System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.LPStr)]
    public static partial string alpm_pkg_get_name(IntPtr pkg);

    [LibraryImport(Lib)]
    [UnmanagedCallConv(CallConvs = [typeof(global::System.Runtime.CompilerServices.CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.LPStr)]
    public static partial string alpm_pkg_get_version(IntPtr pkg);
    
    [LibraryImport(Lib)]
    [UnmanagedCallConv(CallConvs = [typeof(global::System.Runtime.CompilerServices.CallConvCdecl)])]
    public static partial IntPtr alpm_db_get_pkg(
        IntPtr db,
        [MarshalAs(UnmanagedType.LPStr)] string name);
}