using MyClock.Core.Models;

namespace MyClock.Infrastructure.Models;

public class AppSettings
{
    public double WindowX { get; set; } = 100;
    public double WindowY { get; set; } = 100;
    public double Opacity { get; set; } = 0.92;
    public bool Use24HourFormat { get; set; } = true;
    public bool ShowClock { get; set; } = false;
    public string Theme { get; set; } = "Dark";

    // Timer
    public TimerMode TimerMode { get; set; } = TimerMode.Free;
    public string? ActiveSessionSetId { get; set; }
    public List<SessionSet> SessionSets { get; set; } = new();
}
