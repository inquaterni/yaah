using System.Runtime.InteropServices;
using Yaah.Infrastructure.Alpm.Collections;
using Yaah.Infrastructure.Alpm.Interfaces;
using static Yaah.Infrastructure.Alpm.LibAlpm;

namespace Yaah.Infrastructure.Alpm.Nodes;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public unsafe struct AlpmDbListNode : IAlpmListNode<AlpmDbListNode>
{
    public bool Equals(AlpmDbListNode other)
    {
        return new AlpmList<AlpmPkgListNode>(alpm_db_get_pkgcache(Data)).Equals(
            new AlpmList<AlpmPkgListNode>(alpm_db_get_pkgcache(other.Data)));
    }

    public IntPtr Data { get; set; }
    public AlpmDbListNode* Prev { get; set; }
    public AlpmDbListNode* Next { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is AlpmDbListNode node && Equals(node);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Data, unchecked((int)(long)Prev), unchecked((int)(long)Next));
    }

    public void Dispose()
    {
        Marshal.FreeHGlobal(Data);
        Marshal.FreeHGlobal((IntPtr)Next);
        Marshal.FreeHGlobal((IntPtr)Prev);
    }

    public static bool operator ==(AlpmDbListNode left, AlpmDbListNode right)
    {
        return left.Data == right.Data;
    }

    public static bool operator !=(AlpmDbListNode left, AlpmDbListNode right)
    {
        return left.Data != right.Data;
    }
}