using System.Text.Json;

namespace FlightPlan.Services;

public class GitHubStarredService
{
    private readonly IStorageService _storageService;
    private readonly object _lock = new();
    private HashSet<string> _starredUrls;

    public GitHubStarredService(IStorageService storageService)
    {
        _storageService = storageService;
        _starredUrls = LoadStarredUrls();
    }

    private string FilePath => _storageService.GetGitHubStarredPath();

    public IEnumerable<string> GetStarredUrls()
    {
        lock (_lock)
        {
            return _starredUrls.ToList();
        }
    }

    public bool ToggleStar(string url)
    {
        lock (_lock)
        {
            bool isStarred;
            if (_starredUrls.Contains(url))
            {
                _starredUrls.Remove(url);
                isStarred = false;
            }
            else
            {
                _starredUrls.Add(url);
                isStarred = true;
            }
            SaveStarredUrls();
            return isStarred;
        }
    }

    private HashSet<string> LoadStarredUrls()
    {
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

    private void SaveStarredUrls()
    {
        var json = JsonSerializer.Serialize(_starredUrls);
        File.WriteAllText(FilePath, json);
    }
}
