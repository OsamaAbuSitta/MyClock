using MyClock.Core.Models;

namespace MyClock.Core.Interfaces;

public interface ISessionService
{
    FocusSession? CurrentSession { get; }
    IObservable<FocusSession> SessionUpdated { get; }
    IObservable<FocusSession> SessionCompleted { get; }

    void StartSession(string name, TimeSpan? targetDuration);
    void Pause();
    void Resume();
    void Reset();

    TimeSpan GetElapsedTime();
    TimeSpan? GetRemainingTime(); // null for stopwatch mode
}
