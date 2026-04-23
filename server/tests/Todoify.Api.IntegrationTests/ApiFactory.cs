using Todoify.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Http.Json;

namespace Todoify.Api.IntegrationTests
{
    public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        // Keep one open connection for the lifetime of the factory.
        // SQLite drops an in-memory DB the moment its last connection closes.
        private readonly SqliteConnection _connection = new("DataSource=:memory:");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            _ = builder.UseEnvironment("Test");

            _ = builder.ConfigureServices(services =>
            {
                // Remove every EF/DbContext descriptor the real app registered
                _ = services.RemoveAll<AppDbContext>();
                _ = services.RemoveAll<DbContextOptions<AppDbContext>>();
                _ = services.RemoveAll<DbContextOptions>();

                // Re-register using the SAME open SQLite connection.
                // Because it's the same provider (SQLite), no dual-provider conflict.
                _ = services.AddDbContext<AppDbContext>(o =>
                    o.UseSqlite(_connection));

                _ = services.PostConfigure<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>(
                    Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
                    opts =>
                    {
                        opts.TokenValidationParameters.ValidIssuer = "test-issuer";
                        opts.TokenValidationParameters.ValidAudience = "test-audience";
                    });
            });

            _ = builder.UseSetting("Jwt:Key", "super-secret-test-key-that-is-long-enough-32chars");
            _ = builder.UseSetting("Jwt:Issuer", "test-issuer");
            _ = builder.UseSetting("Jwt:Audience", "test-audience");
            _ = builder.UseSetting("Jwt:ExpiryHours", "1");
        }

        public async Task InitializeAsync()
        {
            // Open the connection first — EnsureCreated keeps it alive for all tests
            await _connection.OpenAsync();

            using IServiceScope scope = Services.CreateScope();
            AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            _ = await db.Database.EnsureCreatedAsync();
        }

        public new async Task DisposeAsync()
        {
            await _connection.DisposeAsync(); // closing drops the in-memory SQLite DB
            await base.DisposeAsync();
        }

        public async Task<string> GetTokenAsync(
            HttpClient client,
            string email = "test@example.com",
            string password = "password123",
            string? name = "Test User")
        {
            HttpResponseMessage register = await client.PostAsJsonAsync("/api/auth/register",
                new { email, password, displayName = name });

            if (!register.IsSuccessStatusCode)
            {
                HttpResponseMessage login = await client.PostAsJsonAsync("/api/auth/login",
                    new { email, password });
                _ = login.EnsureSuccessStatusCode();
                AuthTokenResult? auth = await login.Content.ReadFromJsonAsync<AuthTokenResult>();
                return auth!.Token;
            }

            AuthTokenResult? result = await register.Content.ReadFromJsonAsync<AuthTokenResult>();
            return result!.Token;
        }

        private record AuthTokenResult(string Token);
    }
}
