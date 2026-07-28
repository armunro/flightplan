using FlightPlan.Infrastructure.Services;
using FlightPlan.Core.Interfaces;
using FlightPlan.Core.Models;
using FlightPlan.Models.Config;

namespace FlightPlan.Services;

public interface IDashboardService
{
    Task<IEnumerable<JiraIssueDto>> GetMyJiraIssuesAsync(bool includeClosed = true);
    Task<IEnumerable<JiraIssueDto>> GetJiraIssuesByJqlAsync(string jql);
    Task<IEnumerable<GitHubPullRequestDto>> GetMyGitHubPullRequestsAsync();
    Task<IEnumerable<GitHubPullRequestDto>> GetGitHubPullRequestsByQueryAsync(string query);
    Task<bool> UnassignJiraIssueAsync(string issueKey);
    Task<JiraCommentDto?> AddJiraCommentAsync(string issueKey, string body);
    Task<bool> DeleteJiraCommentAsync(string issueKey, string commentId);
    Task<JiraUserDto?> GetCurrentJiraUserAsync();
}

public class DashboardService : IDashboardService
{
    private readonly IJiraService _jiraService;
    private readonly IGitHubService _gitHubService;
    private readonly MockJiraService _mockJiraService;
    private readonly MockGitHubService _mockGitHubService;
    private readonly DashConfig _config;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IJiraService jiraService, 
        IGitHubService gitHubService, 
        MockJiraService mockJiraService,
        MockGitHubService mockGitHubService,
        DashConfig config, 
        ILogger<DashboardService> logger)
    {
        _jiraService = jiraService;
        _gitHubService = gitHubService;
        _mockJiraService = mockJiraService;
        _mockGitHubService = mockGitHubService;
        _config = config;
        _logger = logger;
    }

    private IJiraService CurrentJiraService => _config.Debug.DemoMode ? _mockJiraService : _jiraService;
    private IGitHubService CurrentGitHubService => _config.Debug.DemoMode ? _mockGitHubService : _gitHubService;

    public Task<IEnumerable<JiraIssueDto>> GetMyJiraIssuesAsync(bool includeClosed = true)
    {
        return CurrentJiraService.GetMyIssuesAsync(includeClosed);
    }

    public Task<IEnumerable<JiraIssueDto>> GetJiraIssuesByJqlAsync(string jql)
    {
        return CurrentJiraService.GetIssuesByJqlAsync(jql);
    }

    public Task<IEnumerable<GitHubPullRequestDto>> GetMyGitHubPullRequestsAsync()
    {
        return CurrentGitHubService.GetMyPullRequestsAsync();
    }

    public Task<IEnumerable<GitHubPullRequestDto>> GetGitHubPullRequestsByQueryAsync(string query)
    {
        return CurrentGitHubService.GetPullRequestsByQueryAsync(query);
    }

    public Task<bool> UnassignJiraIssueAsync(string issueKey)
    {
        return CurrentJiraService.UnassignIssueAsync(issueKey);
    }

    public Task<JiraCommentDto?> AddJiraCommentAsync(string issueKey, string body)
    {
        return CurrentJiraService.AddCommentAsync(issueKey, body);
    }

    public Task<bool> DeleteJiraCommentAsync(string issueKey, string commentId)
    {
        return CurrentJiraService.DeleteCommentAsync(issueKey, commentId);
    }

    public Task<JiraUserDto?> GetCurrentJiraUserAsync()
    {
        return CurrentJiraService.GetCurrentUserAsync();
    }
}
