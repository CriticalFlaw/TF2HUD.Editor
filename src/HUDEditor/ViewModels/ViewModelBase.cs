using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace HUDEditor.ViewModels;

public class ViewModelBase : ObservableObject, IDisposable
{
    public virtual void Dispose()
    {

    }
}