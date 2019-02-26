using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;

namespace ProxyList
{
    public class BindingProxyList<T> : BindingList<T>, IBindingList where T : IObservableClass
    {
        private const string DefaultSortProperty = "Index";

        private bool currentValue = true;

        private BindingSourceList<T> Source { get; set; } = null;

        private BindingList<T> Proxy { get; } = new BindingList<T>();

        private Func<T, bool> Filter { get; set; } = null;

        private string SortBy { get; set; } = DefaultSortProperty;

        private PropertyDescriptorCollection PropertyDescriptorCollection { get; set; } = null;

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

        void Source_DeletedEvent(object sender, DeletedEventArgs<T> e)
        {
            Proxy.Remove(e.DeletedObject);
        }

        void Source_ListChanged(object sender, ListChangedEventArgs e)
        {
            T changedItem = default(T);
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    changedItem = Source[e.NewIndex];
                    if (Filter(changedItem))
                    {
                        BinaryInsert(changedItem);
                    }
                    break;
                case ListChangedType.ItemChanged:
                    changedItem = Source[e.NewIndex];
                    bool inProxy = Proxy.Contains(changedItem);
                    bool passesFilter = Filter(changedItem);
                    if (e.PropertyDescriptor.Name == SortBy)
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

        public void ApplySort(string sortBy, ListSortDirection direction)
        {
            PropertyDescriptor descriptor = PropertyDescriptorCollection.Find(sortBy, false);
            Sort(descriptor, direction);
        }

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

        public new bool AllowNew => false;

        public new bool AllowEdit => false;

        public new bool AllowRemove => false;

        public bool SupportsChangeNotification => true;

        public bool SupportsSearching => true;

        public bool SupportsSorting => true;

        public bool IsSorted => true;

        public PropertyDescriptor SortProperty { get; set; }

        public ListSortDirection SortDirection { get; set; }

        public bool IsReadOnly => true;

        public bool IsFixedSize => false;

        public new int Count => ((IBindingList)Proxy).Count;

        public object SyncRoot => ((IBindingList)Proxy).SyncRoot;

        public bool IsSynchronized => Synchronize;

        public new object this[int index] { get => ((IBindingList)Proxy)[index]; set => ((IBindingList)Proxy)[index] = value; }

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

        public new object AddNew()
        {
            throw new NotImplementedException();
        }

        public void AddIndex(PropertyDescriptor property)
        {
            throw new NotImplementedException();
        }

        public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            Sort(property, direction);
        }

        public int Find(PropertyDescriptor property, object key)
        {
            return ((IBindingList)Proxy).Find(property, key);
        }

        public void RemoveIndex(PropertyDescriptor property)
        {
            throw new NotImplementedException();
        }

        public void RemoveSort()
        {
            throw new NotImplementedException();
        }

        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public bool Contains(object value)
        {
            return ((IBindingList)Proxy).Contains(value);
        }

        public new void Clear()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(object value)
        {
            return ((IBindingList)Proxy).IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Remove(int index, object value)
        {
            throw new NotImplementedException();
        }

        public new void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            ((IBindingList)Proxy).CopyTo(array, index);
        }

        public new IEnumerator GetEnumerator()
        {
            return ((IBindingList)Proxy).GetEnumerator();
        }
    }
}
