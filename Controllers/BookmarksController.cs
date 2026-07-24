using FlightPlan.Models;
using FlightPlan.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlightPlan.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookmarksController : ControllerBase
{
    private readonly IBookmarksService _bookmarksService;
    private readonly ILogger<BookmarksController> _logger;

    public BookmarksController(IBookmarksService bookmarksService, ILogger<BookmarksController> logger)
    {
        _bookmarksService = bookmarksService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetBookmarks()
    {
        try
        {
            var bookmarks = await _bookmarksService.GetBookmarksAsync();
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
            await _bookmarksService.SaveBookmarksAsync(bookmarks);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving bookmarks");
            return StatusCode(500, "Internal server error");
        }
    }
}
