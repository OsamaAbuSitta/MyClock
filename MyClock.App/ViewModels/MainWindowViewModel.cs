using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using MyClock.Core.Interfaces;
using MyClock.Core.Models;
using MyClock.Infrastructure.Services;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace MyClock.App.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly IClockService _clock;
    private readonly ISessionService _session;
    private readonly SettingsService _settings;
    private readonly INotificationService _notification;
    private readonly CompositeDisposable _disposables = new();

    // Timer state
    private bool _use24Hour;
    private SessionSet? _activeSet;
    private int _currentSessionIndex = 0;
    private bool _waitingForNext = false;

    // --- Bindable properties ---
    private string _currentTimeDisplay = "--:--:--";
    public string CurrentTimeDisplay
    {
        get => _currentTimeDisplay;
        private set => this.RaiseAndSetIfChanged(ref _currentTimeDisplay, value);
    }

    private string _sessionTimeDisplay = "00:00";
    public string SessionTimeDisplay
    {
        get => _sessionTimeDisplay;
        private set => this.RaiseAndSetIfChanged(ref _sessionTimeDisplay, value);
    }

    private string _sessionStatusDisplay = "No session";
    public string SessionStatusDisplay
    {
        get => _sessionStatusDisplay;
        private set => this.RaiseAndSetIfChanged(ref _sessionStatusDisplay, value);
    }

    private bool _canStart = true;
    public bool CanStart
    {
        get => _canStart;
        private set => this.RaiseAndSetIfChanged(ref _canStart, value);
    }

    private bool _canPause;
    public bool CanPause
    {
        get => _canPause;
        private set => this.RaiseAndSetIfChanged(ref _canPause, value);
    }

    private bool _canResume;
    public bool CanResume
    {
        get => _canResume;
        private set => this.RaiseAndSetIfChanged(ref _canResume, value);
    }

    private bool _canReset;
    public bool CanReset
    {
        get => _canReset;
        private set => this.RaiseAndSetIfChanged(ref _canReset, value);
    }

    private bool _canGoNext;
    public bool CanGoNext
    {
        get => _canGoNext;
        private set => this.RaiseAndSetIfChanged(ref _canGoNext, value);
    }

    private bool _showClock = true;
    public bool ShowClock
    {
        get => _showClock;
        private set => this.RaiseAndSetIfChanged(ref _showClock, value);
    }

    // --- Commands ---
    public ReactiveCommand<Unit, Unit> StartCommand { get; }
    public ReactiveCommand<Unit, Unit> PauseCommand { get; }
    public ReactiveCommand<Unit, Unit> ResumeCommand { get; }
    public ReactiveCommand<Unit, Unit> ResetCommand { get; }
    public ReactiveCommand<Unit, Unit> GoNextCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenSettingsCommand { get; }

    public Interaction<Unit, Unit> OpenSettingsInteraction { get; } = new();

    public MainWindowViewModel(
        IClockService clock,
        ISessionService session,
        SettingsService settings,
        INotificationService notification)
    {
        _clock = clock;
        _session = session;
        _settings = settings;
        _notification = notification;

        _use24Hour = settings.Current.Use24HourFormat;
        _showClock = settings.Current.ShowClock;
        LoadActiveSet();

        // Clock ticks → CurrentTimeDisplay
        _disposables.Add(_clock.CurrentTime
            .ObserveOn(AvaloniaScheduler.Instance)
            .Subscribe(dt =>
            {
                CurrentTimeDisplay = _use24Hour
                    ? dt.ToString("HH:mm:ss")
                    : dt.ToString("hh:mm:ss tt");
            }));

        // Session updates → UI state
        _disposables.Add(_session.SessionUpdated
            .ObserveOn(AvaloniaScheduler.Instance)
            .Subscribe(UpdateSessionDisplay));

        // Session completed → notification
        _disposables.Add(_session.SessionCompleted
            .ObserveOn(AvaloniaScheduler.Instance)
            .Subscribe(s =>
            {
                _notification.PlayAlert();
                UpdateSessionDisplay(s);

                if (_activeSet is not null)
                {
                    int nextIndex = (_currentSessionIndex + 1) % _activeSet.Sessions.Count;
                    var nextName = _activeSet.Sessions[nextIndex].Name;
                    _notification.ShowSessionCompleted(
                        $"{s.Name} done!",
                        $"Ready for '{nextName}'? Click Next.");
                    _waitingForNext = true;
                    CanGoNext = true;
                }
                else
                {
                    _notification.ShowSessionCompleted("Session Complete", $"{s.Name} finished!");
                }
            }));

        // Commands
        StartCommand = ReactiveCommand.Create(() =>
        {
            _waitingForNext = false;
            StartCurrentSession();
        });

        PauseCommand  = ReactiveCommand.Create(_session.Pause);
        ResumeCommand = ReactiveCommand.Create(_session.Resume);

        ResetCommand = ReactiveCommand.Create(() =>
        {
            _waitingForNext = false;
            CanGoNext = _activeSet is not null;
            _session.Reset();
            if (_activeSet is null)
                SessionStatusDisplay = "No session";
        });

        GoNextCommand = ReactiveCommand.Create(() =>
        {
            if (_activeSet is null) return;
            _currentSessionIndex = (_currentSessionIndex + 1) % _activeSet.Sessions.Count;
            _waitingForNext = false;
            StartCurrentSession();
        });

        CloseCommand = ReactiveCommand.Create(() => Environment.Exit(0));

        OpenSettingsCommand = ReactiveCommand.CreateFromObservable(() =>
            OpenSettingsInteraction.Handle(Unit.Default));
    }

    private void LoadActiveSet()
    {
        var s = _settings.Current;
        if (s.TimerMode == TimerMode.SessionSet && s.ActiveSessionSetId is not null)
        {
            _activeSet = s.SessionSets.FirstOrDefault(x => x.Id == s.ActiveSessionSetId);
        }
        else
        {
            _activeSet = null;
        }
        _currentSessionIndex = 0;
        _waitingForNext = false;
        CanGoNext = _activeSet is not null;
    }

    private void StartCurrentSession()
    {
        if (_activeSet is not null && _activeSet.Sessions.Count > 0)
        {
            var item = _activeSet.Sessions[_currentSessionIndex];
            _session.StartSession(item.Name, TimeSpan.FromMinutes(item.DurationMinutes));
        }
        else
        {
            _session.StartSession("Timer", null);
        }
    }

    private void UpdateSessionDisplay(FocusSession s)
    {
        var elapsed   = _session.GetElapsedTime();
        var remaining = _session.GetRemainingTime();

        SessionTimeDisplay = s.IsCountdown && remaining.HasValue
            ? remaining.Value.ToString(@"mm\:ss")
            : elapsed.ToString(@"mm\:ss");

        if (_activeSet is not null)
        {
            var total = _activeSet.Sessions.Count;
            var sessionName = _activeSet.Sessions.Count > _currentSessionIndex
                ? _activeSet.Sessions[_currentSessionIndex].Name
                : s.Name;

            var label = $"{_activeSet.Name} · {sessionName} ({_currentSessionIndex + 1}/{total})";
            SessionStatusDisplay = s.IsPaused ? $"{label} — Paused" : label;
        }
        else
        {
            SessionStatusDisplay = s switch
            {
                { IsCompleted: true } => $"{s.Name} — Done",
                { IsPaused: true }    => $"{s.Name} — Paused",
                { IsRunning: true }   => s.IsCountdown ? $"{s.Name} — Running" : "Timer — Running",
                _                     => "No session"
            };
        }

        CanStart  = (!s.IsRunning || s.IsCompleted) && !_waitingForNext;
        CanPause  = s.IsRunning && !s.IsPaused && !s.IsCompleted;
        CanResume = s.IsPaused;
        CanReset  = s.IsRunning || s.IsPaused || s.IsCompleted || _waitingForNext;
        CanGoNext = _activeSet is not null && (s.IsRunning || s.IsCompleted || _waitingForNext);
    }

    // Called by App.axaml.cs after settings are saved
    public void OnSettingsSaved()
    {
        var oldSetId = _activeSet?.Id;
        _use24Hour = _settings.Current.Use24HourFormat;
        ShowClock  = _settings.Current.ShowClock;
        LoadActiveSet();
        // Reset index when the active set changed
        if (_activeSet?.Id != oldSetId)
            _currentSessionIndex = 0;
    }

    public void Dispose() => _disposables.Dispose();
}
