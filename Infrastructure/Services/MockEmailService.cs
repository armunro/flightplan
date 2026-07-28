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
            new MailFolderDto("inbox", "Inbox", 40, 10, null, 0),
            new MailFolderDto("archive", "Archive", 200, 0, null, 0),
            new MailFolderDto("deleted", "Deleted Items", 50, 0, null, 0),
            new MailFolderDto("junk", "Junk Email", 15, 5, null, 0),
            new MailFolderDto("sent", "Sent Items", 120, 0, null, 0),
            new MailFolderDto("drafts", "Drafts", 4, 4, null, 0),
            new MailFolderDto("projects", "Projects", 80, 2, null, 0),
            new MailFolderDto("personal", "Personal", 35, 1, null, 0),
            new MailFolderDto("travel", "Travel", 12, 0, null, 0),
            new MailFolderDto("finance", "Finance", 25, 0, null, 0),
            new MailFolderDto("notifications", "Notifications", 150, 0, null, 0),
            new MailFolderDto("social", "Social", 45, 12, null, 0)
        };

        _mockEmails = new List<EmailDto>
        {
            new EmailDto(
                "msg-1",
                "Jira",
                "jira@example.com",
                "[JIRA] (DEMO-101) Fix layout issues in the dashboard",
                "Junie assigned DEMO-101 to you. The dashboard layout is broken on mobile devices. We need to fix the CSS grid to ensure the charts don't overlap on smaller screens.",
                DateTimeOffset.UtcNow.AddHours(-1),
                "https://outlook.office.com/mail/inbox/id/msg-1",
                new List<string>()
            ),
            new EmailDto(
                "msg-2",
                "GitHub",
                "noreply@github.com",
                "[GitHub] Pull Request #45 opened in ARmunro/flightplan",
                "Andrew opened a new pull request: Feature/demo-mode. Please review the changes that enable the new demo mode for the application.",
                DateTimeOffset.UtcNow.AddHours(-3),
                "https://outlook.office.com/mail/inbox/id/msg-2",
                new List<string>()
            ),
            new EmailDto(
                "msg-3",
                "Andrew Munro",
                "andrew@example.com",
                "Flight Plan Project Update",
                "Hi, I've updated the project requirements for the next sprint. Let's discuss this afternoon at 2 PM.",
                DateTimeOffset.UtcNow.AddDays(-1),
                "https://outlook.office.com/mail/inbox/id/msg-3",
                new List<string>()
            ),
            new EmailDto(
                "msg-4",
                "System Admin",
                "admin@example.com",
                "Weekly Security Report",
                "No security issues detected in the last 7 days. Your system is up to date and all patches have been applied.",
                DateTimeOffset.UtcNow.AddDays(-2),
                "https://outlook.office.com/mail/inbox/id/msg-4",
                new List<string>()
            ),
            new EmailDto(
                "msg-5",
                "Azure Alerts",
                "alerts@azure.com",
                "Azure Alerts: High CPU usage on Production-Web-01",
                "Production-Web-01 has exceeded 90% CPU usage for the last 15 minutes. Please investigate potential traffic spikes.",
                DateTimeOffset.UtcNow.AddMinutes(-45),
                "https://outlook.office.com/mail/inbox/id/msg-5",
                new List<string>()
            ),
            new EmailDto(
                "msg-6",
                "Team Manager",
                "manager@example.com",
                "Upcoming Team Offsite",
                "We are planning a team offsite for next month. Please fill out the survey to choose your preferred location and dietary requirements.",
                DateTimeOffset.UtcNow.AddHours(-5),
                "https://outlook.office.com/mail/inbox/id/msg-6",
                new List<string>()
            ),
            new EmailDto(
                "msg-7",
                "Cloud Provider",
                "billing@cloud.com",
                "Monthly Invoice - June 2026",
                "Your monthly invoice for June 2026 is now available. The total amount of $450.00 will be charged to your card on file.",
                DateTimeOffset.UtcNow.AddDays(-3),
                "https://outlook.office.com/mail/inbox/id/msg-7",
                new List<string>()
            ),
            new EmailDto(
                "msg-8",
                "Support Desk",
                "support@example.com",
                "New Ticket: #12938 - Password Reset",
                "A new support ticket has been opened regarding a password reset request. User: Sarah Jenkins.",
                DateTimeOffset.UtcNow.AddHours(-8),
                "https://outlook.office.com/mail/inbox/id/msg-8",
                new List<string>()
            ),
            new EmailDto(
                "msg-9",
                "Marketing Team",
                "marketing@example.com",
                "Newsletter - Q3 Plans",
                "Here are the marketing plans for Q3. We are focusing on social media outreach and new blog content.",
                DateTimeOffset.UtcNow.AddDays(-4),
                "https://outlook.office.com/mail/inbox/id/msg-9",
                new List<string>()
            ),
            new EmailDto(
                "msg-10",
                "HR Department",
                "hr@example.com",
                "Policy Update: Remote Work",
                "We have updated the remote work policy. Please review the attached document for the new guidelines.",
                DateTimeOffset.UtcNow.AddDays(-5),
                "https://outlook.office.com/mail/inbox/id/msg-10",
                new List<string>()
            ),
            new EmailDto(
                "msg-11",
                "Newsletter",
                "tech@newsletter.com",
                "Top 10 Trends in Web Development 2026",
                "Check out the latest trends: AI-driven UI, WebAssembly growth, and the rise of edge computing.",
                DateTimeOffset.UtcNow.AddDays(-6),
                "https://outlook.office.com/mail/inbox/id/msg-11",
                new List<string>()
            ),
            new EmailDto(
                "msg-12",
                "External Client",
                "client@partner.com",
                "Proposal for Q4 Integration",
                "Attached is our proposal for the Q4 integration project. We look forward to your feedback.",
                DateTimeOffset.UtcNow.AddDays(-7),
                "https://outlook.office.com/mail/inbox/id/msg-12",
                new List<string>()
            ),
            new EmailDto(
                "msg-13",
                "Meeting Bot",
                "bot@meeting.com",
                "Summary: Daily Standup",
                "Here is the summary of today's standup. Key action items: Andrew to fix DEMO-101, Junie to review PR #45.",
                DateTimeOffset.UtcNow.AddHours(-2),
                "https://outlook.office.com/mail/inbox/id/msg-13",
                new List<string>()
            ),
            new EmailDto(
                "msg-14",
                "LinkedIn",
                "notifications@linkedin.com",
                "You have 3 new connection requests",
                "See who wants to connect with you and expand your professional network on LinkedIn.",
                DateTimeOffset.UtcNow.AddDays(-1),
                "https://outlook.office.com/mail/inbox/id/msg-14",
                new List<string>()
            ),
            new EmailDto(
                "msg-15",
                "Slack",
                "notifications@slack.com",
                "Direct message from Sarah in #general",
                "Sarah: 'Hey Andrew, do you have a minute to look at the new design mockups?'",
                DateTimeOffset.UtcNow.AddMinutes(-10),
                "https://outlook.office.com/mail/inbox/id/msg-15",
                new List<string>()
            ),
            new EmailDto(
                "msg-16",
                "Travel Agency",
                "booking@travel.com",
                "Confirmation: Flight to Seattle",
                "Your flight to Seattle (SEA) on July 30th is confirmed. Booking reference: ABC123XYZ.",
                DateTimeOffset.UtcNow.AddDays(-10),
                "https://outlook.office.com/mail/inbox/id/msg-16",
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
