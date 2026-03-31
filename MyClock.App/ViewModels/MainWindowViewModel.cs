using System;
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

    // Pomodoro state
    private bool _use24Hour;
    private PomodoroPhase _currentPhase = PomodoroPhase.Focus;
    private int _completedCycles = 0;
    private bool _pomodoroActive = false;

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

    // --- Commands ---
    public ReactiveCommand<Unit, Unit> StartCommand { get; }
    public ReactiveCommand<Unit, Unit> PauseCommand { get; }
    public ReactiveCommand<Unit, Unit> ResumeCommand { get; }
    public ReactiveCommand<Unit, Unit> ResetCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenSettingsCommand { get; }

    // Interaction: App.axaml.cs registers the handler that opens the dialog
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

        // Session completed → notification + Pomodoro advance
        _disposables.Add(_session.SessionCompleted
            .ObserveOn(AvaloniaScheduler.Instance)
            .Subscribe(s =>
            {
                _notification.ShowSessionCompleted(s.Name);
                _notification.PlayAlert();
                UpdateSessionDisplay(s);

                if (_settings.Current.PomodoroEnabled && _pomodoroActive)
                    AdvancePomodoroPhase();
            }));

        // Commands
        StartCommand = ReactiveCommand.Create(() =>
        {
            var s = _settings.Current;
            if (s.PomodoroEnabled)
            {
                _pomodoroActive = true;
                _completedCycles = 0;
                _currentPhase = PomodoroPhase.Focus;
                StartNextPomodoroPhase();
            }
            else
            {
                _pomodoroActive = false;
                var duration = s.LastUsedDurationSeconds.HasValue
                    ? TimeSpan.FromSeconds(s.LastUsedDurationSeconds.Value)
                    : (TimeSpan?)null;
                _session.StartSession("Focus Session", duration);
            }
        });

        PauseCommand  = ReactiveCommand.Create(_session.Pause);
        ResumeCommand = ReactiveCommand.Create(_session.Resume);

        ResetCommand = ReactiveCommand.Create(() =>
        {
            _pomodoroActive = false;
            _completedCycles = 0;
            _currentPhase = PomodoroPhase.Focus;
            _session.Reset();
            SessionStatusDisplay = "No session";
        });

        CloseCommand = ReactiveCommand.Create(() => Environment.Exit(0));

        OpenSettingsCommand = ReactiveCommand.CreateFromObservable(() =>
            OpenSettingsInteraction.Handle(Unit.Default));
    }

    private void AdvancePomodoroPhase()
    {
        var s = _settings.Current;
        if (_currentPhase == PomodoroPhase.Focus)
        {
            _completedCycles++;
            _currentPhase = (_completedCycles % s.CyclesBeforeLongBreak == 0)
                ? PomodoroPhase.LongBreak
                : PomodoroPhase.ShortBreak;
        }
        else
        {
            _currentPhase = PomodoroPhase.Focus;
        }
        StartNextPomodoroPhase();
    }

    private void StartNextPomodoroPhase()
    {
        var s = _settings.Current;
        var (name, minutes) = _currentPhase switch
        {
            PomodoroPhase.Focus      => ("Focus Session", s.FocusDurationMinutes),
            PomodoroPhase.ShortBreak => ("Short Break",   s.ShortBreakMinutes),
            PomodoroPhase.LongBreak  => ("Long Break",    s.LongBreakMinutes),
            _                        => ("Focus Session", s.FocusDurationMinutes)
        };
        _session.StartSession(name, TimeSpan.FromMinutes(minutes));
    }

    private string BuildPomodoroPhaseLabel()
    {
        var s = _settings.Current;
        int cycleInSet = (_completedCycles % s.CyclesBeforeLongBreak) + 1;
        return _currentPhase switch
        {
            PomodoroPhase.Focus      =>
                $"Focus · {Math.Min(cycleInSet, s.CyclesBeforeLongBreak)}/{s.CyclesBeforeLongBreak}",
            PomodoroPhase.ShortBreak => "Short Break",
            PomodoroPhase.LongBreak  => "Long Break",
            _                        => ""
        };
    }

    private void UpdateSessionDisplay(FocusSession s)
    {
        var elapsed   = _session.GetElapsedTime();
        var remaining = _session.GetRemainingTime();

        SessionTimeDisplay = s.IsCountdown && remaining.HasValue
            ? remaining.Value.ToString(@"mm\:ss")
            : elapsed.ToString(@"mm\:ss");

        if (_pomodoroActive)
        {
            var phaseLabel = BuildPomodoroPhaseLabel();
            SessionStatusDisplay = s.IsPaused
                ? $"{phaseLabel} — Paused"
                : phaseLabel;
        }
        else
        {
            SessionStatusDisplay = s switch
            {
                { IsCompleted: true } => $"{s.Name} — Done",
                { IsPaused: true }    => $"{s.Name} — Paused",
                { IsRunning: true }   => $"{s.Name} — Running",
                _                     => "No session"
            };
        }

        CanStart  = !s.IsRunning || s.IsCompleted;
        CanPause  = s.IsRunning && !s.IsPaused && !s.IsCompleted;
        CanResume = s.IsPaused;
        CanReset  = s.IsRunning || s.IsPaused || s.IsCompleted;
    }

    // Called by App.axaml.cs after settings are saved
    public void OnSettingsSaved()
    {
        _use24Hour = _settings.Current.Use24HourFormat;
        // Opacity is applied to the window directly by App.axaml.cs
    }

    public void Dispose() => _disposables.Dispose();
}
