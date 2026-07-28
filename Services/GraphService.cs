using FlightPlan.Infrastructure.Services;
using FlightPlan.Core.Interfaces;
using FlightPlan.Core.Models;
using FlightPlan.Models.Config;

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
    private readonly MockEmailService _mockEmailService;
    private readonly MockCalendarService _mockCalendarService;
    private readonly DashConfig _config;
    private readonly ILogger<GraphService> _logger;

    public GraphService(
        IEmailService emailService, 
        ICalendarService calendarService, 
        MockEmailService mockEmailService,
        MockCalendarService mockCalendarService,
        DashConfig config, 
        ILogger<GraphService> logger)
    {
        _emailService = emailService;
        _calendarService = calendarService;
        _mockEmailService = mockEmailService;
        _mockCalendarService = mockCalendarService;
        _config = config;
        _logger = logger;
    }

    private IEmailService CurrentEmailService => _config.Debug.DemoMode ? _mockEmailService : _emailService;
    private ICalendarService CurrentCalendarService => _config.Debug.DemoMode ? _mockCalendarService : _calendarService;

    public Task<IEnumerable<MailFolderDto>> GetMailFoldersAsync()
    {
        return CurrentEmailService.GetMailFoldersAsync();
    }

    public Task<IEnumerable<EmailDto>> GetEmailsAsync(string folderId = "inbox", int top = 10)
    {
        return CurrentEmailService.GetEmailsAsync(folderId, top);
    }

    public Task<EmailDto?> GetEmailAsync(string messageId)
    {
        return CurrentEmailService.GetEmailAsync(messageId);
    }

    public Task MoveEmailToDeletedItemsAsync(string messageId)
    {
        return CurrentEmailService.MoveEmailToDeletedItemsAsync(messageId);
    }

    public Task ApplyRuleActionsAsync(string messageId, List<Models.RuleAction> actions)
    {
        return CurrentEmailService.ApplyRuleActionsAsync(messageId, actions);
    }

    public Task<IEnumerable<MailFolderDto>> GetCalendarsAsync()
    {
        return CurrentCalendarService.GetCalendarsAsync();
    }

    public Task<IEnumerable<CalendarEventDto>> GetNextEventsAsync(string? calendarId = null, int top = 10, DateTime? start = null, DateTime? end = null)
    {
        return CurrentCalendarService.GetNextEventsAsync(calendarId, top, start, end);
    }
}
