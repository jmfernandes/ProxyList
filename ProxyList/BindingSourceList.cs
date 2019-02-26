using System;
using System.ComponentModel;
using System.Linq;

namespace ProxyList
{
    /// <summary>
    /// Binding source list.
    /// </summary>
    public class BindingSourceList<T> : BindingList<T> where T : IObservableClass
    {
        /// <summary>
        /// Occurs when item is deleted.
        /// </summary>
        public event EventHandler<DeletedEventArgs<T>> DeletedEvent;

        /// <summary>
        /// Add the specified addedObject.
        /// </summary>
        /// <param name="addedObject">Added object.</param>
        public new void Add(T addedObject)
        {
            addedObject.Index = Count;
            base.Add(addedObject);
        }

        /// <summary>
        /// Insert the specified index and insertedObject.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <param name="insertedObject">Inserted object.</param>
        public new void Insert(int index, T insertedObject)
        {
            insertedObject.Index = index;
            for (int i = index; i < Count; i++)
            {
                this.ElementAt(i).Index = i + 1;
            }
            base.Insert(index, insertedObject);
        }

        /// <summary>
        /// Remove the specified deletedObject.
        /// </summary>
        /// <returns>The remove.</returns>
        /// <param name="deletedObject">Deleted object.</param>
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

        /// <summary>
        /// Finds the object.
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="property">Property.</param>
        /// <param name="value">Value.</param>
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

    /// <summary>
    /// Deleted event arguments.
    /// </summary>
    public class DeletedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets or sets the deleted object.
        /// </summary>
        /// <value>The deleted object.</value>
        public T DeletedObject { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ProxyList.DeletedEventArgs`1"/> class.
        /// </summary>
        /// <param name="deleted">Deleted.</param>
        public DeletedEventArgs(T deleted)
        {
            DeletedObject = deleted;
        }
    }
}
