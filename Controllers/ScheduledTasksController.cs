using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlightPlan.Infrastructure.Services;
using FlightPlan.Core.Interfaces;
using FlightPlan.Models;
using FlightPlan.Models.Config;
using Microsoft.AspNetCore.Mvc;

namespace FlightPlan.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScheduledTasksController : ControllerBase
{
    private readonly IScheduledTaskService _scheduledTaskService;
    private readonly MockScheduledTaskService _mockScheduledTaskService;
    private readonly DashConfig _config;

    public ScheduledTasksController(
        IScheduledTaskService scheduledTaskService, 
        MockScheduledTaskService mockScheduledTaskService,
        DashConfig config)
    {
        _scheduledTaskService = scheduledTaskService;
        _mockScheduledTaskService = mockScheduledTaskService;
        _config = config;
    }

    private IScheduledTaskService CurrentService => _config.Debug.DemoMode ? _mockScheduledTaskService : _scheduledTaskService;

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(CurrentService.GetAllTasks());
    }

    [HttpGet("{id}")]
    public IActionResult GetById(Guid id)
    {
        var task = CurrentService.GetTaskById(id);
        if (task == null) return NotFound();
        return Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ScheduledTask task)
    {
        await CurrentService.AddTaskAsync(task);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ScheduledTask task)
    {
        if (id != task.Id) return BadRequest();
        await CurrentService.UpdateTaskAsync(task);
        return Ok(task);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await CurrentService.DeleteTaskAsync(id);
        return NoContent();
    }
}
