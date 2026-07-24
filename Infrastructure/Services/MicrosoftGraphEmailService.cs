using FlightPlan.Services;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using FlightPlan.Core.Interfaces;
using FlightPlan.Core.Models;
using FlightPlan.Models.Config;
using FlightPlan.Models;

namespace FlightPlan.Infrastructure.Services;

public class MicrosoftGraphEmailService : MicrosoftGraphBase, IEmailService
{
    public MicrosoftGraphEmailService(DashConfig config, ILogger<MicrosoftGraphEmailService> logger, IStorageService storageService) : base(config, logger, storageService) { }

    public async Task<IEnumerable<MailFolderDto>> GetMailFoldersAsync()
    {
        try
        {
            var client = await GetClientAsync();
            var allFolders = new List<MailFolderDto>();
            
            // Fetch top-level folders
            var topLevelFolders = await client.Me.MailFolders.GetAsync(config =>
            {
                config.QueryParameters.Top = 100;
            });

            if (topLevelFolders?.Value != null)
            {
                foreach (var folder in topLevelFolders.Value)
                {
                    await ProcessFolderAsync(client, folder, allFolders);
                }
            }

            return allFolders;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error fetching mail folders.");
            return Enumerable.Empty<MailFolderDto>();
        }
    }

    private async Task ProcessFolderAsync(Microsoft.Graph.GraphServiceClient client, Microsoft.Graph.Models.MailFolder folder, List<MailFolderDto> allFolders)
    {
        if (allFolders.Any(f => f.Id == folder.Id)) return;

        allFolders.Add(new MailFolderDto(
            folder.Id ?? "",
            folder.DisplayName ?? "Unknown",
            folder.TotalItemCount,
            folder.UnreadItemCount,
            folder.ParentFolderId,
            folder.ChildFolderCount
        ));

        if (folder.ChildFolderCount > 0)
        {
            var childFolders = await client.Me.MailFolders[folder.Id].ChildFolders.GetAsync(config =>
            {
                config.QueryParameters.Top = 100;
            });

            if (childFolders?.Value != null)
            {
                foreach (var child in childFolders.Value)
                {
                    await ProcessFolderAsync(client, child, allFolders);
                }
            }
        }
    }

    public async Task<IEnumerable<EmailDto>> GetEmailsAsync(string folderId = "inbox", int top = 10)
    {
        try
        {
            var client = await GetClientAsync();
            var messages = await client.Me.MailFolders[folderId].Messages
                .GetAsync(config =>
                {
                    config.QueryParameters.Top = top;
                    config.QueryParameters.Select = new[] { "id", "subject", "from", "receivedDateTime", "bodyPreview", "webLink" };
                    config.QueryParameters.Orderby = new[] { "receivedDateTime desc" };
                });

            return messages?.Value?.Select(m => new EmailDto(
                m.Id ?? "",
                m.From?.EmailAddress?.Name ?? m.From?.EmailAddress?.Address ?? "Unknown",
                m.From?.EmailAddress?.Address ?? "",
                m.Subject ?? "(No Subject)",
                m.BodyPreview ?? "",
                m.ReceivedDateTime ?? DateTimeOffset.MinValue,
                m.WebLink ?? ""
            )) ?? Enumerable.Empty<EmailDto>();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error fetching emails from folder {FolderId}.", folderId);
            return Enumerable.Empty<EmailDto>();
        }
    }

    public async Task<EmailDto?> GetEmailAsync(string messageId)
    {
        try
        {
            var client = await GetClientAsync();
            var m = await client.Me.Messages[messageId]
                .GetAsync(config =>
                {
                    config.QueryParameters.Select = new[] { "id", "subject", "from", "receivedDateTime", "bodyPreview", "webLink" };
                });

            if (m == null) return null;

            return new EmailDto(
                m.Id ?? "",
                m.From?.EmailAddress?.Name ?? m.From?.EmailAddress?.Address ?? "Unknown",
                m.From?.EmailAddress?.Address ?? "",
                m.Subject ?? "(No Subject)",
                m.BodyPreview ?? "",
                m.ReceivedDateTime ?? DateTimeOffset.MinValue,
                m.WebLink ?? ""
            );
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error fetching email {Id}", messageId);
            return null;
        }
    }

    public async Task MoveEmailToDeletedItemsAsync(string messageId)
    {
        var client = await GetClientAsync();
        var moveRequest = new Microsoft.Graph.Me.Messages.Item.Move.MovePostRequestBody { DestinationId = "deleteditems" };
        await client.Me.Messages[messageId].Move.PostAsync(moveRequest);
    }

    public async Task ApplyRuleActionsAsync(string messageId, List<RuleAction> actions)
    {
        var client = await GetClientAsync();
        var updateMessage = new Message();
        bool needsUpdate = false;
        string? destinationId = null;

        foreach (var action in actions)
        {
            switch (action.Type)
            {
                case ActionType.Star:
                    updateMessage.Flag = new FollowupFlag { FlagStatus = FollowupFlagStatus.Flagged };
                    needsUpdate = true;
                    break;
                case ActionType.ClearFlag:
                    updateMessage.Flag = new FollowupFlag { FlagStatus = FollowupFlagStatus.NotFlagged };
                    needsUpdate = true;
                    break;
                case ActionType.MarkAsRead:
                    updateMessage.IsRead = true;
                    needsUpdate = true;
                    break;
                case ActionType.AddCategory:
                    if (action.Value != null)
                    {
                        var currentMessage = await client.Me.Messages[messageId].GetAsync(c => c.QueryParameters.Select = new[] { "categories" });
                        var categories = currentMessage?.Categories?.ToList() ?? new List<string>();
                        if (!categories.Contains(action.Value))
                        {
                            categories.Add(action.Value);
                            updateMessage.Categories = categories;
                            needsUpdate = true;
                        }
                    }
                    break;
                case ActionType.Archive:
                    destinationId = "archive";
                    break;
                case ActionType.Move:
                    if (action.Value != null) destinationId = await GetFolderIdByPathAsync(action.Value);
                    break;
            }
        }

        if (needsUpdate) await client.Me.Messages[messageId].PatchAsync(updateMessage);
        if (destinationId != null)
        {
            await client.Me.Messages[messageId].Move.PostAsync(new Microsoft.Graph.Me.Messages.Item.Move.MovePostRequestBody { DestinationId = destinationId });
        }
    }

    private async Task<string?> GetFolderIdByPathAsync(string path)
    {
        var client = await GetClientAsync();
        var folderNames = path.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
        string? currentFolderId = null;

        foreach (var folderName in folderNames)
        {
            Microsoft.Graph.Models.MailFolderCollectionResponse? folders;
            if (currentFolderId == null)
            {
                folders = await client.Me.MailFolders.GetAsync(config => 
                {
                    config.QueryParameters.Filter = $"displayName eq '{folderName.Replace("'", "''")}'";
                    config.QueryParameters.Top = 1;
                });
            }
            else
            {
                folders = await client.Me.MailFolders[currentFolderId].ChildFolders.GetAsync(config => 
                {
                    config.QueryParameters.Filter = $"displayName eq '{folderName.Replace("'", "''")}'";
                    config.QueryParameters.Top = 1;
                });
            }

            var folder = folders?.Value?.FirstOrDefault();
            if (folder == null)
            {
                // Fallback: If it's the first level and a well-known name
                if (currentFolderId == null)
                {
                    currentFolderId = folderName.ToLower().Replace(" ", "");
                    continue; 
                }
                return null;
            }
            currentFolderId = folder.Id;
        }
        return currentFolderId;
    }
}
