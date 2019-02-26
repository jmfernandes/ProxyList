using System;
using System.Runtime.Serialization;

namespace ProxyList
{
    public class StringValue : ObservableClass, ISerializable
    {
        private string v;

        private int c;

        private string y;

        public string Value { get { return v; } set { SetProperty(ref v, value); } }

        public int Count { get { return c; } set { SetProperty(ref c, value); } }

        public string Year { get { return y; } set { SetProperty(ref y, value); } }

        public StringValue(string s)
        {
            Value = s;
            Count = s.Length;
            Year = "2019";
        }

        public bool Contains(string s)
        {
            return (Value.Contains(s));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Type myType = GetType();
            foreach(System.Reflection.PropertyInfo currentProperty in myType.GetProperties())
            {
                info.AddValue(currentProperty.Name, currentProperty.GetValue(this, null));
            }
        }
    }
}
