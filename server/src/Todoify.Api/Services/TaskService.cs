using Microsoft.EntityFrameworkCore;
using Todoify.Api.Data;
using Todoify.Api.DTOs.Tasks;
using Todoify.Api.Models;

namespace Todoify.Api.Services;

public interface ITaskService
{
    Task<List<TaskResponse>> GetAllAsync(
        string userId, bool? isComplete, Priority? priority, string? sortBy, CancellationToken ct = default);
    Task<TaskResponse?> GetByIdAsync(Guid id, string userId, CancellationToken ct = default);
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, string userId, CancellationToken ct = default);
    Task<TaskResponse?> UpdateAsync(Guid id, UpdateTaskRequest request, string userId, CancellationToken ct = default);
    Task<TaskResponse?> ToggleCompleteAsync(Guid id, string userId, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, string userId, CancellationToken ct = default);
}

public class TaskService(AppDbContext db) : ITaskService
{
    public async Task<List<TaskResponse>> GetAllAsync(
        string userId, bool? isComplete, Priority? priority, string? sortBy, CancellationToken ct = default)
    {
        var query = db.Tasks.Where(t => t.UserId == userId);

        if (isComplete.HasValue)
            query = query.Where(t => t.IsComplete == isComplete.Value);

        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority.Value);

        query = sortBy switch
        {
            "dueDate" => query.OrderBy(t => t.DueDate),
            "priority" => query.OrderBy(t => t.Priority),
            "title" => query.OrderBy(t => t.Title),
            _ => query.OrderByDescending(t => t.CreatedAt)
        };

        return await query.Select(t => t.ToResponse()).ToListAsync(ct);
    }

    public async Task<TaskResponse?> GetByIdAsync(Guid id, string userId, CancellationToken ct = default)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(
            t => t.Id == id && t.UserId == userId, ct);
        return task?.ToResponse();
    }

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
        db.Tasks.Add(task);
        await db.SaveChangesAsync(ct);
        return task.ToResponse();
    }

    public async Task<TaskResponse?> UpdateAsync(
        Guid id, UpdateTaskRequest request, string userId, CancellationToken ct = default)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(
            t => t.Id == id && t.UserId == userId);
        if (task is null) return null;

        task.Title = request.Title;
        task.Description = request.Description;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return task.ToResponse();
    }

    public async Task<TaskResponse?> ToggleCompleteAsync(Guid id, string userId, CancellationToken ct = default)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(
            t => t.Id == id && t.UserId == userId);
        if (task is null) return null;

        task.IsComplete = !task.IsComplete;
        task.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return task.ToResponse();
    }

    public async Task<bool> DeleteAsync(Guid id, string userId, CancellationToken ct = default)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);
        if (task is null) return false;
        db.Tasks.Remove(task);
        await db.SaveChangesAsync(ct);
        return true;
    }
}

// Extension to keep mapping in one place
public static class TaskMappingExtensions
{
    public static TaskResponse ToResponse(this TaskItem t) => new(
        t.Id, t.Title, t.Description, t.IsComplete,
        t.Priority, t.DueDate, t.CreatedAt, t.UpdatedAt);
}