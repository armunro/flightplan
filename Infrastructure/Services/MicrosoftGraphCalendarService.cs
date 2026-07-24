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
                        config.QueryParameters.Select = new[] { "id", "subject", "start", "end", "location", "webLink" };
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
                        calendarId
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
                                    config.QueryParameters.Select = new[] { "id", "subject", "start", "end", "location", "webLink" };
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
                                    cal.Id
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
}
