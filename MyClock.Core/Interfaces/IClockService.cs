namespace MyClock.Core.Interfaces;

public interface IClockService
{
    IObservable<DateTime> CurrentTime { get; }
    void Start();
    void Stop();
}
