namespace FlightPlan.Models;

public class TaskType
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#cccccc";
    public string Icon { get; set; } = "bi-tag";
}
