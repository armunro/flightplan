using FlightPlan.Core.Interfaces;
using FlightPlan.Core.Models;
using FlightPlan.Models;
using FlightPlan.Models.Config;

namespace FlightPlan.Infrastructure.Services;

public class MockEmailService : IEmailService
{
    private readonly DashConfig _config;
    private readonly List<EmailDto> _mockEmails;
    private readonly List<MailFolderDto> _mockFolders;

    public MockEmailService(DashConfig config)
    {
        _config = config;
        _mockFolders = new List<MailFolderDto>
        {
            new MailFolderDto("inbox", "Inbox", 10, 2, null, 0),
            new MailFolderDto("archive", "Archive", 50, 0, null, 0),
            new MailFolderDto("deleted", "Deleted Items", 5, 0, null, 0)
        };

        _mockEmails = new List<EmailDto>
        {
            new EmailDto(
                "msg-1",
                "Jira",
                "jira@example.com",
                "[JIRA] (DEMO-101) Fix layout issues in the dashboard",
                "Junie assigned DEMO-101 to you. The dashboard layout is broken on mobile devices...",
                DateTimeOffset.UtcNow.AddHours(-1),
                "https://outlook.office.com/mail/inbox/id/msg-1",
                new List<string>()
            ),
            new EmailDto(
                "msg-2",
                "GitHub",
                "noreply@github.com",
                "[GitHub] Pull Request #45 opened in ARmunro/flightplan",
                "Andrew opened a new pull request: Feature/demo-mode. Please review the changes...",
                DateTimeOffset.UtcNow.AddHours(-3),
                "https://outlook.office.com/mail/inbox/id/msg-2",
                new List<string>()
            ),
            new EmailDto(
                "msg-3",
                "Andrew Munro",
                "andrew@example.com",
                "Flight Plan Project Update",
                "Hi, I've updated the project requirements for the next sprint. Let's discuss this afternoon.",
                DateTimeOffset.UtcNow.AddDays(-1),
                "https://outlook.office.com/mail/inbox/id/msg-3",
                new List<string>()
            ),
            new EmailDto(
                "msg-4",
                "System Admin",
                "admin@example.com",
                "Weekly Security Report",
                "No security issues detected in the last 7 days. Your system is up to date.",
                DateTimeOffset.UtcNow.AddDays(-2),
                "https://outlook.office.com/mail/inbox/id/msg-4",
                new List<string>()
            )
        };
    }

    public Task<IEnumerable<MailFolderDto>> GetMailFoldersAsync()
    {
        return Task.FromResult(_mockFolders.AsEnumerable());
    }

    public Task<IEnumerable<EmailDto>> GetEmailsAsync(string folderId = "inbox", int top = 10)
    {
        // Return a copy of the list to simulate API behavior
        IEnumerable<EmailDto> result = _mockEmails;

        if (folderId != "all")
        {
             // For simplicity, we'll just return all for inbox and empty for others in this mock
             if (folderId != "inbox")
             {
                 result = Enumerable.Empty<EmailDto>();
             }
        }

        return Task.FromResult(result.Take(top).ToList().AsEnumerable());
    }

    public Task<EmailDto?> GetEmailAsync(string messageId)
    {
        var email = _mockEmails.FirstOrDefault(e => e.Id == messageId);
        return Task.FromResult(email);
    }

    public Task MoveEmailToDeletedItemsAsync(string messageId)
    {
        var email = _mockEmails.FirstOrDefault(e => e.Id == messageId);
        if (email != null)
        {
            _mockEmails.Remove(email);
        }
        return Task.CompletedTask;
    }

    public Task ApplyRuleActionsAsync(string messageId, List<RuleAction> actions)
    {
        // Mock implementation
        return Task.CompletedTask;
    }
}
