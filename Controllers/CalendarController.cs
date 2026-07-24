using FlightPlan.Core.Models;
using Microsoft.AspNetCore.Mvc;
using FlightPlan.Services;

namespace FlightPlan.Controllers;

public record CalendarEventResponseDto(string Id, string Subject, DateTimeOffset Start, DateTimeOffset End, string Location, string? WebLink, string? CalendarId);

[ApiController]
[Route("api/[controller]")]
public class CalendarController : ControllerBase
{
    private readonly IGraphService _graphService;
    private readonly IStorageService _storageService;

    public CalendarController(IGraphService graphService, IStorageService storageService)
    {
        _graphService = graphService;
        _storageService = storageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetEvents([FromQuery] string? calendarId = null, [FromQuery] int top = 20, [FromQuery] DateTime? start = null, [FromQuery] DateTime? end = null)
    {
        var events = await _graphService.GetNextEventsAsync(calendarId, top, start, end);
        var result = events.Select(e => new CalendarEventResponseDto(e.Id, e.Subject, e.Start, e.End, e.Location ?? "", e.WebLink, e.CalendarId));
        return Ok(result);
    }

    [HttpGet("folders")]
    public async Task<IActionResult> GetCalendars()
    {
        var calendars = await _graphService.GetCalendarsAsync();
        return Ok(calendars);
    }

    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences()
    {
        var path = _storageService.GetCalendarPreferencesPath();
        if (!System.IO.File.Exists(path))
        {
            return Ok(new { });
        }

        var json = await System.IO.File.ReadAllTextAsync(path);
        return Content(json, "application/json");
    }

    [HttpPost("preferences")]
    public async Task<IActionResult> SavePreferences([FromBody] dynamic preferences)
    {
        var path = _storageService.GetCalendarPreferencesPath();
        var json = System.Text.Json.JsonSerializer.Serialize(preferences);
        await System.IO.File.WriteAllTextAsync(path, json);
        return Ok();
    }
}
