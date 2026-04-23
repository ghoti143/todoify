using Microsoft.AspNetCore.Identity;

namespace Todoify.Api.Models;

public class AppUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = [];
}