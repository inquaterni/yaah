using System.Collections;
using System.Runtime.InteropServices;
using Yaah.Infrastructure.Alpm.Interfaces;
using static Yaah.Infrastructure.Alpm.LibAlpm;

namespace Yaah.Infrastructure.Alpm.Collections;



/// <summary>
/// A doubly linked list.
/// </summary>
/// <see cref="https://man.archlinux.org/man/libalpm_list.3.en"/>
public unsafe class AlpmList<TNode> : ICollection<TNode>
    where TNode : unmanaged, IAlpmListNode<TNode>
{
    internal TNode* Head;
    private TNode* _tail;

    public AlpmList(TNode* head = null)
    {
        Head = head;
        _tail = FindTail();
    }
    public AlpmList(AlpmList<TNode> other)
    {
        Head = other.Head;
        _tail = other._tail;
    }
    public AlpmList(IntPtr head)
    {
        Head = (TNode*)head;
        _tail = FindTail();
    }

    private TNode* FindTail()
    {
        if (Head == null)
            return null;

        var node = Head;
        TNode* last = null;
        
        while (node != null)
        {
            last = node;
            node = node->Next;
            Count++;
        }
        
        return last;
    }

    public IEnumerator<TNode> GetEnumerator()
    {
        return new AlpmListEnumerator<TNode>(this);
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(TNode item)
    {
        if (_tail == null)
        {
            Head = &item;
        }
        else
        {
            _tail->Next = &item;
            item.Prev = _tail;
        }

        _tail = &item;
        Count++;
    }

    public void AddRange(IEnumerable<TNode> items)
    {
        foreach (var item in items)
        {
            Add(item);
        }
    }

    public void Clear()
    {
        var current = Head;
        while (current != null)
        {
            var Next = current->Next;
            // Marshal.FreeHGlobal(current->Data);
            current = Next;
        }
        Head = null;
        _tail = null;
        Count = 0;
    }

    public bool Contains(TNode item)
    {
        var current = Head;
        while (current != null)
        {
            if (current->Equals(item)) return true;
            current = (TNode*)alpm_list_next((IntPtr)current);
        }
        return false;
    }

    public void CopyTo(TNode[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        if (arrayIndex < 0 || arrayIndex >= array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < Count)
            throw new ArgumentException("Destination array has fewer elements than required.");

        var index = arrayIndex;
        foreach (var node in this)
        {
            array[index++] = node;
        }
    }

    public bool Remove(TNode item)
    {
        var current = Head;
        while (current != null)
        {
            if (current->Equals(item))
            {
                // Update neighbors
                if (current->Prev != null) current->Prev->Next = (TNode*)alpm_list_next((IntPtr)current);
                if ((TNode*)alpm_list_next((IntPtr)current) != null) current->Next->Prev = current->Prev;

                // Update head/tail references
                if (current == Head) Head = current->Next;
                if (current == _tail) _tail = current->Prev;

                Count--;
                return true;
            }
            current = current->Next;
        }
        return false;
    }

    public int Count { get; private set; }
    public bool IsReadOnly => false;

    public AlpmList<TNode> Extend(AlpmList<TNode> other, ExtendOptions options = ExtendOptions.SaveEqual)
    {
        if ((IntPtr)Head == IntPtr.Zero) return other;
        if ((IntPtr)other.Head == IntPtr.Zero) return this;
        switch (options)
        {
            case ExtendOptions.SaveEqual:

                if (Head == null)
                {
                    Head = other.Head;
                }
                else
                {
                    _tail->Next = other.Head;
                    other.Head->Prev = _tail;
                }

                _tail = other._tail;

                return this;
            case ExtendOptions.DeleteEqual:
                
                var current = other.Head;
                while (current != null)
                {
                    if (!Contains(*current))
                    {
                        var newNode = MakeNode(current);
                        _tail->Next = newNode;
                    }

                    current = (TNode*)alpm_list_next((IntPtr)current);
                }
                return this;
            default:
                throw new ArgumentOutOfRangeException(nameof(options), options, null);
        }
    }

    private TNode* MakeNode(TNode* current)
    {
        var node = (TNode*)Marshal.AllocHGlobal(sizeof(TNode));
        node->Data = current->Data;
        node->Next = null;
        node->Prev = _tail;
        return node;
    }
}

public unsafe class AlpmListEnumerator<TNode>(AlpmList<TNode> list) : IEnumerator<TNode>
    where TNode : unmanaged, IAlpmListNode<TNode>
{
    private TNode* _current = null;
    private readonly TNode* _head = list.Head;
    private bool _disposed;

    public TNode Current => *_current;
    object IEnumerator.Current => Current;
    

    public bool MoveNext()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_head == null) return false;
            
        if (_current == null)
        {
            _current = _head;
            return true;
        }

        if ((TNode*)alpm_list_next((IntPtr)_current) == null) return false;

        _current = (TNode*)alpm_list_next((IntPtr)_current);
        return true;
    }

    public void Reset()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _current = null;
    }

    public void Dispose()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        // alpm_list_free((IntPtr)_head);
        // GC.SuppressFinalize(this);
        _disposed = true;
    }
}