using FlightPlan.Core.Interfaces;
using FlightPlan.Core.Models;

namespace FlightPlan.Services;

public interface IGraphService
{
    Task<IEnumerable<MailFolderDto>> GetMailFoldersAsync();
    Task<IEnumerable<EmailDto>> GetEmailsAsync(string folderId = "inbox", int top = 10);
    Task<EmailDto?> GetEmailAsync(string messageId);
    Task MoveEmailToDeletedItemsAsync(string messageId);
    Task ApplyRuleActionsAsync(string messageId, List<Models.RuleAction> actions);
    Task<IEnumerable<MailFolderDto>> GetCalendarsAsync();
    Task<IEnumerable<CalendarEventDto>> GetNextEventsAsync(string? calendarId = null, int top = 10, DateTime? start = null, DateTime? end = null);
}

public class GraphService : IGraphService
{
    private readonly IEmailService _emailService;
    private readonly ICalendarService _calendarService;
    private readonly ILogger<GraphService> _logger;

    public GraphService(IEmailService emailService, ICalendarService calendarService, ILogger<GraphService> logger)
    {
        _emailService = emailService;
        _calendarService = calendarService;
        _logger = logger;
    }

    public Task<IEnumerable<MailFolderDto>> GetMailFoldersAsync()
    {
        return _emailService.GetMailFoldersAsync();
    }

    public Task<IEnumerable<EmailDto>> GetEmailsAsync(string folderId = "inbox", int top = 10)
    {
        return _emailService.GetEmailsAsync(folderId, top);
    }

    public Task<EmailDto?> GetEmailAsync(string messageId)
    {
        return _emailService.GetEmailAsync(messageId);
    }

    public Task MoveEmailToDeletedItemsAsync(string messageId)
    {
        return _emailService.MoveEmailToDeletedItemsAsync(messageId);
    }

    public Task ApplyRuleActionsAsync(string messageId, List<Models.RuleAction> actions)
    {
        return _emailService.ApplyRuleActionsAsync(messageId, actions);
    }

    public Task<IEnumerable<MailFolderDto>> GetCalendarsAsync()
    {
        return _calendarService.GetCalendarsAsync();
    }

    public Task<IEnumerable<CalendarEventDto>> GetNextEventsAsync(string? calendarId = null, int top = 10, DateTime? start = null, DateTime? end = null)
    {
        return _calendarService.GetNextEventsAsync(calendarId, top, start, end);
    }
}
