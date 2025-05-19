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

    // [LibraryImport(Lib)]
    // [UnmanagedCallConv(CallConvs = [typeof(global::System.Runtime.CompilerServices.CallConvCdecl)])]
    // public static partial IntPtr alpm_get_syncdbs(IntPtr handle);
    //
    // [LibraryImport(Lib)]
    // [UnmanagedCallConv(CallConvs = [typeof(global::System.Runtime.CompilerServices.CallConvCdecl)])]
    // public static partial IntPtr alpm_db_get_pkgcache(IntPtr db);
    
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