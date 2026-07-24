using Microsoft.AspNetCore.Mvc;
using FlightPlan.Services;

namespace FlightPlan.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotepadController : ControllerBase
{
    private readonly string _notesDirectory;
    private readonly IStorageService _storageService;

    public NotepadController(IStorageService storageService)
    {
        _storageService = storageService;
        _notesDirectory = _storageService.GetNotesDirectory();
        MigrateNotes();
    }

    private void MigrateNotes()
    {
        var oldNotesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Notes");
        if (Directory.Exists(oldNotesDir) && oldNotesDir != _notesDirectory)
        {
            foreach (var file in Directory.GetFiles(oldNotesDir, "*.md"))
            {
                var destFile = Path.Combine(_notesDirectory, Path.GetFileName(file));
                if (!System.IO.File.Exists(destFile))
                {
                    try { System.IO.File.Copy(file, destFile); } catch { }
                }
            }
        }
    }

    [HttpGet("files")]
    public IActionResult GetFiles()
    {
        var files = Directory.GetFiles(_notesDirectory, "*.md")
            .Select(Path.GetFileName)
            .ToList();
        return Ok(files);
    }

    [HttpGet("{filename}")]
    public async Task<IActionResult> GetNote(string filename)
    {
        var filePath = Path.Combine(_notesDirectory, filename);
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var content = await System.IO.File.ReadAllTextAsync(filePath);
        return Ok(new { content });
    }

    [HttpPost("{filename}")]
    public async Task<IActionResult> SaveNote(string filename, [FromBody] NoteUpdate update)
    {
        var filePath = Path.Combine(_notesDirectory, filename);
        await System.IO.File.WriteAllTextAsync(filePath, update.Content ?? "");
        return Ok();
    }

    [HttpDelete("{filename}")]
    public IActionResult DeleteNote(string filename)
    {
        var filePath = Path.Combine(_notesDirectory, filename);
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }
        return Ok();
    }
}

public class NoteUpdate
{
    public string Content { get; set; } = string.Empty;
}
