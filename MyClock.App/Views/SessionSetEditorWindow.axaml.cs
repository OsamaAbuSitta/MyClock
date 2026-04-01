using System;
using Avalonia.Controls;
using MyClock.App.ViewModels;

namespace MyClock.App.Views;

public partial class SessionSetEditorWindow : Window
{
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is SessionSetEditorViewModel vm)
            vm.CloseRequested += Close;
    }
}
