namespace FlightPlan.Models;

public enum TaskPriority
{
    Lowest,
    Low,
    Medium,
    High,
    Highest,
    Critical
}

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public Guid? StatusId { get; set; }
    public Guid? TaskTypeId { get; set; }
    public Guid? PriorityId { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public int EstimateMinutes { get; set; }
    public string? Link { get; set; }
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
    public List<TaskItem> Subtasks { get; set; } = new();
    public List<CustomFieldValue> CustomFieldValues { get; set; } = new();
}
