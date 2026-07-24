using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlightPlan.Infrastructure.Services;
using FlightPlan.Models;
using Microsoft.AspNetCore.Mvc;

namespace FlightPlan.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScheduledTasksController : ControllerBase
{
    private readonly ScheduledTaskService _scheduledTaskService;

    public ScheduledTasksController(ScheduledTaskService scheduledTaskService)
    {
        _scheduledTaskService = scheduledTaskService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_scheduledTaskService.GetAllTasks());
    }

    [HttpGet("{id}")]
    public IActionResult GetById(Guid id)
    {
        var task = _scheduledTaskService.GetTaskById(id);
        if (task == null) return NotFound();
        return Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ScheduledTask task)
    {
        await _scheduledTaskService.AddTaskAsync(task);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ScheduledTask task)
    {
        if (id != task.Id) return BadRequest();
        await _scheduledTaskService.UpdateTaskAsync(task);
        return Ok(task);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _scheduledTaskService.DeleteTaskAsync(id);
        return NoContent();
    }
}
