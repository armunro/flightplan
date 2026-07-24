using FlightPlan.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlightPlan.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RulesController : ControllerBase
{
    private readonly IRuleService _ruleService;

    public RulesController(IRuleService ruleService)
    {
        _ruleService = ruleService;
    }

    [HttpGet]
    public IActionResult GetRules()
    {
        var rules = _ruleService.GetAllRules().Select(r => new { r.Name });
        return Ok(rules);
    }

    [HttpPost("apply")]
    public async Task<IActionResult> ApplyRule([FromQuery] string messageId, [FromQuery] string ruleName)
    {
        try
        {
            await _ruleService.ApplyRuleAsync(messageId, ruleName);
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

    [HttpPost("create-from-email")]
    public async Task<IActionResult> CreateFromEmail([FromQuery] string messageId, [FromQuery] string? ruleName = null)
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
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
