using Avalonia.Controls.Notifications;
using MyClock.Core.Interfaces;

namespace MyClock.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private INotificationManager? _manager;

    public void SetManager(INotificationManager manager) => _manager = manager;

    public void ShowSessionCompleted(string sessionName)
    {
        _manager?.Show(new Notification(
            title: "Session Complete",
            message: $"{sessionName} has ended.",
            type: NotificationType.Success));
    }

    public void PlayAlert()
    {
        // No-op for MVP — sound support is a future enhancement
    }
}
