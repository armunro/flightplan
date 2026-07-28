using FlightPlan.Core.Interfaces;
using FlightPlan.Core.Models;
using FlightPlan.Models.Config;
using Microsoft.AspNetCore.Mvc;
using FlightPlan.Services;

namespace FlightPlan.Controllers;

public record JiraCommentResponseDto(string Id, string Author, string Body, DateTime Created);
public record JiraIssueResponseDto(string Key, string Summary, string Status, string Priority, string Url, string? Assignee, DateTime? Created, DateTime? Updated, string? Description, string? IssueType = null, List<JiraCommentResponseDto>? Comments = null);
public record GitHubCommentResponseDto(string Author, string Body, DateTimeOffset CreatedAt);
public record GitHubPrResponseDto(string Title, string Repository, string Author, string Status, string Url, DateTimeOffset CreatedAt, string? Body, List<GitHubCommentResponseDto>? Comments, bool IsDraft, int Number);

[ApiController]
[Route("api/[controller]")]
public class GitHubController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly DashConfig _config;
    private readonly GitHubStarredService _starredService;

    public GitHubController(IDashboardService dashboardService, DashConfig config, GitHubStarredService starredService)
    {
        _dashboardService = dashboardService;
        _config = config;
        _starredService = starredService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPullRequests([FromQuery] string? query = null)
    {
        IEnumerable<GitHubPullRequestDto> prs;
        if (!string.IsNullOrEmpty(query))
        {
            prs = await _dashboardService.GetGitHubPullRequestsByQueryAsync(query);
        }
        else
        {
            prs = await _dashboardService.GetMyGitHubPullRequestsAsync();
        }

        var result = prs.Select(p => new GitHubPrResponseDto(
            p.Title, 
            p.RepoName, 
            p.Author,
            p.State, 
            p.Url, 
            p.CreatedAt, 
            p.Body, 
            p.Comments?.Select(c => new GitHubCommentResponseDto(c.Author, c.Body, c.CreatedAt)).ToList(),
            p.IsDraft,
            p.Number
        ));
        return Ok(result);
    }

    [HttpGet("queries")]
    public IActionResult GetQueries()
    {
        return Ok(_config.GitHub.Queries);
    }

    [HttpGet("starred")]
    public IActionResult GetStarredPrs()
    {
        return Ok(_starredService.GetStarredUrls());
    }

    [HttpPost("star")]
    public IActionResult ToggleStar([FromQuery] string url)
    {
        if (string.IsNullOrEmpty(url)) return BadRequest("URL is required");
        var isStarred = _starredService.ToggleStar(url);
        return Ok(new { url, isStarred });
    }
}
