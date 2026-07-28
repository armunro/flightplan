using FlightPlan.Models;

namespace FlightPlan.Core.Interfaces;

public interface IScheduledTaskService
{
    IEnumerable<ScheduledTask> GetAllTasks();
    ScheduledTask? GetTaskById(Guid id);
    Task AddTaskAsync(ScheduledTask task);
    Task UpdateTaskAsync(ScheduledTask task);
    Task DeleteTaskAsync(Guid id);
    Task InitializeSchedulesAsync();
    Task UpdateLastRunAsync(Guid taskId, DateTime lastRun);
}
