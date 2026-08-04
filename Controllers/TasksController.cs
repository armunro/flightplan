using Microsoft.AspNetCore.Mvc;
using FlightPlan.Models;
using FlightPlan.Services;
using System.Text.Json;

namespace FlightPlan.Controllers;

public record TaskCreateRequest(Guid ListId, string Title, string? Description, TaskPriority Priority, Guid? StatusId, int EstimateMinutes = 0, DateTime? Start = null, DateTime? End = null, string? Link = null, Guid? TaskTypeId = null, Guid? PriorityId = null, List<CustomFieldValue>? CustomFieldValues = null);
public record TaskUpdateRequest(string? Title, string? Description, TaskPriority? Priority, Guid? StatusId, bool? IsCompleted, int? EstimateMinutes = null, DateTime? Start = null, DateTime? End = null, string? Link = null, Guid? TaskTypeId = null, Guid? PriorityId = null, List<CustomFieldValue>? CustomFieldValues = null);
public record TaskBulkUpdateRequest(List<Guid> TaskIds);
public record TaskBulkDeleteRequest(List<Guid> TaskIds);
public record TaskMoveRequest(Guid? TargetListId, Guid? TargetTaskId, ProjectManager.MovePosition? Position);
public record TaskBulkMoveRequest(List<Guid> TaskIds, Guid? TargetListId, Guid? TargetTaskId, ProjectManager.MovePosition? Position);
public record TaskExportRequest(List<Guid> TaskIds, List<string> Columns);

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

    [HttpPost("export/excel")]
    public IActionResult ExportExcel(TaskExportRequest request)
    {
        var tasks = new List<TaskItem>();
        var allProjects = _pm.GetAllProjects().ToList();
        
        foreach (var id in request.TaskIds)
        {
            var task = _pm.FindTaskById(id);
            if (task != null) tasks.Add(task);
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\"?>");
        sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
        sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
        sb.AppendLine(" xmlns:o=\"urn:schemas-microsoft-com:office:office\"");
        sb.AppendLine(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"");
        sb.AppendLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"");
        sb.AppendLine(" xmlns:html=\"http://www.w3.org/TR/REC-html40\">");
        sb.AppendLine(" <Styles>");
        sb.AppendLine("  <Style ss:ID=\"Default\" ss:Name=\"Normal\">");
        sb.AppendLine("   <Alignment ss:Vertical=\"Bottom\"/>");
        sb.AppendLine("   <Borders/>");
        sb.AppendLine("   <Font ss:FontName=\"Calibri\" x:Family=\"Swiss\" ss:Size=\"11\" ss:Color=\"#000000\"/>");
        sb.AppendLine("   <Interior/>");
        sb.AppendLine("   <NumberFormat/>");
        sb.AppendLine("   <Protection/>");
        sb.AppendLine("  </Style>");
        sb.AppendLine("  <Style ss:ID=\"Header\">");
        sb.AppendLine("   <Font ss:FontName=\"Calibri\" x:Family=\"Swiss\" ss:Size=\"11\" ss:Color=\"#000000\" ss:Bold=\"1\"/>");
        sb.AppendLine("   <Interior ss:Color=\"#F2F2F2\" ss:Pattern=\"Solid\"/>");
        sb.AppendLine("  </Style>");
        sb.AppendLine("  <Style ss:ID=\"sDate\">");
        sb.AppendLine("   <NumberFormat ss:Format=\"Short Date\"/>");
        sb.AppendLine("  </Style>");
        sb.AppendLine(" </Styles>");
        sb.AppendLine(" <Worksheet ss:Name=\"Tasks\">");
        sb.AppendLine("  <Table>");
        
        // Header
        sb.AppendLine("   <Row ss:StyleID=\"Header\">");
        foreach (var col in request.Columns)
        {
            sb.AppendLine($"    <Cell><Data ss:Type=\"String\">{System.Security.SecurityElement.Escape(col)}</Data></Cell>");
        }
        sb.AppendLine("   </Row>");

        // Data
        foreach (var task in tasks)
        {
            sb.AppendLine("   <Row>");
            foreach (var col in request.Columns)
            {
                string val = GetTaskValue(task, col, allProjects);
                string style = "";
                string type = "String";
                
                if (col == "start" || col == "end")
                {
                    if (DateTime.TryParse(val, out DateTime dt))
                    {
                        val = dt.ToString("yyyy-MM-ddTHH:mm:ss.fff");
                        type = "DateTime";
                        style = " ss:StyleID=\"sDate\"";
                    }
                }
                else if (col == "estimateMinutes" && double.TryParse(val, out _))
                {
                    type = "Number";
                }
                
                sb.AppendLine($"    <Cell{style}><Data ss:Type=\"{type}\">{System.Security.SecurityElement.Escape(val)}</Data></Cell>");
            }
            sb.AppendLine("   </Row>");
        }

        sb.AppendLine("  </Table>");
        sb.AppendLine(" </Worksheet>");
        sb.AppendLine("</Workbook>");

        byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        return File(fileBytes, "application/xml", "tasks_export.xml");
    }

    private string GetTaskValue(TaskItem task, string col, List<Project> allProjects)
    {
        Project? project = allProjects.FirstOrDefault(p => p.Lists.Any(l => l.Tasks.Any(t => t.Id == task.Id || (t.Subtasks != null && t.Subtasks.Any(st => st.Id == task.Id)))));
        
        switch (col.ToLower())
        {
            case "id": return task.Id.ToString();
            case "title": return task.Title;
            case "status":
                if (project != null)
                {
                    var status = project.Statuses.FirstOrDefault(s => s.Id == task.StatusId);
                    return status?.Name ?? "";
                }
                return "";
            case "priority":
                if (project != null)
                {
                    var priority = project.Priorities.FirstOrDefault(p => p.Id == task.PriorityId);
                    return priority?.Name ?? "";
                }
                return "";
            case "type":
                if (project != null)
                {
                    var type = project.TaskTypes.FirstOrDefault(t => t.Id == task.TaskTypeId);
                    return type?.Name ?? "";
                }
                return "";
            case "list":
                if (project != null)
                {
                    var list = project.Lists.FirstOrDefault(l => l.Tasks.Any(t => t.Id == task.Id || (t.Subtasks != null && t.Subtasks.Any(st => st.Id == task.Id))));
                    return list?.Name ?? "";
                }
                return "";
            case "start": return task.Start?.ToString("yyyy-MM-dd HH:mm") ?? "";
            case "end": return task.End?.ToString("yyyy-MM-dd HH:mm") ?? "";
            case "estimate": return task.EstimateMinutes > 0 ? $"{task.EstimateMinutes}m" : "";
            case "created": return ""; // TaskItem doesn't seem to have CreatedAt
            default: return "";
        }
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
        if (request.CustomFieldValues != null)
        {
            task.CustomFieldValues.Clear();
            task.CustomFieldValues.AddRange(request.CustomFieldValues);
        }
        
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
        if (request.CustomFieldValues != null)
        {
            subtask.CustomFieldValues.Clear();
            subtask.CustomFieldValues.AddRange(request.CustomFieldValues);
        }

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
                    Link = request.Link,
                    CustomFieldValues = request.CustomFieldValues ?? new List<CustomFieldValue>()
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
                    Link = request.Link,
                    CustomFieldValues = request.CustomFieldValues ?? new List<CustomFieldValue>()
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
    public IActionResult Update(Guid taskId, TaskUpdateRequest request)
    {
        var existing = _pm.FindTaskById(taskId);
        if (existing == null) return NotFound("Task not found");

        var task = _pm.UpdateTask(
            taskId,
            request.Title ?? existing.Title,
            request.Description ?? existing.Description,
            request.Priority ?? existing.Priority,
            request.StatusId ?? existing.StatusId,
            request.IsCompleted ?? existing.IsCompleted,
            request.EstimateMinutes ?? existing.EstimateMinutes,
            request.Start,
            request.End,
            request.Link ?? existing.Link,
            request.TaskTypeId ?? existing.TaskTypeId,
            request.PriorityId ?? existing.PriorityId,
            request.CustomFieldValues ?? existing.CustomFieldValues);

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

            // Find which project this task belongs to, so we can validate statuses/types/priorities
            var projects = _pm.GetAllProjects();
            var taskProject = projects.FirstOrDefault(p => p.Lists.Any(l => _pm.FindTaskInList(l.Tasks, taskId) != null));

            string title = requestBody.TryGetProperty("title", out var titleProp) && titleProp.ValueKind != JsonValueKind.Null ? titleProp.GetString() ?? existing.Title : existing.Title;
            string? description = requestBody.TryGetProperty("description", out var descProp) ? (descProp.ValueKind == JsonValueKind.Null ? null : descProp.GetString()) : existing.Description;
            TaskPriority priority = requestBody.TryGetProperty("priority", out var prioProp) && prioProp.ValueKind == JsonValueKind.Number ? (TaskPriority)prioProp.GetInt32() : existing.Priority;
            Guid? statusId = requestBody.TryGetProperty("statusId", out var statusProp) ? (statusProp.ValueKind == JsonValueKind.Null ? null : (statusProp.TryGetGuid(out var sId) ? sId : null)) : existing.StatusId;
            
            bool? isCompleted = null;
            if (requestBody.TryGetProperty("isCompleted", out var compProp) && (compProp.ValueKind == JsonValueKind.True || compProp.ValueKind == JsonValueKind.False))
            {
                isCompleted = compProp.GetBoolean();
            }
            else if (statusId != existing.StatusId && taskProject != null && statusId.HasValue)
            {
                // If status changed and we didn't get an explicit isCompleted, try to find it from the project's statuses
                var newStatus = taskProject.Statuses.FirstOrDefault(s => s.Id == statusId.Value);
                if (newStatus != null)
                {
                    isCompleted = newStatus.IsCompletedState;
                }
            }

            int estimateMinutes = requestBody.TryGetProperty("estimateMinutes", out var estProp) && estProp.ValueKind == JsonValueKind.Number ? estProp.GetInt32() : existing.EstimateMinutes;
            DateTime? start = requestBody.TryGetProperty("start", out var startProp) ? (startProp.ValueKind == JsonValueKind.Null ? null : (startProp.TryGetDateTime(out var st) ? st : null)) : existing.Start;
            DateTime? end = requestBody.TryGetProperty("end", out var endProp) ? (endProp.ValueKind == JsonValueKind.Null ? null : (endProp.TryGetDateTime(out var en) ? en : null)) : existing.End;
            string? link = requestBody.TryGetProperty("link", out var linkProp) ? (linkProp.ValueKind == JsonValueKind.Null ? null : linkProp.GetString()) : existing.Link;
            Guid? taskTypeId = requestBody.TryGetProperty("taskTypeId", out var typeProp) ? (typeProp.ValueKind == JsonValueKind.Null ? null : (typeProp.TryGetGuid(out var ttId) ? ttId : null)) : existing.TaskTypeId;
            Guid? priorityId = requestBody.TryGetProperty("priorityId", out var prioIdProp) ? (prioIdProp.ValueKind == JsonValueKind.Null ? null : (prioIdProp.TryGetGuid(out var pId) ? pId : null)) : existing.PriorityId;

            List<CustomFieldValue>? customFieldValues = null;
            if (requestBody.TryGetProperty("customFieldValues", out var cfvProp) && cfvProp.ValueKind == JsonValueKind.Array)
            {
                customFieldValues = new List<CustomFieldValue>();
                foreach (var item in cfvProp.EnumerateArray())
                {
                    if (item.TryGetProperty("definitionId", out var defIdProp) && defIdProp.TryGetGuid(out var defId))
                    {
                        var valObj = new CustomFieldValue { DefinitionId = defId };
                        if (item.TryGetProperty("value", out var valProp))
                        {
                            valObj.Value = valProp.ValueKind == JsonValueKind.Null ? null : valProp.GetString();
                        }
                        if (item.TryGetProperty("values", out var valsProp) && valsProp.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var v in valsProp.EnumerateArray())
                            {
                                var s = v.GetString();
                                if (s != null) valObj.Values.Add(s);
                            }
                        }
                        customFieldValues.Add(valObj);
                    }
                }
            }
            else
            {
                customFieldValues = existing.CustomFieldValues;
            }

            _pm.UpdateTask(
                taskId,
                title,
                description,
                priority,
                statusId,
                isCompleted ?? existing.IsCompleted,
                estimateMinutes,
                start,
                end,
                link,
                taskTypeId,
                priorityId,
                customFieldValues);
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

    [HttpPost("{taskId:guid}/copy")]
    public IActionResult Copy(Guid taskId, TaskMoveRequest request)
    {
        var position = request.Position ?? ProjectManager.MovePosition.Inside;
        var task = _pm.CopyTask(taskId, request.TargetListId, request.TargetTaskId, position);
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
