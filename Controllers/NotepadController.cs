using Microsoft.AspNetCore.Mvc;
using FlightPlan.Services;
using FlightPlan.Core.Interfaces;
using FlightPlan.Models.Config;
using FlightPlan.Infrastructure.Services;

namespace FlightPlan.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotepadController : ControllerBase
{
    private readonly INotepadService _notepadService;
    private readonly MockNotepadService _mockNotepadService;
    private readonly DashConfig _config;

    public NotepadController(INotepadService notepadService, MockNotepadService mockNotepadService, DashConfig config)
    {
        _notepadService = notepadService;
        _mockNotepadService = mockNotepadService;
        _config = config;
    }

    private INotepadService CurrentService => _config.Debug.DemoMode ? _mockNotepadService : _notepadService;

    [HttpGet("files")]
    public IActionResult GetFiles()
    {
        return Ok(CurrentService.GetFiles());
    }

    [HttpGet("{filename}")]
    public async Task<IActionResult> GetNote(string filename)
    {
        try
        {
            var content = await CurrentService.GetNoteContentAsync(filename);
            return Ok(new { content });
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{filename}")]
    public async Task<IActionResult> SaveNote(string filename, [FromBody] NoteUpdate update)
    {
        await CurrentService.SaveNoteAsync(filename, update.Content ?? "");
        return Ok();
    }

    [HttpDelete("{filename}")]
    public IActionResult DeleteNote(string filename)
    {
        CurrentService.DeleteNote(filename);
        return Ok();
    }
}

public class NoteUpdate
{
    public string Content { get; set; } = string.Empty;
}
