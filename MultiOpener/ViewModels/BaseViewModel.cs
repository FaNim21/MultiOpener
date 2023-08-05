using System;
using System.ComponentModel;
using System.Windows;

namespace MultiOpener.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;


        protected void OnPropertyChanged(string propertyName)
        {
            if (Application.Current.Dispatcher.CheckAccess())
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            else
                Application.Current.Dispatcher.Invoke(delegate { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); });
        }

        public virtual void Dispose()
        {

        }
    }
}
