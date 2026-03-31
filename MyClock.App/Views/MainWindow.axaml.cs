using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Rendering;

namespace MyClock.App.Views;

public partial class MainWindow : Window
{
    // Fixed offset of the pointer within the window at the moment drag started
    private Point _pointerOffset;
    private bool _isDragging;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnDragBorderPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        _isDragging = true;
        // Record how far the pointer is from the window's top-left corner
        _pointerOffset = e.GetPosition(this);
        e.Pointer.Capture((IInputElement)sender!);
    }

    private void OnDragBorderMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging) return;
        // Convert current pointer position to screen coordinates, then subtract
        // the fixed offset so the window top-left follows the pointer correctly
        var pointerOnScreen = ((IRenderRoot)this).PointToScreen(e.GetPosition(this));
        Position = new PixelPoint(
            pointerOnScreen.X - (int)_pointerOffset.X,
            pointerOnScreen.Y - (int)_pointerOffset.Y);
    }

    private void OnDragBorderReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isDragging = false;
        e.Pointer.Capture(null);
    }
}
