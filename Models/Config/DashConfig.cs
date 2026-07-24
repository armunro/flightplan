namespace FlightPlan.Models.Config;

public class DashConfig
{
    public JiraConfig Jira { get; set; } = new();
    public GitHubConfig GitHub { get; set; } = new();
    public MicrosoftGraphConfig MicrosoftGraph { get; set; } = new();
    public List<PageVisibility> PageVisibilities { get; set; } = new();
}

public class PageVisibility
{
    public string Id { get; set; } = string.Empty;
    public bool Visible { get; set; } = true;
}

public class MicrosoftGraphConfig
{
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
}

public class JiraConfig
{
    public string Url { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;
    public List<JiraQuery> Queries { get; set; } = new();
}

public class JiraQuery
{
    public string Name { get; set; } = string.Empty;
    public string Jql { get; set; } = string.Empty;
}

public class GitHubConfig
{
    public string Organization { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public List<GitHubQuery> Queries { get; set; } = new();
}

public class GitHubQuery
{
    public string Name { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
}
