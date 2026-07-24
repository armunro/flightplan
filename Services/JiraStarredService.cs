using System.Text.Json;

namespace FlightPlan.Services;

public class JiraStarredService
{
    private readonly IStorageService _storageService;
    private readonly object _lock = new();
    private HashSet<string> _starredKeys;

    public JiraStarredService(IStorageService storageService)
    {
        _storageService = storageService;
        _starredKeys = LoadStarredKeys();
    }

    private string FilePath => _storageService.GetJiraStarredPath();

    public IEnumerable<string> GetStarredKeys()
    {
        lock (_lock)
        {
            return _starredKeys.ToList();
        }
    }

    public bool ToggleStar(string key)
    {
        lock (_lock)
        {
            bool isStarred;
            if (_starredKeys.Contains(key))
            {
                _starredKeys.Remove(key);
                isStarred = false;
            }
            else
            {
                _starredKeys.Add(key);
                isStarred = true;
            }
            SaveStarredKeys();
            return isStarred;
        }
    }

    private HashSet<string> LoadStarredKeys()
    {
        // Migration
        if (!File.Exists(FilePath) && File.Exists("jira_starred.json"))
        {
            File.Move("jira_starred.json", FilePath);
        }

        if (!File.Exists(FilePath))
        {
            return new HashSet<string>();
        }

        try
        {
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<HashSet<string>>(json) ?? new HashSet<string>();
        }
        catch
        {
            return new HashSet<string>();
        }
    }

    private void SaveStarredKeys()
    {
        var json = JsonSerializer.Serialize(_starredKeys);
        File.WriteAllText(FilePath, json);
    }
}
