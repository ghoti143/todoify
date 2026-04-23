using Todoify.Api.Data;
using Todoify.Api.DTOs.Tasks;
using Todoify.Api.Models;
using Todoify.Api.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Todoify.Api.UnitTests.Services
{
    /// <summary>
    /// Uses EF Core InMemory provider so no mocking of DbContext is needed.
    /// Each test gets a fresh, uniquely-named database.
    /// </summary>
    public class TaskServiceTests : IDisposable
    {
        private readonly AppDbContext _db;
        private readonly TaskService _sut;
        private const string UserId = "user-001";
        private const string OtherUserId = "user-002";

        public TaskServiceTests()
        {
            DbContextOptions<AppDbContext> opts = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new AppDbContext(opts);
            _sut = new TaskService(_db);
        }

        public void Dispose() => _db.Dispose();

        // -------------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------------

        private TaskItem AddTask(string title = "Test Task", string userId = UserId,
            bool isComplete = false, Priority priority = Priority.Medium)
        {
            var task = new TaskItem
            {
                Title = title,
                UserId = userId,
                IsComplete = isComplete,
                Priority = priority
            };
            _ = _db.Tasks.Add(task);
            _ = _db.SaveChanges();
            return task;
        }

        // -------------------------------------------------------------------------
        // GetAllAsync
        // -------------------------------------------------------------------------

        [Fact]
        public async Task GetAllAsync_ReturnsOnlyCurrentUsersTasks()
        {
            _ = AddTask("Mine", UserId);
            _ = AddTask("Theirs", OtherUserId);

            List<TaskResponse> result = await _sut.GetAllAsync(UserId, null, null, null);

            _ = result.Should().HaveCount(1);
            _ = result[0].Title.Should().Be("Mine");
        }

        [Fact]
        public async Task GetAllAsync_FilterByIsComplete_ReturnsMatchingTasks()
        {
            _ = AddTask("Done", isComplete: true);
            _ = AddTask("Pending", isComplete: false);

            List<TaskResponse> result = await _sut.GetAllAsync(UserId, isComplete: true, null, null);

            _ = result.Should().HaveCount(1);
            _ = result[0].Title.Should().Be("Done");
        }

        [Fact]
        public async Task GetAllAsync_FilterByPriority_ReturnsMatchingTasks()
        {
            _ = AddTask("High priority task", priority: Priority.High);
            _ = AddTask("Low priority task", priority: Priority.Low);

            List<TaskResponse> result = await _sut.GetAllAsync(UserId, null, Priority.High, null);

            _ = result.Should().HaveCount(1);
            _ = result[0].Title.Should().Be("High priority task");
        }

        [Theory]
        [InlineData("title")]
        [InlineData("priority")]
        [InlineData("duedate")]
        [InlineData("unknown")]
        [InlineData(null)]
        public async Task GetAllAsync_SortBy_DoesNotThrow(string? sortBy)
        {
            _ = AddTask("A"); _ = AddTask("B");

            Func<Task<List<TaskResponse>>> act = () => _sut.GetAllAsync(UserId, null, null, sortBy);

            _ = await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetAllAsync_SortByTitle_ReturnsTitlesAscending()
        {
            _ = AddTask("Zebra"); _ = AddTask("Apple"); _ = AddTask("Mango");

            List<TaskResponse> result = await _sut.GetAllAsync(UserId, null, null, "title");

            _ = result.Select(t => t.Title).Should().BeInAscendingOrder();
        }

        // -------------------------------------------------------------------------
        // GetByIdAsync
        // -------------------------------------------------------------------------

        [Fact]
        public async Task GetByIdAsync_ExistingTask_ReturnsTaskResponse()
        {
            TaskItem task = AddTask();

            TaskResponse? result = await _sut.GetByIdAsync(task.Id, UserId);

            _ = result.Should().NotBeNull();
            _ = result.Id.Should().Be(task.Id);
            _ = result.Title.Should().Be(task.Title);
        }

        [Fact]
        public async Task GetByIdAsync_WrongUser_ReturnsNull()
        {
            TaskItem task = AddTask(userId: OtherUserId);

            TaskResponse? result = await _sut.GetByIdAsync(task.Id, UserId);

            _ = result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentId_ReturnsNull()
        {
            TaskResponse? result = await _sut.GetByIdAsync(Guid.NewGuid(), UserId);

            _ = result.Should().BeNull();
        }

        // -------------------------------------------------------------------------
        // CreateAsync
        // -------------------------------------------------------------------------

        [Fact]
        public async Task CreateAsync_ValidRequest_PersistsAndReturnsTask()
        {
            var request = new CreateTaskRequest(
                "New Task", "A description", Priority.High,
                DateOnly.FromDateTime(DateTime.Today.AddDays(7)));

            TaskResponse result = await _sut.CreateAsync(request, UserId);

            _ = result.Id.Should().NotBeEmpty();
            _ = result.Title.Should().Be("New Task");
            _ = result.Priority.Should().Be(Priority.High);
            _ = result.IsComplete.Should().BeFalse();

            _ = _db.Tasks.Should().HaveCount(1);
        }

        [Fact]
        public async Task CreateAsync_SetsUserIdFromParameter_NotFromRequest()
        {
            var request = new CreateTaskRequest("Task", null, Priority.Low, null);

            _ = await _sut.CreateAsync(request, UserId);

            _ = _db.Tasks.Single().UserId.Should().Be(UserId);
        }

        // -------------------------------------------------------------------------
        // UpdateAsync
        // -------------------------------------------------------------------------

        [Fact]
        public async Task UpdateAsync_ExistingTask_UpdatesAndReturnsResponse()
        {
            TaskItem task = AddTask("Old Title");
            var request = new UpdateTaskRequest("New Title", "Desc", Priority.High, null);

            TaskResponse? result = await _sut.UpdateAsync(task.Id, request, UserId);

            _ = result.Should().NotBeNull();
            _ = result.Title.Should().Be("New Title");
            _ = result.Priority.Should().Be(Priority.High);
        }

        [Fact]
        public async Task UpdateAsync_SetsUpdatedAt()
        {
            TaskItem task = AddTask();
            DateTime before = DateTime.UtcNow;
            var request = new UpdateTaskRequest("X", null, Priority.Low, null);

            TaskResponse? result = await _sut.UpdateAsync(task.Id, request, UserId);

            _ = result!.UpdatedAt.Should().BeOnOrAfter(before);
        }

        [Fact]
        public async Task UpdateAsync_WrongUser_ReturnsNull()
        {
            TaskItem task = AddTask(userId: OtherUserId);
            var request = new UpdateTaskRequest("Hacked", null, Priority.Low, null);

            TaskResponse? result = await _sut.UpdateAsync(task.Id, request, UserId);

            _ = result.Should().BeNull();
        }

        // -------------------------------------------------------------------------
        // ToggleCompleteAsync
        // -------------------------------------------------------------------------

        [Fact]
        public async Task ToggleCompleteAsync_IncompleteTask_MarksComplete()
        {
            TaskItem task = AddTask(isComplete: false);

            TaskResponse? result = await _sut.ToggleCompleteAsync(task.Id, UserId);

            _ = result!.IsComplete.Should().BeTrue();
        }

        [Fact]
        public async Task ToggleCompleteAsync_CompleteTask_MarksIncomplete()
        {
            TaskItem task = AddTask(isComplete: true);

            TaskResponse? result = await _sut.ToggleCompleteAsync(task.Id, UserId);

            _ = result!.IsComplete.Should().BeFalse();
        }

        [Fact]
        public async Task ToggleCompleteAsync_NonExistentTask_ReturnsNull()
        {
            TaskResponse? result = await _sut.ToggleCompleteAsync(Guid.NewGuid(), UserId);

            _ = result.Should().BeNull();
        }

        // -------------------------------------------------------------------------
        // DeleteAsync
        // -------------------------------------------------------------------------

        [Fact]
        public async Task DeleteAsync_ExistingTask_DeletesAndReturnsTrue()
        {
            TaskItem task = AddTask();

            bool result = await _sut.DeleteAsync(task.Id, UserId);

            _ = result.Should().BeTrue();
            _ = _db.Tasks.Should().BeEmpty();
        }

        [Fact]
        public async Task DeleteAsync_WrongUser_ReturnsFalseAndDoesNotDelete()
        {
            TaskItem task = AddTask(userId: OtherUserId);

            bool result = await _sut.DeleteAsync(task.Id, UserId);

            _ = result.Should().BeFalse();
            _ = _db.Tasks.Should().HaveCount(1);
        }

        [Fact]
        public async Task DeleteAsync_NonExistentId_ReturnsFalse()
        {
            bool result = await _sut.DeleteAsync(Guid.NewGuid(), UserId);

            _ = result.Should().BeFalse();
        }
    }
}
