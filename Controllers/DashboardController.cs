using Microsoft.AspNetCore.Mvc;
using FlightPlan.Services;
using FlightPlan.Models;
using FlightPlan.Models.Config;
using FlightPlan.Core.Models;
using FlightPlan.Core.Interfaces;

namespace FlightPlan.Controllers;

public record DashboardTaskDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    bool IsCompleted,
    DateTime? End,
    int EstimateMinutes,
    string? TypeName,
    string? TypeColor,
    string? TypeIcon,
    string? StatusName,
    string? StatusColor,
    string? PriorityName,
    string? PriorityColor,
    string? PriorityIcon,
    string? ProjectName,
    string? ProjectColor,
    string? ProjectIcon,
    string? ListName
);

public record DashboardDto(
    List<DashboardTaskDto> UpcomingTasks,
    List<EmailWithRulesDto> RecentEmails,
    List<CalendarEventResponseDto> TodaysEvents,
    List<CalendarEventResponseDto> UpcomingEvents,
    bool EmailVisible,
    bool CalendarVisible,
    Dictionary<string, CalendarPreferenceDto>? CalendarPreferences
);

public record CalendarPreferenceDto(
    int Order,
    bool Hidden,
    string? CustomName,
    string? CustomIcon,
    string? Color
);

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ProjectManager _projectManager;
    private readonly IGraphService _graphService;
    private readonly IRuleService _ruleService;
    private readonly DashConfig _config;
    private readonly IStorageService _storageService;

    public DashboardController(ProjectManager projectManager, IGraphService graphService, IRuleService ruleService, DashConfig config, IStorageService storageService)
    {
        _projectManager = projectManager;
        _graphService = graphService;
        _ruleService = ruleService;
        _config = config;
        _storageService = storageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var emailVisible = _config.PageVisibilities.FirstOrDefault(v => v.Id == "email")?.Visible ?? true;
        var calendarVisible = _config.PageVisibilities.FirstOrDefault(v => v.Id == "calendar")?.Visible ?? true;

        // 1. Upcoming tasks
        var allProjects = _projectManager.GetAllProjects();
        var allUpcoming = new List<DashboardTaskDto>();
        
        foreach (var project in allProjects)
        {
            foreach (var list in project.Lists)
            {
                var incompleteTasks = FindAllIncompleteTasks(list.Tasks);
                var projectUpcoming = incompleteTasks
                    .Where(t => t.End.HasValue)
                    .Select(t =>
                    {
                        var type = project.TaskTypes.FirstOrDefault(tt => tt.Id == t.TaskTypeId);
                        var status = project.Statuses.FirstOrDefault(s => s.Id == t.StatusId);
                        var priority = project.Priorities.FirstOrDefault(p => p.Id == t.PriorityId);

                        return new DashboardTaskDto(
                            t.Id,
                            project.Id,
                            t.Title,
                            t.IsCompleted,
                            t.End,
                            t.EstimateMinutes,
                            type?.Name,
                            type?.Color,
                            type?.Icon,
                            status?.Name,
                            status?.Color,
                            priority?.Name,
                            priority?.Color,
                            priority?.Icon,
                            project.Name,
                            project.Color,
                            project.Icon,
                            list.Name
                        );
                    });

                allUpcoming.AddRange(projectUpcoming);
            }
        }

        var upcomingTasks = allUpcoming
            .OrderBy(t => t.End.Value)
            .Take(_config.Debug.UpcomingTasksCount)
            .ToList();

        // 2. Recent emails (Inbox)
        var recentEmails = new List<EmailWithRulesDto>();
        if (emailVisible)
        {
            try
            {
                var emails = await _graphService.GetEmailsAsync("inbox", 10);
                var rules = _ruleService.GetAllRules();
                recentEmails = emails.Select(email => new EmailWithRulesDto(
                    email.Id,
                    email.Subject,
                    email.From,
                    email.FromAddress,
                    email.ReceivedDateTime,
                    email.BodyPreview,
                    email.WebLink,
                    rules.Where(r => _ruleService.Matches(r, email)).Select(r => new MatchingRuleDto(r.Name, r.Color))
                        .ToList()
                )).ToList();
            }
            catch (Exception ex)
            {
                // Log error but don't fail the whole request
            }
        }

        // 3. Today's and Upcoming events
        var todaysEvents = new List<CalendarEventResponseDto>();
        var upcomingEvents = new List<CalendarEventResponseDto>();
        if (calendarVisible)
        {
            try
            {
                // Today's events
                var events = await _graphService.GetNextEventsAsync(null, 20, today, tomorrow);
                todaysEvents = events.Select(e => new CalendarEventResponseDto(
                    e.Id, 
                    e.Subject, 
                    e.IsAllDay ? e.Start.ToString("yyyy-MM-dd") : e.Start.ToString("yyyy-MM-ddTHH:mm:ssZ"), 
                    e.IsAllDay ? e.End.ToString("yyyy-MM-dd") : e.End.ToString("yyyy-MM-ddTHH:mm:ssZ"), 
                    e.Location ?? "", 
                    e.WebLink, 
                    e.CalendarId, 
                    e.IsAllDay
                )).ToList();

                // Upcoming events (starting from tomorrow)
                var upcoming = await _graphService.GetNextEventsAsync(null, 20, tomorrow, tomorrow.AddDays(7));
                upcomingEvents = upcoming.Select(e => new CalendarEventResponseDto(
                    e.Id, 
                    e.Subject, 
                    e.IsAllDay ? e.Start.ToString("yyyy-MM-dd") : e.Start.ToString("yyyy-MM-ddTHH:mm:ssZ"), 
                    e.IsAllDay ? e.End.ToString("yyyy-MM-dd") : e.End.ToString("yyyy-MM-ddTHH:mm:ssZ"), 
                    e.Location ?? "", 
                    e.WebLink, 
                    e.CalendarId, 
                    e.IsAllDay
                )).ToList();
            }
            catch (Exception ex)
            {
                // Log error but don't fail the whole request
            }
        }

        // 4. Calendar preferences
        Dictionary<string, CalendarPreferenceDto>? calendarPreferences = null;
        try
        {
            var prefsPath = _storageService.GetCalendarPreferencesPath();
            if (System.IO.File.Exists(prefsPath))
            {
                var json = System.IO.File.ReadAllText(prefsPath);
                calendarPreferences = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, CalendarPreferenceDto>>(json, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
        }
        catch
        {
            // Ignore pref errors
        }

        return Ok(new DashboardDto(upcomingTasks, recentEmails, todaysEvents, upcomingEvents, emailVisible, calendarVisible, calendarPreferences));
    }

    private List<TaskItem> FindAllIncompleteTasks(List<TaskItem> tasks)
    {
        var result = new List<TaskItem>();
        foreach (var task in tasks)
        {
            if (!task.IsCompleted)
            {
                result.Add(task);
            }
            
            // Recurse into subtasks
            if (task.Subtasks.Any())
            {
                result.AddRange(FindAllIncompleteTasks(task.Subtasks));
            }
        }
        return result;
    }
}
