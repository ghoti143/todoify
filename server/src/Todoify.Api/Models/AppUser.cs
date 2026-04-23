using Microsoft.AspNetCore.Identity;

namespace Todoify.Api.Models
{
    /// <summary>
    /// Represents an application user, extending the default ASP.NET Core Identity user
    /// with Todoify-specific properties.
    /// </summary>
    public class AppUser : IdentityUser
    {
        /// <summary>
        /// Gets or sets an optional display name for the user.
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the collection of tasks owned by this user.
        /// </summary>
        public ICollection<TaskItem> Tasks { get; set; } = [];
    }
}
