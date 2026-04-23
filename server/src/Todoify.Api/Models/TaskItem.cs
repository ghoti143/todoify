namespace Todoify.Api.Models
{
    /// <summary>
    /// Represents a task item belonging to a user.
    /// </summary>
    public class TaskItem
    {
        /// <summary>
        /// Gets or sets the unique identifier for the task.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the title of the task.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Gets or sets an optional description providing additional detail about the task.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the task has been completed.
        /// </summary>
        public bool IsComplete { get; set; } = false;

        /// <summary>
        /// Gets or sets the priority level of the task. Defaults to <see cref="Priority.Medium"/>.
        /// </summary>
        public Priority Priority { get; set; } = Priority.Medium;

        /// <summary>
        /// Gets or sets the optional due date for the task.
        /// </summary>
        public DateOnly? DueDate { get; set; }

        /// <summary>
        /// Gets or sets the UTC date and time when the task was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the UTC date and time when the task was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the ID of the user who owns this task.
        /// Always sourced from the JWT claim — never from the request body.
        /// </summary>
        public required string UserId { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the owning <see cref="AppUser"/>.
        /// </summary>
        public AppUser User { get; set; } = null!;
    }

    /// <summary>
    /// Represents the priority level of a <see cref="TaskItem"/>.
    /// </summary>
    public enum Priority
    {
        /// <summary>Low priority.</summary>
        Low,

        /// <summary>Medium priority.</summary>
        Medium,

        /// <summary>High priority.</summary>
        High
    }
}
