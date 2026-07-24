using FlightPlan.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FlightPlan.Services;

public interface IBookmarksService
{
    Task<List<BookmarkCategory>> GetBookmarksAsync();
    Task SaveBookmarksAsync(List<BookmarkCategory> bookmarks);
}

public class BookmarksService : IBookmarksService
{
    private readonly IStorageService _storageService;
    private readonly ILogger<BookmarksService> _logger;
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    public BookmarksService(IStorageService storageService, ILogger<BookmarksService> logger)
    {
        _storageService = storageService;
        _logger = logger;

        _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public async Task<List<BookmarkCategory>> GetBookmarksAsync()
    {
        var path = _storageService.GetBookmarksPath();
        if (!File.Exists(path))
        {
            return new List<BookmarkCategory>();
        }

        try
        {
            var yaml = await File.ReadAllTextAsync(path);
            var bookmarks = _deserializer.Deserialize<List<BookmarkCategory>>(yaml);
            return bookmarks ?? new List<BookmarkCategory>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading bookmarks from {Path}", path);
            return new List<BookmarkCategory>();
        }
    }

    public async Task SaveBookmarksAsync(List<BookmarkCategory> bookmarks)
    {
        var path = _storageService.GetBookmarksPath();
        try
        {
            var yaml = _serializer.Serialize(bookmarks);
            await File.WriteAllTextAsync(path, yaml);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving bookmarks to {Path}", path);
            throw;
        }
    }
}
