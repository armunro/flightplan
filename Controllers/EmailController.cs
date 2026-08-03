using Microsoft.AspNetCore.Mvc;
using FlightPlan.Services;
using FlightPlan.Models;
using FlightPlan.Core.Models;

namespace FlightPlan.Controllers;

public record MatchingRuleDto(string Name, string? Color);
public record EmailWithRulesDto(string Id, string Subject, string From, string FromAddress, DateTimeOffset ReceivedDateTime, string BodyPreview, string WebLink, string? Body, List<MatchingRuleDto> MatchingRules);

[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly IGraphService _graphService;
    private readonly IRuleService _ruleService;
    private readonly IStorageService _storageService;

    public EmailController(IGraphService graphService, IRuleService ruleService, IStorageService storageService)
    {
        _graphService = graphService;
        _ruleService = ruleService;
        _storageService = storageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetEmails([FromQuery] string folderId = "inbox", [FromQuery] int top = 10)
    {
        var emails = await _graphService.GetEmailsAsync(folderId, top);
        var rules = _ruleService.GetAllRules();

        var result = emails.Select(email => new EmailWithRulesDto(
            email.Id,
            email.Subject,
            email.From,
            email.FromAddress,
            email.ReceivedDateTime,
            email.BodyPreview,
            email.WebLink,
            email.Body,
            rules.Where(r => _ruleService.Matches(r, email)).Select(r => new MatchingRuleDto(r.Name, r.Color)).ToList()
        ));

        return Ok(result);
    }

    [HttpGet("folders")]
    public async Task<IActionResult> GetFolders()
    {
        var folders = await _graphService.GetMailFoldersAsync();
        return Ok(folders);
    }

    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences()
    {
        var path = _storageService.GetEmailPreferencesPath();
        if (!System.IO.File.Exists(path))
        {
            return Ok(new { });
        }

        var json = await System.IO.File.ReadAllTextAsync(path);
        return Content(json, "application/json");
    }

    [HttpPost("preferences")]
    public async Task<IActionResult> SavePreferences([FromBody] dynamic preferences)
    {
        var path = _storageService.GetEmailPreferencesPath();
        var json = System.Text.Json.JsonSerializer.Serialize(preferences);
        await System.IO.File.WriteAllTextAsync(path, json);
        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmail(string id)
    {
        var email = await _graphService.GetEmailAsync(id);
        if (email == null)
        {
            return NotFound();
        }

        var rules = _ruleService.GetAllRules();
        var result = new EmailWithRulesDto(
            email.Id,
            email.Subject,
            email.From,
            email.FromAddress,
            email.ReceivedDateTime,
            email.BodyPreview,
            email.WebLink,
            email.Body,
            rules.Where(r => _ruleService.Matches(r, email)).Select(r => new MatchingRuleDto(r.Name, r.Color)).ToList()
        );

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> MoveToDeletedItems(string id)
    {
        await _graphService.MoveEmailToDeletedItemsAsync(id);
        return NoContent();
    }


    [HttpPost("{id}/apply-rule")]
    public async Task<IActionResult> ApplyRule(string id, [FromBody] string ruleName)
    {
        try
        {
            await _ruleService.ApplyRuleAsync(id, ruleName);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("apply-rule-all")]
    public async Task ApplyRuleAll([FromBody] string ruleName, [FromQuery] string folderId = "inbox", [FromQuery] int top = 50)
    {
        Response.ContentType = "text/event-stream";
        var rules = _ruleService.GetAllRules();
        var targetRule = rules.FirstOrDefault(r => r.Name.Equals(ruleName, StringComparison.OrdinalIgnoreCase));

        if (targetRule == null)
        {
            await Response.WriteAsync($"data: {System.Text.Json.JsonSerializer.Serialize(new { error = "Rule not found" })}\n\n");
            return;
        }

        try
        {
            var emails = (await _graphService.GetEmailsAsync(folderId, top)).ToList();
            int total = emails.Count;
            int processed = 0;

            foreach (var email in emails)
            {
                processed++;
                if (_ruleService.Matches(targetRule, email))
                {
                    await _ruleService.ApplyRuleAsync(email.Id, ruleName);
                }

                await Response.WriteAsync($"data: {System.Text.Json.JsonSerializer.Serialize(new { current = processed, total = total, subject = email.Subject })}\n\n");
                await Response.Body.FlushAsync();
            }

            await Response.WriteAsync("data: [DONE]\n\n");
            await Response.Body.FlushAsync();
        }
        catch (Exception ex)
        {
            await Response.WriteAsync($"data: {System.Text.Json.JsonSerializer.Serialize(new { error = ex.Message })}\n\n");
            await Response.Body.FlushAsync();
        }
    }

    [HttpGet("rules")]
    public IActionResult GetRules()
    {
        return Ok(_ruleService.GetAllRules());
    }

    [HttpPost("rules")]
    public async Task<IActionResult> SaveRule([FromBody] FilterRule rule)
    {
        await _ruleService.SaveRuleAsync(rule);
        return Ok();
    }

    [HttpPost("rules/create-from-email")]
    public async Task<IActionResult> CreateRuleFromEmail([FromQuery] string messageId, [FromQuery] string? ruleName = null)
    {
        try
        {
            var rule = await _ruleService.CreateRuleFromEmailAsync(messageId, ruleName);
            return Ok(rule);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("rules/{name}")]
    public async Task<IActionResult> DeleteRule(string name)
    {
        await _ruleService.DeleteRuleAsync(name);
        return Ok();
    }

    [HttpPost("rules/add-sender")]
    public async Task<IActionResult> AddSenderToRule([FromQuery] string ruleName, [FromQuery] string senderEmail, [FromQuery] string subject)
    {
        try
        {
            var rule = await _ruleService.AddSenderToRuleAsync(ruleName, senderEmail, subject);
            return Ok(rule);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
