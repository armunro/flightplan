using FlightPlan.Models;
using FlightPlan.Models.Config;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using TaskStatus = FlightPlan.Models.TaskStatus;

namespace FlightPlan.Services;

public class ProjectManager
{
    private readonly DashConfig _config;
    private readonly List<Project> _projects = new();
    private List<Project>? _mockProjects = null;
    private readonly object _fileLock = new();

    public ProjectManager(DashConfig config)
    {
        _config = config;
    }

    private List<Project> ActiveProjects
    {
        get
        {
            if (_config.Debug.DemoMode)
            {
                if (_mockProjects == null)
                {
                    _mockProjects = CreateMockProjects();
                }
                return _mockProjects;
            }
            return _projects;
        }
    }

    private List<Project> CreateMockProjects()
    {
        var projects = new List<Project>();
        
        // Project 1: Software Development (Demonstrates complex subtasks and multiple lists)
        var p1 = new Project
        {
            Name = "Demo: Website Redesign",
            Description = "Full stack overhaul of the corporate portal",
            Icon = "bi-globe",
            Color = "#f1c40f"
        };
        p1.Statuses.AddRange(GetDefaultStatuses());
        p1.TaskTypes.AddRange(GetDefaultTaskTypes());
        p1.Priorities.AddRange(GetDefaultPriorities());

        var now = DateTime.UtcNow;
        var today = now.Date;

        // List 1: Planning & Design
        var list1_1 = new TaskList { Name = "Planning & Design" };
        
        var t1 = new TaskItem 
        { 
            Title = "UI/UX Mockups", 
            Description = "Create high-fidelity designs for all core pages.", 
            Priority = TaskPriority.High,
            StatusId = p1.Statuses.First(s => s.Name == "In Progress").Id,
            TaskTypeId = p1.TaskTypes.First(t => t.Name == "Work").Id,
            PriorityId = p1.Priorities.First(p => p.Name == "High").Id,
            EstimateMinutes = 960,
            End = today.AddDays(2).AddHours(17)
        };
        
        var subT1_1 = new TaskItem { Title = "Mobile View", StatusId = p1.Statuses.First(s => s.Name == "Done").Id, IsCompleted = true };
        subT1_1.Subtasks.Add(new TaskItem { Title = "Navigation menu", IsCompleted = true, StatusId = p1.Statuses.Last().Id });
        subT1_1.Subtasks.Add(new TaskItem { Title = "Footer links", IsCompleted = true, StatusId = p1.Statuses.Last().Id });
        
        t1.Subtasks.Add(subT1_1);
        t1.Subtasks.Add(new TaskItem { Title = "Desktop View", StatusId = p1.Statuses.First(s => s.Name == "In Progress").Id, End = today.AddDays(1).AddHours(12) });
        t1.Subtasks.Add(new TaskItem { Title = "Accessibility Audit", StatusId = p1.Statuses.First(s => s.Name == "To Do").Id, End = today.AddDays(3) });
        
        list1_1.Tasks.Add(t1);
        list1_1.Tasks.Add(new TaskItem { Title = "User Interviews", Priority = TaskPriority.Medium, StatusId = p1.Statuses.First(s => s.Name == "Done").Id, IsCompleted = true, End = today.AddDays(-2) });
        list1_1.Tasks.Add(new TaskItem { Title = "Site Map Definition", StatusId = p1.Statuses.First(s => s.Name == "Done").Id, IsCompleted = true, End = today.AddDays(-1) });
        
        p1.Lists.Add(list1_1);

        // List 2: Frontend Development
        var list1_2 = new TaskList { Name = "Frontend Development" };
        list1_2.Tasks.Add(new TaskItem { Title = "Setup Vue.js Project", StatusId = p1.Statuses.Last().Id, IsCompleted = true, End = today.AddDays(-5) });
        list1_2.Tasks.Add(new TaskItem { Title = "Implement Header Component", StatusId = p1.Statuses[1].Id, End = today.AddDays(4) });
        list1_2.Tasks.Add(new TaskItem { Title = "Landing Page Layout", StatusId = p1.Statuses[0].Id, End = today.AddDays(5) });
        list1_2.Tasks.Add(new TaskItem { Title = "Responsive Design Fixes", Priority = TaskPriority.High, StatusId = p1.Statuses[0].Id, End = today.AddDays(1) });
        p1.Lists.Add(list1_2);

        // List 3: Backend & API
        var list1_3 = new TaskList { Name = "Backend & API" };
        list1_3.Tasks.Add(new TaskItem { Title = "Database Schema Design", StatusId = p1.Statuses.Last().Id, IsCompleted = true, End = today.AddDays(-3) });
        list1_3.Tasks.Add(new TaskItem { Title = "Auth System Implementation", StatusId = p1.Statuses[1].Id, End = today.AddDays(7) });
        list1_3.Tasks.Add(new TaskItem { Title = "API Documentation", StatusId = p1.Statuses[0].Id, End = today.AddDays(10) });
        p1.Lists.Add(list1_3);

        projects.Add(p1);

        // Project 2: Customer Support (Demonstrates custom statuses, types, and high task volume)
        var p2 = new Project
        {
            Name = "Demo: Support Desk",
            Description = "Global customer support and incident management",
            Icon = "bi-headset",
            Color = "#e74c3c"
        };
        
        p2.Statuses.Add(new TaskStatus { Name = "New", Color = "#3498db", Order = 0 });
        p2.Statuses.Add(new TaskStatus { Name = "Triaged", Color = "#9b59b6", Order = 1 });
        p2.Statuses.Add(new TaskStatus { Name = "Waiting on Customer", Color = "#f1c40f", Order = 2 });
        p2.Statuses.Add(new TaskStatus { Name = "Escalated", Color = "#e67e22", Order = 3 });
        p2.Statuses.Add(new TaskStatus { Name = "Resolved", Color = "#2ecc71", Order = 4, IsCompletedState = true });
        
        p2.TaskTypes.Add(new TaskType { Name = "Bug Report", Icon = "bi-bug", Color = "#e74c3c" });
        p2.TaskTypes.Add(new TaskType { Name = "Feature Request", Icon = "bi-lightbulb", Color = "#f1c40f" });
        p2.TaskTypes.Add(new TaskType { Name = "Security Issue", Icon = "bi-shield-lock", Color = "#000000" });
        
        p2.Priorities.AddRange(GetDefaultPriorities());

        // List 1: Urgent Issues
        var list2_1 = new TaskList { Name = "Urgent Issues" };
        var tBug = new TaskItem 
        { 
            Title = "Login failure on mobile app", 
            Description = "Users reporting 401 Unauthorized after latest update.", 
            Priority = TaskPriority.Critical,
            StatusId = p2.Statuses.First(s => s.Name == "Escalated").Id,
            TaskTypeId = p2.TaskTypes.First(t => t.Name == "Bug Report").Id,
            End = now.AddHours(4)
        };
        tBug.Subtasks.Add(new TaskItem { Title = "Reproduce in dev", StatusId = p2.Statuses[4].Id, IsCompleted = true, End = now.AddHours(-2) });
        tBug.Subtasks.Add(new TaskItem { Title = "Identify root cause", StatusId = p2.Statuses[1].Id, End = now.AddHours(2) });
        list2_1.Tasks.Add(tBug);
        
        list2_1.Tasks.Add(new TaskItem { Title = "Server latency in EU region", Priority = TaskPriority.High, StatusId = p2.Statuses[1].Id, End = now.AddHours(8) });
        list2_1.Tasks.Add(new TaskItem { Title = "Payment gateway timeout", Priority = TaskPriority.Critical, StatusId = p2.Statuses[0].Id, End = now.AddHours(1) });
        p2.Lists.Add(list2_1);

        // List 2: General Support
        var list2_2 = new TaskList { Name = "General Support" };
        list2_2.Tasks.Add(new TaskItem { Title = "Password reset help", StatusId = p2.Statuses[4].Id, IsCompleted = true, End = now.AddDays(-1) });
        list2_2.Tasks.Add(new TaskItem { Title = "Data export request", StatusId = p2.Statuses[2].Id, End = now.AddDays(2) });
        list2_2.Tasks.Add(new TaskItem { Title = "Inquiry about API limits", StatusId = p2.Statuses[2].Id, End = now.AddDays(1) });
        list2_2.Tasks.Add(new TaskItem { Title = "Update billing info", StatusId = p2.Statuses[4].Id, IsCompleted = true, End = now.AddDays(-2) });
        p2.Lists.Add(list2_2);

        // List 3: Feature Requests
        var list2_3 = new TaskList { Name = "Feature Requests" };
        list2_3.Tasks.Add(new TaskItem { Title = "Dark mode support", TaskTypeId = p2.TaskTypes[1].Id, StatusId = p2.Statuses[1].Id, End = today.AddDays(14) });
        list2_3.Tasks.Add(new TaskItem { Title = "Export to PDF", TaskTypeId = p2.TaskTypes[1].Id, StatusId = p2.Statuses[0].Id, End = today.AddDays(21) });
        list2_3.Tasks.Add(new TaskItem { Title = "Bulk delete options", TaskTypeId = p2.TaskTypes[1].Id, StatusId = p2.Statuses[0].Id, End = today.AddDays(30) });
        p2.Lists.Add(list2_3);

        projects.Add(p2);

        // Project 3: Marketing & Outreach (Demonstrates mixed priorities and multiple lists)
        var p3 = new Project
        {
            Name = "Demo: Q3 Launch",
            Description = "Coordinating the big summer release",
            Icon = "bi-megaphone",
            Color = "#3498db"
        };
        p3.Statuses.AddRange(GetDefaultStatuses());
        p3.TaskTypes.AddRange(GetDefaultTaskTypes());
        p3.Priorities.AddRange(GetDefaultPriorities());
        
        // List 1: Content Creation
        var list3_1 = new TaskList { Name = "Content Creation" };
        var tBlog = new TaskItem { Title = "Launch Blog Post", Priority = TaskPriority.High, StatusId = p3.Statuses[1].Id, End = today.AddDays(3) };
        tBlog.Subtasks.Add(new TaskItem { Title = "Draft", StatusId = p3.Statuses[2].Id, IsCompleted = true, End = today.AddDays(-1) });
        tBlog.Subtasks.Add(new TaskItem { Title = "Review", StatusId = p3.Statuses[1].Id, End = today.AddDays(1) });
        tBlog.Subtasks.Add(new TaskItem { Title = "Social graphics", StatusId = p3.Statuses[0].Id, End = today.AddDays(2) });
        list3_1.Tasks.Add(tBlog);
        list3_1.Tasks.Add(new TaskItem { Title = "Product Demo Video", Priority = TaskPriority.Medium, StatusId = p3.Statuses[0].Id, End = today.AddDays(7) });
        list3_1.Tasks.Add(new TaskItem { Title = "Newsletter Template", StatusId = p3.Statuses[2].Id, IsCompleted = true, End = today.AddDays(-5) });
        p3.Lists.Add(list3_1);

        // List 2: Paid Ads
        var list3_2 = new TaskList { Name = "Paid Ads" };
        list3_2.Tasks.Add(new TaskItem { Title = "Google Ads Setup", StatusId = p3.Statuses[1].Id, End = today.AddDays(2) });
        list3_2.Tasks.Add(new TaskItem { Title = "LinkedIn Campaign", StatusId = p3.Statuses[0].Id, End = today.AddDays(5) });
        list3_2.Tasks.Add(new TaskItem { Title = "Twitter Re-marketing", StatusId = p3.Statuses[0].Id, End = today.AddDays(5) });
        p3.Lists.Add(list3_2);

        // List 3: PR & Media
        var list3_3 = new TaskList { Name = "PR & Media" };
        list3_3.Tasks.Add(new TaskItem { Title = "Press Release Draft", StatusId = p3.Statuses[2].Id, IsCompleted = true, End = today.AddDays(-2) });
        list3_3.Tasks.Add(new TaskItem { Title = "Outreach to Tech Blogs", Priority = TaskPriority.High, StatusId = p3.Statuses[1].Id, End = today.AddDays(4) });
        list3_3.Tasks.Add(new TaskItem { Title = "Schedule Interviews", StatusId = p3.Statuses[0].Id, End = today.AddDays(6) });
        p3.Lists.Add(list3_3);

        projects.Add(p3);

        // Project 4: Research & Development (Demonstrates deep nesting and complex subtasks)
        var p4 = new Project
        {
            Name = "Demo: R&D Lab",
            Description = "Exploration of next-gen technologies",
            Icon = "bi-search",
            Color = "#9b59b6"
        };
        p4.Statuses.AddRange(GetDefaultStatuses());
        p4.TaskTypes.AddRange(GetDefaultTaskTypes());
        p4.Priorities.AddRange(GetDefaultPriorities());
        
        var list4_1 = new TaskList { Name = "AI Integration" };
        var tAI = new TaskItem { Title = "LLM Evaluation", Priority = TaskPriority.High, StatusId = p3.Statuses[1].Id, End = today.AddDays(10) };
        
        var tOpenAI = new TaskItem { Title = "OpenAI GPT-4", StatusId = p3.Statuses[2].Id, IsCompleted = true, End = today.AddDays(-7) };
        tOpenAI.Subtasks.Add(new TaskItem { Title = "Token costs analysis", IsCompleted = true, StatusId = p3.Statuses[2].Id, End = today.AddDays(-10) });
        tOpenAI.Subtasks.Add(new TaskItem { Title = "Latency tests", IsCompleted = true, StatusId = p3.Statuses[2].Id, End = today.AddDays(-8) });
        
        var tAnthropic = new TaskItem { Title = "Anthropic Claude", StatusId = p3.Statuses[1].Id, End = today.AddDays(5) };
        tAnthropic.Subtasks.Add(new TaskItem { Title = "Context window verification", StatusId = p3.Statuses[1].Id, End = today.AddDays(2) });
        tAnthropic.Subtasks.Add(new TaskItem { Title = "Prompt engineering", StatusId = p3.Statuses[0].Id, End = today.AddDays(4) });
        
        tAI.Subtasks.Add(tOpenAI);
        tAI.Subtasks.Add(tAnthropic);
        tAI.Subtasks.Add(new TaskItem { Title = "Llama 3 (Self-hosted)", StatusId = p3.Statuses[0].Id, End = today.AddDays(9) });
        
        list4_1.Tasks.Add(tAI);
        list4_1.Tasks.Add(new TaskItem { Title = "Vector Database POC", Description = "Testing Pinecone vs Milvus.", StatusId = p3.Statuses[1].Id, End = today.AddDays(6) });
        p4.Lists.Add(list4_1);

        var list4_2 = new TaskList { Name = "Infrastructure" };
        list4_2.Tasks.Add(new TaskItem { Title = "Kubernetes Cluster Upgrade", Priority = TaskPriority.High, StatusId = p3.Statuses[0].Id, End = today.AddDays(1) });
        list4_2.Tasks.Add(new TaskItem { Title = "Terraform Refactor", StatusId = p3.Statuses[2].Id, IsCompleted = true, End = today.AddDays(-4) });
        p4.Lists.Add(list4_2);

        projects.Add(p4);

        return projects;
    }

    public Project CreateProject(string name, string? description = null, string? icon = null, string? color = null, List<TaskStatus>? statuses = null, List<TaskType>? taskTypes = null, List<ProjectPriority>? priorities = null, List<CustomFieldDefinition>? customFields = null)
    {
        var project = new Project 
        { 
            Name = name, 
            Description = description,
            Icon = icon ?? "bi-folder",
            Color = color ?? "#58a6ff"
        };

        if (statuses != null && statuses.Any())
        {
            project.Statuses.AddRange(statuses);
        }
        else
        {
            project.Statuses.AddRange(GetDefaultStatuses());
        }

        if (taskTypes != null && taskTypes.Any())
        {
            project.TaskTypes.AddRange(taskTypes);
        }
        else
        {
            project.TaskTypes.AddRange(GetDefaultTaskTypes());
        }

        if (priorities != null && priorities.Any())
        {
            project.Priorities.AddRange(priorities);
        }
        else
        {
            project.Priorities.AddRange(GetDefaultPriorities());
        }

        if (customFields != null)
        {
            project.CustomFields.AddRange(customFields);
        }

        _projects.Add(project);
        return project;
    }

    private List<TaskStatus> GetDefaultStatuses()
    {
        return new List<TaskStatus>
        {
            new TaskStatus { Name = "To Do", Color = "#525b67", Order = 0, IsCompletedState = false },
            new TaskStatus { Name = "In Progress", Color = "#4a7dc7", Order = 1, IsCompletedState = false },
            new TaskStatus { Name = "Done", Color = "#87bf51", Order = 2, IsCompletedState = true }
        };
    }

    private List<TaskType> GetDefaultTaskTypes()
    {
        return new List<TaskType>
        {
            new TaskType { Name = "Project", Icon = "bi-folder", Color = "#a277ff" },
            new TaskType { Name = "Work", Icon = "bi-briefcase", Color = "#3498db" },
            new TaskType { Name = "Contact", Icon = "bi-person", Color = "#2ecc71" },
            new TaskType { Name = "Transit", Icon = "bi-truck", Color = "#f1c40f" }
        };
    }

    private List<ProjectPriority> GetDefaultPriorities()
    {
        return new List<ProjectPriority>
        {
            new ProjectPriority { Name = "Lowest", Color = "#888", Icon = "bi-chevron-double-down", Order = 0 },
            new ProjectPriority { Name = "Low", Color = "#6a9955", Icon = "bi-chevron-down", Order = 1 },
            new ProjectPriority { Name = "Medium", Color = "#d1b100", Icon = "bi-dash-lg", Order = 2 },
            new ProjectPriority { Name = "High", Color = "#ce9178", Icon = "bi-chevron-up", Order = 3 },
            new ProjectPriority { Name = "Highest", Color = "#f44747", Icon = "bi-chevron-double-up", Order = 4 },
            new ProjectPriority { Name = "Critical", Color = "#ff0000", Icon = "bi-exclamation-octagon", Order = 5 }
        };
    }

    public TaskList AddListToProject(Project project, string listName)
    {
        var list = new TaskList { Name = listName };
        project.Lists.Add(list);
        return list;
    }

    public Project? FindProjectById(Guid id)
    {
        return _projects.FirstOrDefault(p => p.Id == id);
    }

    public TaskItem AddTaskToList(TaskList list, string title, string? description = null, TaskPriority priority = TaskPriority.Medium, Guid? statusId = null, int estimateMinutes = 0, DateTime? start = null, DateTime? end = null, string? link = null, Guid? taskTypeId = null)
    {
        if (!statusId.HasValue || statusId == Guid.Empty)
        {
            var project = _projects.FirstOrDefault(p => p.Lists.Contains(list));
            statusId = project?.Statuses.FirstOrDefault()?.Id;
        }
        var task = new TaskItem { Title = title, Description = description, Priority = priority, StatusId = statusId, TaskTypeId = taskTypeId, EstimateMinutes = estimateMinutes, Start = start, End = end, Link = link };
        list.Tasks.Add(task);
        return task;
    }

    public TaskItem AddSubtaskToTask(TaskItem parentTaskItem, string title, string? description = null, TaskPriority priority = TaskPriority.Medium, Guid? statusId = null, int estimateMinutes = 0, DateTime? start = null, DateTime? end = null, string? link = null, Guid? taskTypeId = null)
    {
        if (!statusId.HasValue || statusId == Guid.Empty)
        {
            statusId = parentTaskItem.StatusId;
            
            // If parent has no status, try to find the project this task belongs to
            if (!statusId.HasValue || statusId == Guid.Empty)
            {
                foreach (var project in _projects)
                {
                    foreach (var list in project.Lists)
                    {
                        if (FindTaskInList(list.Tasks, parentTaskItem.Id) != null)
                        {
                            statusId = project.Statuses.FirstOrDefault()?.Id;
                            break;
                        }
                    }
                    if (statusId.HasValue) break;
                }
            }
        }
        
        if (!taskTypeId.HasValue || taskTypeId == Guid.Empty)
        {
            taskTypeId = parentTaskItem.TaskTypeId;
        }

        var subtask = new TaskItem { Title = title, Description = description, Priority = priority, StatusId = statusId, TaskTypeId = taskTypeId, EstimateMinutes = estimateMinutes, Start = start, End = end, Link = link };
        parentTaskItem.Subtasks.Add(subtask);
        return subtask;
    }

    public Project? UpdateProject(Guid projectId, string name, string? description, string? icon = null, string? color = null, List<TaskStatus>? statuses = null, List<TaskType>? taskTypes = null, List<ProjectPriority>? priorities = null, List<CustomFieldDefinition>? customFields = null)
    {
        var project = FindProjectById(projectId);
        if (project == null) return null;

        project.Name = name;
        project.Description = description;
        if (icon != null) project.Icon = icon;
        if (color != null) project.Color = color;

        if (statuses != null)
        {
            project.Statuses.Clear();
            project.Statuses.AddRange(statuses);
        }

        if (taskTypes != null)
        {
            project.TaskTypes.Clear();
            project.TaskTypes.AddRange(taskTypes);
        }

        if (priorities != null)
        {
            project.Priorities.Clear();
            project.Priorities.AddRange(priorities);
        }

        if (customFields != null)
        {
            project.CustomFields.Clear();
            project.CustomFields.AddRange(customFields);
        }

        return project;
    }

    public TaskItem? UpdateTask(Guid taskId, string title, string? description, TaskPriority priority, Guid? statusId = null, bool? isCompleted = null, int estimateMinutes = 0, DateTime? start = null, DateTime? end = null, string? link = null, Guid? taskTypeId = null, Guid? priorityId = null, List<CustomFieldValue>? customFieldValues = null)
    {
        var task = FindTaskById(taskId);
        if (task == null) return null;

        task.Title = title;
        task.Description = description;
        task.Priority = priority;
        task.EstimateMinutes = estimateMinutes;
        task.Start = start;
        task.End = end;
        task.Link = link;
        
        // Only update status if a valid non-empty Guid is provided
        if (statusId.HasValue && statusId.Value != Guid.Empty) 
        {
            task.StatusId = statusId;
        }
        
        // Update TaskTypeId
        task.TaskTypeId = taskTypeId;

        // Update PriorityId
        if (priorityId.HasValue && priorityId.Value != Guid.Empty)
        {
            task.PriorityId = priorityId;
        }
            
        if (isCompleted.HasValue) task.IsCompleted = isCompleted.Value;

        if (customFieldValues != null)
        {
            task.CustomFieldValues.Clear();
            task.CustomFieldValues.AddRange(customFieldValues);
        }

        return task;
    }

    public TaskItem? FindTaskById(Guid id)
    {
        foreach (var project in _projects)
        {
            foreach (var list in project.Lists)
            {
                var task = FindTaskInList(list.Tasks, id);
                if (task != null) return task;
            }
        }
        return null;
    }

    private TaskItem? FindTaskInList(List<TaskItem> tasks, Guid id)
    {
        foreach (var task in tasks)
        {
            if (task.Id == id) return task;
            var subtask = FindTaskInList(task.Subtasks, id);
            if (subtask != null) return subtask;
        }
        return null;
    }

    public TaskList? FindListById(Guid id)
    {
        foreach (var project in _projects)
        {
            foreach (var list in project.Lists)
            {
                if (list.Id == id) return list;
            }
        }
        return null;
    }

    public void SaveProjectsToYaml(string filePath)
    {
        lock (_fileLock)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yaml = serializer.Serialize(_projects);

            int retries = 5;
            while (retries > 0)
            {
                try
                {
                    File.WriteAllText(filePath, yaml);
                    break;
                }
                catch (IOException)
                {
                    retries--;
                    if (retries == 0) throw;
                    Thread.Sleep(100);
                }
            }
        }
    }

    public void LoadProjectsFromYaml(string filePath)
    {
        lock (_fileLock)
        {
            if (!File.Exists(filePath))
            {
                // Ensure a default file is created if it doesn't exist
                var defaultProject = new Project { Name = "Default Project", Description = "Auto-generated project" };
                defaultProject.Statuses.AddRange(GetDefaultStatuses());
                var list = new TaskList { Name = "To Do" };
                defaultProject.Lists.Add(list);
                var task = new TaskItem { Title = "Welcome to TM2", Description = "This is an auto-generated task." };
                task.StatusId = defaultProject.Statuses.FirstOrDefault()?.Id;
                list.Tasks.Add(task);

                _projects.Clear();
                _projects.Add(defaultProject);
                SaveProjectsToYaml(filePath);
                return;
            }

            string yaml;
            int retries = 5;
            while (true)
            {
                try
                {
                    yaml = File.ReadAllText(filePath);
                    break;
                }
                catch (IOException)
                {
                    retries--;
                    if (retries == 0) throw;
                    Thread.Sleep(100);
                }
            }

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new CustomFieldOptionConverter())
                .Build();

            var projects = deserializer.Deserialize<List<Project>>(yaml);

            // Migration and initialization
            foreach (var project in projects)
            {
                if (project.Statuses == null || project.Statuses.Count == 0)
                {
                    project.Statuses = GetDefaultStatuses();
                }

                // Migrate TaskTypes if they are the old defaults or missing
                if (project.TaskTypes == null || project.TaskTypes.Count == 0 || 
                    project.TaskTypes.Any(t => t.Name == "Task" || t.Name == "Bug" || t.Name == "Feature" || t.Name == "Improvement"))
                {
                    project.TaskTypes = GetDefaultTaskTypes();
                }

                if (project.Priorities == null || project.Priorities.Count == 0)
                {
                    project.Priorities = GetDefaultPriorities();
                }

                if (project.CustomFields == null)
                {
                    project.CustomFields = new List<CustomFieldDefinition>();
                }
                else
                {
                    foreach (var cf in project.CustomFields)
                    {
                        cf.Options ??= new List<CustomFieldOption>();
                    }
                }

                // Assign status to tasks that don't have one
                foreach (var list in project.Lists)
                {
                    AssignDefaultStatuses(list.Tasks, project.Statuses);
                    AssignDefaultPriorities(list.Tasks, project.Priorities);
                }
            }

            _projects.Clear();
            _projects.AddRange(projects);
        }
    }

    private void AssignDefaultStatuses(List<TaskItem> tasks, List<TaskStatus> statuses)
    {
        var doneStatus = statuses.FirstOrDefault(s => s.IsCompletedState) ?? statuses.Last();
        var todoStatus = statuses.FirstOrDefault(s => !s.IsCompletedState) ?? statuses.First();

        foreach (var task in tasks)
        {
            if (task.StatusId == null || task.StatusId == Guid.Empty)
            {
                task.StatusId = task.IsCompleted ? doneStatus.Id : todoStatus.Id;
            }
            AssignDefaultStatuses(task.Subtasks, statuses);
        }
    }

    private void AssignDefaultPriorities(List<TaskItem> tasks, List<ProjectPriority> priorities)
    {
        foreach (var task in tasks)
        {
            if (task.PriorityId == null || task.PriorityId == Guid.Empty)
            {
                // Map from legacy TaskPriority enum to new PriorityId
                int pIndex = (int)task.Priority;
                if (pIndex >= 0 && pIndex < priorities.Count)
                {
                    task.PriorityId = priorities[pIndex].Id;
                }
                else
                {
                    // Default to Medium (usually index 2)
                    var medium = priorities.FirstOrDefault(p => p.Name == "Medium") ?? priorities.ElementAtOrDefault(2) ?? priorities.FirstOrDefault();
                    task.PriorityId = medium?.Id;
                }
            }
            AssignDefaultPriorities(task.Subtasks, priorities);
        }
    }

    public IEnumerable<Project> GetAllProjects() => ActiveProjects;
    
    public enum MovePosition { Before, After, Inside }

    public class CustomFieldOptionConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(CustomFieldOption);

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer nestedObjectDeserializer)
        {
            if (parser.TryConsume<Scalar>(out var scalar))
            {
                return new CustomFieldOption { Name = scalar.Value, Color = "#6e7681", Icon = "" };
            }

            if (parser.Current is MappingStart)
            {
                var option = new CustomFieldOption();
                parser.Consume<MappingStart>();
                while (!parser.TryConsume<MappingEnd>(out _))
                {
                    var key = parser.Consume<Scalar>().Value;
                    var value = parser.Consume<Scalar>().Value;

                    if (key == "name") option.Name = value;
                    else if (key == "color") option.Color = value;
                    else if (key == "icon") option.Icon = value;
                }
                return option;
            }

            return null;
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            var option = (CustomFieldOption)value!;
            emitter.Emit(new MappingStart());
            emitter.Emit(new Scalar("name"));
            emitter.Emit(new Scalar(option.Name));
            if (!string.IsNullOrEmpty(option.Color))
            {
                emitter.Emit(new Scalar("color"));
                emitter.Emit(new Scalar(option.Color));
            }
            if (!string.IsNullOrEmpty(option.Icon))
            {
                emitter.Emit(new Scalar("icon"));
                emitter.Emit(new Scalar(option.Icon));
            }
            emitter.Emit(new MappingEnd());
        }
    }

    public bool DeleteProject(Guid projectId)
    {
        var project = FindProjectById(projectId);
        if (project == null) return false;
        return _projects.Remove(project);
    }

    public TaskItem? MoveTask(Guid taskId, Guid? targetListId, Guid? targetTaskId, MovePosition position = MovePosition.Inside)
    {
        var task = FindTaskById(taskId);
        if (task == null) return null;

        // Prevent moving a task into itself or its own subtasks
        if (targetTaskId.HasValue && IsDescendant(task, targetTaskId.Value))
        {
            return null;
        }

        // Remove from current location
        DeleteTask(taskId);

        if (targetTaskId.HasValue)
        {
            var targetTask = FindTaskById(targetTaskId.Value);
            if (targetTask != null)
            {
                if (position == MovePosition.Inside)
                {
                    targetTask.Subtasks.Add(task);
                    return task;
                }
                else
                {
                    // Find parent of targetTask
                    return InsertNearTask(targetTask, task, position);
                }
            }
        }

        if (targetListId.HasValue)
        {
            var list = FindListById(targetListId.Value);
            if (list != null)
            {
                list.Tasks.Add(task);
                return task;
            }
        }

        return null;
    }

    public TaskItem? CopyTask(Guid taskId, Guid? targetListId, Guid? targetTaskId, MovePosition position = MovePosition.Inside)
    {
        var sourceTask = FindTaskById(taskId);
        if (sourceTask == null) return null;

        var newTask = DeepCopyTask(sourceTask);

        if (targetTaskId.HasValue)
        {
            var targetTask = FindTaskById(targetTaskId.Value);
            if (targetTask != null)
            {
                if (position == MovePosition.Inside)
                {
                    targetTask.Subtasks.Add(newTask);
                    return newTask;
                }
                else
                {
                    return InsertNearTask(targetTask, newTask, position);
                }
            }
        }

        if (targetListId.HasValue)
        {
            var list = FindListById(targetListId.Value);
            if (list != null)
            {
                list.Tasks.Add(newTask);
                return newTask;
            }
        }

        return null;
    }

    private TaskItem DeepCopyTask(TaskItem source)
    {
        var newTask = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = source.Title,
            Description = source.Description,
            IsCompleted = source.IsCompleted,
            StatusId = source.StatusId,
            TaskTypeId = source.TaskTypeId,
            PriorityId = source.PriorityId,
            Priority = source.Priority,
            EstimateMinutes = source.EstimateMinutes,
            Link = source.Link,
            Start = source.Start,
            End = source.End,
            Subtasks = source.Subtasks.Select(DeepCopyTask).ToList()
        };
        return newTask;
    }

    private bool IsDescendant(TaskItem parent, Guid taskId)
    {
        if (parent.Id == taskId) return true;
        foreach (var sub in parent.Subtasks)
        {
            if (IsDescendant(sub, taskId)) return true;
        }
        return false;
    }

    private TaskItem? InsertNearTask(TaskItem targetTask, TaskItem taskToMove, MovePosition position)
    {
        foreach (var project in _projects)
        {
            foreach (var list in project.Lists)
            {
                if (InsertInListIfFound(list.Tasks, targetTask, taskToMove, position))
                    return taskToMove;
            }
        }
        return null;
    }

    private bool InsertInListIfFound(List<TaskItem> tasks, TaskItem targetTask, TaskItem taskToMove, MovePosition position)
    {
        for (int i = 0; i < tasks.Count; i++)
        {
            if (tasks[i].Id == targetTask.Id)
            {
                int insertIndex = (position == MovePosition.Before) ? i : i + 1;
                tasks.Insert(insertIndex, taskToMove);
                return true;
            }
            if (InsertInListIfFound(tasks[i].Subtasks, targetTask, taskToMove, position))
                return true;
        }
        return false;
    }

    public TaskList? MoveList(Guid projectId, Guid listId, Guid? targetListId, MovePosition position = MovePosition.After)
    {
        var project = FindProjectById(projectId);
        if (project == null) return null;

        var list = project.Lists.FirstOrDefault(l => l.Id == listId);
        if (list == null) return null;

        project.Lists.Remove(list);

        if (targetListId.HasValue)
        {
            var targetIndex = project.Lists.FindIndex(l => l.Id == targetListId.Value);
            if (targetIndex != -1)
            {
                int insertIndex = (position == MovePosition.Before) ? targetIndex : targetIndex + 1;
                project.Lists.Insert(insertIndex, list);
                return list;
            }
        }

        project.Lists.Add(list);
        return list;
    }

    public TaskList? UpdateList(Guid projectId, Guid listId, string name)
    {
        var project = FindProjectById(projectId);
        if (project == null) return null;

        var list = project.Lists.FirstOrDefault(l => l.Id == listId);
        if (list == null) return null;

        list.Name = name;
        return list;
    }

    public Project? MoveProject(Guid projectId, Guid? targetProjectId, MovePosition position = MovePosition.After)
    {
        var project = FindProjectById(projectId);
        if (project == null) return null;

        _projects.Remove(project);

        if (targetProjectId.HasValue)
        {
            var targetIndex = _projects.FindIndex(p => p.Id == targetProjectId.Value);
            if (targetIndex != -1)
            {
                int insertIndex = (position == MovePosition.Before) ? targetIndex : targetIndex + 1;
                _projects.Insert(insertIndex, project);
                return project;
            }
        }

        _projects.Add(project);
        return project;
    }

    public bool DeleteTask(Guid taskId)
    {
        foreach (var project in _projects)
        {
            foreach (var list in project.Lists)
            {
                if (RemoveTaskFromList(list.Tasks, taskId))
                    return true;
            }
        }
        return false;
    }

    public bool DeleteList(Guid projectId, Guid listId)
    {
        var project = FindProjectById(projectId);
        if (project == null) return false;

        var list = project.Lists.FirstOrDefault(l => l.Id == listId);
        if (list == null) return false;

        return project.Lists.Remove(list);
    }

    private bool RemoveTaskFromList(List<TaskItem> tasks, Guid taskId)
    {
        for (int i = 0; i < tasks.Count; i++)
        {
            if (tasks[i].Id == taskId)
            {
                tasks.RemoveAt(i);
                return true;
            }
            if (RemoveTaskFromList(tasks[i].Subtasks, taskId))
                return true;
        }
        return false;
    }
}
