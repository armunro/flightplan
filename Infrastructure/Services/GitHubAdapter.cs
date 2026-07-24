using FlightPlan.Core.Interfaces;
using FlightPlan.Core.Models;
using FlightPlan.Models.Config;
using Octokit;

namespace FlightPlan.Infrastructure.Services;

public class GitHubAdapter : IGitHubService
{
    private readonly DashConfig _config;
    private readonly ILogger<GitHubAdapter> _logger;

    public GitHubAdapter(DashConfig config, ILogger<GitHubAdapter> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<IEnumerable<GitHubPullRequestDto>> GetMyPullRequestsAsync()
    {
        return await GetPullRequestsByQueryAsync(null);
    }

    public async Task<IEnumerable<GitHubPullRequestDto>> GetPullRequestsByQueryAsync(string? query)
    {
        var username = _config.GitHub.Username;
        var token = _config.GitHub.AccessToken;

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("GitHub AccessToken is missing.");
            return Enumerable.Empty<GitHubPullRequestDto>();
        }

        try
        {
            _logger.LogInformation("Fetching GitHub PRs. Query: {Query}, User: {Username}", query ?? "default", username);
            var client = new GitHubClient(new ProductHeaderValue("FlightPlan-Dashboard"));
            client.Credentials = new Credentials(token);

            SearchIssuesRequest request;
            if (string.IsNullOrEmpty(query))
            {
                request = new SearchIssuesRequest
                {
                    Type = IssueTypeQualifier.PullRequest,
                    Author = username,
                    Is = new[] { IssueIsQualifier.Open }
                };
            }
            else
            {
                var fullQuery = query;
                if (!string.IsNullOrEmpty(_config.GitHub.Organization) && !query.Contains("org:") && !query.Contains("user:") && !query.Contains("repo:"))
                {
                    fullQuery += $" org:{_config.GitHub.Organization}";
                }
                
                request = new SearchIssuesRequest(fullQuery)
                {
                    Type = IssueTypeQualifier.PullRequest
                };
            }

            var result = await client.Search.SearchIssues(request);
            _logger.LogInformation("Found {Count} GitHub PRs", result.TotalCount);

            var tasks = result.Items.Select(async pr =>
            {
                try
                {
                    var repoFullName = ExtractRepoName(pr.HtmlUrl);
                    var repoParts = repoFullName.Split('/');
                    var owner = repoParts[0];
                    var repoName = repoParts[1];

                    var comments = await client.Issue.Comment.GetAllForIssue(owner, repoName, pr.Number);
                    var fullPr = await client.PullRequest.Get(owner, repoName, pr.Number);

                    return new GitHubPullRequestDto(
                        repoFullName,
                        pr.Number,
                        pr.Title,
                        pr.User.Login,
                        pr.State.StringValue,
                        pr.CreatedAt,
                        pr.HtmlUrl,
                        fullPr.Draft,
                        pr.Body,
                        comments.Select(c => new GitHubCommentDto(c.User.Login, c.Body, c.CreatedAt)).ToList()
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error mapping GitHub issue to DTO. Issue: {Number}", pr.Number);
                    return null;
                }
            });

            var prs = await Task.WhenAll(tasks);
            return prs.Where(pr => pr != null)!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching GitHub pull requests");
            return Enumerable.Empty<GitHubPullRequestDto>();
        }
    }

    private string ExtractRepoName(string url)
    {
        var parts = url.Split('/');
        if (parts.Length >= 5)
        {
            return $"{parts[3]}/{parts[4]}";
        }
        return url;
    }
}
