using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HUDEdit.ViewModels;

public class ViewModelBase : ObservableObject, INotifyPropertyChanged, IDisposable
{
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Invokes a WPF Binding update of a property.
    /// </summary>
    /// <param name="propertyName">Name of property to update</param>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public virtual void Dispose()
    {

    }
}