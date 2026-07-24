namespace FlightPlan.Core.Models;

public record JiraCommentDto(
    string Author,
    string Body,
    DateTime Created);

public record JiraIssueDto(
    string Key,
    string Summary,
    string Status,
    string Priority,
    string? Assignee,
    DateTime? Created,
    DateTime? Updated,
    string Url,
    string? Description,
    List<JiraCommentDto>? Comments = null);

public record GitHubCommentDto(
    string Author,
    string Body,
    DateTimeOffset CreatedAt);

public record GitHubPullRequestDto(
    string RepoName,
    int Number,
    string Title,
    string Author,
    string State,
    DateTimeOffset CreatedAt,
    string Url,
    bool IsDraft,
    string? Body = null,
    List<GitHubCommentDto>? Comments = null);
