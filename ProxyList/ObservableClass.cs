﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProxyList
{
    public class ObservableClass : INotifyPropertyChanged, IObservableClass
    {
        public int Index { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected internal void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            OnPropertyChanged(propertyName);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {

        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }
            storage = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
    }

    public interface IObservableClass 
    { 
        int Index { get; set; }
    }

}