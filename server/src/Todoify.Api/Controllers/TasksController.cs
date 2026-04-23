using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Todoify.Api.DTOs.Tasks;
using Todoify.Api.Models;
using Todoify.Api.Services;

namespace Todoify.Api.Controllers
{
    /// <summary>
    /// Handles CRUD operations for tasks belonging to the authenticated user.
    /// </summary>
    [ApiController]
    [Route("api/tasks")]
    [Authorize]
    public class TasksController(ITaskService taskService) : ControllerBase
    {
        private string UserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /// <summary>
        /// Retrieves all tasks for the authenticated user, with optional filtering and sorting.
        /// </summary>
        /// <param name="isComplete">When provided, filters tasks by completion status.</param>
        /// <param name="priority">When provided, filters tasks by priority level.</param>
        /// <param name="sortBy">The field name to sort results by.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns><see cref="OkObjectResult"/> containing a list of <see cref="TaskResponse"/>.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] bool? isComplete,
            [FromQuery] Priority? priority,
            [FromQuery] string? sortBy,
            CancellationToken ct)
        {
            List<TaskResponse> tasks = await taskService.GetAllAsync(UserId, isComplete, priority, sortBy, ct);
            return Ok(tasks);
        }

        /// <summary>
        /// Retrieves a single task by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the task.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// <see cref="OkObjectResult"/> with the <see cref="TaskResponse"/> if found, or
        /// <see cref="NotFoundResult"/> if no matching task exists for the authenticated user.
        /// </returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            TaskResponse? task = await taskService.GetByIdAsync(id, UserId, ct);
            return task is null ? NotFound() : Ok(task);
        }

        /// <summary>
        /// Creates a new task for the authenticated user.
        /// </summary>
        /// <param name="request">The task creation details.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns><see cref="CreatedAtActionResult"/> containing the newly created <see cref="TaskResponse"/>.</returns>
        [HttpPost]
        public async Task<IActionResult> Create(CreateTaskRequest request, CancellationToken ct)
        {
            TaskResponse task = await taskService.CreateAsync(request, UserId, ct);
            return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
        }

        /// <summary>
        /// Updates an existing task by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the task to update.</param>
        /// <param name="request">The updated task details.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// <see cref="OkObjectResult"/> with the updated <see cref="TaskResponse"/> if found, or
        /// <see cref="NotFoundResult"/> if no matching task exists for the authenticated user.
        /// </returns>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateTaskRequest request, CancellationToken ct)
        {
            TaskResponse? task = await taskService.UpdateAsync(id, request, UserId, ct);
            return task is null ? NotFound() : Ok(task);
        }

        /// <summary>
        /// Toggles the completion status of a task.
        /// </summary>
        /// <param name="id">The unique identifier of the task to toggle.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// <see cref="OkObjectResult"/> with the updated <see cref="TaskResponse"/> if found, or
        /// <see cref="NotFoundResult"/> if no matching task exists for the authenticated user.
        /// </returns>
        [HttpPatch("{id:guid}/complete")]
        public async Task<IActionResult> ToggleComplete(Guid id, CancellationToken ct)
        {
            TaskResponse? task = await taskService.ToggleCompleteAsync(id, UserId, ct);
            return task is null ? NotFound() : Ok(task);
        }

        /// <summary>
        /// Deletes a task by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the task to delete.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// <see cref="NoContentResult"/> on successful deletion, or
        /// <see cref="NotFoundResult"/> if no matching task exists for the authenticated user.
        /// </returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            bool deleted = await taskService.DeleteAsync(id, UserId, ct);
            return deleted ? NoContent() : NotFound();
        }
    }
}
