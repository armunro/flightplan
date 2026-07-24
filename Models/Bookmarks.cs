namespace FlightPlan.Models;

public class BookmarkCategory
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public List<BookmarkItem> Bookmarks { get; set; } = new();
    public List<BookmarkCategory> Subcategories { get; set; } = new();
}

public class BookmarkItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
}
