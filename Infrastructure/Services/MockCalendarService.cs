using FlightPlan.Core.Interfaces;
using FlightPlan.Core.Models;
using FlightPlan.Models.Config;

namespace FlightPlan.Infrastructure.Services;

public class MockCalendarService : ICalendarService
{
    private readonly DashConfig _config;
    private readonly List<CalendarEventDto> _mockEvents;
    private readonly List<MailFolderDto> _mockCalendars;

    public MockCalendarService(DashConfig config)
    {
        _config = config;
        _mockCalendars = new List<MailFolderDto>
        {
            new MailFolderDto("cal-1", "Calendar", 0, 0, null, 0),
            new MailFolderDto("cal-2", "Project X", 0, 0, null, 0)
        };

        var today = DateTime.UtcNow.Date;
        _mockEvents = new List<CalendarEventDto>
        {
            new CalendarEventDto(
                "ev-1",
                "Daily Standup",
                new DateTimeOffset(today.AddHours(9), TimeSpan.Zero),
                new DateTimeOffset(today.AddHours(9).AddMinutes(15), TimeSpan.Zero),
                "Microsoft Teams",
                "https://example.com/teams/meeting1",
                "cal-1"
            ),
            new CalendarEventDto(
                "ev-2",
                "Flight Plan Sprint Planning",
                new DateTimeOffset(today.AddHours(14), TimeSpan.Zero),
                new DateTimeOffset(today.AddHours(15), TimeSpan.Zero),
                "Conference Room A",
                "https://example.com/teams/meeting2",
                "cal-1"
            ),
            new CalendarEventDto(
                "ev-3",
                "1:1 with Manager",
                new DateTimeOffset(today.AddDays(1).AddHours(11), TimeSpan.Zero),
                new DateTimeOffset(today.AddDays(1).AddHours(11).AddMinutes(30), TimeSpan.Zero),
                "Office",
                null,
                "cal-1"
            ),
            new CalendarEventDto(
                "ev-4",
                "[Project X] Client Demo",
                new DateTimeOffset(today.AddDays(2).AddHours(10), TimeSpan.Zero),
                new DateTimeOffset(today.AddDays(2).AddHours(11), TimeSpan.Zero),
                "Remote",
                "https://example.com/zoom/meeting3",
                "cal-2"
            )
        };
    }

    public Task<IEnumerable<MailFolderDto>> GetCalendarsAsync()
    {
        return Task.FromResult(_mockCalendars.AsEnumerable());
    }

    public Task<IEnumerable<CalendarEventDto>> GetNextEventsAsync(string? calendarId = null, int top = 10, DateTime? start = null, DateTime? end = null)
    {
        var query = _mockEvents.AsEnumerable();
        
        if (!string.IsNullOrEmpty(calendarId) && calendarId != "all")
        {
            query = query.Where(e => e.CalendarId == calendarId);
        }

        if (start.HasValue)
        {
            var startOffset = new DateTimeOffset(start.Value);
            query = query.Where(e => e.Start >= startOffset);
        }

        if (end.HasValue)
        {
            var endOffset = new DateTimeOffset(end.Value);
            query = query.Where(e => e.End <= endOffset);
        }

        return Task.FromResult(query.OrderBy(e => e.Start).Take(top).ToList().AsEnumerable());
    }
}
