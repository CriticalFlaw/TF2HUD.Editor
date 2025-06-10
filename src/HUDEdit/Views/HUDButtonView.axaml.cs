using Avalonia.Controls;
using Avalonia.VisualTree;
using HUDEdit.ViewModels;
using System;

namespace HUDEdit.Views;

public partial class HUDButtonView : UserControl
{
    private DateTime _lastClickTime = DateTime.MinValue;
    private const int DoubleClickThresholdMs = 300;

    public HUDButtonView()
    {
        InitializeComponent();
    }

    private void Border_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (DataContext is not HUDButtonViewModel hudVM) return;

        var mainVM = (this.GetVisualRoot() as Window)?.DataContext as MainWindowViewModel;
        if (mainVM == null) return;

        var now = DateTime.UtcNow;
        var interval = (now - _lastClickTime).TotalMilliseconds;
        _lastClickTime = now;

        if (interval < DoubleClickThresholdMs)
        {
            if (mainVM.SelectHUDCommand.CanExecute(hudVM.Hud))
                mainVM.SelectHUDCommand.Execute(hudVM.Hud);
        }
        else
        {
            if (mainVM.HighlightHUDCommand.CanExecute(hudVM.Hud))
                mainVM.HighlightHUDCommand.Execute(hudVM.Hud);
        }
    }
}