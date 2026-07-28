using FlightPlan.Core.Interfaces;
using FlightPlan.Core.Models;
using FlightPlan.Models.Config;

namespace FlightPlan.Infrastructure.Services;

public class MockJiraService : IJiraService
{
    private readonly DashConfig _config;
    private readonly List<JiraIssueDto> _mockIssues;

    public MockJiraService(DashConfig config)
    {
        _config = config;
        var now = DateTime.UtcNow;
        _mockIssues = new List<JiraIssueDto>
        {
            new JiraIssueDto(
                "DEMO-101",
                "Fix layout issues in the dashboard",
                "In Progress",
                "High",
                "Junie",
                now.AddDays(-2),
                now.AddHours(-3),
                "https://example.atlassian.net/browse/DEMO-101",
                "{\"type\":\"doc\",\"version\":1,\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"The dashboard layout is broken on mobile devices. We need to fix the CSS grid.\"}]}]}",
                "Bug",
                new List<JiraCommentDto> {
                    new JiraCommentDto("1001", "Manager", "{\"type\":\"doc\",\"version\":1,\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"Please prioritize this for the next release.\"}]}]}", now.AddDays(-1)),
                    new JiraCommentDto("1002", "You (Demo)", "{\"type\":\"doc\",\"version\":1,\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"I am working on this right now.\"}]}]}", now.AddMinutes(-30))
                },
                "Andrew"
            ),
            new JiraIssueDto(
                "DEMO-102",
                "Implement new authentication provider",
                "To Do",
                "Medium",
                "Andrew",
                now.AddDays(-5),
                now.AddDays(-5),
                "https://example.atlassian.net/browse/DEMO-102",
                "{\"type\":\"doc\",\"version\":1,\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"Add support for OAuth2 authentication.\"}]}]}",
                "Story",
                new List<JiraCommentDto>(),
                "Manager"
            ),
            new JiraIssueDto(
                "DEMO-103",
                "Update documentation for API v2",
                "Done",
                "Low",
                "Junie",
                now.AddDays(-10),
                now.AddDays(-1),
                "https://example.atlassian.net/browse/DEMO-103",
                "{\"type\":\"doc\",\"version\":1,\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"Documentation needs to be updated to reflect the changes in API v2.\"}]}]}",
                "Task",
                new List<JiraCommentDto>(),
                "Andrew"
            ),
            new JiraIssueDto(
                "DEMO-104",
                "Performance optimization for data tables",
                "In Progress",
                "High",
                "Andrew",
                now.AddDays(-3),
                now.AddHours(-5),
                "https://example.atlassian.net/browse/DEMO-104",
                "{\"type\":\"doc\",\"version\":1,\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"Data tables are slow when loading more than 100 rows. We need to implement virtualization.\"}]}]}",
                "Bug",
                new List<JiraCommentDto>(),
                "Junie"
            ),
            new JiraIssueDto(
                "DEMO-105",
                "Integrate with external analytics service",
                "To Do",
                "Medium",
                "Junie",
                now.AddDays(-7),
                now.AddDays(-7),
                "https://example.atlassian.net/browse/DEMO-105",
                "{\"type\":\"doc\",\"version\":1,\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"We need to start tracking user engagement using the new analytics API.\"}]}]}",
                "Story",
                new List<JiraCommentDto>(),
                "Manager"
            ),
            new JiraIssueDto(
                "DEMO-106",
                "Security audit findings - Q2",
                "In Progress",
                "Highest",
                "Andrew",
                now.AddDays(-1),
                now.AddHours(-1),
                "https://example.atlassian.net/browse/DEMO-106",
                "{\"type\":\"doc\",\"version\":1,\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"Address the vulnerabilities found during the last security audit.\"}]}]}",
                "Bug",
                new List<JiraCommentDto>(),
                "Security Team"
            ),
            new JiraIssueDto(
                "DEMO-107",
                "Refactor state management in frontend",
                "To Do",
                "Low",
                "Junie",
                now.AddDays(-14),
                now.AddDays(-2),
                "https://example.atlassian.net/browse/DEMO-107",
                "{\"type\":\"doc\",\"version\":1,\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"Switch from local state to a centralized store for better maintainability.\"}]}]}",
                "Task",
                new List<JiraCommentDto>(),
                "Andrew"
            ),
            new JiraIssueDto(
                "DEMO-108",
                "Add multi-language support (i18n)",
                "To Do",
                "Medium",
                "Andrew",
                DateTime.Now.AddDays(-20),
                DateTime.Now.AddDays(-20),
                "https://example.atlassian.net/browse/DEMO-108",
                "{\"type\":\"doc\",\"version\":1,\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"The application should support at least English, Spanish, and French.\"}]}]}",
                "Story",
                new List<JiraCommentDto>(),
                "Manager"
            ),
            new JiraIssueDto(
                "DEMO-109",
                "Fix memory leak in background worker",
                "Done",
                "High",
                "Andrew",
                DateTime.Now.AddDays(-30),
                DateTime.Now.AddDays(-5),
                "https://example.atlassian.net/browse/DEMO-109",
                "{\"type\":\"doc\",\"version\":1,\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"Background worker was consuming too much RAM over time. Fixed by disposing HttpClient instances.\"}]}]}",
                "Bug",
                new List<JiraCommentDto>(),
                "Junie"
            ),
            new JiraIssueDto(
                "DEMO-110",
                "New user onboarding walkthrough",
                "In Progress",
                "Medium",
                "Junie",
                DateTime.Now.AddDays(-4),
                DateTime.Now.AddHours(-2),
                "https://example.atlassian.net/browse/DEMO-110",
                "{\"type\":\"doc\",\"version\":1,\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"Implement a step-by-step guide for new users to explain key features.\"}]}]}",
                "Story",
                new List<JiraCommentDto>(),
                "Andrew"
            ),
            new JiraIssueDto(
                "DEMO-111",
                "Export reports to PDF/Excel",
                "To Do",
                "Low",
                "Andrew",
                DateTime.Now.AddDays(-12),
                DateTime.Now.AddDays(-12),
                "https://example.atlassian.net/browse/DEMO-111",
                "{\"type\":\"doc\",\"version\":1,\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"Add buttons to export the current dashboard view to PDF or Excel formats.\"}]}]}",
                "Task",
                new List<JiraCommentDto>(),
                "Manager"
            ),
            new JiraIssueDto(
                "DEMO-112",
                "Database migration to PostgreSQL",
                "Done",
                "Highest",
                "Andrew",
                DateTime.Now.AddDays(-60),
                DateTime.Now.AddDays(-10),
                "https://example.atlassian.net/browse/DEMO-112",
                "{\"type\":\"doc\",\"version\":1,\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"Migration from SQL Server to PostgreSQL is complete. Performance improved by 20%.\"}]}]}",
                "Story",
                new List<JiraCommentDto>(),
                "Infrastructure Team"
            )
        };
    }

    public Task<IEnumerable<JiraIssueDto>> GetMyIssuesAsync(bool includeClosed = true)
    {
        var result = includeClosed ? _mockIssues : _mockIssues.Where(i => i.Status != "Done");
        return Task.FromResult(result.AsEnumerable());
    }

    public Task<IEnumerable<JiraIssueDto>> GetIssuesByJqlAsync(string jql)
    {
        // Simple mock: return all or filter by key if present in JQL
        if (jql.Contains("key ="))
        {
            var parts = jql.Split('\"');
            if (parts.Length >= 2)
            {
                var key = parts[1];
                var issue = _mockIssues.FirstOrDefault(i => i.Key == key);
                return Task.FromResult(issue != null ? new[] { issue }.AsEnumerable() : Enumerable.Empty<JiraIssueDto>());
            }
        }
        return Task.FromResult(_mockIssues.AsEnumerable());
    }

    public Task<bool> UnassignIssueAsync(string issueKey)
    {
        var issue = _mockIssues.FirstOrDefault(i => i.Key == issueKey);
        if (issue != null)
        {
            // In a real app, we'd update the list, but since it's volatile, just return true
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<JiraCommentDto?> AddCommentAsync(string issueKey, string body)
    {
        var issue = _mockIssues.FirstOrDefault(i => i.Key == issueKey);
        if (issue != null && issue.Comments != null)
        {
            var comment = new JiraCommentDto(
                Guid.NewGuid().ToString(),
                "You (Demo)",
                $"{{\"type\":\"doc\",\"version\":1,\"content\":[{{\"type\":\"paragraph\",\"content\":[{{\"type\":\"text\",\"text\":\"{body}\"}}]}}]}}",
                DateTime.Now
            );
            issue.Comments.Add(comment);
            return Task.FromResult<JiraCommentDto?>(comment);
        }
        return Task.FromResult<JiraCommentDto?>(null);
    }

    public Task<bool> DeleteCommentAsync(string issueKey, string commentId)
    {
        var issue = _mockIssues.FirstOrDefault(i => i.Key == issueKey);
        if (issue != null && issue.Comments != null)
        {
            var comment = issue.Comments.FirstOrDefault(c => c.Id == commentId || (commentId.StartsWith("temp-") && c.Author == "You (Demo)"));
            if (comment != null)
            {
                issue.Comments.Remove(comment);
                return Task.FromResult(true);
            }
        }
        return Task.FromResult(false);
    }

    public Task<JiraUserDto?> GetCurrentUserAsync()
    {
        return Task.FromResult<JiraUserDto?>(new JiraUserDto("demo-user", "You (Demo)", "demo@example.com"));
    }
}
