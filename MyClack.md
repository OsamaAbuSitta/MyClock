# Project: MyClock (Avalonia Cross-Platform Floating Focus Clock)

## Overview

MyClock is a lightweight, cross-platform desktop application built with .NET 8 and Avalonia UI.

It provides a small floating widget that stays always visible on top of all windows, allowing users to track time and manage focus sessions without interrupting their workflow.

The application focuses on simplicity, performance, and minimal interaction.

---

## Tech Stack

* .NET 8
* Avalonia UI
* MVVM pattern
* ReactiveUI (optional but preferred)
* Local storage via JSON (System.Text.Json)

---

## Core Features (MVP)

### 1. Floating Always-On-Top Widget

* Borderless window
* Always stays on top
* Draggable across screen
* Small compact UI
* Optional transparency (opacity control)

### 2. Clock Display

* Shows current local time (HH:mm:ss)
* Updates every second
* Supports 12h / 24h format toggle

### 3. Focus Session Timer

#### Modes:

* Stopwatch mode (count up)
* Countdown mode (user-defined duration, e.g. 25 min)

### 4. Session Controls

* Start
* Pause
* Resume
* Reset
* New Session

### 5. Session State Display

* Session name
* Elapsed or remaining time
* Status (Running / Paused / Completed)

### 6. Notifications

* Notify when session completes
* Optional sound
* System notification if supported

---

## Architecture

### Solution Structure

* MyClock.App → Avalonia UI
* MyClock.Core → domain models and services
* MyClock.Infrastructure → persistence and settings

---

## Core Model

```csharp
public class FocusSession
{
    public string Name { get; set; } = "Focus Session";
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan? TargetDuration { get; set; }
    public bool IsRunning { get; set; }
    public bool IsPaused { get; set; }
}
```

---

## Services

### SessionService

Handles:

* StartSession()
* Pause()
* Resume()
* Reset()
* GetElapsedTime()
* GetRemainingTime()

### ClockService

* Emits current time every second
* Uses timer or observable

### SettingsService

Stores:

* Window position
* Opacity
* Theme
* Last used duration

### NotificationService

* Show system notifications
* Play sound alerts

---

## UI Implementation (Avalonia)

### MainWindow Requirements

* Window should be:

  * `CanResize = false`
  * `SystemDecorations = None`
  * `Topmost = true`
  * Transparent background support

### Dragging Behavior

* Implement manual dragging using pointer events

### Layout

Vertical layout:

* Current Time (large font)
* Session Timer (medium font)
* Control buttons (Start / Pause / Reset)

---

## Example Avalonia XAML (MainWindow)

```xml
<Window xmlns="https://github.com/avaloniaui"
        Width="220" Height="140"
        Topmost="True"
        SystemDecorations="None"
        Background="Transparent">

    <Border Background="#DD1E1E1E" CornerRadius="10" Padding="10">
        <StackPanel Spacing="6">
            
            <TextBlock Text="{Binding CurrentTime}"
                       FontSize="18"
                       HorizontalAlignment="Center"/>

            <TextBlock Text="{Binding SessionTime}"
                       FontSize="22"
                       FontWeight="Bold"
                       HorizontalAlignment="Center"/>

            <StackPanel Orientation="Horizontal" HorizontalAlignment="Center" Spacing="5">
                <Button Content="Start" Command="{Binding StartCommand}"/>
                <Button Content="Pause" Command="{Binding PauseCommand}"/>
                <Button Content="Reset" Command="{Binding ResetCommand}"/>
            </StackPanel>

        </StackPanel>
    </Border>
</Window>
```

---

## Behavior

### On App Start

* Open floating widget
* Restore last position
* No active session

### Session Flow

1. User clicks Start
2. Timer begins
3. User can pause/resume
4. On completion:

   * Notification triggered
   * Option to restart

---

## Future Enhancements

* Pomodoro cycles (focus + break)
* Tray icon support
* Click-through mode
* Keyboard shortcuts
* Session history (SQLite)
* Analytics dashboard
* Multi-monitor awareness

---

## Key Design Principles

* Minimal UI
* Zero friction interaction
* Always visible but non-intrusive
* Fast and lightweight

---

## Goal

Build a simple but powerful focus tool that stays out of the way while helping users maintain deep work sessions.
