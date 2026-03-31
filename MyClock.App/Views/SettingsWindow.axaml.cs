using System;
using Avalonia.Controls;
using MyClock.App.ViewModels;

namespace MyClock.App.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is SettingsWindowViewModel vm)
            vm.CloseRequested += Close;
    }
}
