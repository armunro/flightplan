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
        _mockIssues = new List<JiraIssueDto>
        {
            new JiraIssueDto(
                "DEMO-101",
                "Fix layout issues in the dashboard",
                "In Progress",
                "High",
                "Junie",
                DateTime.Now.AddDays(-2),
                DateTime.Now.AddHours(-3),
                "https://example.atlassian.net/browse/DEMO-101",
                "{\"type\":\"doc\",\"version\":1,\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"The dashboard layout is broken on mobile devices. We need to fix the CSS grid.\"}]}]}",
                "Bug",
                new List<JiraCommentDto> {
                    new JiraCommentDto("1001", "Manager", "{\"type\":\"doc\",\"version\":1,\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"Please prioritize this for the next release.\"}]}]}", DateTime.Now.AddDays(-1)),
                    new JiraCommentDto("1002", "You (Demo)", "{\"type\":\"doc\",\"version\":1,\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"I am working on this right now.\"}]}]}", DateTime.Now.AddMinutes(-30))
                },
                "Andrew"
            ),
            new JiraIssueDto(
                "DEMO-102",
                "Implement new authentication provider",
                "To Do",
                "Medium",
                "Andrew",
                DateTime.Now.AddDays(-5),
                DateTime.Now.AddDays(-5),
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
                DateTime.Now.AddDays(-10),
                DateTime.Now.AddDays(-1),
                "https://example.atlassian.net/browse/DEMO-103",
                "{\"type\":\"doc\",\"version\":1,\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"Documentation needs to be updated to reflect the changes in API v2.\"}]}]}",
                "Task",
                new List<JiraCommentDto>(),
                "Andrew"
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
