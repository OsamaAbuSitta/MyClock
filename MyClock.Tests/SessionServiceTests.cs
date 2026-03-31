using System.Reactive.Subjects;
using MyClock.Core.Interfaces;
using MyClock.Core.Models;
using MyClock.Core.Services;

namespace MyClock.Tests;

/// <summary>
/// Fake clock service that exposes a Subject so tests can manually advance time.
/// </summary>
internal class FakeClockService : IClockService
{
    private readonly Subject<DateTime> _subject = new();
    public IObservable<DateTime> CurrentTime => _subject;
    public void Tick(DateTime at) => _subject.OnNext(at);
    public void Start() { }
    public void Stop() { }
}

public class SessionServiceTests
{
    [Fact]
    public void GetElapsedTime_WhenNoSessionStarted_ReturnsZero()
    {
        var clock = new FakeClockService();
        var svc = new SessionService(clock);

        Assert.Equal(TimeSpan.Zero, svc.GetElapsedTime());
    }

    [Fact]
    public void GetRemainingTime_InStopwatchMode_ReturnsNull()
    {
        var clock = new FakeClockService();
        var svc = new SessionService(clock);

        svc.StartSession("Test", targetDuration: null);

        Assert.Null(svc.GetRemainingTime());
    }

    [Fact]
    public void SessionUpdated_Fires_OnTick()
    {
        var clock = new FakeClockService();
        var svc = new SessionService(clock);
        svc.StartSession("Test", null);

        int fired = 0;
        svc.SessionUpdated.Subscribe(_ => fired++);

        clock.Tick(DateTime.Now);
        clock.Tick(DateTime.Now);

        Assert.Equal(2, fired);
    }

    [Fact]
    public void Pause_StopsElapsedTimeFromAdvancing()
    {
        var clock = new FakeClockService();
        var svc = new SessionService(clock);
        svc.StartSession("Test", null);

        svc.Pause();

        // Simulate time passing while paused — elapsed should barely change (≤1ms due to DateTime.Now precision)
        System.Threading.Thread.Sleep(50);
        var elapsed = svc.GetElapsedTime();

        Assert.True(elapsed < TimeSpan.FromMilliseconds(1),
            $"Elapsed {elapsed.TotalMilliseconds}ms should not advance during pause");
    }

    [Fact]
    public void Resume_AfterPause_ElapsedExcludesPauseDuration()
    {
        var clock = new FakeClockService();
        var svc = new SessionService(clock);
        svc.StartSession("Test", null);

        svc.Pause();
        System.Threading.Thread.Sleep(200); // 200ms pause
        svc.Resume();

        // After resume, elapsed should be very small (not include 200ms of pause)
        var elapsed = svc.GetElapsedTime();
        Assert.True(elapsed < TimeSpan.FromMilliseconds(150),
            $"Elapsed {elapsed.TotalMilliseconds}ms should exclude pause duration");
    }

    [Fact]
    public void Reset_ClearsSession()
    {
        var clock = new FakeClockService();
        var svc = new SessionService(clock);
        svc.StartSession("Test", null);
        clock.Tick(DateTime.Now);

        svc.Reset();

        Assert.Null(svc.CurrentSession);
        Assert.Equal(TimeSpan.Zero, svc.GetElapsedTime());
    }

    [Fact]
    public void SessionCompleted_FiresOnce_WhenCountdownReachesZero()
    {
        var clock = new FakeClockService();
        var svc = new SessionService(clock);

        // Start a 0-second countdown so it fires immediately on first tick
        svc.StartSession("Test", TimeSpan.FromSeconds(0));

        int completedCount = 0;
        svc.SessionCompleted.Subscribe(_ => completedCount++);

        // Multiple ticks should only fire once
        clock.Tick(DateTime.Now);
        clock.Tick(DateTime.Now);
        clock.Tick(DateTime.Now);

        Assert.Equal(1, completedCount);
    }

    [Fact]
    public void GetRemainingTime_Countdown_DecreasesOverTime()
    {
        var clock = new FakeClockService();
        var svc = new SessionService(clock);
        svc.StartSession("Test", TimeSpan.FromSeconds(60));

        var first = svc.GetRemainingTime();
        System.Threading.Thread.Sleep(50);
        var second = svc.GetRemainingTime();

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.True(second < first, "Remaining time should decrease over time");
    }

    [Fact]
    public void IsCountdown_WhenTargetDurationSet_ReturnsTrue()
    {
        var session = new FocusSession { TargetDuration = TimeSpan.FromMinutes(25) };
        Assert.True(session.IsCountdown);
    }

    [Fact]
    public void IsCountdown_WhenNoTargetDuration_ReturnsFalse()
    {
        var session = new FocusSession { TargetDuration = null };
        Assert.False(session.IsCountdown);
    }
}
