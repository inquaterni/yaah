using System.Runtime.InteropServices;
using Yaah.Infrastructure.Alpm.Interfaces;
using static Yaah.Infrastructure.Alpm.LibAlpm;

namespace Yaah.Infrastructure.Alpm.Nodes;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public unsafe struct AlpmPkgListNode : IAlpmListNode<AlpmPkgListNode>
{
    public IntPtr Data { get; set; }
    public AlpmPkgListNode* Next { get; set; }
    public AlpmPkgListNode* Prev { get; set; }

    public bool Equals(AlpmPkgListNode other)
    {
        return alpm_pkg_get_name(Data) == alpm_pkg_get_name(other.Data) && Equals(Next, other.Next) &&
               Equals(Prev, other.Prev);
    }

    private static bool Equals(AlpmPkgListNode* self, AlpmPkgListNode* other)
    {
        return alpm_pkg_get_name(self->Data) == alpm_pkg_get_name(other->Data) && Equals(self->Next, other->Next) &&
               Equals(self->Prev, other->Prev);
    }

    public override bool Equals(object? obj)
    {
        return obj is AlpmPkgListNode other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Data, unchecked((int)(long)Next), unchecked((int)(long)Prev));
    }

    public static bool operator ==(AlpmPkgListNode lhs, AlpmPkgListNode rhs)
    {
        return lhs.Data == rhs.Data;
    }

    public static bool operator !=(AlpmPkgListNode lhs, AlpmPkgListNode rhs)
    {
        return lhs.Data != rhs.Data;
    }

    public void Dispose()
    {
        Marshal.FreeHGlobal(Data);
        Marshal.FreeHGlobal((IntPtr)Next);
        Marshal.FreeHGlobal((IntPtr)Prev);
    }
}