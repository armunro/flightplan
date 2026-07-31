using Microsoft.AspNetCore.Mvc;
using FlightPlan.Models;
using FlightPlan.Services;

namespace FlightPlan.Controllers;

public record ListCreateRequest(string Name);
public record ListUpdateRequest(string Name);
public record ListMoveRequest(Guid? TargetListId, ProjectManager.MovePosition? Position);
public record TaskStatusDto(Guid? Id, string Name, string Color, bool IsCompletedState, int Order);
public record TaskTypeDto(Guid? Id, string Name, string Color, string Icon);
public record ProjectPriorityDto(Guid? Id, string Name, string Color, string Icon, int Order);
public record CustomFieldDefinitionDto(Guid? Id, string Name, int Type, List<string>? Options);

public record ProjectCreateRequest(string Name, string? Description, string? Icon, string? Color, List<TaskStatusDto>? Statuses = null, List<TaskTypeDto>? TaskTypes = null, List<ProjectPriorityDto>? Priorities = null, List<CustomFieldDefinitionDto>? CustomFields = null);
public record ProjectUpdateRequest(string Name, string? Description, string? Icon, string? Color, List<TaskStatusDto>? Statuses = null, List<TaskTypeDto>? TaskTypes = null, List<ProjectPriorityDto>? Priorities = null, List<CustomFieldDefinitionDto>? CustomFields = null);
public record ProjectMoveRequest(Guid? TargetProjectId, ProjectManager.MovePosition? Position);

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly ProjectManager _projectManager;
    private readonly IStorageService _storageService;

    public ProjectsController(ProjectManager projectManager, IStorageService storageService)
    {
        _projectManager = projectManager;
        _storageService = storageService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_projectManager.GetAllProjects());
    }

    [HttpPost]
    public IActionResult CreateProject(ProjectCreateRequest request)
    {
        var statuses = request.Statuses?.Select(s => new FlightPlan.Models.TaskStatus 
        { 
            Id = s.Id ?? Guid.NewGuid(), 
            Name = s.Name, 
            Color = s.Color, 
            IsCompletedState = s.IsCompletedState, 
            Order = s.Order 
        }).ToList();

        var taskTypes = request.TaskTypes?.Select(t => new TaskType 
        { 
            Id = t.Id ?? Guid.NewGuid(), 
            Name = t.Name, 
            Color = t.Color, 
            Icon = t.Icon 
        }).ToList();

        var priorities = request.Priorities?.Select(p => new ProjectPriority
        {
            Id = p.Id ?? Guid.NewGuid(),
            Name = p.Name,
            Color = p.Color,
            Icon = p.Icon,
            Order = p.Order
        }).ToList();

        var customFields = request.CustomFields?.Select(f => new CustomFieldDefinition
        {
            Id = f.Id ?? Guid.NewGuid(),
            Name = f.Name,
            Type = (CustomFieldType)f.Type,
            Options = f.Options ?? new List<string>()
        }).ToList();

        var project = _projectManager.CreateProject(request.Name, request.Description, request.Icon, request.Color, statuses, taskTypes, priorities, customFields);
        _projectManager.SaveProjectsToYaml(_storageService.GetProjectsPath());
        return Ok(project);
    }

    [HttpPut("{projectId:guid}")]
    public IActionResult UpdateProject(Guid projectId, ProjectUpdateRequest request)
    {
        var statuses = request.Statuses?.Select(s => new FlightPlan.Models.TaskStatus 
        { 
            Id = s.Id ?? Guid.NewGuid(), 
            Name = s.Name, 
            Color = s.Color, 
            IsCompletedState = s.IsCompletedState, 
            Order = s.Order 
        }).ToList();

        var taskTypes = request.TaskTypes?.Select(t => new TaskType 
        { 
            Id = t.Id ?? Guid.NewGuid(), 
            Name = t.Name, 
            Color = t.Color, 
            Icon = t.Icon 
        }).ToList();

        var priorities = request.Priorities?.Select(p => new ProjectPriority
        {
            Id = p.Id ?? Guid.NewGuid(),
            Name = p.Name,
            Color = p.Color,
            Icon = p.Icon,
            Order = p.Order
        }).ToList();

        var customFields = request.CustomFields?.Select(f => new CustomFieldDefinition
        {
            Id = f.Id ?? Guid.NewGuid(),
            Name = f.Name,
            Type = (CustomFieldType)f.Type,
            Options = f.Options ?? new List<string>()
        }).ToList();

        var project = _projectManager.UpdateProject(projectId, request.Name, request.Description, request.Icon, request.Color, statuses, taskTypes, priorities, customFields);
        if (project == null) return NotFound("Project not found");

        _projectManager.SaveProjectsToYaml(_storageService.GetProjectsPath());
        return Ok(project);
    }

    [HttpPost("{projectId:guid}/move")]
    public IActionResult MoveProject(Guid projectId, ProjectMoveRequest request)
    {
        var position = request.Position ?? ProjectManager.MovePosition.After;
        var project = _projectManager.MoveProject(projectId, request.TargetProjectId, position);
        if (project == null) return NotFound("Project not found");

        _projectManager.SaveProjectsToYaml(_storageService.GetProjectsPath());
        return Ok(project);
    }

    [HttpPost("{projectId:guid}/lists")]
    public IActionResult CreateList(Guid projectId, ListCreateRequest request)
    {
        var project = _projectManager.FindProjectById(projectId);
        if (project == null) return NotFound("Project not found");

        var list = _projectManager.AddListToProject(project, request.Name);
        _projectManager.SaveProjectsToYaml(_storageService.GetProjectsPath());
        return Ok(list);
    }

    [HttpPut("{projectId:guid}/lists/{listId:guid}")]
    public IActionResult UpdateList(Guid projectId, Guid listId, ListUpdateRequest request)
    {
        var list = _projectManager.UpdateList(projectId, listId, request.Name);
        if (list == null) return NotFound("Project or List not found");

        _projectManager.SaveProjectsToYaml(_storageService.GetProjectsPath());
        return Ok(list);
    }

    [HttpPost("{projectId:guid}/lists/{listId:guid}/move")]
    public IActionResult MoveList(Guid projectId, Guid listId, ListMoveRequest request)
    {
        var position = request.Position ?? ProjectManager.MovePosition.After;
        var list = _projectManager.MoveList(projectId, listId, request.TargetListId, position);
        if (list == null) return NotFound("Project or List not found");

        _projectManager.SaveProjectsToYaml(_storageService.GetProjectsPath());
        return Ok(list);
    }

    [HttpDelete("{projectId:guid}/lists/{listId:guid}")]
    public IActionResult DeleteList(Guid projectId, Guid listId)
    {
        if (!_projectManager.DeleteList(projectId, listId))
            return NotFound("Project or List not found");

        _projectManager.SaveProjectsToYaml(_storageService.GetProjectsPath());
        return NoContent();
    }
    [HttpDelete("{projectId:guid}")]
    public IActionResult DeleteProject(Guid projectId)
    {
        if (!_projectManager.DeleteProject(projectId))
            return NotFound("Project not found");

        _projectManager.SaveProjectsToYaml(_storageService.GetProjectsPath());
        return NoContent();
    }
}
