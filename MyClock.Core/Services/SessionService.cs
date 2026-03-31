using System.Reactive.Linq;
using System.Reactive.Subjects;
using MyClock.Core.Interfaces;
using MyClock.Core.Models;

namespace MyClock.Core.Services;

public class SessionService : ISessionService, IDisposable
{
    private readonly Subject<FocusSession> _updated = new();
    private readonly Subject<FocusSession> _completed = new();
    private readonly IDisposable _clockSub;

    private FocusSession? _session;
    private DateTime? _pausedAt;
    private TimeSpan _accumulatedPauseDuration = TimeSpan.Zero;
    private bool _completedFired;

    public FocusSession? CurrentSession => _session;
    public IObservable<FocusSession> SessionUpdated => _updated.AsObservable();
    public IObservable<FocusSession> SessionCompleted => _completed.AsObservable();

    public SessionService(IClockService clockService)
    {
        _clockSub = clockService.CurrentTime.Subscribe(_ => OnTick());
    }

    private void OnTick()
    {
        if (_session is null || !_session.IsRunning || _session.IsCompleted) return;

        _updated.OnNext(_session);

        if (_session.IsCountdown && !_completedFired)
        {
            var remaining = GetRemainingTime();
            if (remaining.HasValue && remaining.Value <= TimeSpan.Zero)
            {
                _completedFired = true;
                _session.IsRunning = false;
                _session.EndTime = DateTime.Now;
                _completed.OnNext(_session);
            }
        }
    }

    public void StartSession(string name, TimeSpan? targetDuration)
    {
        _session = new FocusSession
        {
            Name = name,
            StartTime = DateTime.Now,
            TargetDuration = targetDuration,
            IsRunning = true,
            IsPaused = false
        };
        _pausedAt = null;
        _accumulatedPauseDuration = TimeSpan.Zero;
        _completedFired = false;
        _updated.OnNext(_session);
    }

    public void Pause()
    {
        if (_session is null || !_session.IsRunning || _session.IsPaused || _session.IsCompleted) return;
        _session.IsPaused = true;
        _pausedAt = DateTime.Now;
        _updated.OnNext(_session);
    }

    public void Resume()
    {
        if (_session is null || !_session.IsPaused) return;
        if (_pausedAt.HasValue)
            _accumulatedPauseDuration += DateTime.Now - _pausedAt.Value;
        _pausedAt = null;
        _session.IsPaused = false;
        _updated.OnNext(_session);
    }

    public void Reset()
    {
        if (_session is null) return;
        _session.IsRunning = false;
        _session.IsPaused = false;
        _session.EndTime = null;
        _pausedAt = null;
        _accumulatedPauseDuration = TimeSpan.Zero;
        _completedFired = false;
        _updated.OnNext(_session);
        _session = null;
    }

    public TimeSpan GetElapsedTime()
    {
        if (_session is null || !_session.IsRunning) return TimeSpan.Zero;

        var elapsed = DateTime.Now - _session.StartTime - _accumulatedPauseDuration;

        // Subtract current pause duration if paused right now
        if (_session.IsPaused && _pausedAt.HasValue)
            elapsed -= DateTime.Now - _pausedAt.Value;

        return elapsed < TimeSpan.Zero ? TimeSpan.Zero : elapsed;
    }

    public TimeSpan? GetRemainingTime()
    {
        if (_session?.TargetDuration is null) return null;
        var remaining = _session.TargetDuration.Value - GetElapsedTime();
        return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
    }

    public void Dispose()
    {
        _clockSub.Dispose();
        _updated.Dispose();
        _completed.Dispose();
    }
}
