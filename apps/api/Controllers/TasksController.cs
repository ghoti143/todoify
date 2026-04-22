using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.DTOs.Tasks;
using Api.Models;
using Api.Services;

namespace Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController(ITaskService taskService) : ControllerBase
{
    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? isComplete,
        [FromQuery] Priority? priority,
        [FromQuery] string? sortBy)
    {
        var tasks = await taskService.GetAllAsync(UserId, isComplete, priority, sortBy);
        return Ok(tasks);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var task = await taskService.GetByIdAsync(id, UserId);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskRequest request)
    {
        var task = await taskService.CreateAsync(request, UserId);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateTaskRequest request)
    {
        var task = await taskService.UpdateAsync(id, request, UserId);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> ToggleComplete(Guid id)
    {
        var task = await taskService.ToggleCompleteAsync(id, UserId);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await taskService.DeleteAsync(id, UserId);
        return deleted ? NoContent() : NotFound();
    }
}