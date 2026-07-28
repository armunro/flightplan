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
