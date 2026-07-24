namespace FlightPlan.Models;

public class ProjectPriority
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#cccccc";
    public string Icon { get; set; } = "bi-dash-lg";
    public int Order { get; set; }
}
