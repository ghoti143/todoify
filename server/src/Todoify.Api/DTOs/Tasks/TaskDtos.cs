using FluentValidation;
using Todoify.Api.Models;

namespace Todoify.Api.DTOs.Tasks;

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

// --- Validators ---

public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must be 200 characters or fewer.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must be 2000 characters or fewer.")
            .When(x => x.Description is not null);

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Due date must be today or in the future.")
            .When(x => x.DueDate.HasValue);
    }
}

public class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must be 200 characters or fewer.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must be 2000 characters or fewer.")
            .When(x => x.Description is not null);

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Due date must be today or in the future.")
            .When(x => x.DueDate.HasValue);
    }
}