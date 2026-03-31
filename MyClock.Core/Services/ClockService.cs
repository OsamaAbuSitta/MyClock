using System.Reactive.Linq;
using System.Reactive.Subjects;
using MyClock.Core.Interfaces;

namespace MyClock.Core.Services;

public class ClockService : IClockService, IDisposable
{
    private readonly Subject<DateTime> _subject = new();
    private IDisposable? _subscription;

    public IObservable<DateTime> CurrentTime => _subject.AsObservable();

    public void Start()
    {
        _subscription = Observable
            .Interval(TimeSpan.FromSeconds(1))
            .Select(_ => DateTime.Now)
            .Subscribe(_subject);
    }

    public void Stop() => _subscription?.Dispose();

    public void Dispose()
    {
        Stop();
        _subject.Dispose();
    }
}
