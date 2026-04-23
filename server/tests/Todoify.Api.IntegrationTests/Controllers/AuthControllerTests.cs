using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Todoify.Api.IntegrationTests.Controllers
{
    public class AuthControllerTests(ApiFactory factory)
        : IClassFixture<ApiFactory>
    {
        private readonly HttpClient _client = factory.CreateClient();

        // -------------------------------------------------------------------------
        // POST /api/auth/register
        // -------------------------------------------------------------------------

        [Fact]
        public async Task Register_WithValidData_Returns200WithToken()
        {
            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/register", new
            {
                email = $"new_{Guid.NewGuid()}@test.com",
                password = "password123",
                displayName = "Alice"
            });

            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);

            AuthResponseBody? body = await response.Content.ReadFromJsonAsync<AuthResponseBody>();
            _ = body!.Token.Should().NotBeNullOrEmpty();
            _ = body.Email.Should().NotBeNullOrEmpty();
            _ = body.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        }

        [Fact]
        public async Task Register_DuplicateEmail_Returns400()
        {
            string email = $"dup_{Guid.NewGuid()}@test.com";

            _ = await _client.PostAsJsonAsync("/api/auth/register",
                new { email, password = "password123" });

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/register",
                new { email, password = "password123" });

            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_MissingEmail_Returns400()
        {
            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/register",
                new { password = "password123" });

            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // -------------------------------------------------------------------------
        // POST /api/auth/login
        // -------------------------------------------------------------------------

        [Fact]
        public async Task Login_WithValidCredentials_Returns200WithToken()
        {
            string email = $"login_{Guid.NewGuid()}@test.com";
            string password = "password123";

            _ = await _client.PostAsJsonAsync("/api/auth/register", new { email, password });

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/login",
                new { email, password });

            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
            AuthResponseBody? body = await response.Content.ReadFromJsonAsync<AuthResponseBody>();
            _ = body!.Token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_WithWrongPassword_Returns401()
        {
            string email = $"wrongpw_{Guid.NewGuid()}@test.com";

            _ = await _client.PostAsJsonAsync("/api/auth/register",
                new { email, password = "correctpassword" });

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/login",
                new { email, password = "wrongpassword" });

            _ = response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_WithUnknownEmail_Returns401()
        {
            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/login",
                new { email = "nobody@test.com", password = "whatever" });

            _ = response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // -------------------------------------------------------------------------
        // DTOs
        // -------------------------------------------------------------------------

        private record AuthResponseBody(string Token, string Email, string? DisplayName, DateTime ExpiresAt);
    }
}
