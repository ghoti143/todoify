using FluentValidation;
using Todoify.Api.Models;

namespace Todoify.Api.DTOs.Tasks
{
    /// <summary>
    /// Represents the request body for creating a new task.
    /// </summary>
    /// <param name="Title">The task title. Required, maximum 200 characters.</param>
    /// <param name="Description">An optional description of the task. Maximum 2000 characters.</param>
    /// <param name="Priority">The priority level of the task.</param>
    /// <param name="DueDate">An optional due date for the task. Must be today or in the future.</param>
    public record CreateTaskRequest(
        string Title,
        string? Description,
        Priority Priority,
        DateOnly? DueDate
    );

    /// <summary>
    /// Represents the request body for updating an existing task.
    /// </summary>
    /// <param name="Title">The updated task title. Required, maximum 200 characters.</param>
    /// <param name="Description">An optional updated description. Maximum 2000 characters.</param>
    /// <param name="Priority">The updated priority level of the task.</param>
    /// <param name="DueDate">An optional updated due date. Must be today or in the future.</param>
    public record UpdateTaskRequest(
        string Title,
        string? Description,
        Priority Priority,
        DateOnly? DueDate
    );

    /// <summary>
    /// Represents the response body returned for a task.
    /// </summary>
    /// <param name="Id">The unique identifier of the task.</param>
    /// <param name="Title">The task title.</param>
    /// <param name="Description">The optional task description.</param>
    /// <param name="IsComplete">Indicates whether the task has been marked as complete.</param>
    /// <param name="Priority">The priority level of the task.</param>
    /// <param name="DueDate">The optional due date of the task.</param>
    /// <param name="CreatedAt">The UTC date and time when the task was created.</param>
    /// <param name="UpdatedAt">The UTC date and time when the task was last updated.</param>
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

    /// <summary>
    /// FluentValidation validator for <see cref="CreateTaskRequest"/>.
    /// </summary>
    public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CreateTaskRequestValidator"/> with validation rules.
        /// </summary>
        public CreateTaskRequestValidator()
        {
            _ = RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title must be 200 characters or fewer.");

            _ = RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description must be 2000 characters or fewer.")
                .When(x => x.Description is not null);

            _ = RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Due date must be today or in the future.")
                .When(x => x.DueDate.HasValue);
        }
    }

    /// <summary>
    /// FluentValidation validator for <see cref="UpdateTaskRequest"/>.
    /// </summary>
    public class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="UpdateTaskRequestValidator"/> with validation rules.
        /// </summary>
        public UpdateTaskRequestValidator()
        {
            _ = RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title must be 200 characters or fewer.");

            _ = RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description must be 2000 characters or fewer.")
                .When(x => x.Description is not null);

            _ = RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Due date must be today or in the future.")
                .When(x => x.DueDate.HasValue);
        }
    }
}
