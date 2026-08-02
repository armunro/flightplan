using FlightPlan.Core.Models;
using FlightPlan.Models;

namespace FlightPlan.Core.Interfaces;

public interface IEmailService
{
    Task<IEnumerable<MailFolderDto>> GetMailFoldersAsync();
    Task<IEnumerable<EmailDto>> GetEmailsAsync(string folderId = "inbox", int top = 10);
    Task<EmailDto?> GetEmailAsync(string messageId);
    Task MoveEmailToDeletedItemsAsync(string messageId);
    Task ApplyRuleActionsAsync(string messageId, List<RuleAction> actions);
}

public interface ICalendarService
{
    Task<IEnumerable<MailFolderDto>> GetCalendarsAsync();
    Task<IEnumerable<CalendarEventDto>> GetNextEventsAsync(string? calendarId = null, int top = 10, DateTime? start = null, DateTime? end = null);
    Task<CalendarEventDto> AddEventAsync(CalendarEventDto eventDto);
    Task DeleteEventAsync(string eventId, string? calendarId = null);
    Task<CalendarEventDto> UpdateEventAsync(string eventId, CalendarEventDto eventDto);
    Task<CalendarEventDto?> GetEventAsync(string eventId, string? calendarId = null);
}
