using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Yaah.Infrastructure.Alpm;

public static partial class LibAlpm
{
    private const string Lib = "libalpm";

    [LibraryImport(Lib, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr alpm_initialize(
        [MarshalAs(UnmanagedType.LPStr)] string root,
        [MarshalAs(UnmanagedType.LPStr)] string dbpath,
        out int err
    );

    [LibraryImport(Lib, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int alpm_release(IntPtr handle);
    
    [LibraryImport(Lib, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr alpm_get_localdb(IntPtr handle);
    
    [LibraryImport(Lib, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr alpm_get_syncdbs(IntPtr handle);
    
    [LibraryImport(Lib, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr alpm_register_syncdb(
        IntPtr handle,
        [MarshalAs(UnmanagedType.LPStr)]string treename,
        int siglevel);
    
    // public delegate int AlpmListCmp(IntPtr a, IntPtr b);
    // public delegate int AlpmListFree(IntPtr item);
    
    // [LibraryImport(Lib)]
    // [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    // public static partial IntPtr alpm_list_diff(IntPtr lhs, IntPtr rhs, AlpmListCmp fn);
    //
    // [LibraryImport(Lib)]
    // [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    // public static partial void alpm_list_free_inner(IntPtr item, AlpmListFree fn);
    
    // [LibraryImport(Lib)]
    // [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    // public static partial void alpm_list_free(IntPtr item);
    
    // [LibraryImport(Lib)]
    // [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    // public static partial IntPtr alpm_list_add(IntPtr item);
    
    // [LibraryImport(Lib)]
    // [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    // public static partial IntPtr alpm_list_join(IntPtr first, IntPtr second);
    
    [LibraryImport(Lib, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int alpm_errno(IntPtr handle);

    [LibraryImport(Lib, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr alpm_strerror(int err);

    [LibraryImport(Lib, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr alpm_db_get_pkgcache(IntPtr db);
    
    [LibraryImport(Lib, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr alpm_list_next(IntPtr list);
    
    [LibraryImport(Lib, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr alpm_pkg_get_db(IntPtr pkg);
    
    [LibraryImport(Lib, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.LPStr)]
    public static partial string alpm_db_get_name(IntPtr db);

    [LibraryImport(Lib, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int alpm_pkg_vercmp(
        [MarshalAs(UnmanagedType.LPStr)] string a,
        [MarshalAs(UnmanagedType.LPStr)] string b);

    [LibraryImport(Lib, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.LPStr)]
    public static partial string alpm_pkg_get_name(IntPtr pkg);

    [LibraryImport(Lib, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.LPStr)]
    public static partial string alpm_pkg_get_version(IntPtr pkg);

    [LibraryImport(Lib, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr alpm_db_get_pkg(
        IntPtr db,
        [MarshalAs(UnmanagedType.LPStr)] string name);
}