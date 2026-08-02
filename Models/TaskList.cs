namespace FlightPlan.Models;

public class TaskList
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public List<TaskItem> Tasks { get; set; } = new();
}
