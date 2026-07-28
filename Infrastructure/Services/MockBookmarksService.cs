using FlightPlan.Models;
using FlightPlan.Services;

namespace FlightPlan.Infrastructure.Services;

public class MockBookmarksService : IBookmarksService
{
    private List<BookmarkCategory> _mockBookmarks = new()
    {
        new BookmarkCategory
        {
            Name = "Work",
            Color = "#58a6ff",
            Bookmarks = new List<BookmarkItem>
            {
                new BookmarkItem { Title = "Jira", Url = "https://jira.example.com", Description = "Company Jira instance" },
                new BookmarkItem { Title = "GitHub", Url = "https://github.com", Description = "Source control" },
                new BookmarkItem { Title = "Slack", Url = "https://slack.com", Description = "Communication" }
            },
            Subcategories = new List<BookmarkCategory>
            {
                new BookmarkCategory
                {
                    Name = "Documentation",
                    Bookmarks = new List<BookmarkItem>
                    {
                        new BookmarkItem { Title = "Confluence", Url = "https://confluence.example.com" },
                        new BookmarkItem { Title = "API Docs", Url = "https://api.example.com/docs" }
                    }
                }
            }
        },
        new BookmarkCategory
        {
            Name = "Personal",
            Color = "#d29922",
            Bookmarks = new List<BookmarkItem>
            {
                new BookmarkItem { Title = "Reddit", Url = "https://reddit.com" },
                new BookmarkItem { Title = "YouTube", Url = "https://youtube.com" }
            }
        }
    };

    public Task<List<BookmarkCategory>> GetBookmarksAsync()
    {
        return Task.FromResult(_mockBookmarks);
    }

    public Task SaveBookmarksAsync(List<BookmarkCategory> bookmarks)
    {
        _mockBookmarks = bookmarks;
        return Task.CompletedTask;
    }
}
