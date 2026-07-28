using FlightPlan.Core.Interfaces;

namespace FlightPlan.Infrastructure.Services;

public class MockNotepadService : INotepadService
{
    private readonly Dictionary<string, string> _mockNotes = new()
    {
        { "Welcome.md", "# Welcome to FlightPlan\n\nThis is a demo of the notepad feature. You can create, edit, and delete notes here." },
        { "Project Ideas.md", "## Next Big Things\n- AI-driven task prioritization\n- Real-time collaboration features\n- Mobile app companion\n- Voice-to-task integration\n- Custom dashboard widgets" },
        { "Shopping List.md", "### Groceries\n- Organic Milk\n- Free-range Eggs\n- Sourdough Bread\n- Espresso Coffee beans\n- Avocados\n- Greek Yogurt" },
        { "Meeting Notes - Sprint 42.md", "# Sprint 42 Planning\n\n**Attendees:** Andrew, Junie, Sarah\n\n**Goals:**\n1. Complete Demo Mode mock data enhancement.\n2. Fix CSS grid issues on mobile.\n3. Implement OAuth2 flow.\n\n**Action Items:**\n- [ ] Junie: Review PR #45\n- [ ] Andrew: Update project models\n- [ ] Sarah: Draft documentation" },
        { "Deployment Checklist.md", "1. Run all unit tests\n2. Build frontend assets (`npm run build`)\n3. Run database migrations\n4. Update version number in `Program.cs`\n5. Push to production branch\n6. Monitor logs for errors" },
        { "Books to Read.md", "- *Clean Code* by Robert C. Martin\n- *Designing Data-Intensive Applications* by Martin Kleppmann\n- *The Pragmatic Programmer*\n- *Refactoring* by Martin Fowler\n- *Site Reliability Engineering* (Google)" },
        { "Travel Plans.md", "### Seattle Trip\n- **Flight:** July 30th, SEA\n- **Hotel:** The Mediterranean Inn\n- **Places to visit:**\n  - Space Needle\n  - Pike Place Market\n  - Chihuly Garden and Glass\n  - Gas Works Park" },
        { "Workout Routine.md", "#### Monday: Push\n- Bench Press: 3x10\n- Overhead Press: 3x12\n- Tricep Extensions: 3x15\n\n#### Wednesday: Pull\n- Deadlifts: 3x5\n- Pull-ups: 3xMax\n- Barbell Rows: 3x10\n\n#### Friday: Legs\n- Squats: 3x8\n- Lunges: 3x12\n- Leg Curls: 3x15" },
        { "Recipes.md", "## Guacamole\n- 3 Avocados\n- 1 Lime (juiced)\n- 1 tsp Salt\n- 1/2 cup diced Onion\n- 3 tbsp chopped Cilantro\n- 2 Roma Tomatoes, diced\n- 1 tsp minced Garlic\n- 1 pinch ground Cayenne Pepper" },
        { "System Config.md", "```json\n{\n  \"Environment\": \"Production\",\n  \"Version\": \"2.4.0\",\n  \"Features\": {\n    \"DemoMode\": true,\n    \"Analytics\": false,\n    \"MultiLanguage\": true\n  }\n}\n```" },
        { "Random Thoughts.md", "The secret of getting ahead is getting started. - Mark Twain\n\nIs there a way to automate the daily standup summary? Maybe using the Jira API to pull the last 24 hours of activity." },
        { "Gift Ideas.md", "- Sarah: Noise-canceling headphones\n- Andrew: Mechanical keyboard (Brown switches)\n- Mom: Gardening tool set\n- Dad: History book on aviation" }
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
