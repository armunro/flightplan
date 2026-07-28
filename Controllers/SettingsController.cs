using FlightPlan.Models.Config;
using FlightPlan.Services;
using Microsoft.AspNetCore.Mvc;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FlightPlan.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private DashConfig _config;
    private readonly IStorageService _storageService;

    public SettingsController(DashConfig config, IStorageService storageService)
    {
        _config = config;
        _storageService = storageService;
    }

    [HttpGet]
    public IActionResult GetConfig()
    {
        return Ok(_config);
    }

    [HttpPost]
    public IActionResult UpdateConfig([FromBody] DashConfig newConfig)
    {
        if (newConfig == null) return BadRequest("Invalid configuration");

        // Update the in-memory singleton
        _config.Jira = newConfig.Jira;
        _config.GitHub = newConfig.GitHub;
        _config.MicrosoftGraph = newConfig.MicrosoftGraph;
        _config.PageVisibilities = newConfig.PageVisibilities;
        _config.ColorSchemes = newConfig.ColorSchemes;
        _config.Debug = newConfig.Debug;

        try
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var yaml = serializer.Serialize(_config);
            System.IO.File.WriteAllText(_storageService.GetConfigPath(), yaml);
            return Ok(_config);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error saving configuration: {ex.Message}");
        }
    }
}
