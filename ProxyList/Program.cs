using System;
using System.ComponentModel;

namespace ProxyList
{
    class Program
    {
        static void Main(string[] args)
        {
            BindingSourceList<StringValue> total = new BindingSourceList<StringValue>
            {
                new StringValue("one"),
                new StringValue("two"),
                new StringValue("three"),
                new StringValue("four"),
                new StringValue("five"),
                new StringValue("six")
            };
            Func<StringValue, bool> filter = (t => t.Contains("e"));
            BindingProxyList<StringValue> Proxy = new BindingProxyList<StringValue>(total, filter, "Value",ListSortDirection.Descending);
            foreach (StringValue item in Proxy)
            {
                Console.WriteLine(item.Value);
            }
        }
    }
}
