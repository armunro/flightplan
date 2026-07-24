using FlightPlan.Core.Models;

namespace FlightPlan.Core.Interfaces;

public interface IJiraService
{
    Task<IEnumerable<JiraIssueDto>> GetMyIssuesAsync(bool includeClosed = true);
    Task<IEnumerable<JiraIssueDto>> GetIssuesByJqlAsync(string jql);
    Task<bool> UnassignIssueAsync(string issueKey);
}

public interface IGitHubService
{
    Task<IEnumerable<GitHubPullRequestDto>> GetMyPullRequestsAsync();
    Task<IEnumerable<GitHubPullRequestDto>> GetPullRequestsByQueryAsync(string query);
}
