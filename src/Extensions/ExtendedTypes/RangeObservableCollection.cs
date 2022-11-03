using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace FileTransfer.Extensions.ExtendedTypes;

public class RangeObservableCollection<T> : ObservableCollection<T>
{
    /// <summary>
    /// Adds a range of items to the <see cref="ObservableCollection{T}"/> and raises a notification event.
    /// </summary>
    /// <param name="items">An <see cref="IEnumerable{T}"/> of items to be added.</param>
    /// <exception cref="ArgumentNullException">Throws if <paramref name="items"/> is null.</exception>
    public void AddRange(IEnumerable<T> items)
    {
        if (items is null)
            throw new ArgumentNullException(nameof(items));
        
        this.CheckReentrancy();
        foreach(var item in items)
            this.Items.Add(item);
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}