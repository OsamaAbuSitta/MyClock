using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using MyClock.Core.Models;
using MyClock.Infrastructure.Services;
using ReactiveUI;

namespace MyClock.App.ViewModels;

public class SettingsWindowViewModel : ViewModelBase
{
    private readonly SettingsService _settingsService;

    public event Action? CloseRequested;
    public bool Saved { get; private set; }

    // Clock visibility
    private bool _showClock;
    public bool ShowClock
    {
        get => _showClock;
        set => this.RaiseAndSetIfChanged(ref _showClock, value);
    }

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

    // Timer mode
    private bool _isFreeMode;
    public bool IsFreeMode
    {
        get => _isFreeMode;
        set
        {
            this.RaiseAndSetIfChanged(ref _isFreeMode, value);
            if (value) IsSessionSetMode = false;
        }
    }

    private bool _isSessionSetMode;
    public bool IsSessionSetMode
    {
        get => _isSessionSetMode;
        set
        {
            this.RaiseAndSetIfChanged(ref _isSessionSetMode, value);
            if (value) IsFreeMode = false;
        }
    }

    // Session sets
    public ObservableCollection<SessionSet> SessionSets { get; } = new();

    private SessionSet? _selectedSessionSet;
    public SessionSet? SelectedSessionSet
    {
        get => _selectedSessionSet;
        set => this.RaiseAndSetIfChanged(ref _selectedSessionSet, value);
    }

    // Commands
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> RestoreDefaultsCommand { get; }
    public ReactiveCommand<Unit, Unit> ManageSetsCommand { get; }

    // Interaction to open the session set editor
    public Interaction<Unit, Unit> OpenEditorInteraction { get; } = new();

    public SettingsWindowViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        LoadFromCurrent();

        SaveCommand            = ReactiveCommand.Create(ExecuteSave);
        CancelCommand          = ReactiveCommand.Create(ExecuteCancel);
        RestoreDefaultsCommand = ReactiveCommand.Create(ExecuteRestoreDefaults);
        ManageSetsCommand      = ReactiveCommand.CreateFromObservable(() =>
            OpenEditorInteraction.Handle(Unit.Default));
    }

    private void LoadFromCurrent()
    {
        var s = _settingsService.Current;
        ShowClock       = s.ShowClock;
        Use24HourFormat = s.Use24HourFormat;
        Opacity         = s.Opacity;
        IsFreeMode      = s.TimerMode == TimerMode.Free;
        IsSessionSetMode = s.TimerMode == TimerMode.SessionSet;

        SessionSets.Clear();
        foreach (var set in s.SessionSets)
            SessionSets.Add(set);

        SelectedSessionSet = s.ActiveSessionSetId is not null
            ? s.SessionSets.FirstOrDefault(x => x.Id == s.ActiveSessionSetId)
            : s.SessionSets.FirstOrDefault();
    }

    private void ExecuteSave()
    {
        var s = _settingsService.Current;
        s.ShowClock          = ShowClock;
        s.Use24HourFormat    = Use24HourFormat;
        s.Opacity            = Opacity;
        s.TimerMode          = IsSessionSetMode ? TimerMode.SessionSet : TimerMode.Free;
        s.ActiveSessionSetId = SelectedSessionSet?.Id;
        _settingsService.Save();
        Saved = true;
        CloseRequested?.Invoke();
    }

    private void ExecuteCancel()
    {
        Saved = false;
        CloseRequested?.Invoke();
    }

    private void ExecuteRestoreDefaults()
    {
        _settingsService.ResetAllDefaults();
        // Refresh the list
        SessionSets.Clear();
        foreach (var set in _settingsService.Current.SessionSets)
            SessionSets.Add(set);
        // Re-select if current selection was a built-in that got restored
        if (SelectedSessionSet is not null)
            SelectedSessionSet = SessionSets.FirstOrDefault(x => x.Id == SelectedSessionSet.Id)
                                 ?? SessionSets.FirstOrDefault();
    }
}
