using FlightPlan.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using TaskStatus = FlightPlan.Models.TaskStatus;

namespace FlightPlan.Services;

public class ProjectManager
{
    private readonly List<Project> _projects = new();
    private readonly object _fileLock = new();

    public Project CreateProject(string name, string? description = null, string? icon = null, string? color = null, List<TaskStatus>? statuses = null, List<TaskType>? taskTypes = null, List<ProjectPriority>? priorities = null)
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

    public Project? UpdateProject(Guid projectId, string name, string? description, string? icon = null, string? color = null, List<TaskStatus>? statuses = null, List<TaskType>? taskTypes = null, List<ProjectPriority>? priorities = null)
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

        return project;
    }

    public TaskItem? UpdateTask(Guid taskId, string title, string? description, TaskPriority priority, Guid? statusId = null, bool? isCompleted = null, int estimateMinutes = 0, DateTime? start = null, DateTime? end = null, string? link = null, Guid? taskTypeId = null, Guid? priorityId = null)
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

    public IEnumerable<Project> GetAllProjects() => _projects;
    
    public enum MovePosition { Before, After, Inside }

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
