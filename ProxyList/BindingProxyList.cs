using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;

namespace ProxyList
{
    /// <summary>
    /// Binding proxy list. 
    /// </summary>
    public class BindingProxyList<T> : BindingList<T>, IBindingList where T : IObservableClass
    {
        #region Private Fields
        /// <summary>
        /// The default sort property.
        /// </summary>
        private const string DefaultSortProperty = "Index";

        /// <summary>
        /// The current value of whether or not the list is synchronized.
        /// </summary>
        private bool currentValue = true;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        private BindingSourceList<T> Source { get; set; } = null;

        /// <summary>
        /// Gets the proxy.
        /// </summary>
        private BindingList<T> Proxy { get; } = new BindingList<T>();

        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        private Func<T, bool> Filter { get; set; } = null;

        /// <summary>
        /// Gets or sets the sort by.
        /// </summary>
        private string SortBy { get; set; } = DefaultSortProperty;

        /// <summary>
        /// Gets or sets the property descriptor collection.
        /// </summary>
        private PropertyDescriptorCollection PropertyDescriptorCollection { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:ProxyList.BindingProxyList`1"/> is synchronize.
        /// </summary>
        /// <value><c>true</c> if synchronize; otherwise, <c>false</c>.</value>
        public bool Synchronize
        {
            protected get
            {
                return currentValue;
            }
            set
            {
                if (value != currentValue)
                {
                    if (value == false)
                    {
                        Source.ListChanged -= Source_ListChanged;
                        Source.DeletedEvent -= Source_DeletedEvent;
                        currentValue = value;
                    }
                    else
                    {
                        Source.ListChanged += Source_ListChanged;
                        Source.DeletedEvent += Source_DeletedEvent;
                        Source_ListChanged(null, new ListChangedEventArgs(ListChangedType.Reset, -1));
                        currentValue = value;
                    }
                }
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ProxyList.BindingProxyList`1"/> class.
        /// </summary>
        /// <param name="source">Source.</param>
        /// <param name="filter">Filter.</param>
        /// <param name="sortBy">Sort by.</param>
        /// <param name="sortDirection">Sort direction.</param>
        public BindingProxyList(BindingSourceList<T> source, Func<T, bool> filter = null, string sortBy = "", ListSortDirection sortDirection = ListSortDirection.Ascending) : base(source)
        {
            Source = source;
            Filter = filter;
            SortBy = sortBy;
            SortDirection = sortDirection;
            PropertyDescriptorCollection = TypeDescriptor.GetProperties(typeof(T));
            SortProperty = PropertyDescriptorCollection.Find(sortBy, false);
            if (SortProperty == null)
            {
                SortProperty = PropertyDescriptorCollection.Find(DefaultSortProperty, false);
                SortBy = DefaultSortProperty;
            }
            if (Filter == null)
            {
                Filter = (f => true);
            }
            source.ListChanged += Source_ListChanged;
            source.DeletedEvent += Source_DeletedEvent;
            Source_ListChanged(null, new ListChangedEventArgs(ListChangedType.Reset, -1));
        }
        #endregion

        #region Events
        /// <summary>
        /// Sources the deleted event.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="args">Arguments.</param>
        void Source_DeletedEvent(object sender, DeletedEventArgs<T> args)
        {
            Proxy.Remove(args.DeletedObject);
        }

        /// <summary>
        /// Sources the list changed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="args">Arguments.</param>
        void Source_ListChanged(object sender, ListChangedEventArgs args)
        {
            T changedItem = default(T);
            switch (args.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    changedItem = Source[args.NewIndex];
                    if (Filter(changedItem))
                    {
                        BinaryInsert(changedItem);
                    }
                    break;
                case ListChangedType.ItemChanged:
                    changedItem = Source[args.NewIndex];
                    bool inProxy = Proxy.Contains(changedItem);
                    bool passesFilter = Filter(changedItem);
                    if (args.PropertyDescriptor.Name == SortBy)
                    {
                        if (inProxy)
                        {
                            Proxy.Remove(changedItem);
                        }
                        if (passesFilter)
                        {
                            BinaryInsert(changedItem);
                        }
                    }
                    else if (inProxy && !passesFilter)
                    {
                        Proxy.Remove(changedItem);
                    }
                    else if (passesFilter && !inProxy)
                    {
                        BinaryInsert(changedItem);
                    }
                    break;
                case ListChangedType.Reset:
                    Proxy.Clear();
                    foreach(T item in Source.Where(Filter))
                    {
                        BinaryInsert(item);
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the sort.
        /// </summary>
        /// <param name="sortBy">Sort by.</param>
        /// <param name="direction">Direction.</param>
        public void ApplySort(string sortBy, ListSortDirection direction)
        {
            PropertyDescriptor descriptor = PropertyDescriptorCollection.Find(sortBy, false);
            Sort(descriptor, direction);
        }

        /// <summary>
        /// Sort the specified descriptor and direction.
        /// </summary>
        /// <param name="descriptor">Descriptor.</param>
        /// <param name="direction">Direction.</param>
        private void Sort(PropertyDescriptor descriptor, ListSortDirection direction)
        {
            if (descriptor != null)
            {
                SortProperty = descriptor;
                SortDirection = direction;
                SortBy = descriptor.Name;
                Source_ListChanged(null, new ListChangedEventArgs(ListChangedType.Reset, -1));
            }
        }

        /// <summary>
        /// Binaries the insert.
        /// </summary>
        /// <param name="value">Value.</param>
        public void BinaryInsert(T value)
        {
            object value1 = null;
            object value2 = null;
            int lower = 0;
            int middle = 0;
            int upper = Proxy.Count - 1;
            int comparisonResult = 0;
            while (lower <= upper)
            {
                middle = lower + (upper - lower) / 2;
                value1 = SortProperty.GetValue(value);
                value2 = SortProperty.GetValue(Proxy[middle]);
                comparisonResult = (SortDirection == ListSortDirection.Ascending) ? Comparer.Default.Compare(value1, value2) : Comparer.Default.Compare(value2, value1);
                if (comparisonResult == 0)
                {
                    lower = middle;
                    break;
                }
                else if (comparisonResult < 0)
                {
                    upper = middle - 1;
                }
                else
                {
                    lower = middle + 1;
                }
            }
            Proxy.Insert(lower, value);
        }
        #endregion

        #region IBindingList Members
        /// <summary>
        /// Gets a value indicating whether this <see cref="T:ProxyList.BindingProxyList`1"/> allow new.
        /// </summary>
        public new bool AllowNew => false;

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:ProxyList.BindingProxyList`1"/> allow edit.
        /// </summary>
        public new bool AllowEdit => false;

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:ProxyList.BindingProxyList`1"/> allow remove.
        /// </summary>
        public new bool AllowRemove => false;

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:ProxyList.BindingProxyList`1"/> supports change notification.
        /// </summary>
        public bool SupportsChangeNotification => true;

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:ProxyList.BindingProxyList`1"/> supports searching.
        /// </summary>
        public bool SupportsSearching => true;

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:ProxyList.BindingProxyList`1"/> supports sorting.
        /// </summary>
        public bool SupportsSorting => true;

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:ProxyList.BindingProxyList`1"/> is sorted.
        /// </summary>
        public bool IsSorted => true;

        /// <summary>
        /// Gets or sets the sort property.
        /// </summary>
        public PropertyDescriptor SortProperty { get; set; }

        /// <summary>
        /// Gets or sets the sort direction.
        /// </summary>
        public ListSortDirection SortDirection { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:ProxyList.BindingProxyList`1"/> is read only.
        /// </summary>
        public bool IsReadOnly => true;

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:ProxyList.BindingProxyList`1"/> is fixed size.
        /// </summary>
        public bool IsFixedSize => false;

        /// <summary>
        /// Gets the count.
        /// </summary>
        public new int Count => ((IBindingList)Proxy).Count;

        /// <summary>
        /// Gets the sync root.
        /// </summary>
        public object SyncRoot => ((IBindingList)Proxy).SyncRoot;

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:ProxyList.BindingProxyList`1"/> is synchronized.
        /// </summary>
        public bool IsSynchronized => Synchronize;

        /// <summary>
        /// Gets or sets the <see cref="T:ProxyList.BindingProxyList`1"/> at the specified index.
        /// </summary>
        public new object this[int index] { get => ((IBindingList)Proxy)[index]; set => ((IBindingList)Proxy)[index] = value; }

        /// <summary>
        /// Occurs when list changed.
        /// </summary>
        public new event ListChangedEventHandler ListChanged
        {
            add
            {
                ((IBindingList)Proxy).ListChanged += value;
            }
            remove
            {
                ((IBindingList)Proxy).ListChanged -= value;
            }
        }

        /// <summary>
        /// Not Implemented.
        /// </summary>
        public new object AddNew()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///  Not Implemented.
        /// </summary>
        public void AddIndex(PropertyDescriptor property)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Applies the sort.
        /// </summary>
        /// <param name="property">Property.</param>
        /// <param name="direction">Direction.</param>
        public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            Sort(property, direction);
        }

        /// <summary>
        /// Find the specified property and key.
        /// </summary>
        /// <returns>The find.</returns>
        /// <param name="property">Property.</param>
        /// <param name="key">Key.</param>
        public int Find(PropertyDescriptor property, object key)
        {
            return ((IBindingList)Proxy).Find(property, key);
        }

        /// <summary>
        /// Not Implemented.
        /// </summary>
        public void RemoveIndex(PropertyDescriptor property)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented.
        /// </summary>
        public void RemoveSort()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented.
        /// </summary>
        /// <returns>The add.</returns>
        /// <param name="value">Value.</param>
        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Contains the specified value.
        /// </summary>
        /// <returns>The contains.</returns>
        /// <param name="value">Value.</param>
        public bool Contains(object value)
        {
            return ((IBindingList)Proxy).Contains(value);
        }

        /// <summary>
        /// Not Implemented.
        /// </summary>
        public new void Clear()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets index of object.
        /// </summary>
        /// <returns>The of.</returns>
        /// <param name="value">Value.</param>
        public int IndexOf(object value)
        {
            return ((IBindingList)Proxy).IndexOf(value);
        }

        /// <summary>
        /// Not Implemented.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <param name="value">Value.</param>
        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <param name="value">Value.</param>
        public void Remove(int index, object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented.
        /// </summary>
        /// <param name="index">Index.</param>
        public new void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">Array.</param>
        /// <param name="index">Index.</param>
        public void CopyTo(Array array, int index)
        {
            ((IBindingList)Proxy).CopyTo(array, index);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public new IEnumerator GetEnumerator()
        {
            return ((IBindingList)Proxy).GetEnumerator();
        }
        #endregion
    }
}
