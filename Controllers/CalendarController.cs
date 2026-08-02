using FlightPlan.Core.Models;
using Microsoft.AspNetCore.Mvc;
using FlightPlan.Services;

namespace FlightPlan.Controllers;

public record CalendarEventResponseDto(string Id, string Subject, string Start, string End, string Location, string? WebLink, string? CalendarId, bool IsAllDay);

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
        var result = events.Select(e => new CalendarEventResponseDto(
            e.Id, 
            e.Subject, 
            e.IsAllDay ? e.Start.ToString("yyyy-MM-dd") : e.Start.ToString("yyyy-MM-ddTHH:mm:ssZ"), 
            e.IsAllDay ? e.End.ToString("yyyy-MM-dd") : e.End.ToString("yyyy-MM-ddTHH:mm:ssZ"), 
            e.Location ?? "", 
            e.WebLink, 
            e.CalendarId, 
            e.IsAllDay));
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

    [HttpPost]
    public async Task<IActionResult> AddEvent([FromBody] CalendarEventDto eventDto)
    {
        try
        {
            var result = await _graphService.AddEventAsync(eventDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(string id, [FromQuery] string? calendarId = null)
    {
        try
        {
            await _graphService.DeleteEventAsync(id, calendarId);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateEvent(string id, [FromBody] CalendarEventDto eventDto)
    {
        try
        {
            var result = await _graphService.UpdateEventAsync(id, eventDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpGet("event/{id}")]
    public async Task<IActionResult> GetEvent(string id, [FromQuery] string? calendarId = null)
    {
        try
        {
            var e = await _graphService.GetEventAsync(id, calendarId);
            if (e == null) return NotFound();
            
            var result = new CalendarEventResponseDto(
                e.Id, 
                e.Subject, 
                e.IsAllDay ? e.Start.ToString("yyyy-MM-dd") : e.Start.ToString("yyyy-MM-ddTHH:mm:ssZ"), 
                e.IsAllDay ? e.End.ToString("yyyy-MM-dd") : e.End.ToString("yyyy-MM-ddTHH:mm:ssZ"), 
                e.Location ?? "", 
                e.WebLink, 
                e.CalendarId, 
                e.IsAllDay);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
