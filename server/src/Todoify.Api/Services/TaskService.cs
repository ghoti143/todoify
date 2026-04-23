using Microsoft.EntityFrameworkCore;
using Todoify.Api.Data;
using Todoify.Api.DTOs.Tasks;
using Todoify.Api.Models;

namespace Todoify.Api.Services
{
    /// <summary>
    /// Defines task management operations scoped to individual users.
    /// </summary>
    public interface ITaskService
    {
        /// <summary>
        /// Retrieves all tasks for a user with optional filtering and sorting.
        /// </summary>
        /// <param name="userId">The ID of the authenticated user.</param>
        /// <param name="isComplete">When provided, filters by completion status.</param>
        /// <param name="priority">When provided, filters by priority level.</param>
        /// <param name="sortBy">The field to sort by: <c>dueDate</c>, <c>priority</c>, or <c>title</c>. Defaults to creation date descending.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of <see cref="TaskResponse"/> matching the filters.</returns>
        Task<List<TaskResponse>> GetAllAsync(
            string userId, bool? isComplete, Priority? priority, string? sortBy, CancellationToken ct = default);

        /// <summary>
        /// Retrieves a single task by ID, scoped to the authenticated user.
        /// </summary>
        /// <param name="id">The unique identifier of the task.</param>
        /// <param name="userId">The ID of the authenticated user.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The <see cref="TaskResponse"/> if found, or <c>null</c> if no matching task exists.</returns>
        Task<TaskResponse?> GetByIdAsync(Guid id, string userId, CancellationToken ct = default);

        /// <summary>
        /// Creates a new task for the authenticated user.
        /// </summary>
        /// <param name="request">The task creation details.</param>
        /// <param name="userId">The ID of the authenticated user.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The <see cref="TaskResponse"/> for the newly created task.</returns>
        Task<TaskResponse> CreateAsync(CreateTaskRequest request, string userId, CancellationToken ct = default);

        /// <summary>
        /// Updates an existing task by ID, scoped to the authenticated user.
        /// </summary>
        /// <param name="id">The unique identifier of the task to update.</param>
        /// <param name="request">The updated task details.</param>
        /// <param name="userId">The ID of the authenticated user.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated <see cref="TaskResponse"/>, or <c>null</c> if no matching task exists.</returns>
        Task<TaskResponse?> UpdateAsync(Guid id, UpdateTaskRequest request, string userId, CancellationToken ct = default);

        /// <summary>
        /// Toggles the completion status of a task, scoped to the authenticated user.
        /// </summary>
        /// <param name="id">The unique identifier of the task to toggle.</param>
        /// <param name="userId">The ID of the authenticated user.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The updated <see cref="TaskResponse"/>, or <c>null</c> if no matching task exists.</returns>
        Task<TaskResponse?> ToggleCompleteAsync(Guid id, string userId, CancellationToken ct = default);

        /// <summary>
        /// Deletes a task by ID, scoped to the authenticated user.
        /// </summary>
        /// <param name="id">The unique identifier of the task to delete.</param>
        /// <param name="userId">The ID of the authenticated user.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns><c>true</c> if the task was deleted, or <c>false</c> if no matching task exists.</returns>
        Task<bool> DeleteAsync(Guid id, string userId, CancellationToken ct = default);
    }

    /// <summary>
    /// Provides task management operations backed by <see cref="AppDbContext"/>.
    /// </summary>
    public class TaskService(AppDbContext db) : ITaskService
    {
        /// <inheritdoc/>
        public async Task<List<TaskResponse>> GetAllAsync(
            string userId, bool? isComplete, Priority? priority, string? sortBy, CancellationToken ct = default)
        {
            IQueryable<TaskItem> query = db.Tasks.Where(t => t.UserId == userId);

            if (isComplete.HasValue)
            {
                query = query.Where(t => t.IsComplete == isComplete.Value);
            }

            if (priority.HasValue)
            {
                query = query.Where(t => t.Priority == priority.Value);
            }

            query = sortBy switch
            {
                "dueDate" => query.OrderBy(t => t.DueDate),
                "priority" => query.OrderBy(t => t.Priority),
                "title" => query.OrderBy(t => t.Title),
                _ => query.OrderByDescending(t => t.CreatedAt)
            };

            return await query.Select(t => t.ToResponse()).ToListAsync(ct);
        }

        /// <inheritdoc/>
        public async Task<TaskResponse?> GetByIdAsync(Guid id, string userId, CancellationToken ct = default)
        {
            TaskItem? task = await db.Tasks.FirstOrDefaultAsync(
                t => t.Id == id && t.UserId == userId, ct);
            return task?.ToResponse();
        }

        /// <inheritdoc/>
        public async Task<TaskResponse> CreateAsync(
            CreateTaskRequest request, string userId, CancellationToken ct = default)
        {
            var task = new TaskItem
            {
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                DueDate = request.DueDate,
                UserId = userId
            };
            _ = db.Tasks.Add(task);
            _ = await db.SaveChangesAsync(ct);
            return task.ToResponse();
        }

        /// <inheritdoc/>
        public async Task<TaskResponse?> UpdateAsync(
            Guid id, UpdateTaskRequest request, string userId, CancellationToken ct = default)
        {
            TaskItem? task = await db.Tasks.FirstOrDefaultAsync(
                t => t.Id == id && t.UserId == userId);
            if (task is null)
            {
                return null;
            }

            task.Title = request.Title;
            task.Description = request.Description;
            task.Priority = request.Priority;
            task.DueDate = request.DueDate;
            task.UpdatedAt = DateTime.UtcNow;

            _ = await db.SaveChangesAsync(ct);
            return task.ToResponse();
        }

        /// <inheritdoc/>
        public async Task<TaskResponse?> ToggleCompleteAsync(Guid id, string userId, CancellationToken ct = default)
        {
            TaskItem? task = await db.Tasks.FirstOrDefaultAsync(
                t => t.Id == id && t.UserId == userId);
            if (task is null)
            {
                return null;
            }

            task.IsComplete = !task.IsComplete;
            task.UpdatedAt = DateTime.UtcNow;
            _ = await db.SaveChangesAsync(ct);
            return task.ToResponse();
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id, string userId, CancellationToken ct = default)
        {
            TaskItem? task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);
            if (task is null)
            {
                return false;
            }

            _ = db.Tasks.Remove(task);
            _ = await db.SaveChangesAsync(ct);
            return true;
        }
    }

    /// <summary>
    /// Provides mapping extension methods for <see cref="TaskItem"/>.
    /// </summary>
    public static class TaskMappingExtensions
    {
        /// <summary>
        /// Maps a <see cref="TaskItem"/> entity to a <see cref="TaskResponse"/> DTO.
        /// </summary>
        /// <param name="t">The task entity to map.</param>
        /// <returns>A <see cref="TaskResponse"/> representing the task.</returns>
        public static TaskResponse ToResponse(this TaskItem t) => new(
            t.Id, t.Title, t.Description, t.IsComplete,
            t.Priority, t.DueDate, t.CreatedAt, t.UpdatedAt);
    }
}
