namespace MyClock.Core.Models;

public class FocusSession
{
    public string Name { get; set; } = "Focus Session";
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan? TargetDuration { get; set; } // null = stopwatch, set = countdown
    public bool IsRunning { get; set; }
    public bool IsPaused { get; set; }

    public bool IsCountdown => TargetDuration.HasValue;
    public bool IsCompleted => EndTime.HasValue;
}
