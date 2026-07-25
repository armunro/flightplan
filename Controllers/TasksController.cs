using Microsoft.AspNetCore.Mvc;
using FlightPlan.Models;
using FlightPlan.Services;
using System.Text.Json;

namespace FlightPlan.Controllers;

public record TaskCreateRequest(Guid ListId, string Title, string? Description, TaskPriority Priority, Guid? StatusId, int EstimateMinutes = 0, DateTime? Start = null, DateTime? End = null, string? Link = null, Guid? TaskTypeId = null, Guid? PriorityId = null);
public record TaskUpdateRequest(string? Title, string? Description, TaskPriority? Priority, Guid? StatusId, bool? IsCompleted, int? EstimateMinutes = null, DateTime? Start = null, DateTime? End = null, string? Link = null, Guid? TaskTypeId = null, Guid? PriorityId = null);
public record TaskBulkUpdateRequest(List<Guid> TaskIds);
public record TaskBulkDeleteRequest(List<Guid> TaskIds);
public record TaskMoveRequest(Guid? TargetListId, Guid? TargetTaskId, ProjectManager.MovePosition? Position);
public record TaskBulkMoveRequest(List<Guid> TaskIds, Guid? TargetListId, Guid? TargetTaskId, ProjectManager.MovePosition? Position);

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ProjectManager _pm;
    private readonly IStorageService _storageService;

    public TasksController(ProjectManager pm, IStorageService storageService)
    {
        _pm = pm;
        _storageService = storageService;
    }

    [HttpGet("{taskId:guid}")]
    public IActionResult Get(Guid taskId)
    {
        var task = _pm.FindTaskById(taskId);
        if (task == null) return NotFound("Task not found");
        return Ok(task);
    }

    [HttpPost]
    public IActionResult Create(TaskCreateRequest request)
    {
        var list = _pm.FindListById(request.ListId);
        if (list == null) return NotFound("List not found");

        var task = _pm.AddTaskToList(list, request.Title, request.Description, request.Priority, request.StatusId, request.EstimateMinutes, request.Start, request.End, request.Link, request.TaskTypeId);
        if (request.PriorityId.HasValue) task.PriorityId = request.PriorityId;
        
        _pm.SaveProjectsToYaml(_storageService.GetProjectsPath());
        return Ok(task);
    }

    [HttpPost("{taskId:guid}/subtasks")]
    public IActionResult CreateSubtask(Guid taskId, TaskCreateRequest request)
    {
        var parentTask = _pm.FindTaskById(taskId);
        if (parentTask == null) return NotFound("Parent task not found");

        var subtask = _pm.AddSubtaskToTask(parentTask, request.Title, request.Description, request.Priority, request.StatusId, request.EstimateMinutes, request.Start, request.End, request.Link, request.TaskTypeId);
        if (request.PriorityId.HasValue) subtask.PriorityId = request.PriorityId;

        _pm.SaveProjectsToYaml(_storageService.GetProjectsPath());
        return Ok(subtask);
    }

    [HttpPost("{taskId:guid}/sibling")]
    public IActionResult CreateSibling(Guid taskId, TaskCreateRequest request)
    {
        var task = _pm.FindTaskById(taskId);
        if (task == null) return NotFound("Task not found");

        var projects = _pm.GetAllProjects();
        TaskItem? sibling = null;

        foreach (var project in projects)
        {
            foreach (var list in project.Lists)
            {
                int index = list.Tasks.FindIndex(t => t.Id == taskId);
                if (index != -1)
                {
                    sibling = new TaskItem { 
                    Title = request.Title, 
                    Description = request.Description, 
                    Priority = request.Priority, 
                    PriorityId = request.PriorityId,
                    StatusId = request.StatusId ?? task.StatusId,
                    TaskTypeId = request.TaskTypeId ?? task.TaskTypeId,
                    EstimateMinutes = request.EstimateMinutes, 
                    Start = request.Start, 
                    End = request.End, 
                    Link = request.Link 
                };
                    list.Tasks.Insert(index + 1, sibling);
                    break;
                }
                
                sibling = CreateSiblingInSubtasks(list.Tasks, taskId, request);
                if (sibling != null) break;
            }
            if (sibling != null) break;
        }

        if (sibling == null) return NotFound("Task parent container not found");

        _pm.SaveProjectsToYaml(_storageService.GetProjectsPath());
        return Ok(sibling);
    }

    private TaskItem? CreateSiblingInSubtasks(List<TaskItem> tasks, Guid taskId, TaskCreateRequest request)
    {
        foreach (var task in tasks)
        {
            int index = task.Subtasks.FindIndex(t => t.Id == taskId);
            if (index != -1)
            {
                var sibling = new TaskItem { 
                    Title = request.Title, 
                    Description = request.Description, 
                    Priority = request.Priority, 
                    PriorityId = request.PriorityId,
                    StatusId = request.StatusId ?? task.Subtasks[index].StatusId,
                    TaskTypeId = request.TaskTypeId ?? task.Subtasks[index].TaskTypeId,
                    EstimateMinutes = request.EstimateMinutes, 
                    Start = request.Start, 
                    End = request.End, 
                    Link = request.Link 
                };
                task.Subtasks.Insert(index + 1, sibling);
                return sibling;
            }
            var found = CreateSiblingInSubtasks(task.Subtasks, taskId, request);
            if (found != null) return found;
        }
        return null;
    }

    [HttpPut("{taskId:guid}")]
    public IActionResult Update(Guid taskId, [FromBody] JsonElement requestBody)
    {
        var existing = _pm.FindTaskById(taskId);
        if (existing == null) return NotFound("Task not found");

        string title = requestBody.TryGetProperty("title", out var titleProp) && titleProp.ValueKind != JsonValueKind.Null ? titleProp.GetString() ?? existing.Title : existing.Title;
        string? description = requestBody.TryGetProperty("description", out var descProp) ? (descProp.ValueKind == JsonValueKind.Null ? null : descProp.GetString()) : existing.Description;
        TaskPriority priority = requestBody.TryGetProperty("priority", out var prioProp) && prioProp.ValueKind == JsonValueKind.Number ? (TaskPriority)prioProp.GetInt32() : existing.Priority;
        Guid? statusId = requestBody.TryGetProperty("statusId", out var statusProp) ? (statusProp.ValueKind == JsonValueKind.Null ? null : (statusProp.TryGetGuid(out var sId) ? sId : null)) : existing.StatusId;
        bool isCompleted = requestBody.TryGetProperty("isCompleted", out var compProp) && (compProp.ValueKind == JsonValueKind.True || compProp.ValueKind == JsonValueKind.False) ? compProp.GetBoolean() : existing.IsCompleted;
        int estimateMinutes = requestBody.TryGetProperty("estimateMinutes", out var estProp) && estProp.ValueKind == JsonValueKind.Number ? estProp.GetInt32() : existing.EstimateMinutes;
        DateTime? start = requestBody.TryGetProperty("start", out var startProp) ? (startProp.ValueKind == JsonValueKind.Null ? null : (startProp.TryGetDateTime(out var st) ? st : null)) : existing.Start;
        DateTime? end = requestBody.TryGetProperty("end", out var endProp) ? (endProp.ValueKind == JsonValueKind.Null ? null : (endProp.TryGetDateTime(out var en) ? en : null)) : existing.End;
        string? link = requestBody.TryGetProperty("link", out var linkProp) ? (linkProp.ValueKind == JsonValueKind.Null ? null : linkProp.GetString()) : existing.Link;
        Guid? taskTypeId = requestBody.TryGetProperty("taskTypeId", out var typeProp) ? (typeProp.ValueKind == JsonValueKind.Null ? null : (typeProp.TryGetGuid(out var ttId) ? ttId : null)) : existing.TaskTypeId;
        Guid? priorityId = requestBody.TryGetProperty("priorityId", out var prioIdProp) ? (prioIdProp.ValueKind == JsonValueKind.Null ? null : (prioIdProp.TryGetGuid(out var pId) ? pId : null)) : existing.PriorityId;

        var task = _pm.UpdateTask(
            taskId,
            title,
            description,
            priority,
            statusId,
            isCompleted,
            estimateMinutes,
            start,
            end,
            link,
            taskTypeId,
            priorityId);

        if (task == null) return NotFound("Task not found");

        _pm.SaveProjectsToYaml(_storageService.GetProjectsPath());
        return Ok(task);
    }

    [HttpPut("bulk-update")]
    public IActionResult BulkUpdate([FromBody] JsonElement requestBody)
    {
        if (requestBody.ValueKind != JsonValueKind.Object) return BadRequest("Invalid request body");

        if (!requestBody.TryGetProperty("taskIds", out var taskIdsProp) || taskIdsProp.ValueKind != JsonValueKind.Array)
            return BadRequest("taskIds is required and must be an array");

        var taskIds = new List<Guid>();
        foreach (var idElem in taskIdsProp.EnumerateArray())
        {
            if (idElem.TryGetGuid(out var guid)) taskIds.Add(guid);
        }

        foreach (var taskId in taskIds)
        {
            var existing = _pm.FindTaskById(taskId);
            if (existing == null) continue;

            string title = requestBody.TryGetProperty("title", out var titleProp) && titleProp.ValueKind != JsonValueKind.Null ? titleProp.GetString() ?? existing.Title : existing.Title;
            string? description = requestBody.TryGetProperty("description", out var descProp) ? (descProp.ValueKind == JsonValueKind.Null ? null : descProp.GetString()) : existing.Description;
            TaskPriority priority = requestBody.TryGetProperty("priority", out var prioProp) && prioProp.ValueKind == JsonValueKind.Number ? (TaskPriority)prioProp.GetInt32() : existing.Priority;
            Guid? statusId = requestBody.TryGetProperty("statusId", out var statusProp) ? (statusProp.ValueKind == JsonValueKind.Null ? null : (statusProp.TryGetGuid(out var sId) ? sId : null)) : existing.StatusId;
            bool isCompleted = requestBody.TryGetProperty("isCompleted", out var compProp) && (compProp.ValueKind == JsonValueKind.True || compProp.ValueKind == JsonValueKind.False) ? compProp.GetBoolean() : existing.IsCompleted;
            int estimateMinutes = requestBody.TryGetProperty("estimateMinutes", out var estProp) && estProp.ValueKind == JsonValueKind.Number ? estProp.GetInt32() : existing.EstimateMinutes;
            DateTime? start = requestBody.TryGetProperty("start", out var startProp) ? (startProp.ValueKind == JsonValueKind.Null ? null : (startProp.TryGetDateTime(out var st) ? st : null)) : existing.Start;
            DateTime? end = requestBody.TryGetProperty("end", out var endProp) ? (endProp.ValueKind == JsonValueKind.Null ? null : (endProp.TryGetDateTime(out var en) ? en : null)) : existing.End;
            string? link = requestBody.TryGetProperty("link", out var linkProp) ? (linkProp.ValueKind == JsonValueKind.Null ? null : linkProp.GetString()) : existing.Link;
            Guid? taskTypeId = requestBody.TryGetProperty("taskTypeId", out var typeProp) ? (typeProp.ValueKind == JsonValueKind.Null ? null : (typeProp.TryGetGuid(out var ttId) ? ttId : null)) : existing.TaskTypeId;
            Guid? priorityId = requestBody.TryGetProperty("priorityId", out var prioIdProp) ? (prioIdProp.ValueKind == JsonValueKind.Null ? null : (prioIdProp.TryGetGuid(out var pId) ? pId : null)) : existing.PriorityId;

            _pm.UpdateTask(
                taskId,
                title,
                description,
                priority,
                statusId,
                isCompleted,
                estimateMinutes,
                start,
                end,
                link,
                taskTypeId,
                priorityId);
        }

        _pm.SaveProjectsToYaml(_storageService.GetProjectsPath());
        return Ok();
    }

    [HttpDelete("bulk-delete")]
    public IActionResult BulkDelete(TaskBulkDeleteRequest request)
    {
        foreach (var taskId in request.TaskIds)
        {
            _pm.DeleteTask(taskId);
        }
        _pm.SaveProjectsToYaml(_storageService.GetProjectsPath());
        return NoContent();
    }

    [HttpDelete("{taskId:guid}")]
    public IActionResult Delete(Guid taskId)
    {
        if (_pm.DeleteTask(taskId))
        {
            _pm.SaveProjectsToYaml(_storageService.GetProjectsPath());
            return NoContent();
        }
        return NotFound("Task not found");
    }

    [HttpPost("{taskId:guid}/move")]
    public IActionResult Move(Guid taskId, TaskMoveRequest request)
    {
        var position = request.Position ?? ProjectManager.MovePosition.Inside;
        var task = _pm.MoveTask(taskId, request.TargetListId, request.TargetTaskId, position);
        if (task == null) return NotFound("Task or Target not found");

        _pm.SaveProjectsToYaml(_storageService.GetProjectsPath());
        return Ok(task);
    }

    [HttpPost("bulk-move")]
    public IActionResult BulkMove(TaskBulkMoveRequest request)
    {
        var position = request.Position ?? ProjectManager.MovePosition.Inside;
        foreach (var taskId in request.TaskIds)
        {
            _pm.MoveTask(taskId, request.TargetListId, request.TargetTaskId, position);
        }

        _pm.SaveProjectsToYaml(_storageService.GetProjectsPath());
        return Ok();
    }

    [HttpPost("from-email")]
    public IActionResult CreateFromEmail([FromQuery] string subject, [FromQuery] string sender, [FromQuery] string? link)
    {
        // Find or create "Email" project
        var project = _pm.GetAllProjects().FirstOrDefault(p => p.Name.Equals("Email", StringComparison.OrdinalIgnoreCase));
        if (project == null)
        {
            project = _pm.CreateProject("Email", "Tasks created from emails");
        }

        // Find or create "Email" list in that project
        var list = project.Lists.FirstOrDefault(l => l.Name.Equals("Email", StringComparison.OrdinalIgnoreCase));
        if (list == null)
        {
            list = _pm.AddListToProject(project, "Email");
        }

        var title = subject;
        var description = $"From: {sender}";
        
        var task = _pm.AddTaskToList(list, title, description, TaskPriority.Medium, null, 0, null, null, link);
        _pm.SaveProjectsToYaml(_storageService.GetProjectsPath());
        
        return Ok(task);
    }

    [HttpPost("from-jira")]
    public IActionResult CreateFromJira([FromQuery] string key, [FromQuery] string summary, [FromQuery] string link, [FromQuery] Guid? listId)
    {
        TaskList? list = null;

        if (listId.HasValue)
        {
            list = _pm.FindListById(listId.Value);
        }

        if (list == null)
        {
            // Find or create "Jira" project
            var project = _pm.GetAllProjects().FirstOrDefault(p => p.Name.Equals("Jira", StringComparison.OrdinalIgnoreCase));
            if (project == null)
            {
                project = _pm.CreateProject("Jira", "Tasks created from Jira issues");
            }

            // Find or create "Inbox" list in that project
            list = project.Lists.FirstOrDefault(l => l.Name.Equals("Inbox", StringComparison.OrdinalIgnoreCase));
            if (list == null)
            {
                list = _pm.AddListToProject(project, "Inbox");
            }
        }

        var title = $"[{key}] {summary}";
        
        var task = _pm.AddTaskToList(list, title, null, TaskPriority.Medium, null, 0, null, null, link);
        _pm.SaveProjectsToYaml(_storageService.GetProjectsPath());
        
        return Ok(task);
    }
}
