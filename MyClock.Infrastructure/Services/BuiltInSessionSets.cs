using MyClock.Core.Models;

namespace MyClock.Infrastructure.Services;

public static class BuiltInSessionSets
{
    public const string ClassicId  = "builtin-classic";
    public const string DeepWorkId = "builtin-deepwork";
    public const string QuickId    = "builtin-quick";

    public static SessionSet ClassicPomodoro() => new()
    {
        Id = ClassicId, Name = "Classic Pomodoro", IsBuiltIn = true,
        Sessions =
        [
            new() { Name = "Focus",       DurationMinutes = 25, Order = 0 },
            new() { Name = "Short Break", DurationMinutes = 5,  Order = 1 },
            new() { Name = "Focus",       DurationMinutes = 25, Order = 2 },
            new() { Name = "Short Break", DurationMinutes = 5,  Order = 3 },
            new() { Name = "Focus",       DurationMinutes = 25, Order = 4 },
            new() { Name = "Short Break", DurationMinutes = 5,  Order = 5 },
            new() { Name = "Focus",       DurationMinutes = 25, Order = 6 },
            new() { Name = "Long Break",  DurationMinutes = 15, Order = 7 },
        ]
    };

    public static SessionSet DeepWork() => new()
    {
        Id = DeepWorkId, Name = "Deep Work", IsBuiltIn = true,
        Sessions =
        [
            new() { Name = "Focus",      DurationMinutes = 50, Order = 0 },
            new() { Name = "Break",      DurationMinutes = 10, Order = 1 },
            new() { Name = "Focus",      DurationMinutes = 50, Order = 2 },
            new() { Name = "Long Break", DurationMinutes = 20, Order = 3 },
        ]
    };

    public static SessionSet QuickFocus() => new()
    {
        Id = QuickId, Name = "Quick Focus", IsBuiltIn = true,
        Sessions =
        [
            new() { Name = "Focus", DurationMinutes = 15, Order = 0 },
            new() { Name = "Break", DurationMinutes = 3,  Order = 1 },
            new() { Name = "Focus", DurationMinutes = 15, Order = 2 },
            new() { Name = "Break", DurationMinutes = 5,  Order = 3 },
        ]
    };

    public static IEnumerable<SessionSet> All() =>
        [ClassicPomodoro(), DeepWork(), QuickFocus()];
}
