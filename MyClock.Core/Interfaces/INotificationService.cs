namespace MyClock.Core.Interfaces;

public interface INotificationService
{
    void ShowSessionCompleted(string title, string message);
    void PlayAlert();
}
