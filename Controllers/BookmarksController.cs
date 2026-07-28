using FlightPlan.Models;
using FlightPlan.Services;
using FlightPlan.Models.Config;
using FlightPlan.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlightPlan.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookmarksController : ControllerBase
{
    private readonly IBookmarksService _bookmarksService;
    private readonly MockBookmarksService _mockBookmarksService;
    private readonly DashConfig _config;
    private readonly ILogger<BookmarksController> _logger;

    public BookmarksController(
        IBookmarksService bookmarksService, 
        MockBookmarksService mockBookmarksService,
        DashConfig config,
        ILogger<BookmarksController> logger)
    {
        _bookmarksService = bookmarksService;
        _mockBookmarksService = mockBookmarksService;
        _config = config;
        _logger = logger;
    }

    private IBookmarksService CurrentService => _config.Debug.DemoMode ? _mockBookmarksService : _bookmarksService;

    [HttpGet]
    public async Task<IActionResult> GetBookmarks()
    {
        try
        {
            var bookmarks = await CurrentService.GetBookmarksAsync();
            return Ok(bookmarks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bookmarks");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> SaveBookmarks([FromBody] List<BookmarkCategory> bookmarks)
    {
        if (bookmarks == null)
        {
            return BadRequest("Bookmarks cannot be null");
        }

        try
        {
            await CurrentService.SaveBookmarksAsync(bookmarks);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving bookmarks");
            return StatusCode(500, "Internal server error");
        }
    }
}
