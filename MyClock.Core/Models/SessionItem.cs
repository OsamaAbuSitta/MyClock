namespace MyClock.Core.Models;

public class SessionItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "Session";
    public int DurationMinutes { get; set; } = 25;
    public int Order { get; set; }
}
