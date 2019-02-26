using System;
using System.ComponentModel;
using System.Linq;

namespace ProxyList
{
    public class BindingSourceList<T> : BindingList<T> where T : IObservableClass
    {
        public event EventHandler<DeletedEventArgs<T>> DeletedEvent;

        public new void Add(T addedObject)
        {
            addedObject.Index = Count;
            base.Add(addedObject);
        }

        public new void Insert(int index, T insertedObject)
        {
            insertedObject.Index = index;
            for (int i = index; i < Count; i++)
            {
                this.ElementAt(i).Index = i + 1;
            }
            base.Insert(index, insertedObject);
        }

        public new bool Remove(T deletedObject)
        {
            if (deletedObject != null)
            {
                DeletedEvent?.Invoke(this, new DeletedEventArgs<T>(deletedObject));
                for (int i = deletedObject.Index; i < Count; i++)
                {
                    this.ElementAt(i).Index = i - 1;
                }
                return base.Remove(deletedObject);
            }
            return false;
        }

        public T FindObject(string property, object value)
        {
            T returnValue = default(T);
            object comparingObject = null;
            System.Reflection.PropertyInfo reflectedProperty = typeof(T).GetProperty(property);
            if (reflectedProperty == null)
            {
                throw new ArgumentException(string.Format("Property {0} does not exist for given type.", property));
            }
            foreach (T item in this)
            {
                comparingObject = reflectedProperty.GetValue(item, null);
                if (comparingObject.Equals(value))
                {
                    returnValue = item;
                    break;
                }
            }
            return returnValue;
        }
    }

    public class DeletedEventArgs<T> : EventArgs
    {
        public T DeletedObject { get; set; }

        public DeletedEventArgs(T deleted)
        {
            DeletedObject = deleted;
        }
    }
}
