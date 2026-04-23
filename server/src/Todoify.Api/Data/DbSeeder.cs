using Microsoft.AspNetCore.Identity;
using Todoify.Api.Models;

namespace Todoify.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var db = services.GetRequiredService<AppDbContext>();

        const string seedEmail = "demo@example.com";
        const string seedPassword = "demo123";

        if (await userManager.FindByEmailAsync(seedEmail) is not null) return;

        var user = new AppUser
        {
            Email = seedEmail,
            UserName = seedEmail,
            DisplayName = "Demo User"
        };
        await userManager.CreateAsync(user, seedPassword);

        var tasks = new List<TaskItem>
        {
            new() { Title = "Set up CI/CD pipeline", Description = "Configure GitHub Actions for build and deploy.", Priority = Priority.High, DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(3)), UserId = user.Id },
            new() { Title = "Write API documentation", Description = "Document all endpoints using Swagger annotations.", Priority = Priority.Medium, DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)), UserId = user.Id },
            new() { Title = "Add unit tests", Priority = Priority.Medium, UserId = user.Id },
            new() { Title = "Review pull requests", Priority = Priority.Low, IsComplete = true, UserId = user.Id },
            new() { Title = "Update dependencies", Description = "Check for outdated NuGet and npm packages.", Priority = Priority.Low, DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(14)), UserId = user.Id },
        };

        db.Tasks.AddRange(tasks);
        await db.SaveChangesAsync();
    }
}