namespace Api.Models;

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Title { get; set; }
    public string? Description { get; set; }
    public bool IsComplete { get; set; } = false;
    public Priority Priority { get; set; } = Priority.Medium;
    public DateOnly? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Ownership — always set from JWT claim, never from request body
    public required string UserId { get; set; }
    public AppUser User { get; set; } = null!;
}

public enum Priority { Low, Medium, High }