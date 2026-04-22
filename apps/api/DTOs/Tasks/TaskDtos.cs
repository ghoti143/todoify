using Api.Models;

namespace Api.DTOs.Tasks;

public record CreateTaskRequest(
    string Title,
    string? Description,
    Priority Priority,
    DateOnly? DueDate
);

public record UpdateTaskRequest(
    string Title,
    string? Description,
    Priority Priority,
    DateOnly? DueDate
);

public record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    bool IsComplete,
    Priority Priority,
    DateOnly? DueDate,
    DateTime CreatedAt,
    DateTime UpdatedAt
);