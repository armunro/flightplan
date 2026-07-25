using System.IO;

namespace FlightPlan.Services;

public interface IStorageService
{
    string GetConfigPath();
    string GetProjectsPath();
    string GetJiraStarredPath();
    string GetGitHubStarredPath();
    string GetAuthRecordPath();
    string GetRulesDirectory();
    string GetNotesDirectory();
    string GetEmailPreferencesPath();
    string GetCalendarPreferencesPath();
    string GetBookmarksPath();
    string GetScheduledTasksPath();
}

public class StorageService : IStorageService
{
    private readonly string _basePath;

    public StorageService()
    {
        _basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FlightPlan");
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public string GetConfigPath() => Path.Combine(_basePath, "config.yaml");
    public string GetProjectsPath() => Path.Combine(_basePath, "projects.yaml");
    public string GetJiraStarredPath() => Path.Combine(_basePath, "jira_starred.json");
    public string GetGitHubStarredPath() => Path.Combine(_basePath, "github_starred.json");
    public string GetAuthRecordPath() => Path.Combine(_basePath, "auth_record_flightplan.bin");
    public string GetEmailPreferencesPath() => Path.Combine(_basePath, "email_preferences.json");
    public string GetCalendarPreferencesPath() => Path.Combine(_basePath, "calendar_preferences.json");
    public string GetBookmarksPath() => Path.Combine(_basePath, "bookmarks.yaml");
    public string GetScheduledTasksPath() => Path.Combine(_basePath, "scheduled_tasks.yaml");
    
    public string GetRulesDirectory()
    {
        var path = Path.Combine(_basePath, "Rules");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return path;
    }

    public string GetNotesDirectory()
    {
        var path = Path.Combine(_basePath, "Notes");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return path;
    }
}
