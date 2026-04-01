using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Rendering;

namespace MyClock.App.Views;

public partial class MainWindow : Window
{
    private Point _pointerOffset;
    private bool _isDragging;
    private bool _dragThresholdReached;
    private const double DragThreshold = 4; // pixels before drag activates

    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnDragBorderPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        _isDragging = true;
        _dragThresholdReached = false;
        _pointerOffset = e.GetPosition(this);
        e.Pointer.Capture((IInputElement)sender!);
    }

    private void OnDragBorderMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging) return;

        var currentPos = e.GetPosition(this);

        // Don't move until the pointer has travelled past the threshold —
        // this prevents a plain click from nudging the window position.
        if (!_dragThresholdReached)
        {
            var delta = currentPos - _pointerOffset;
            if (Math.Abs(delta.X) < DragThreshold && Math.Abs(delta.Y) < DragThreshold)
                return;
            _dragThresholdReached = true;
        }

        var pointerOnScreen = ((IRenderRoot)this).PointToScreen(currentPos);
        Position = new PixelPoint(
            pointerOnScreen.X - (int)_pointerOffset.X,
            pointerOnScreen.Y - (int)_pointerOffset.Y);
    }

    private void OnDragBorderReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isDragging = false;
        _dragThresholdReached = false;
        e.Pointer.Capture(null);
    }
}
