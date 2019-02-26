using System.ComponentModel;

namespace ProxyList
{
    /// <summary>
    /// Only compatible with .NET4.5 or later.
    /// </summary>
    public class ObservableClass : INotifyPropertyChanged, IObservableClass
    {
        #region Property
        /// <summary>
        /// The index in the list.
        /// </summary>
        public int Index { get; set; }
        #endregion

        #region Event
        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        protected internal void RaisePropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Method
        /// <summary>
        /// Used to set the property while also invoking the PropertyChanged event handler.
        /// </summary>
        /// <returns><c>true</c>, if property was set, <c>false</c> otherwise.</returns>
        /// <param name="storage">The private field that the property sets.</param>
        /// <param name="value">Should always be "value".</param>
        /// <param name="propertyName">Don't set this parameter.</param>
        protected bool SetProperty<T>(ref T storage, T value, string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }
            storage = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
        #endregion
    }

    /// <summary>
    /// The public interface to expose the index.
    /// </summary>
    public interface IObservableClass 
    { 
        int Index { get; set; }
    }

}
