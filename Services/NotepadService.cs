using FlightPlan.Core.Interfaces;
using FlightPlan.Services;

namespace FlightPlan.Services;

public class NotepadService : INotepadService
{
    private readonly string _notesDirectory;

    public NotepadService(IStorageService storageService)
    {
        _notesDirectory = storageService.GetNotesDirectory();
        MigrateNotes(storageService);
    }

    private void MigrateNotes(IStorageService storageService)
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

    public IEnumerable<string> GetFiles()
    {
        return Directory.GetFiles(_notesDirectory, "*.md")
            .Select(Path.GetFileName)
            .ToList()!;
    }

    public async Task<string> GetNoteContentAsync(string filename)
    {
        var filePath = Path.Combine(_notesDirectory, filename);
        if (!System.IO.File.Exists(filePath))
        {
            throw new FileNotFoundException("Note not found", filename);
        }

        return await System.IO.File.ReadAllTextAsync(filePath);
    }

    public async Task SaveNoteAsync(string filename, string content)
    {
        var filePath = Path.Combine(_notesDirectory, filename);
        await System.IO.File.WriteAllTextAsync(filePath, content ?? "");
    }

    public void DeleteNote(string filename)
    {
        var filePath = Path.Combine(_notesDirectory, filename);
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }
    }
}
