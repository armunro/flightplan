using FlightPlan.Services;
using FlightPlan.Core.Interfaces;
using FlightPlan.Core.Models;
using FlightPlan.Models.Config;

namespace FlightPlan.Infrastructure.Services;

public class MicrosoftGraphCalendarService : MicrosoftGraphBase, ICalendarService
{
    public MicrosoftGraphCalendarService(DashConfig config, ILogger<MicrosoftGraphCalendarService> logger, IStorageService storageService) : base(config, logger, storageService) { }

    public async Task<IEnumerable<MailFolderDto>> GetCalendarsAsync()
    {
        try
        {
            var client = await GetClientAsync();
            var calendars = await client.Me.Calendars.GetAsync();
            if (calendars?.Value == null) return Enumerable.Empty<MailFolderDto>();

            return calendars.Value.Select(c => new MailFolderDto(
                c.Id ?? "",
                c.Name ?? "Unknown",
                0, 0, null, 0
            )).ToList();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error fetching calendars.");
            return Enumerable.Empty<MailFolderDto>();
        }
    }

    public async Task<IEnumerable<CalendarEventDto>> GetNextEventsAsync(string? calendarId = null, int top = 20, DateTime? start = null, DateTime? end = null)
    {
        try
        {
            var client = await GetClientAsync();
            var startDateTime = start?.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") 
                             ?? DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ssZ");
            var endDateTime = end?.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ") 
                           ?? DateTime.UtcNow.AddDays(90).ToString("yyyy-MM-ddTHH:mm:ssZ");

            var allEvents = new List<CalendarEventDto>();
            
            if (!string.IsNullOrEmpty(calendarId))
            {
                var events = await client.Me.Calendars[calendarId].CalendarView
                    .GetAsync(config =>
                    {
                        config.Headers.Add("Prefer", "outlook.timezone=\"UTC\"");
                        config.QueryParameters.Top = top;
                        config.QueryParameters.Select = new[] { "id", "subject", "start", "end", "location", "webLink", "isAllDay" };
                        config.QueryParameters.StartDateTime = startDateTime;
                        config.QueryParameters.EndDateTime = endDateTime;
                    });

                if (events?.Value != null)
                {
                    allEvents.AddRange(events.Value.Select(e => new CalendarEventDto(
                        e.Id ?? "",
                        e.Subject ?? "(No Subject)",
                        e.Start?.DateTime != null ? DateTimeOffset.Parse(e.Start.DateTime) : DateTimeOffset.MinValue,
                        e.End?.DateTime != null ? DateTimeOffset.Parse(e.End.DateTime) : DateTimeOffset.MinValue,
                        e.Location?.DisplayName ?? "",
                        e.WebLink,
                        calendarId,
                        e.IsAllDay ?? false
                    )));
                }
            }
            else
            {
                var calendars = await client.Me.Calendars.GetAsync();
                if (calendars?.Value != null)
                {
                    foreach (var cal in calendars.Value)
                    {
                        try
                        {
                            var events = await client.Me.Calendars[cal.Id].CalendarView
                                .GetAsync(config =>
                                {
                                    config.Headers.Add("Prefer", "outlook.timezone=\"UTC\"");
                                    config.QueryParameters.Top = top;
                                    config.QueryParameters.Select = new[] { "id", "subject", "start", "end", "location", "webLink", "isAllDay" };
                                    config.QueryParameters.StartDateTime = startDateTime;
                                    config.QueryParameters.EndDateTime = endDateTime;
                                });

                            if (events?.Value != null)
                            {
                                allEvents.AddRange(events.Value.Select(e => new CalendarEventDto(
                                    e.Id ?? "",
                                    (cal.Name != "Calendar" ? $"[{cal.Name}] " : "") + (e.Subject ?? "(No Subject)"),
                                    e.Start?.DateTime != null ? DateTimeOffset.Parse(e.Start.DateTime) : DateTimeOffset.MinValue,
                                    e.End?.DateTime != null ? DateTimeOffset.Parse(e.End.DateTime) : DateTimeOffset.MinValue,
                                    e.Location?.DisplayName ?? "",
                                    e.WebLink,
                                    cal.Id,
                                    e.IsAllDay ?? false
                                )));
                            }
                        }
                        catch (Exception calEx)
                        {
                            Logger.LogWarning(calEx, "Error fetching events from calendar {Name}", cal.Name);
                        }
                    }
                }
            }

            return allEvents.OrderBy(e => e.Start).Take(top).ToList();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error fetching calendar events.");
            return Enumerable.Empty<CalendarEventDto>();
        }
    }
    public async Task<CalendarEventDto> AddEventAsync(CalendarEventDto eventDto)
    {
        try
        {
            var client = await GetClientAsync();
            var start = eventDto.Start;
            var end = eventDto.End;

            if (eventDto.IsAllDay)
            {
                // For all-day events, Microsoft Graph requires the time to be midnight.
                // It also requires the duration to be at least 24 hours.
                start = new DateTimeOffset(start.Year, start.Month, start.Day, 0, 0, 0, start.Offset);
                end = new DateTimeOffset(end.Year, end.Month, end.Day, 0, 0, 0, end.Offset);
                
                if (end <= start)
                {
                    end = start.AddDays(1);
                }
            }

            var newEvent = new Microsoft.Graph.Models.Event
            {
                Subject = eventDto.Subject,
                Start = new Microsoft.Graph.Models.DateTimeTimeZone
                {
                    DateTime = start.ToString("yyyy-MM-ddTHH:mm:ss.fffffff"),
                    TimeZone = "UTC"
                },
                End = new Microsoft.Graph.Models.DateTimeTimeZone
                {
                    DateTime = end.ToString("yyyy-MM-ddTHH:mm:ss.fffffff"),
                    TimeZone = "UTC"
                },
                Location = new Microsoft.Graph.Models.Location
                {
                    DisplayName = eventDto.Location
                },
                IsAllDay = eventDto.IsAllDay
            };

            var calendarId = eventDto.CalendarId ?? "primary";
            var createdEvent = await client.Me.Calendars[calendarId].Events.PostAsync(newEvent);

            return new CalendarEventDto(
                createdEvent?.Id ?? "",
                createdEvent?.Subject ?? "",
                createdEvent?.Start?.DateTime != null ? DateTimeOffset.Parse(createdEvent.Start.DateTime) : DateTimeOffset.MinValue,
                createdEvent?.End?.DateTime != null ? DateTimeOffset.Parse(createdEvent.End.DateTime) : DateTimeOffset.MinValue,
                createdEvent?.Location?.DisplayName ?? "",
                createdEvent?.WebLink,
                calendarId,
                createdEvent?.IsAllDay ?? false
            );
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating calendar event.");
            throw;
        }
    }

    public async Task DeleteEventAsync(string eventId, string? calendarId = null)
    {
        try
        {
            var client = await GetClientAsync();
            var targetCalendarId = calendarId ?? "primary";
            await client.Me.Calendars[targetCalendarId].Events[eventId].DeleteAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting calendar event {EventId} from calendar {CalendarId}.", eventId, calendarId);
            throw;
        }
    }

    public async Task<CalendarEventDto> UpdateEventAsync(string eventId, CalendarEventDto eventDto)
    {
        try
        {
            var client = await GetClientAsync();
            var start = eventDto.Start;
            var end = eventDto.End;

            if (eventDto.IsAllDay)
            {
                // For all-day events, Microsoft Graph requires the time to be midnight.
                // It also requires the duration to be at least 24 hours.
                start = new DateTimeOffset(start.Year, start.Month, start.Day, 0, 0, 0, start.Offset);
                end = new DateTimeOffset(end.Year, end.Month, end.Day, 0, 0, 0, end.Offset);
                
                if (end <= start)
                {
                    end = start.AddDays(1);
                }
            }

            var updateEvent = new Microsoft.Graph.Models.Event
            {
                Subject = eventDto.Subject,
                Start = new Microsoft.Graph.Models.DateTimeTimeZone
                {
                    DateTime = start.ToString("yyyy-MM-ddTHH:mm:ss.fffffff"),
                    TimeZone = "UTC"
                },
                End = new Microsoft.Graph.Models.DateTimeTimeZone
                {
                    DateTime = end.ToString("yyyy-MM-ddTHH:mm:ss.fffffff"),
                    TimeZone = "UTC"
                },
                Location = new Microsoft.Graph.Models.Location
                {
                    DisplayName = eventDto.Location
                },
                IsAllDay = eventDto.IsAllDay
            };

            var calendarId = eventDto.CalendarId ?? "primary";
            var updatedEvent = await client.Me.Calendars[calendarId].Events[eventId].PatchAsync(updateEvent);

            return new CalendarEventDto(
                updatedEvent?.Id ?? "",
                updatedEvent?.Subject ?? "",
                updatedEvent?.Start?.DateTime != null ? DateTimeOffset.Parse(updatedEvent.Start.DateTime) : DateTimeOffset.MinValue,
                updatedEvent?.End?.DateTime != null ? DateTimeOffset.Parse(updatedEvent.End.DateTime) : DateTimeOffset.MinValue,
                updatedEvent?.Location?.DisplayName ?? "",
                updatedEvent?.WebLink,
                calendarId,
                updatedEvent?.IsAllDay ?? false
            );
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating calendar event {EventId}.", eventId);
            throw;
        }
    }

    public async Task<CalendarEventDto?> GetEventAsync(string eventId, string? calendarId = null)
    {
        try
        {
            var client = await GetClientAsync();
            var ev = await client.Me.Calendars[calendarId ?? "default"].Events[eventId].GetAsync();
            
            if (ev == null) return null;

            return new CalendarEventDto(
                ev.Id,
                ev.Subject ?? "",
                DateTimeOffset.Parse(ev.Start?.DateTime ?? DateTimeOffset.Now.ToString()),
                DateTimeOffset.Parse(ev.End?.DateTime ?? DateTimeOffset.Now.ToString()),
                ev.Location?.DisplayName,
                ev.WebLink,
                calendarId ?? "default",
                ev.IsAllDay ?? false
            );
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error fetching calendar event {EventId}", eventId);
            return null;
        }
    }
}
