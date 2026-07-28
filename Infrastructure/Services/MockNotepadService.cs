using FlightPlan.Core.Interfaces;

namespace FlightPlan.Infrastructure.Services;

public class MockNotepadService : INotepadService
{
    private readonly Dictionary<string, string> _mockNotes = new()
    {
        { "Welcome.md", "# Welcome to FlightPlan\n\nThis is a demo of the notepad feature. You can create, edit, and delete notes here." },
        { "Project Ideas.md", "- Build a dashboard\n- Integrate Jira and GitHub\n- Add a calendar view\n- Implement a notepad" },
        { "Shopping List.md", "- Milk\n- Eggs\n- Bread\n- Coffee beans" }
    };

    public IEnumerable<string> GetFiles()
    {
        return _mockNotes.Keys;
    }

    public Task<string> GetNoteContentAsync(string filename)
    {
        if (_mockNotes.TryGetValue(filename, out var content))
        {
            return Task.FromResult(content);
        }
        return Task.FromException<string>(new FileNotFoundException("Note not found", filename));
    }

    public Task SaveNoteAsync(string filename, string content)
    {
        _mockNotes[filename] = content;
        return Task.CompletedTask;
    }

    public void DeleteNote(string filename)
    {
        _mockNotes.Remove(filename);
    }
}
