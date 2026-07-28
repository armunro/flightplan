using Microsoft.AspNetCore.Mvc;

namespace FlightPlan.Core.Interfaces;

public interface INotepadService
{
    IEnumerable<string> GetFiles();
    Task<string> GetNoteContentAsync(string filename);
    Task SaveNoteAsync(string filename, string content);
    void DeleteNote(string filename);
}
