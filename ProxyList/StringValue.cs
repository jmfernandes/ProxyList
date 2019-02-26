using System;
using System.Runtime.Serialization;

namespace ProxyList
{
    /// <summary>
    /// Example class to show how BindingProxyList works.
    /// </summary>
    public class StringValue : ObservableClass, ISerializable
    {
        #region Private Fields
        /// <summary>
        /// The value string.
        /// </summary>
        private string v;

        /// <summary>
        /// The count int.
        /// </summary>
        private int c;

        /// <summary>
        /// The year string.
        /// </summary>
        private string y;
        #endregion

        #region Properties
        /// <summary>
        /// The property for the value.
        /// </summary>
        public string Value { get { return v; } set { SetProperty(ref v, value); } }

        /// <summary>
        /// The property for the Count.
        /// </summary>
        public int Count { get { return c; } set { SetProperty(ref c, value); } }

        /// <summary>
        /// The property for the Year.
        /// </summary>
        public string Year { get { return y; } set { SetProperty(ref y, value); } }
        #endregion

        #region Constructor
        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="s">The string to store as Value.</param>
        public StringValue(string s)
        {
            Value = s;
            Count = s.Length;
            Year = "2019";
        }
        #endregion

        #region Methods
        /// <summary>
        /// Determines if Value contains a certain string.
        /// </summary>
        /// <returns>Returns true if the string is in Value.</returns>
        /// <param name="s">The string to see if its in Value.</param>
        public bool Contains(string s)
        {
            return (Value.Contains(s));
        }
        #endregion

        #region ISerializable Members
        /// <summary>
        /// Gets the object data.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Type myType = GetType();
            foreach(System.Reflection.PropertyInfo currentProperty in myType.GetProperties())
            {
                info.AddValue(currentProperty.Name, currentProperty.GetValue(this, null));
            }
        }
        #endregion
    }
}
