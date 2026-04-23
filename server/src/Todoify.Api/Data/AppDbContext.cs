using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Todoify.Api.Models;

namespace Todoify.Api.Data
{
    /// <summary>
    /// The Entity Framework Core database context for the Todoify application.
    /// Extends <see cref="IdentityDbContext{TUser}"/> to include ASP.NET Core Identity tables.
    /// </summary>
    public class AppDbContext(DbContextOptions<AppDbContext> options)
        : IdentityDbContext<AppUser>(options)
    {
        /// <summary>
        /// Gets the <see cref="DbSet{TEntity}"/> of <see cref="TaskItem"/> entities.
        /// </summary>
        public DbSet<TaskItem> Tasks => Set<TaskItem>();

        /// <summary>
        /// Configures the entity model and relationships for the database schema.
        /// </summary>
        /// <param name="builder">The <see cref="ModelBuilder"/> used to construct the model.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            _ = builder.Entity<TaskItem>(e =>
            {
                _ = e.HasKey(t => t.Id);
                _ = e.Property(t => t.Title).IsRequired().HasMaxLength(200);
                _ = e.Property(t => t.Description).HasMaxLength(2000);
                _ = e.Property(t => t.Priority).HasConversion<string>();
                _ = e.HasOne(t => t.User)
                 .WithMany(u => u.Tasks)
                 .HasForeignKey(t => t.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
