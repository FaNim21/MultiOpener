using System.Collections.Generic;
using System.Collections.Specialized;

namespace MultiOpener.Utils;

public class ObservableLinkedList<T> : IEnumerable<T>, INotifyCollectionChanged
{
    private readonly LinkedList<T> _list;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public ObservableLinkedList()
    {
        _list = new LinkedList<T>();
    }

    public void Add(T item)
    {
        _list.AddFirst(item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
    }

    public bool Remove(T item)
    {
        bool removed = _list.Remove(item);
        if (removed) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
        return removed;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        throw new System.NotImplementedException();
    }
}