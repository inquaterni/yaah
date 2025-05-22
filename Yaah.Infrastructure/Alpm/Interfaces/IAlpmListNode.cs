namespace Yaah.Infrastructure.Alpm.Interfaces;

public unsafe interface IAlpmListNode<TNode>: IEquatable<TNode>, IDisposable where TNode : unmanaged, IAlpmListNode<TNode>
{
    public IntPtr Data {get; set;}
    public TNode* Prev {get; set;}
    public TNode* Next {get; set;}
    
}