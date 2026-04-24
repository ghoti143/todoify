using Microsoft.AspNetCore.Identity;
using Todoify.Api.Models;

namespace Todoify.Api.Data
{
    /// <summary>
    /// Provides database seeding functionality for development and demo environments.
    /// </summary>
    public static class DbSeeder
    {
        /// <summary>
        /// Seeds the database with a demo user and a set of sample tasks if they do not already exist.
        /// </summary>
        /// <param name="services">The application's <see cref="IServiceProvider"/> used to resolve dependencies.</param>
        public static async Task SeedAsync(IServiceProvider services)
        {
            UserManager<AppUser> userManager = services.GetRequiredService<UserManager<AppUser>>();
            AppDbContext db = services.GetRequiredService<AppDbContext>();

            const string seedEmail = "demo@mytodoifyapp.com";
            const string seedPassword = "demomytodoifyapp123";

            if (await userManager.FindByEmailAsync(seedEmail) is not null)
            {
                return;
            }

            var user = new AppUser
            {
                Email = seedEmail,
                UserName = seedEmail,
                DisplayName = "Demo User"
            };
            _ = await userManager.CreateAsync(user, seedPassword);

            var tasks = new List<TaskItem>
            {
                new() { Title = "Set up CI/CD pipeline", Description = "Configure GitHub Actions for build and deploy.", Priority = Priority.High, DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(3)), UserId = user.Id },
                new() { Title = "Write API documentation", Description = "Document all endpoints using Swagger annotations.", Priority = Priority.Medium, DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)), UserId = user.Id },
                new() { Title = "Add unit tests", Priority = Priority.Medium, UserId = user.Id },
                new() { Title = "Review pull requests", Priority = Priority.Low, IsComplete = true, UserId = user.Id },
                new() { Title = "Update dependencies", Description = "Check for outdated NuGet and npm packages.", Priority = Priority.Low, DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(14)), UserId = user.Id },
            };

            db.Tasks.AddRange(tasks);
            _ = await db.SaveChangesAsync();
        }
    }
}
