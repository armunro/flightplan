using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FlightPlan.Models;
using FlightPlan.Services;
using Quartz;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FlightPlan.Infrastructure.Services;

public class ScheduledTaskService
{
    private readonly IStorageService _storageService;
    private readonly ISchedulerFactory _schedulerFactory;
    private List<ScheduledTask> _scheduledTasks = new();
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    public ScheduledTaskService(IStorageService storageService, ISchedulerFactory schedulerFactory)
    {
        _storageService = storageService;
        _schedulerFactory = schedulerFactory;
        
        _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
            
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
            
        LoadTasks();
    }

    private void LoadTasks()
    {
        var path = _storageService.GetScheduledTasksPath();
        if (File.Exists(path))
        {
            var yaml = File.ReadAllText(path);
            _scheduledTasks = _deserializer.Deserialize<List<ScheduledTask>>(yaml) ?? new List<ScheduledTask>();
            
            // Fix any empty GUIDs
            var updated = false;
            foreach (var task in _scheduledTasks)
            {
                if (task.Id == Guid.Empty)
                {
                    task.Id = Guid.NewGuid();
                    updated = true;
                }
            }
            if (updated) SaveTasks();
        }
    }

    private void SaveTasks()
    {
        var path = _storageService.GetScheduledTasksPath();
        var yaml = _serializer.Serialize(_scheduledTasks);
        File.WriteAllText(path, yaml);
    }

    public IEnumerable<ScheduledTask> GetAllTasks() => _scheduledTasks;

    public ScheduledTask? GetTaskById(Guid id) => _scheduledTasks.FirstOrDefault(t => t.Id == id);

    public async Task AddTaskAsync(ScheduledTask task)
    {
        if (task.Id == Guid.Empty)
        {
            task.Id = Guid.NewGuid();
        }
        
        _scheduledTasks.Add(task);
        if (task.IsEnabled)
        {
            await ScheduleQuartzJob(task);
        }
        SaveTasks();
    }

    public async Task UpdateTaskAsync(ScheduledTask task)
    {
        var existing = GetTaskById(task.Id);
        if (existing != null)
        {
            var wasEnabled = existing.IsEnabled;
            
            existing.Name = task.Name;
            existing.RecurrenceType = task.RecurrenceType;
            existing.StartDate = task.StartDate;
            existing.StartTime = task.StartTime;
            existing.Interval = task.Interval;
            existing.IntervalUnit = task.IntervalUnit;
            existing.CronSchedule = task.CronSchedule;
            existing.IsEnabled = task.IsEnabled;
            existing.StatusId = task.StatusId;
            existing.TaskTypeId = task.TaskTypeId;
            existing.TaskTitleTemplate = task.TaskTitleTemplate;
            existing.TaskDescription = task.TaskDescription;
            existing.Priority = task.Priority;
            existing.ProjectId = task.ProjectId;
            existing.ListId = task.ListId;
            
            // Explicitly unschedule first to be safe, regardless of wasEnabled
            await UnscheduleQuartzJob(task.Id);

            if (existing.IsEnabled)
            {
                await ScheduleQuartzJob(existing);
            }

            SaveTasks();
        }
    }

    public async Task DeleteTaskAsync(Guid id)
    {
        var task = GetTaskById(id);
        if (task != null)
        {
            await UnscheduleQuartzJob(id);
            _scheduledTasks.Remove(task);
            SaveTasks();
        }
    }

    public async Task InitializeSchedulesAsync()
    {
        foreach (var task in _scheduledTasks.Where(t => t.IsEnabled))
        {
            try
            {
                await ScheduleQuartzJob(task);
            }
            catch (Exception ex)
            {
                // If a task has an invalid cron expression, don't crash the whole app.
                // Just log it and continue.
                Console.WriteLine($"[ERROR] Failed to schedule task {task.Name} ({task.Id}): {ex.Message}");
            }
        }
    }

    private async Task ScheduleQuartzJob(ScheduledTask task)
    {
        var scheduler = await _schedulerFactory.GetScheduler();
        
        // Ensure we don't have existing job/trigger for this task
        await UnscheduleQuartzJob(task.Id);
        
        var job = JobBuilder.Create<CreateTaskFromTemplateJob>()
            .WithIdentity($"job_{task.Id}", "scheduled_tasks")
            .UsingJobData("TaskId", task.Id.ToString())
            .Build();

        ITrigger trigger;

        if (task.RecurrenceType == "Custom" && task.StartDate.HasValue)
        {
            var startAt = task.StartDate.Value.Date;
            if (!string.IsNullOrEmpty(task.StartTime) && TimeSpan.TryParse(task.StartTime, out var time))
            {
                startAt = startAt.Add(time);
            }

            var triggerBuilder = TriggerBuilder.Create()
                .WithIdentity($"trigger_{task.Id}", "scheduled_tasks")
                .StartAt(new DateTimeOffset(startAt));

            switch (task.IntervalUnit)
            {
                case "Days":
                    triggerBuilder.WithCalendarIntervalSchedule(x => x.WithIntervalInDays(task.Interval).WithMisfireHandlingInstructionDoNothing());
                    break;
                case "Weeks":
                    triggerBuilder.WithCalendarIntervalSchedule(x => x.WithIntervalInWeeks(task.Interval).WithMisfireHandlingInstructionDoNothing());
                    break;
                case "Months":
                    triggerBuilder.WithCalendarIntervalSchedule(x => x.WithIntervalInMonths(task.Interval).WithMisfireHandlingInstructionDoNothing());
                    break;
                case "Years":
                    triggerBuilder.WithCalendarIntervalSchedule(x => x.WithIntervalInYears(task.Interval).WithMisfireHandlingInstructionDoNothing());
                    break;
                default:
                    triggerBuilder.WithCalendarIntervalSchedule(x => x.WithIntervalInDays(task.Interval).WithMisfireHandlingInstructionDoNothing());
                    break;
            }

            trigger = triggerBuilder.Build();
        }
        else
        {
            trigger = TriggerBuilder.Create()
                .WithIdentity($"trigger_{task.Id}", "scheduled_tasks")
                .WithCronSchedule(NormalizeCronExpression(task.CronSchedule))
                .Build();
        }

        await scheduler.ScheduleJob(job, trigger);
        
        var nextFireTime = (await scheduler.GetTrigger(trigger.Key))?.GetNextFireTimeUtc();
        if (nextFireTime.HasValue && nextFireTime.Value < DateTimeOffset.UtcNow)
        {
            // If the next fire time is in the past, get the next one after now
            nextFireTime = trigger.GetFireTimeAfter(DateTimeOffset.UtcNow);
        }
        
        task.NextRun = nextFireTime?.LocalDateTime;
    }

    private string NormalizeCronExpression(string cron)
    {
        if (string.IsNullOrWhiteSpace(cron)) return cron;
        
        var parts = cron.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 6) return cron;

        // Quartz cron format: s m h dom M dow [y]
        // parts[3] is day-of-month
        // parts[5] is day-of-week
        
        bool hasDom = parts[3] != "*" && parts[3] != "?";
        bool hasDow = parts[5] != "*" && parts[5] != "?";

        if (hasDom && hasDow)
        {
            // If both are specified, Quartz fails. 
            // Often people put * for both when they mean "every day".
            // If one of them is *, we can safely change it to ?
            if (parts[3] == "*") parts[3] = "?";
            else if (parts[5] == "*") parts[5] = "?";
        }
        else if (!hasDom && !hasDow)
        {
            // If neither is specified (both are * or ?), Quartz expects one to be ?
            // Default to day-of-week being ? if both are *
            if (parts[5] == "*") parts[5] = "?";
        }

        return string.Join(" ", parts);
    }

    private async Task UnscheduleQuartzJob(Guid taskId)
    {
        var scheduler = await _schedulerFactory.GetScheduler();
        var triggerKey = new TriggerKey($"trigger_{taskId}", "scheduled_tasks");
        await scheduler.UnscheduleJob(triggerKey);
    }
    
    public async Task UpdateLastRunAsync(Guid taskId, DateTime lastRun)
    {
        var task = GetTaskById(taskId);
        if (task != null)
        {
            task.LastRun = lastRun;
            
            // Re-schedule to update NextRun
            try 
            {
                // UnscheduleQuartzJob is already called inside ScheduleQuartzJob
                if (task.IsEnabled)
                {
                    await ScheduleQuartzJob(task);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to re-schedule task {task.Name} after run: {ex.Message}");
            }

            SaveTasks();
        }
    }
}

public class CreateTaskFromTemplateJob : IJob
{
    private readonly ProjectManager _projectManager;
    private readonly ScheduledTaskService _scheduledTaskService;
    private readonly IStorageService _storageService;

    public CreateTaskFromTemplateJob(ProjectManager projectManager, ScheduledTaskService scheduledTaskService, IStorageService storageService)
    {
        _projectManager = projectManager;
        _scheduledTaskService = scheduledTaskService;
        _storageService = storageService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var taskIdStr = context.MergedJobDataMap.GetString("TaskId");
        if (Guid.TryParse(taskIdStr, out var taskId))
        {
            var scheduledTask = _scheduledTaskService.GetTaskById(taskId);
            if (scheduledTask != null)
            {
                var project = _projectManager.FindProjectById(scheduledTask.ProjectId);
                var list = _projectManager.FindListById(scheduledTask.ListId);

                if (project != null && list != null)
                {
                    // Basic template replacement for now, just {{date}}
                    var title = scheduledTask.TaskTitleTemplate.Replace("{{date}}", DateTime.Now.ToString("yyyy-MM-dd"));
                    
                    _projectManager.AddTaskToList(
                        list,
                        title,
                        scheduledTask.TaskDescription,
                        scheduledTask.Priority,
                        scheduledTask.StatusId,
                        taskTypeId: scheduledTask.TaskTypeId
                    );
                    
                    _projectManager.SaveProjectsToYaml(_storageService.GetProjectsPath());
                    await _scheduledTaskService.UpdateLastRunAsync(taskId, DateTime.Now);
                }
            }
        }
    }
}
