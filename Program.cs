using Autofac;
using Autofac.Extensions.DependencyInjection;
using FlightPlan.Models.Config;
using FlightPlan.Services;
using FlightPlan.Core.Interfaces;
using FlightPlan.Infrastructure.Services;
using Quartz;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

var builder = WebApplication.CreateBuilder(args);

if (string.IsNullOrEmpty(builder.Environment.WebRootPath))
{
    var defaultWebRoot = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
    if (Directory.Exists(defaultWebRoot))
    {
        builder.Environment.WebRootPath = defaultWebRoot;
    }
}

// Storage Service
var storageService = new StorageService();

// Load YAML config
var configPath = storageService.GetConfigPath();
if (!File.Exists(configPath))
{
    // Migration: Check if config.yaml exists in current directory
    if (File.Exists("config.yaml"))
    {
        File.Copy("config.yaml", configPath);
    }
    else
    {
        var defaultConfig = new DashConfig
        {
            Jira = new JiraConfig
            {
                Url = "https://your-domain.atlassian.net/",
                Username = "user@example.com",
                ApiToken = "your-api-token",
                Queries = new List<JiraQuery>()
            },
            GitHub = new GitHubConfig
            {
                Organization = "your-org",
                Username = "your-username",
                AccessToken = "your-access-token"
            }
        };
        
        defaultConfig.PageVisibilities = new List<PageVisibility>
        {
            new PageVisibility { Id = "jira", Visible = true },
            new PageVisibility { Id = "github", Visible = true },
            new PageVisibility { Id = "tasks", Visible = true },
            new PageVisibility { Id = "scheduledtasks", Visible = true },
            new PageVisibility { Id = "email", Visible = true },
            new PageVisibility { Id = "calendar", Visible = true },
            new PageVisibility { Id = "links", Visible = true },
            new PageVisibility { Id = "notepad", Visible = true },
            new PageVisibility { Id = "debug", Visible = true }
        };

        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var yaml = serializer.Serialize(defaultConfig);
        File.WriteAllText(configPath, yaml);
    }
}

var deserializer = new DeserializerBuilder()
    .WithNamingConvention(CamelCaseNamingConvention.Instance)
    .IgnoreUnmatchedProperties()
    .Build();
var dashConfig = deserializer.Deserialize<DashConfig>(File.ReadAllText(configPath));

// Ensure default queries if none are present in the loaded config
if (dashConfig.Jira.Queries == null || dashConfig.Jira.Queries.Count == 0)
{
    dashConfig.Jira.Queries = new List<JiraQuery>();
}

if (dashConfig.GitHub.Queries == null || dashConfig.GitHub.Queries.Count == 0)
{
    dashConfig.GitHub.Queries = new List<GitHubQuery>
    {
        new GitHubQuery { Name = "Approve", Query = $"is:open is:pr review-requested:{dashConfig.GitHub.Username}", Color = "#fd7e14", Icon = "bi-check2-circle" },
        new GitHubQuery { Name = "Drafts", Query = $"is:open is:pr author:{dashConfig.GitHub.Username} draft:true", Color = "#adb5bd", Icon = "bi-file-earmark-diff" }
    };
}

if (dashConfig.PageVisibilities == null || dashConfig.PageVisibilities.Count == 0)
{
    dashConfig.PageVisibilities = new List<PageVisibility>
    {
        new PageVisibility { Id = "jira", Visible = true },
        new PageVisibility { Id = "github", Visible = true },
        new PageVisibility { Id = "tasks", Visible = true },
        new PageVisibility { Id = "scheduledtasks", Visible = true },
        new PageVisibility { Id = "email", Visible = true },
        new PageVisibility { Id = "calendar", Visible = true },
        new PageVisibility { Id = "links", Visible = true },
        new PageVisibility { Id = "notepad", Visible = true },
        new PageVisibility { Id = "debug", Visible = true }
    };
}

// Use Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    // Register FlightPlan services
    containerBuilder.RegisterInstance(storageService).As<IStorageService>().SingleInstance();
    containerBuilder.RegisterInstance(dashConfig).AsSelf().SingleInstance();
    
    // Adapters (Infrastructure)
    containerBuilder.RegisterType<JiraAdapter>().As<IJiraService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<GitHubAdapter>().As<IGitHubService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<MicrosoftGraphEmailService>().As<IEmailService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<MicrosoftGraphCalendarService>().As<ICalendarService>().InstancePerLifetimeScope();

    // Application Services
    containerBuilder.RegisterType<DashboardService>().As<IDashboardService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<GraphService>().As<IGraphService>().InstancePerLifetimeScope();
    
    // Mock Services
    containerBuilder.RegisterType<MockJiraService>().AsSelf().InstancePerLifetimeScope();
    containerBuilder.RegisterType<MockGitHubService>().AsSelf().InstancePerLifetimeScope();
    containerBuilder.RegisterType<MockEmailService>().AsSelf().InstancePerLifetimeScope();
    containerBuilder.RegisterType<MockCalendarService>().AsSelf().InstancePerLifetimeScope();
    containerBuilder.RegisterType<MockNotepadService>().AsSelf().InstancePerLifetimeScope();
    containerBuilder.RegisterType<MockBookmarksService>().AsSelf().InstancePerLifetimeScope();
    containerBuilder.RegisterType<MockScheduledTaskService>().AsSelf().InstancePerLifetimeScope();
    containerBuilder.RegisterType<MockRuleService>().AsSelf().InstancePerLifetimeScope();
    
    if (dashConfig.Debug.DemoMode)
    {
        containerBuilder.RegisterType<MockRuleService>().As<IRuleService>().SingleInstance();
    }
    else
    {
        containerBuilder.RegisterType<RuleService>().As<IRuleService>().SingleInstance();
    }
    
    containerBuilder.RegisterType<BookmarksService>().As<IBookmarksService>().SingleInstance();
    containerBuilder.RegisterType<NotepadService>().As<INotepadService>().SingleInstance();
    containerBuilder.RegisterType<JiraStarredService>().AsSelf().SingleInstance();
    containerBuilder.RegisterType<GitHubStarredService>().AsSelf().SingleInstance();
    containerBuilder.RegisterType<ScheduledTaskService>().As<IScheduledTaskService>().SingleInstance();

    // Migration for projects.yaml
    var projectsPath = storageService.GetProjectsPath();
    if (!File.Exists(projectsPath) && File.Exists("projects.yaml"))
    {
        File.Copy("projects.yaml", projectsPath);
    }

    var projectManager = new ProjectManager(dashConfig);
    projectManager.LoadProjectsFromYaml(projectsPath);
    containerBuilder.RegisterInstance(projectManager).AsSelf().SingleInstance();
});

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddRazorPages();
builder.Services.AddSingleton<IViteAssetService, ViteAssetService>();

builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();
});
builder.Services.AddQuartzHostedService(opt =>
{
    opt.WaitForJobsToComplete = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.MapGet("/", () => Results.Redirect("/Dashboard"));

using (var scope = app.Services.CreateScope())
{
    var scheduledTaskService = scope.ServiceProvider.GetRequiredService<IScheduledTaskService>();
    await scheduledTaskService.InitializeSchedulesAsync();
}

app.Run("http://localhost:5155");