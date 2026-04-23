using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todoify.Api.DTOs.Tasks;
using Todoify.Api.Models;
using Todoify.Api.Services;

namespace Todoify.Api.Controllers;

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
        [FromQuery] string? sortBy,
        CancellationToken ct)
    {
        var tasks = await taskService.GetAllAsync(UserId, isComplete, priority, sortBy, ct);
        return Ok(tasks);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var task = await taskService.GetByIdAsync(id, UserId, ct);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskRequest request, CancellationToken ct)
    {
        var task = await taskService.CreateAsync(request, UserId, ct);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateTaskRequest request, CancellationToken ct)
    {
        var task = await taskService.UpdateAsync(id, request, UserId, ct);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> ToggleComplete(Guid id, CancellationToken ct)
    {
        var task = await taskService.ToggleCompleteAsync(id, UserId, ct);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await taskService.DeleteAsync(id, UserId, ct);
        return deleted ? NoContent() : NotFound();
    }
}