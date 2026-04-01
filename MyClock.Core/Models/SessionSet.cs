namespace MyClock.Core.Models;

public class SessionSet
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "My Sessions";
    public bool IsBuiltIn { get; set; } = false;
    public List<SessionItem> Sessions { get; set; } = new();
}
