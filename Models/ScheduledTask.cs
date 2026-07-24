using System;

namespace FlightPlan.Models;

public class ScheduledTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string CronSchedule { get; set; } = string.Empty; // Standard Cron expression
    public bool IsEnabled { get; set; } = true;
    
    // Template for the task to be created
    public Guid ProjectId { get; set; }
    public Guid ListId { get; set; }
    public string TaskTitleTemplate { get; set; } = string.Empty;
    public string? TaskDescription { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public Guid? StatusId { get; set; }
    public Guid? TaskTypeId { get; set; }
    
    public DateTime? LastRun { get; set; }
    public DateTime? NextRun { get; set; }
}
