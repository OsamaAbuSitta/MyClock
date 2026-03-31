using System;
using MyClock.Infrastructure.Services;
using ReactiveUI;

namespace MyClock.App.ViewModels;

public class SettingsWindowViewModel : ViewModelBase
{
    private readonly SettingsService _settingsService;

    // Raised when the dialog should close (Save or Cancel)
    public event Action? CloseRequested;
    public bool Saved { get; private set; }

    // Time format
    private bool _use24HourFormat;
    public bool Use24HourFormat
    {
        get => _use24HourFormat;
        set => this.RaiseAndSetIfChanged(ref _use24HourFormat, value);
    }

    // Opacity
    private double _opacity;
    public double Opacity
    {
        get => _opacity;
        set => this.RaiseAndSetIfChanged(ref _opacity, value);
    }

    // Pomodoro toggle
    private bool _pomodoroEnabled;
    public bool PomodoroEnabled
    {
        get => _pomodoroEnabled;
        set => this.RaiseAndSetIfChanged(ref _pomodoroEnabled, value);
    }

    // Duration properties use decimal to match Avalonia NumericUpDown.Value (decimal?)
    private decimal _focusDurationMinutes;
    public decimal FocusDurationMinutes
    {
        get => _focusDurationMinutes;
        set => this.RaiseAndSetIfChanged(ref _focusDurationMinutes, value);
    }

    private decimal _shortBreakMinutes;
    public decimal ShortBreakMinutes
    {
        get => _shortBreakMinutes;
        set => this.RaiseAndSetIfChanged(ref _shortBreakMinutes, value);
    }

    private decimal _longBreakMinutes;
    public decimal LongBreakMinutes
    {
        get => _longBreakMinutes;
        set => this.RaiseAndSetIfChanged(ref _longBreakMinutes, value);
    }

    private decimal _cyclesBeforeLongBreak;
    public decimal CyclesBeforeLongBreak
    {
        get => _cyclesBeforeLongBreak;
        set => this.RaiseAndSetIfChanged(ref _cyclesBeforeLongBreak, value);
    }

    // Commands
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> SaveCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> CancelCommand { get; }

    public SettingsWindowViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        LoadFromCurrent();

        SaveCommand   = ReactiveCommand.Create(ExecuteSave);
        CancelCommand = ReactiveCommand.Create(ExecuteCancel);
    }

    private void LoadFromCurrent()
    {
        var s = _settingsService.Current;
        Use24HourFormat        = s.Use24HourFormat;
        Opacity                = s.Opacity;
        PomodoroEnabled        = s.PomodoroEnabled;
        FocusDurationMinutes   = s.FocusDurationMinutes;
        ShortBreakMinutes      = s.ShortBreakMinutes;
        LongBreakMinutes       = s.LongBreakMinutes;
        CyclesBeforeLongBreak  = s.CyclesBeforeLongBreak;
    }

    private void ExecuteSave()
    {
        var s = _settingsService.Current;
        s.Use24HourFormat       = Use24HourFormat;
        s.Opacity               = Opacity;
        s.PomodoroEnabled       = PomodoroEnabled;
        s.FocusDurationMinutes  = (int)FocusDurationMinutes;
        s.ShortBreakMinutes     = (int)ShortBreakMinutes;
        s.LongBreakMinutes      = (int)LongBreakMinutes;
        s.CyclesBeforeLongBreak = (int)CyclesBeforeLongBreak;
        _settingsService.Save();
        Saved = true;
        CloseRequested?.Invoke();
    }

    private void ExecuteCancel()
    {
        Saved = false;
        CloseRequested?.Invoke();
    }
}
