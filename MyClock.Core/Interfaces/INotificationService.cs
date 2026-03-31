namespace MyClock.Core.Interfaces;

public interface INotificationService
{
    void ShowSessionCompleted(string sessionName);
    void PlayAlert();
}
