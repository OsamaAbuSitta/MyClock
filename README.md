# MyClock

A lightweight, always-on-top floating focus timer for macOS, Windows, and Linux. Built with .NET 10 and Avalonia UI.

## Features

- **Floating widget** — borderless, transparent, always on top of all windows
- **Draggable** — click and drag to position anywhere on screen
- **Clock display** — current time in 12h or 24h format, updated every second
- **Focus session timer** — stopwatch (count up) or countdown mode
- **Session controls** — Start, Pause, Resume, Reset
- **Notifications** — system notification when a countdown session completes
- **Persistent settings** — window position and preferences saved across restarts

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Run

```bash
dotnet run --project MyClock.App
```

### Build

```bash
dotnet build
```

### Test

```bash
dotnet test
dotnet test --filter "FullyQualifiedName~SessionService"  # single test class
```

## Usage

| Action | How |
|---|---|
| Move the widget | Click and drag anywhere on the widget |
| Start a session | Click **Start** |
| Pause / Resume | Click **Pause** / **Resume** |
| Reset | Click **Reset** |
| Close | Click the **×** button |

**Countdown mode:** set `LastUsedDurationSeconds` in the settings file (e.g. `1500` for 25 minutes). If unset, the timer runs as a stopwatch.

## Settings File

Settings are saved automatically on close.

| Platform | Path |
|---|---|
| macOS | `~/Library/Application Support/MyClock/settings.json` |
| Windows | `%APPDATA%\MyClock\settings.json` |
| Linux | `~/.config/MyClock/settings.json` |

```json
{
  "WindowX": 100.0,
  "WindowY": 100.0,
  "Opacity": 0.92,
  "Use24HourFormat": true,
  "Theme": "Dark",
  "LastUsedDurationSeconds": null
}
```

## Project Structure

```
MyClock.Core/           Domain model (FocusSession) and services (ClockService, SessionService)
MyClock.Infrastructure/ Settings persistence and system notifications
MyClock.App/            Avalonia UI — MainWindow, MainWindowViewModel, App wiring
MyClock.Tests/          xUnit unit tests for SessionService
```

## Tech Stack

- [.NET 10](https://dotnet.microsoft.com/)
- [Avalonia UI 11.3](https://avaloniaui.net/)
- [ReactiveUI](https://reactiveui.net/)
