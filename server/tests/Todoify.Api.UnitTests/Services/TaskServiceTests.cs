using Todoify.Api.Data;
using Todoify.Api.DTOs.Tasks;
using Todoify.Api.Models;
using Todoify.Api.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Todoify.Api.UnitTests.Services;

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
        var opts = new DbContextOptionsBuilder<AppDbContext>()
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
        _db.Tasks.Add(task);
        _db.SaveChanges();
        return task;
    }

    // -------------------------------------------------------------------------
    // GetAllAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyCurrentUsersTasks()
    {
        AddTask("Mine", UserId);
        AddTask("Theirs", OtherUserId);

        var result = await _sut.GetAllAsync(UserId, null, null, null);

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Mine");
    }

    [Fact]
    public async Task GetAllAsync_FilterByIsComplete_ReturnsMatchingTasks()
    {
        AddTask("Done", isComplete: true);
        AddTask("Pending", isComplete: false);

        var result = await _sut.GetAllAsync(UserId, isComplete: true, null, null);

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Done");
    }

    [Fact]
    public async Task GetAllAsync_FilterByPriority_ReturnsMatchingTasks()
    {
        AddTask("High priority task", priority: Priority.High);
        AddTask("Low priority task", priority: Priority.Low);

        var result = await _sut.GetAllAsync(UserId, null, Priority.High, null);

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("High priority task");
    }

    [Theory]
    [InlineData("title")]
    [InlineData("priority")]
    [InlineData("duedate")]
    [InlineData("unknown")]
    [InlineData(null)]
    public async Task GetAllAsync_SortBy_DoesNotThrow(string? sortBy)
    {
        AddTask("A"); AddTask("B");

        var act = () => _sut.GetAllAsync(UserId, null, null, sortBy);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetAllAsync_SortByTitle_ReturnsTitlesAscending()
    {
        AddTask("Zebra"); AddTask("Apple"); AddTask("Mango");

        var result = await _sut.GetAllAsync(UserId, null, null, "title");

        result.Select(t => t.Title).Should().BeInAscendingOrder();
    }

    // -------------------------------------------------------------------------
    // GetByIdAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ExistingTask_ReturnsTaskResponse()
    {
        var task = AddTask();

        var result = await _sut.GetByIdAsync(task.Id, UserId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(task.Id);
        result.Title.Should().Be(task.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WrongUser_ReturnsNull()
    {
        var task = AddTask(userId: OtherUserId);

        var result = await _sut.GetByIdAsync(task.Id, UserId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid(), UserId);

        result.Should().BeNull();
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

        var result = await _sut.CreateAsync(request, UserId);

        result.Id.Should().NotBeEmpty();
        result.Title.Should().Be("New Task");
        result.Priority.Should().Be(Priority.High);
        result.IsComplete.Should().BeFalse();

        _db.Tasks.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAsync_SetsUserIdFromParameter_NotFromRequest()
    {
        var request = new CreateTaskRequest("Task", null, Priority.Low, null);

        await _sut.CreateAsync(request, UserId);

        _db.Tasks.Single().UserId.Should().Be(UserId);
    }

    // -------------------------------------------------------------------------
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ExistingTask_UpdatesAndReturnsResponse()
    {
        var task = AddTask("Old Title");
        var request = new UpdateTaskRequest("New Title", "Desc", Priority.High, null);

        var result = await _sut.UpdateAsync(task.Id, request, UserId);

        result.Should().NotBeNull();
        result!.Title.Should().Be("New Title");
        result.Priority.Should().Be(Priority.High);
    }

    [Fact]
    public async Task UpdateAsync_SetsUpdatedAt()
    {
        var task = AddTask();
        var before = DateTime.UtcNow;
        var request = new UpdateTaskRequest("X", null, Priority.Low, null);

        var result = await _sut.UpdateAsync(task.Id, request, UserId);

        result!.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public async Task UpdateAsync_WrongUser_ReturnsNull()
    {
        var task = AddTask(userId: OtherUserId);
        var request = new UpdateTaskRequest("Hacked", null, Priority.Low, null);

        var result = await _sut.UpdateAsync(task.Id, request, UserId);

        result.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // ToggleCompleteAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ToggleCompleteAsync_IncompleteTask_MarksComplete()
    {
        var task = AddTask(isComplete: false);

        var result = await _sut.ToggleCompleteAsync(task.Id, UserId);

        result!.IsComplete.Should().BeTrue();
    }

    [Fact]
    public async Task ToggleCompleteAsync_CompleteTask_MarksIncomplete()
    {
        var task = AddTask(isComplete: true);

        var result = await _sut.ToggleCompleteAsync(task.Id, UserId);

        result!.IsComplete.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleCompleteAsync_NonExistentTask_ReturnsNull()
    {
        var result = await _sut.ToggleCompleteAsync(Guid.NewGuid(), UserId);

        result.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // DeleteAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ExistingTask_DeletesAndReturnsTrue()
    {
        var task = AddTask();

        var result = await _sut.DeleteAsync(task.Id, UserId);

        result.Should().BeTrue();
        _db.Tasks.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_WrongUser_ReturnsFalseAndDoesNotDelete()
    {
        var task = AddTask(userId: OtherUserId);

        var result = await _sut.DeleteAsync(task.Id, UserId);

        result.Should().BeFalse();
        _db.Tasks.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentId_ReturnsFalse()
    {
        var result = await _sut.DeleteAsync(Guid.NewGuid(), UserId);

        result.Should().BeFalse();
    }
}