using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FlightPlan.Core.Interfaces;
using FlightPlan.Core.Models;
using FlightPlan.Models.Config;

namespace FlightPlan.Infrastructure.Services;

public class JiraAdapter : IJiraService
{
    private readonly DashConfig _config;
    private readonly ILogger<JiraAdapter> _logger;

    public JiraAdapter(DashConfig config, ILogger<JiraAdapter> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<IEnumerable<JiraIssueDto>> GetMyIssuesAsync(bool includeClosed = true)
    {
        var username = _config.Jira.Username;
        var accountId = await ResolveAccountIdAsync(username);

        var jql = $"(assignee = \"{accountId}\" OR reporter = \"{accountId}\" OR text ~ \"{username}\")";
        if (!includeClosed)
        {
            jql += " AND statusCategory != Done";
        }
        jql += " ORDER BY updated DESC";
        
        return await GetIssuesByJqlAsync(jql);
    }

    public async Task<IEnumerable<JiraIssueDto>> GetIssuesByJqlAsync(string jql)
    {
        var jiraUrl = _config.Jira.Url?.TrimEnd('/');
        var username = _config.Jira.Username;
        var apiToken = _config.Jira.ApiToken;

        if (string.IsNullOrEmpty(apiToken) || string.IsNullOrEmpty(jiraUrl))
        {
            _logger.LogWarning("Jira configuration is missing.");
            return Enumerable.Empty<JiraIssueDto>();
        }

        try
        {
            using var client = new HttpClient();
            var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{apiToken}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var requestUrl = $"{jiraUrl}/rest/api/3/search/jql?jql={Uri.EscapeDataString(jql)}&fields=summary,status,priority,key,assignee,created,updated,description,comment,issuetype&maxResults=10";

            var response = await client.GetAsync(requestUrl);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Jira API returned {StatusCode}: {Error}", response.StatusCode, errorContent);
                return Enumerable.Empty<JiraIssueDto>();
            }

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            
            if (!doc.RootElement.TryGetProperty("issues", out var issuesElement))
            {
                if (!doc.RootElement.TryGetProperty("results", out issuesElement))
                {
                    return Enumerable.Empty<JiraIssueDto>();
                }
            }

            var result = new List<JiraIssueDto>();
            foreach (var issue in issuesElement.EnumerateArray())
            {
                try
                {
                    var key = issue.GetProperty("key").GetString()!;
                    var fields = issue.GetProperty("fields");
                    var summary = fields.GetProperty("summary").GetString() ?? "";
                    
                    var status = fields.TryGetProperty("status", out var st) && st.TryGetProperty("name", out var stn) 
                        ? stn.GetString() ?? "Unknown" : "Unknown";

                    var priority = fields.TryGetProperty("priority", out var p) && p.TryGetProperty("name", out var pn) 
                        ? pn.GetString() ?? "Medium" : "Medium";

                    var issueType = fields.TryGetProperty("issuetype", out var it) && it.TryGetProperty("name", out var itn)
                        ? itn.GetString() : null;

                    var assignee = fields.TryGetProperty("assignee", out var a) && a.ValueKind == JsonValueKind.Object
                        ? (a.TryGetProperty("displayName", out var dn) ? dn.GetString() : 
                           a.TryGetProperty("name", out var n) ? n.GetString() : 
                           a.TryGetProperty("emailAddress", out var ea) ? ea.GetString() : null)
                        : null;

                    if (assignee == null && fields.TryGetProperty("assignee", out var a2) && a2.ValueKind == JsonValueKind.Object)
                    {
                        _logger.LogInformation("[DEBUG_LOG] Assignee object found for {Key} but no known name field: {AssigneeJson}", key, a2.GetRawText());
                    }

                    if (assignee == null)
                    {
                        _logger.LogInformation("[DEBUG_LOG] Assignee is null for issue {Key}. Fields: {Fields}", key, fields.GetRawText());
                    }

                    var createdStr = fields.TryGetProperty("created", out var c) ? c.GetString() : null;
                    var updatedStr = fields.TryGetProperty("updated", out var u) ? u.GetString() : null;
                    var description = fields.TryGetProperty("description", out var desc) ? ExtractTextFromAdf(desc) : null;
                    
                    var comments = new List<JiraCommentDto>();
                    if (fields.TryGetProperty("comment", out var commentElement) && commentElement.TryGetProperty("comments", out var commentsArray))
                    {
                        foreach (var cmt in commentsArray.EnumerateArray())
                        {
                            var author = cmt.TryGetProperty("author", out var auth) && auth.TryGetProperty("displayName", out var adn) ? adn.GetString() ?? "Unknown" : "Unknown";
                            var body = cmt.TryGetProperty("body", out var bdy) ? ExtractTextFromAdf(bdy) : "";
                            var cCreatedStr = cmt.TryGetProperty("created", out var cc) ? cc.GetString() : null;
                            var cCreated = DateTime.MinValue;
                            if (cCreatedStr != null && DateTime.TryParse(cCreatedStr, out var ccd)) cCreated = ccd;
                            
                            comments.Add(new JiraCommentDto(author, body, cCreated));
                        }
                    }

                    DateTime? created = null;
                    if (createdStr != null && DateTime.TryParse(createdStr, out var cd)) created = cd;

                    DateTime? updated = null;
                    if (updatedStr != null && DateTime.TryParse(updatedStr, out var ud)) updated = ud;

                    result.Add(new JiraIssueDto(
                        key,
                        summary,
                        status,
                        priority,
                        assignee,
                        created,
                        updated,
                        $"{jiraUrl}/browse/{key}",
                        description,
                        issueType,
                        comments
                    ));
                }
                catch (Exception ex)
                {
                    var issueJson = issue.GetRawText();
                    _logger.LogWarning(ex, "Error parsing individual Jira issue. Issue JSON: {IssueJson}", issueJson);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Jira issues");
            return Enumerable.Empty<JiraIssueDto>();
        }
    }

    public async Task<bool> UnassignIssueAsync(string issueKey)
    {
        var jiraUrl = _config.Jira.Url?.TrimEnd('/');
        var username = _config.Jira.Username;
        var apiToken = _config.Jira.ApiToken;

        if (string.IsNullOrEmpty(apiToken) || string.IsNullOrEmpty(jiraUrl))
        {
            _logger.LogWarning("Jira configuration is missing.");
            return false;
        }

        try
        {
            using var client = new HttpClient();
            var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{apiToken}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var requestUrl = $"{jiraUrl}/rest/api/3/issue/{issueKey}/assignee";
            var body = new { accountId = (string?)null };
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(requestUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Jira API (Unassign) returned {StatusCode}: {Error}", response.StatusCode, errorContent);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unassigning Jira issue {IssueKey}", issueKey);
            return false;
        }
    }

    private async Task<string> ResolveAccountIdAsync(string username)
    {
        var jiraUrl = _config.Jira.Url?.TrimEnd('/');
        var apiToken = _config.Jira.ApiToken;
        
        if (!string.IsNullOrEmpty(username) && !username.Contains(":"))
        {
            try
            {
                using var client = new HttpClient();
                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{apiToken}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
                
                var userSearchUrl = $"{jiraUrl}/rest/api/3/user/search?query={Uri.EscapeDataString(username)}";
                var userResponse = await client.GetAsync(userSearchUrl);
                if (userResponse.IsSuccessStatusCode)
                {
                    var userContent = await userResponse.Content.ReadAsStringAsync();
                    using var userDoc = JsonDocument.Parse(userContent);
                    if (userDoc.RootElement.GetArrayLength() > 0)
                    {
                        return userDoc.RootElement[0].GetProperty("accountId").GetString() ?? username;
                    }
                }
            }
            catch {}
        }
        return username;
    }

    private string ExtractTextFromAdf(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            var str = element.GetString() ?? "";
            // Check if it's a string that contains ADF JSON
            if (str.TrimStart().StartsWith("{"))
            {
                try
                {
                    using var doc = JsonDocument.Parse(str);
                    return ExtractTextFromAdf(doc.RootElement);
                }
                catch
                {
                    // If parsing fails, just use the string as is
                }
            }
            return str;
        }

        if (element.ValueKind != JsonValueKind.Object)
        {
            return "";
        }

        var sb = new StringBuilder();
        ProcessAdfNode(element, sb);
        return sb.ToString().Trim();
    }

    private void ProcessAdfNode(JsonElement node, StringBuilder sb)
    {
        if (node.ValueKind != JsonValueKind.Object) return;

        if (node.TryGetProperty("text", out var textProp))
        {
            sb.Append(textProp.GetString());
        }

        if (node.TryGetProperty("content", out var contentProp) && contentProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in contentProp.EnumerateArray())
            {
                ProcessAdfNode(child, sb);

                // Add newlines for certain block types to maintain some readability
                if (child.TryGetProperty("type", out var typeProp))
                {
                    var type = typeProp.GetString();
                    if (type == "paragraph" || type == "heading" || type == "bulletList" || type == "orderedList" || type == "listItem")
                    {
                        sb.AppendLine();
                    }
                }
            }
        }
    }
}
