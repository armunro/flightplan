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
            new MailFolderDto("cal-2", "Project X", 0, 0, null, 0),
            new MailFolderDto("cal-3", "Personal", 0, 0, null, 0),
            new MailFolderDto("cal-4", "Holiday", 0, 0, null, 0),
            new MailFolderDto("cal-5", "Recruitment", 0, 0, null, 0),
            new MailFolderDto("cal-6", "Training", 0, 0, null, 0),
            new MailFolderDto("cal-7", "Birthdays", 0, 0, null, 0),
            new MailFolderDto("cal-8", "Tasks", 0, 0, null, 0)
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
                "cal-1",
                false
            ),
            new CalendarEventDto(
                "ev-2",
                "Flight Plan Sprint Planning",
                new DateTimeOffset(today.AddHours(14), TimeSpan.Zero),
                new DateTimeOffset(today.AddHours(15), TimeSpan.Zero),
                "Conference Room A",
                "https://example.com/teams/meeting2",
                "cal-1",
                false
            ),
            new CalendarEventDto(
                "ev-3",
                "1:1 with Manager",
                new DateTimeOffset(today.AddDays(1).AddHours(11), TimeSpan.Zero),
                new DateTimeOffset(today.AddDays(1).AddHours(11).AddMinutes(30), TimeSpan.Zero),
                "Office",
                null,
                "cal-1",
                false
            ),
            new CalendarEventDto(
                "ev-4",
                "[Project X] Client Demo",
                new DateTimeOffset(today.AddDays(2).AddHours(10), TimeSpan.Zero),
                new DateTimeOffset(today.AddDays(2).AddHours(11), TimeSpan.Zero),
                "Remote",
                "https://example.com/zoom/meeting3",
                "cal-2",
                false
            ),
            new CalendarEventDto(
                "ev-5",
                "Dentist Appointment",
                new DateTimeOffset(today.AddDays(3).AddHours(8), TimeSpan.Zero),
                new DateTimeOffset(today.AddDays(3).AddHours(9), TimeSpan.Zero),
                "Smile Clinic",
                null,
                "cal-3",
                false
            ),
            new CalendarEventDto(
                "ev-6",
                "Product Review Meeting",
                new DateTimeOffset(today.AddHours(11), TimeSpan.Zero),
                new DateTimeOffset(today.AddHours(12), TimeSpan.Zero),
                "Meeting Room 4",
                "https://example.com/teams/meeting-review",
                "cal-1",
                false
            ),
            new CalendarEventDto(
                "ev-7",
                "Gym Session",
                new DateTimeOffset(today.AddDays(1).AddHours(17), TimeSpan.Zero),
                new DateTimeOffset(today.AddDays(1).AddHours(18), TimeSpan.Zero),
                "City Gym",
                null,
                "cal-3",
                false
            ),
            new CalendarEventDto(
                "ev-8",
                "Team Lunch",
                new DateTimeOffset(today.AddDays(4).AddHours(12), TimeSpan.Zero),
                new DateTimeOffset(today.AddDays(4).AddHours(13).AddMinutes(30), TimeSpan.Zero),
                "The Italian Place",
                null,
                "cal-1",
                false
            ),
            new CalendarEventDto(
                "ev-9",
                "JavaScript Workshop",
                new DateTimeOffset(today.AddDays(5).AddHours(13), TimeSpan.Zero),
                new DateTimeOffset(today.AddDays(5).AddHours(16), TimeSpan.Zero),
                "Online",
                "https://example.com/training/js",
                "cal-6",
                false
            ),
            new CalendarEventDto(
                "ev-10",
                "Sarah's Birthday",
                new DateTimeOffset(today.AddDays(1), TimeSpan.Zero),
                new DateTimeOffset(today.AddDays(2), TimeSpan.Zero),
                "All Day",
                null,
                "cal-7",
                true
            ),
            new CalendarEventDto(
                "ev-10.1",
                "Multi-day All Day Event",
                new DateTimeOffset(today.AddDays(2), TimeSpan.Zero),
                new DateTimeOffset(today.AddDays(5), TimeSpan.Zero),
                "Everywhere",
                null,
                "cal-7",
                true
            ),
            new CalendarEventDto(
                "ev-11",
                "Candidate Interview - Senior Dev",
                new DateTimeOffset(today.AddDays(2).AddHours(14), TimeSpan.Zero),
                new DateTimeOffset(today.AddDays(2).AddHours(15), TimeSpan.Zero),
                "Teams",
                "https://example.com/interview/123",
                "cal-5",
                false
            ),
            new CalendarEventDto(
                "ev-12",
                "Weekly Sync - Project Y",
                new DateTimeOffset(today.AddDays(1).AddHours(10), TimeSpan.Zero),
                new DateTimeOffset(today.AddDays(1).AddHours(10).AddMinutes(30), TimeSpan.Zero),
                "Room 2",
                null,
                "cal-1",
                false
            ),
            new CalendarEventDto(
                "ev-13",
                "Architecture Review",
                new DateTimeOffset(today.AddDays(3).AddHours(15), TimeSpan.Zero),
                new DateTimeOffset(today.AddDays(3).AddHours(17), TimeSpan.Zero),
                "Boardroom",
                null,
                "cal-1",
                false
            ),
            new CalendarEventDto(
                "ev-14",
                "Car Service",
                new DateTimeOffset(today.AddDays(2).AddHours(8), TimeSpan.Zero),
                new DateTimeOffset(today.AddDays(2).AddHours(9), TimeSpan.Zero),
                "Local Garage",
                null,
                "cal-3",
                false
            ),
            new CalendarEventDto(
                "ev-15",
                "Quarterly Planning",
                new DateTimeOffset(today.AddDays(7).AddHours(9), TimeSpan.Zero),
                new DateTimeOffset(today.AddDays(7).AddHours(17), TimeSpan.Zero),
                "Offsite Location",
                null,
                "cal-1",
                false
            ),
            new CalendarEventDto(
                "ev-16",
                "Deployment Window",
                new DateTimeOffset(today.AddDays(3).AddHours(20), TimeSpan.Zero),
                new DateTimeOffset(today.AddDays(3).AddHours(22), TimeSpan.Zero),
                "Production Environment",
                null,
                "cal-1",
                false
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
            query = query.Where(e => e.End >= startOffset);
        }

        if (end.HasValue)
        {
            var endOffset = new DateTimeOffset(end.Value);
            query = query.Where(e => e.Start <= endOffset);
        }

        return Task.FromResult(query.OrderBy(e => e.Start).Take(top).ToList().AsEnumerable());
    }

    public Task<CalendarEventDto> AddEventAsync(CalendarEventDto eventDto)
    {
        var start = eventDto.Start;
        var end = eventDto.End;

        if (eventDto.IsAllDay)
        {
            start = new DateTimeOffset(start.Year, start.Month, start.Day, 0, 0, 0, start.Offset);
            end = new DateTimeOffset(end.Year, end.Month, end.Day, 0, 0, 0, end.Offset);
            
            if (end <= start)
            {
                end = start.AddDays(1);
            }
        }

        var newEvent = eventDto with { Id = $"ev-{Guid.NewGuid()}", Start = start, End = end };
        _mockEvents.Add(newEvent);
        return Task.FromResult(newEvent);
    }

    public Task DeleteEventAsync(string eventId, string? calendarId = null)
    {
        var ev = _mockEvents.FirstOrDefault(e => e.Id == eventId);
        if (ev != null)
        {
            _mockEvents.Remove(ev);
        }
        return Task.CompletedTask;
    }

    public Task<CalendarEventDto> UpdateEventAsync(string eventId, CalendarEventDto eventDto)
    {
        var index = _mockEvents.FindIndex(e => e.Id == eventId);
        if (index != -1)
        {
            var start = eventDto.Start;
            var end = eventDto.End;

            if (eventDto.IsAllDay)
            {
                start = new DateTimeOffset(start.Year, start.Month, start.Day, 0, 0, 0, start.Offset);
                end = new DateTimeOffset(end.Year, end.Month, end.Day, 0, 0, 0, end.Offset);

                if (end <= start)
                {
                    end = start.AddDays(1);
                }
            }

            var updated = eventDto with { Id = eventId, Start = start, End = end };
            _mockEvents[index] = updated;
            return Task.FromResult(updated);
        }
        return Task.FromResult(eventDto);
    }
    public Task<CalendarEventDto?> GetEventAsync(string eventId, string? calendarId = null)
    {
        var ev = _mockEvents.FirstOrDefault(e => e.Id == eventId);
        return Task.FromResult(ev);
    }
}
