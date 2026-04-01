using System.Text.Json;
using MyClock.Core.Models;
using MyClock.Infrastructure.Models;

namespace MyClock.Infrastructure.Services;

public class SettingsService
{
    private readonly string _filePath;
    private AppSettings _current = new();

    public AppSettings Current => _current;

    public SettingsService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _filePath = Path.Combine(appData, "MyClock", "settings.json");
    }

    public void Load()
    {
        if (!File.Exists(_filePath))
        {
            EnsureDefaultSets();
            return;
        }
        try
        {
            var text = File.ReadAllText(_filePath);
            using var doc = JsonDocument.Parse(text);

            if (doc.RootElement.TryGetProperty("PomodoroEnabled", out _))
            {
                // Legacy format — migrate
                var legacy = JsonSerializer.Deserialize<LegacyAppSettings>(text) ?? new LegacyAppSettings();
                _current = MigrateFromLegacy(legacy);
            }
            else
            {
                _current = JsonSerializer.Deserialize<AppSettings>(text) ?? new AppSettings();
            }
        }
        catch
        {
            _current = new AppSettings();
        }
        EnsureDefaultSets();
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        var json = JsonSerializer.Serialize(_current, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }

    // Inserts any missing built-in sets without overwriting existing ones
    public void EnsureDefaultSets()
    {
        int insertAt = 0;
        foreach (var factory in new Func<SessionSet>[]
            { BuiltInSessionSets.ClassicPomodoro, BuiltInSessionSets.DeepWork, BuiltInSessionSets.QuickFocus })
        {
            var def = factory();
            if (!_current.SessionSets.Any(s => s.Id == def.Id))
            {
                _current.SessionSets.Insert(insertAt, def);
            }
            insertAt++;
        }
    }

    // Replaces a built-in set with its original values
    public void RestoreDefaultSet(string id)
    {
        var factory = id switch
        {
            BuiltInSessionSets.ClassicId  => (Func<SessionSet>)BuiltInSessionSets.ClassicPomodoro,
            BuiltInSessionSets.DeepWorkId => BuiltInSessionSets.DeepWork,
            BuiltInSessionSets.QuickId    => BuiltInSessionSets.QuickFocus,
            _                             => null
        };
        if (factory is null) return;

        var idx = _current.SessionSets.FindIndex(s => s.Id == id);
        var restored = factory();
        if (idx >= 0)
            _current.SessionSets[idx] = restored;
        else
            _current.SessionSets.Insert(0, restored);
    }

    public void ResetAllDefaults()
    {
        RestoreDefaultSet(BuiltInSessionSets.ClassicId);
        RestoreDefaultSet(BuiltInSessionSets.DeepWorkId);
        RestoreDefaultSet(BuiltInSessionSets.QuickId);
    }

    private static AppSettings MigrateFromLegacy(LegacyAppSettings legacy)
    {
        var settings = new AppSettings
        {
            WindowX       = legacy.WindowX,
            WindowY       = legacy.WindowY,
            Opacity       = legacy.Opacity,
            Use24HourFormat = legacy.Use24HourFormat,
            ShowClock     = legacy.ShowClock,
            Theme         = legacy.Theme,
        };

        if (legacy.PomodoroEnabled)
        {
            // Build a custom set from the old Pomodoro values
            var sessions = new List<SessionItem>();
            for (int i = 0; i < legacy.CyclesBeforeLongBreak; i++)
            {
                sessions.Add(new SessionItem { Name = "Focus",       DurationMinutes = legacy.FocusDurationMinutes, Order = sessions.Count });
                sessions.Add(new SessionItem { Name = "Short Break", DurationMinutes = legacy.ShortBreakMinutes,    Order = sessions.Count });
            }
            // Replace last short break with long break
            if (sessions.Count >= 2)
                sessions[^1] = new SessionItem { Name = "Long Break", DurationMinutes = legacy.LongBreakMinutes, Order = sessions.Count - 1 };

            var mySet = new SessionSet { Name = "My Pomodoro", Sessions = sessions };
            settings.SessionSets.Add(mySet);
            settings.ActiveSessionSetId = mySet.Id;
            settings.TimerMode = TimerMode.SessionSet;
        }
        else
        {
            settings.TimerMode = TimerMode.Free;
        }

        return settings;
    }

    // Used only for reading old settings.json files
    private sealed class LegacyAppSettings
    {
        public double WindowX { get; set; } = 100;
        public double WindowY { get; set; } = 100;
        public double Opacity { get; set; } = 0.92;
        public bool Use24HourFormat { get; set; } = true;
        public bool ShowClock { get; set; } = false;
        public string Theme { get; set; } = "Dark";
        public bool PomodoroEnabled { get; set; } = false;
        public int FocusDurationMinutes { get; set; } = 25;
        public int ShortBreakMinutes { get; set; } = 5;
        public int LongBreakMinutes { get; set; } = 15;
        public int CyclesBeforeLongBreak { get; set; } = 4;
    }
}
