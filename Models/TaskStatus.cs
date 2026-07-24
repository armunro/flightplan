namespace FlightPlan.Models;

public class TaskStatus
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#cccccc";
    public bool IsCompletedState { get; set; }
    public int Order { get; set; }
}
