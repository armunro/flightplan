namespace FlightPlan.Models;

public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Icon { get; set; } = "bi-folder";
    public string Color { get; set; } = "#58a6ff";
    public List<TaskStatus> Statuses { get; set; } = new();
    public List<TaskType> TaskTypes { get; set; } = new();
    public List<ProjectPriority> Priorities { get; set; } = new();
    public List<TaskList> Lists { get; set; } = new();
}
