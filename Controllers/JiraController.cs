using FlightPlan.Core.Interfaces;
using FlightPlan.Core.Models;
using FlightPlan.Models.Config;
using Microsoft.AspNetCore.Mvc;
using FlightPlan.Services;

namespace FlightPlan.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JiraController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly DashConfig _config;
    private readonly JiraStarredService _starredService;

    public JiraController(IDashboardService dashboardService, DashConfig config, JiraStarredService starredService)
    {
        _dashboardService = dashboardService;
        _config = config;
        _starredService = starredService;
    }

    [HttpGet]
    public async Task<IActionResult> GetIssues([FromQuery] bool showClosed = true, [FromQuery] string? jql = null)
    {
        IEnumerable<JiraIssueDto> issues;
        if (!string.IsNullOrEmpty(jql))
        {
            issues = await _dashboardService.GetJiraIssuesByJqlAsync(jql);
        }
        else
        {
            issues = await _dashboardService.GetMyJiraIssuesAsync(showClosed);
        }

        var result = issues.Select(i => new JiraIssueResponseDto(
            i.Key, 
            i.Summary, 
            i.Status, 
            i.Priority, 
            i.Url, 
            i.Assignee, 
            i.Created, 
            i.Updated, 
            i.Description,
            i.IssueType,
            i.Comments?.Select(c => new JiraCommentResponseDto(c.Id, c.Author, c.Body, c.Created)).ToList(),
            i.Reporter
        ));
        return Ok(result);
    }

    [HttpGet("issue/{key}")]
    public async Task<IActionResult> GetIssue(string key)
    {
        if (string.IsNullOrEmpty(key)) return BadRequest("Key is required");
        
        var issues = await _dashboardService.GetJiraIssuesByJqlAsync($"key = \"{key}\"");
        var issue = issues.FirstOrDefault();
        
        if (issue == null) return NotFound();

        var result = new JiraIssueResponseDto(
            issue.Key, 
            issue.Summary, 
            issue.Status, 
            issue.Priority, 
            issue.Url, 
            issue.Assignee, 
            issue.Created, 
            issue.Updated, 
            issue.Description,
            issue.IssueType,
            issue.Comments?.Select(c => new JiraCommentResponseDto(c.Id, c.Author, c.Body, c.Created)).ToList(),
            issue.Reporter
        );
        
        return Ok(result);
    }

    [HttpGet("queries")]
    public IActionResult GetQueries()
    {
        return Ok(_config.Jira.Queries);
    }

    [HttpGet("starred")]
    public IActionResult GetStarredIssues()
    {
        return Ok(_starredService.GetStarredKeys());
    }

    [HttpPost("star")]
    public IActionResult ToggleStar([FromQuery] string key)
    {
        if (string.IsNullOrEmpty(key)) return BadRequest("Key is required");
        var isStarred = _starredService.ToggleStar(key);
        return Ok(new { key, isStarred });
    }
    
    [HttpPost("unassign")]
    public async Task<IActionResult> UnassignIssue([FromQuery] string key)
    {
        if (string.IsNullOrEmpty(key)) return BadRequest("Key is required");
        var success = await _dashboardService.UnassignJiraIssueAsync(key);
        if (success) return Ok();
        return BadRequest("Failed to unassign issue");
    }

    [HttpPost("comment")]
    public async Task<IActionResult> AddComment([FromQuery] string key, [FromBody] JiraCommentRequest request)
    {
        if (string.IsNullOrEmpty(key)) return BadRequest("Key is required");
        if (string.IsNullOrEmpty(request.Body)) return BadRequest("Comment body is required");

        var comment = await _dashboardService.AddJiraCommentAsync(key, request.Body);
        if (comment != null) return Ok(new JiraCommentResponseDto(comment.Id, comment.Author, comment.Body, comment.Created));
        return BadRequest("Failed to add comment");
    }

    [HttpGet("myself")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _dashboardService.GetCurrentJiraUserAsync();
        if (user != null) return Ok(user);
        return NotFound("User not found or Jira not configured");
    }

    [HttpDelete("comment")]
    public async Task<IActionResult> DeleteComment([FromQuery] string key, [FromQuery] string id)
    {
        if (string.IsNullOrEmpty(key)) return BadRequest("Key is required");
        if (string.IsNullOrEmpty(id)) return BadRequest("Comment ID is required");

        var success = await _dashboardService.DeleteJiraCommentAsync(key, id);
        if (success) return Ok();
        return BadRequest("Failed to delete comment");
    }

    public class JiraCommentRequest
    {
        public string Body { get; set; } = string.Empty;
    }
}
