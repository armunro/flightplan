using FlightPlan.Core.Interfaces;
using FlightPlan.Models;

namespace FlightPlan.Infrastructure.Services;

public class MockScheduledTaskService : IScheduledTaskService
{
    private List<ScheduledTask> _mockTasks = new()
    {
        new ScheduledTask
        {
            Id = Guid.NewGuid(),
            Name = "Daily Project Sync",
            CronSchedule = "0 0 9 ? * MON-FRI",
            IsEnabled = true,
            RecurrenceType = "Cron",
            TaskTitleTemplate = "Sync with project team",
            Priority = TaskPriority.High,
            LastRun = DateTime.Now.AddDays(-1).AddHours(9),
            NextRun = DateTime.Now.AddHours(2)
        },
        new ScheduledTask
        {
            Id = Guid.NewGuid(),
            Name = "Weekly Review",
            CronSchedule = "0 0 15 ? * FRI",
            IsEnabled = true,
            RecurrenceType = "Cron",
            TaskTitleTemplate = "Complete weekly status report",
            Priority = TaskPriority.Medium,
            LastRun = DateTime.Now.AddDays(-3),
            NextRun = DateTime.Now.AddDays(4)
        },
        new ScheduledTask
        {
            Id = Guid.NewGuid(),
            Name = "Monthly Database Backup",
            CronSchedule = "0 0 0 1 * ?",
            IsEnabled = true,
            RecurrenceType = "Cron",
            TaskTitleTemplate = "Verify monthly backup integrity",
            Priority = TaskPriority.Critical,
            LastRun = DateTime.Now.AddDays(-26),
            NextRun = DateTime.Now.AddDays(4)
        },
        new ScheduledTask
        {
            Id = Guid.NewGuid(),
            Name = "Update Dependencies",
            CronSchedule = "0 0 2 ? * MON",
            IsEnabled = true,
            RecurrenceType = "Cron",
            TaskTitleTemplate = "Check and update project dependencies",
            Priority = TaskPriority.Low,
            LastRun = DateTime.Now.AddDays(-7),
            NextRun = DateTime.Now.AddDays(6)
        },
        new ScheduledTask
        {
            Id = Guid.NewGuid(),
            Name = "QBR Preparation",
            CronSchedule = "0 0 10 ? * TUE",
            IsEnabled = false,
            RecurrenceType = "Cron",
            TaskTitleTemplate = "Compile data for Quarterly Business Review",
            Priority = TaskPriority.High,
            LastRun = DateTime.Now.AddDays(-14),
            NextRun = DateTime.Now.AddDays(1)
        },
        new ScheduledTask
        {
            Id = Guid.NewGuid(),
            Name = "Timesheet Submission",
            CronSchedule = "0 0 17 ? * FRI",
            IsEnabled = true,
            RecurrenceType = "Cron",
            TaskTitleTemplate = "Submit timesheet for the week",
            Priority = TaskPriority.Highest,
            LastRun = DateTime.Now.AddDays(-3),
            NextRun = DateTime.Now.AddDays(4)
        },
        new ScheduledTask
        {
            Id = Guid.NewGuid(),
            Name = "Team Social Reminder",
            CronSchedule = "0 0 16 ? * THU",
            IsEnabled = true,
            RecurrenceType = "Cron",
            TaskTitleTemplate = "Coordinate team social event",
            Priority = TaskPriority.Low,
            LastRun = DateTime.Now.AddDays(-4),
            NextRun = DateTime.Now.AddDays(3)
        },
        new ScheduledTask
        {
            Id = Guid.NewGuid(),
            Name = "Security Patch Audit",
            CronSchedule = "0 0 1 ? * WED",
            IsEnabled = true,
            RecurrenceType = "Cron",
            TaskTitleTemplate = "Review available security patches",
            Priority = TaskPriority.Critical,
            LastRun = DateTime.Now.AddDays(-5),
            NextRun = DateTime.Now.AddDays(2)
        }
    };

    public IEnumerable<ScheduledTask> GetAllTasks()
    {
        return _mockTasks;
    }

    public ScheduledTask? GetTaskById(Guid id)
    {
        return _mockTasks.FirstOrDefault(t => t.Id == id);
    }

    public Task AddTaskAsync(ScheduledTask task)
    {
        _mockTasks.Add(task);
        return Task.CompletedTask;
    }

    public Task UpdateTaskAsync(ScheduledTask task)
    {
        var existing = _mockTasks.FirstOrDefault(t => t.Id == task.Id);
        if (existing != null)
        {
            _mockTasks.Remove(existing);
            _mockTasks.Add(task);
        }
        return Task.CompletedTask;
    }

    public Task DeleteTaskAsync(Guid id)
    {
        var existing = _mockTasks.FirstOrDefault(t => t.Id == id);
        if (existing != null)
        {
            _mockTasks.Remove(existing);
        }
        return Task.CompletedTask;
    }

    public Task InitializeSchedulesAsync()
    {
        return Task.CompletedTask;
    }

    public Task UpdateLastRunAsync(Guid taskId, DateTime lastRun)
    {
        var existing = _mockTasks.FirstOrDefault(t => t.Id == taskId);
        if (existing != null)
        {
            existing.LastRun = lastRun;
        }
        return Task.CompletedTask;
    }
}
