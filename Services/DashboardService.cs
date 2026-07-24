using FlightPlan.Core.Interfaces;
using FlightPlan.Core.Models;

namespace FlightPlan.Services;

public interface IDashboardService
{
    Task<IEnumerable<JiraIssueDto>> GetMyJiraIssuesAsync(bool includeClosed = true);
    Task<IEnumerable<JiraIssueDto>> GetJiraIssuesByJqlAsync(string jql);
    Task<IEnumerable<GitHubPullRequestDto>> GetMyGitHubPullRequestsAsync();
    Task<IEnumerable<GitHubPullRequestDto>> GetGitHubPullRequestsByQueryAsync(string query);
    Task<bool> UnassignJiraIssueAsync(string issueKey);
}

public class DashboardService : IDashboardService
{
    private readonly IJiraService _jiraService;
    private readonly IGitHubService _gitHubService;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(IJiraService jiraService, IGitHubService gitHubService, ILogger<DashboardService> logger)
    {
        _jiraService = jiraService;
        _gitHubService = gitHubService;
        _logger = logger;
    }

    public Task<IEnumerable<JiraIssueDto>> GetMyJiraIssuesAsync(bool includeClosed = true)
    {
        return _jiraService.GetMyIssuesAsync(includeClosed);
    }

    public Task<IEnumerable<JiraIssueDto>> GetJiraIssuesByJqlAsync(string jql)
    {
        return _jiraService.GetIssuesByJqlAsync(jql);
    }

    public Task<IEnumerable<GitHubPullRequestDto>> GetMyGitHubPullRequestsAsync()
    {
        return _gitHubService.GetMyPullRequestsAsync();
    }

    public Task<IEnumerable<GitHubPullRequestDto>> GetGitHubPullRequestsByQueryAsync(string query)
    {
        return _gitHubService.GetPullRequestsByQueryAsync(query);
    }

    public Task<bool> UnassignJiraIssueAsync(string issueKey)
    {
        return _jiraService.UnassignIssueAsync(issueKey);
    }
}
