namespace FlightPlan.Core.Models;

public record EmailDto(
    string Id,
    string From,
    string FromAddress,
    string Subject,
    string BodyPreview,
    DateTimeOffset ReceivedDateTime,
    string WebLink,
    List<string>? MatchingRules = null);

public record MailFolderDto(
    string Id,
    string DisplayName,
    int? TotalItemCount = 0,
    int? UnreadItemCount = 0,
    string? ParentFolderId = null,
    int? ChildFolderCount = 0);

public record CalendarEventDto(
    string Id,
    string Subject,
    DateTimeOffset Start,
    DateTimeOffset End,
    string? Location,
    string? WebLink = null,
    string? CalendarId = null,
    bool IsAllDay = false);
