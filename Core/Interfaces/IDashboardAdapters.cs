using FlightPlan.Core.Models;

namespace FlightPlan.Core.Interfaces;

public interface IJiraService
{
    Task<IEnumerable<JiraIssueDto>> GetMyIssuesAsync(bool includeClosed = true);
    Task<IEnumerable<JiraIssueDto>> GetIssuesByJqlAsync(string jql);
    Task<bool> UnassignIssueAsync(string issueKey);
    Task<JiraCommentDto?> AddCommentAsync(string issueKey, string body);
    Task<bool> DeleteCommentAsync(string issueKey, string commentId);
    Task<JiraUserDto?> GetCurrentUserAsync();
}

public interface IGitHubService
{
    Task<IEnumerable<GitHubPullRequestDto>> GetMyPullRequestsAsync();
    Task<IEnumerable<GitHubPullRequestDto>> GetPullRequestsByQueryAsync(string query);
}

public record JiraUserDto(string AccountId, string DisplayName, string EmailAddress);
