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
                new BookmarkItem { Title = "Slack", Url = "https://slack.com", Description = "Communication" },
                new BookmarkItem { Title = "Teams", Url = "https://teams.microsoft.com", Description = "Collaboration" },
                new BookmarkItem { Title = "Outlook Web", Url = "https://outlook.office.com", Description = "Email" }
            },
            Subcategories = new List<BookmarkCategory>
            {
                new BookmarkCategory
                {
                    Name = "Documentation",
                    Bookmarks = new List<BookmarkItem>
                    {
                        new BookmarkItem { Title = "Confluence", Url = "https://confluence.example.com" },
                        new BookmarkItem { Title = "API Docs", Url = "https://api.example.com/docs" },
                        new BookmarkItem { Title = "Architecture Wiki", Url = "https://wiki.example.com/arch" },
                        new BookmarkItem { Title = "Security Policies", Url = "https://secure.example.com/policies" }
                    }
                },
                new BookmarkCategory
                {
                    Name = "Infrastructure",
                    Bookmarks = new List<BookmarkItem>
                    {
                        new BookmarkItem { Title = "Azure Portal", Url = "https://portal.azure.com" },
                        new BookmarkItem { Title = "AWS Console", Url = "https://aws.amazon.com/console" },
                        new BookmarkItem { Title = "Kubernetes Dashboard", Url = "https://k8s.example.com" }
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
                new BookmarkItem { Title = "YouTube", Url = "https://youtube.com" },
                // 4x more personal links
                new BookmarkItem { Title = "Twitter / X", Url = "https://twitter.com" },
                new BookmarkItem { Title = "Hacker News", Url = "https://news.ycombinator.com" },
                new BookmarkItem { Title = "Medium", Url = "https://medium.com" },
                new BookmarkItem { Title = "Netflix", Url = "https://netflix.com" }
            }
        },
        new BookmarkCategory
        {
            Name = "Development Tools",
            Color = "#238636",
            Bookmarks = new List<BookmarkItem>
            {
                new BookmarkItem { Title = "Stack Overflow", Url = "https://stackoverflow.com" },
                new BookmarkItem { Title = "C# Documentation", Url = "https://docs.microsoft.com/en-us/dotnet/csharp/" },
                new BookmarkItem { Title = "Vue.js Docs", Url = "https://vuejs.org/guide/introduction.html" },
                new BookmarkItem { Title = "Regex101", Url = "https://regex101.com" },
                new BookmarkItem { Title = "JSON Formatter", Url = "https://jsonformatter.curiousconcept.com" },
                new BookmarkItem { Title = "Can I Use?", Url = "https://caniuse.com" }
            }
        },
        new BookmarkCategory
        {
            Name = "News & Learning",
            Color = "#8b949e",
            Bookmarks = new List<BookmarkItem>
            {
                new BookmarkItem { Title = "BBC News", Url = "https://bbc.com/news" },
                new BookmarkItem { Title = "TechCrunch", Url = "https://techcrunch.com" },
                new BookmarkItem { Title = "Coursera", Url = "https://coursera.org" },
                new BookmarkItem { Title = "Pluralsight", Url = "https://pluralsight.com" },
                new BookmarkItem { Title = "Udemy", Url = "https://udemy.com" }
            }
        },
        new BookmarkCategory
        {
            Name = "Finance",
            Color = "#3fb950",
            Bookmarks = new List<BookmarkItem>
            {
                new BookmarkItem { Title = "Online Banking", Url = "https://bank.example.com" },
                new BookmarkItem { Title = "Investment Portfolio", Url = "https://invest.example.com" },
                new BookmarkItem { Title = "Stock Market News", Url = "https://finance.yahoo.com" }
            }
        },
        new BookmarkCategory
        {
            Name = "Shopping",
            Color = "#f85149",
            Bookmarks = new List<BookmarkItem>
            {
                new BookmarkItem { Title = "Amazon", Url = "https://amazon.com" },
                new BookmarkItem { Title = "eBay", Url = "https://ebay.com" },
                new BookmarkItem { Title = "Digital Storefront", Url = "https://steampowered.com" }
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
