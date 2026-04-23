using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace Todoify.Api.IntegrationTests.Controllers
{
    /// <summary>
    /// Full end-to-end tests against the real HTTP pipeline.
    /// Each test that needs auth calls GetAuthenticatedClient() which
    /// registers a fresh user so tests are fully isolated.
    /// </summary>
    public class TasksControllerTests(ApiFactory factory)
        : IClassFixture<ApiFactory>
    {
        // -------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------

        private async Task<HttpClient> GetAuthenticatedClient(string? userSuffix = null)
        {
            HttpClient client = factory.CreateClient();
            string email = $"user{userSuffix ?? Guid.NewGuid().ToString()}@test.com";
            string token = await factory.GetTokenAsync(client, email);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private static async Task<TaskBody> CreateTaskAsync(HttpClient client,
            string title = "My Task", string? description = null,
            string priority = "Medium", string? dueDate = null)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync("/api/tasks", new
            {
                title,
                description,
                priority,
                dueDate
            });
            _ = response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<TaskBody>())!;
        }

        // -------------------------------------------------------------------------
        // Authentication guard
        // -------------------------------------------------------------------------

        [Fact]
        public async Task GetAll_WithoutToken_Returns401()
        {
            HttpClient client = factory.CreateClient();
            HttpResponseMessage response = await client.GetAsync("/api/tasks");
            _ = response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // -------------------------------------------------------------------------
        // GET /api/tasks
        // -------------------------------------------------------------------------

        [Fact]
        public async Task GetAll_AuthenticatedUser_ReturnsOwnTasksOnly()
        {
            HttpClient client1 = await GetAuthenticatedClient();
            HttpClient client2 = await GetAuthenticatedClient();

            _ = await CreateTaskAsync(client1, "User1 Task");
            _ = await CreateTaskAsync(client2, "User2 Task");

            HttpResponseMessage response = await client1.GetAsync("/api/tasks");
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);

            List<TaskBody>? tasks = await response.Content.ReadFromJsonAsync<List<TaskBody>>();
            _ = tasks.Should().HaveCount(1);
            _ = tasks[0].Title.Should().Be("User1 Task");
        }

        [Fact]
        public async Task GetAll_FilterByIsComplete_ReturnsMatchingTasks()
        {
            HttpClient client = await GetAuthenticatedClient();
            TaskBody task = await CreateTaskAsync(client, "Task A");

            // Toggle it complete
            _ = await client.PatchAsync($"/api/tasks/{task.Id}/complete", null);

            _ = await CreateTaskAsync(client, "Task B"); // stays incomplete

            HttpResponseMessage response = await client.GetAsync("/api/tasks?isComplete=true");
            List<TaskBody>? tasks = await response.Content.ReadFromJsonAsync<List<TaskBody>>();

            _ = tasks.Should().HaveCount(1);
            _ = tasks[0].Title.Should().Be("Task A");
        }

        [Fact]
        public async Task GetAll_FilterByPriority_ReturnsMatchingTasks()
        {
            HttpClient client = await GetAuthenticatedClient();
            _ = await CreateTaskAsync(client, "High Task", priority: "High");
            _ = await CreateTaskAsync(client, "Low Task", priority: "Low");

            HttpResponseMessage response = await client.GetAsync("/api/tasks?priority=High");
            List<TaskBody>? tasks = await response.Content.ReadFromJsonAsync<List<TaskBody>>();

            _ = tasks.Should().HaveCount(1);
            _ = tasks[0].Title.Should().Be("High Task");
        }

        [Theory]
        [InlineData("title")]
        [InlineData("priority")]
        [InlineData("duedate")]
        [InlineData("createdAt")]
        public async Task GetAll_WithSortBy_Returns200(string sortBy)
        {
            HttpClient client = await GetAuthenticatedClient();
            _ = await CreateTaskAsync(client, "A");
            _ = await CreateTaskAsync(client, "B");

            HttpResponseMessage response = await client.GetAsync($"/api/tasks?sortBy={sortBy}");
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // -------------------------------------------------------------------------
        // GET /api/tasks/{id}
        // -------------------------------------------------------------------------

        [Fact]
        public async Task GetById_ExistingTask_Returns200WithTask()
        {
            HttpClient client = await GetAuthenticatedClient();
            TaskBody created = await CreateTaskAsync(client, "Findable Task");

            HttpResponseMessage response = await client.GetAsync($"/api/tasks/{created.Id}");
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);

            TaskBody? task = await response.Content.ReadFromJsonAsync<TaskBody>();
            _ = task!.Title.Should().Be("Findable Task");
        }

        [Fact]
        public async Task GetById_OtherUserTask_Returns404()
        {
            HttpClient client1 = await GetAuthenticatedClient();
            HttpClient client2 = await GetAuthenticatedClient();

            TaskBody task = await CreateTaskAsync(client1, "Private Task");
            HttpResponseMessage response = await client2.GetAsync($"/api/tasks/{task.Id}");

            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetById_NonExistentId_Returns404()
        {
            HttpClient client = await GetAuthenticatedClient();
            HttpResponseMessage response = await client.GetAsync($"/api/tasks/{Guid.NewGuid()}");
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // -------------------------------------------------------------------------
        // POST /api/tasks
        // -------------------------------------------------------------------------

        [Fact]
        public async Task Create_ValidRequest_Returns201WithTask()
        {
            HttpClient client = await GetAuthenticatedClient();
            HttpResponseMessage response = await client.PostAsJsonAsync("/api/tasks", new
            {
                title = "New Task",
                description = "Details here",
                priority = "High",
                dueDate = DateTime.Today.AddDays(7).ToString("yyyy-MM-dd")
            });

            _ = response.StatusCode.Should().Be(HttpStatusCode.Created);
            _ = response.Headers.Location.Should().NotBeNull();

            TaskBody? task = await response.Content.ReadFromJsonAsync<TaskBody>();
            _ = task!.Title.Should().Be("New Task");
            _ = task.IsComplete.Should().BeFalse();
            _ = task.Priority.Should().Be("High");
        }

        [Fact]
        public async Task Create_EmptyTitle_Returns400()
        {
            HttpClient client = await GetAuthenticatedClient();
            HttpResponseMessage response = await client.PostAsJsonAsync("/api/tasks",
                new { title = "", priority = "Low" });

            _ = response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // -------------------------------------------------------------------------
        // PUT /api/tasks/{id}
        // -------------------------------------------------------------------------

        [Fact]
        public async Task Update_ExistingTask_Returns200WithUpdatedValues()
        {
            HttpClient client = await GetAuthenticatedClient();
            TaskBody created = await CreateTaskAsync(client, "Original");

            HttpResponseMessage response = await client.PutAsJsonAsync($"/api/tasks/{created.Id}", new
            {
                title = "Updated Title",
                description = "New desc",
                priority = "High",
                dueDate = (string?)null
            });

            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
            TaskBody? task = await response.Content.ReadFromJsonAsync<TaskBody>();
            _ = task!.Title.Should().Be("Updated Title");
            _ = task.Priority.Should().Be("High");
        }

        [Fact]
        public async Task Update_OtherUsersTask_Returns404()
        {
            HttpClient client1 = await GetAuthenticatedClient();
            HttpClient client2 = await GetAuthenticatedClient();

            TaskBody task = await CreateTaskAsync(client1);
            HttpResponseMessage response = await client2.PutAsJsonAsync($"/api/tasks/{task.Id}", new
            {
                title = "Hijacked",
                priority = "Low"
            });

            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // -------------------------------------------------------------------------
        // PATCH /api/tasks/{id}/complete
        // -------------------------------------------------------------------------

        [Fact]
        public async Task ToggleComplete_IncompleteTask_MarksComplete()
        {
            HttpClient client = await GetAuthenticatedClient();
            TaskBody created = await CreateTaskAsync(client);

            _ = created.IsComplete.Should().BeFalse();

            HttpResponseMessage response = await client.PatchAsync($"/api/tasks/{created.Id}/complete", null);
            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);

            TaskBody? task = await response.Content.ReadFromJsonAsync<TaskBody>();
            _ = task!.IsComplete.Should().BeTrue();
        }

        [Fact]
        public async Task ToggleComplete_CompleteTask_MarksIncomplete()
        {
            HttpClient client = await GetAuthenticatedClient();
            TaskBody created = await CreateTaskAsync(client);

            // Toggle on
            _ = await client.PatchAsync($"/api/tasks/{created.Id}/complete", null);

            // Toggle off
            HttpResponseMessage response = await client.PatchAsync($"/api/tasks/{created.Id}/complete", null);
            TaskBody? task = await response.Content.ReadFromJsonAsync<TaskBody>();

            _ = task!.IsComplete.Should().BeFalse();
        }

        [Fact]
        public async Task ToggleComplete_NonExistentTask_Returns404()
        {
            HttpClient client = await GetAuthenticatedClient();
            HttpResponseMessage response = await client.PatchAsync($"/api/tasks/{Guid.NewGuid()}/complete", null);
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // -------------------------------------------------------------------------
        // DELETE /api/tasks/{id}
        // -------------------------------------------------------------------------

        [Fact]
        public async Task Delete_ExistingTask_Returns204()
        {
            HttpClient client = await GetAuthenticatedClient();
            TaskBody created = await CreateTaskAsync(client);

            HttpResponseMessage response = await client.DeleteAsync($"/api/tasks/{created.Id}");
            _ = response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Confirm it's gone
            HttpResponseMessage get = await client.GetAsync($"/api/tasks/{created.Id}");
            _ = get.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_OtherUsersTask_Returns404()
        {
            HttpClient client1 = await GetAuthenticatedClient();
            HttpClient client2 = await GetAuthenticatedClient();

            TaskBody task = await CreateTaskAsync(client1);
            HttpResponseMessage response = await client2.DeleteAsync($"/api/tasks/{task.Id}");

            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            // Original task still exists
            HttpResponseMessage stillThere = await client1.GetAsync($"/api/tasks/{task.Id}");
            _ = stillThere.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Delete_NonExistentId_Returns404()
        {
            HttpClient client = await GetAuthenticatedClient();
            HttpResponseMessage response = await client.DeleteAsync($"/api/tasks/{Guid.NewGuid()}");
            _ = response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // -------------------------------------------------------------------------
        // DTOs
        // -------------------------------------------------------------------------

        private record TaskBody(
            Guid Id, string Title, string? Description,
            bool IsComplete, string Priority,
            string? DueDate, DateTime CreatedAt, DateTime UpdatedAt);
    }
}
