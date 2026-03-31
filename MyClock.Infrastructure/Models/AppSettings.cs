namespace MyClock.Infrastructure.Models;

public class AppSettings
{
    public double WindowX { get; set; } = 100;
    public double WindowY { get; set; } = 100;
    public double Opacity { get; set; } = 0.92;
    public bool Use24HourFormat { get; set; } = true;
    public string Theme { get; set; } = "Dark";
    public int? LastUsedDurationSeconds { get; set; } // null = stopwatch was last used

    // Pomodoro
    public bool PomodoroEnabled { get; set; } = false;
    public int FocusDurationMinutes { get; set; } = 25;
    public int ShortBreakMinutes { get; set; } = 5;
    public int LongBreakMinutes { get; set; } = 15;
    public int CyclesBeforeLongBreak { get; set; } = 4;
}
