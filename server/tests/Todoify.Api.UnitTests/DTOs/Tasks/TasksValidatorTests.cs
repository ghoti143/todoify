using FluentAssertions;
using FluentValidation.Results;
using Todoify.Api.DTOs.Tasks;
using Todoify.Api.Models;

namespace Todoify.Api.UnitTests.DTOs.Tasks
{
    public class TaskValidatorTests
    {
        // -------------------------------------------------------------------------
        // CreateTaskRequestValidator
        // -------------------------------------------------------------------------

        private readonly CreateTaskRequestValidator _createValidator = new();

        [Fact]
        public void Create_WithValidRequest_ShouldPass()
        {
            var request = new CreateTaskRequest(
                    Title: "Buy groceries",
                    Description: "Milk and eggs",
                Priority: Priority.Medium,
                DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1))
            );

            ValidationResult result = _createValidator.Validate(request);

            _ = result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Create_WithEmptyTitle_ShouldFail()
        {
            var request = new CreateTaskRequest("", null, Priority.Low, null);

            ValidationResult result = _createValidator.Validate(request);

            _ = result.IsValid.Should().BeFalse();
            _ = result.Errors.Should().Contain(e => e.ErrorMessage == "Title is required.");
        }

        [Fact]
        public void Create_WithTitleExceeding200Chars_ShouldFail()
        {
            var request = new CreateTaskRequest(
                Title: new string('a', 201),
                Description: null,
                Priority: Priority.Low,
                DueDate: null
            );

            ValidationResult result = _createValidator.Validate(request);

            _ = result.IsValid.Should().BeFalse();
            _ = result.Errors.Should().Contain(e => e.ErrorMessage == "Title must be 200 characters or fewer.");
        }

        [Fact]
        public void Create_WithDescriptionExceeding2000Chars_ShouldFail()
        {
            var request = new CreateTaskRequest(
                Title: "Valid title",
                Description: new string('a', 2001),
                Priority: Priority.Low,
                DueDate: null
            );

            ValidationResult result = _createValidator.Validate(request);

            _ = result.IsValid.Should().BeFalse();
            _ = result.Errors.Should().Contain(e => e.ErrorMessage == "Description must be 2000 characters or fewer.");
        }

        [Fact]
        public void Create_WithNullDescription_ShouldPass()
        {
            var request = new CreateTaskRequest("Valid title", null, Priority.Low, null);

            ValidationResult result = _createValidator.Validate(request);

            _ = result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Create_WithPastDueDate_ShouldFail()
        {
            var request = new CreateTaskRequest(
                Title: "Valid title",
                Description: null,
                Priority: Priority.Low,
                DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(-1))
            );

            ValidationResult result = _createValidator.Validate(request);

            _ = result.IsValid.Should().BeFalse();
            _ = result.Errors.Should().Contain(e => e.ErrorMessage == "Due date must be today or in the future.");
        }

        [Fact]
        public void Create_WithTodayAsDueDate_ShouldPass()
        {
            var request = new CreateTaskRequest(
                Title: "Valid title",
                Description: null,
                Priority: Priority.Low,
                DueDate: DateOnly.FromDateTime(DateTime.Today)
            );

            ValidationResult result = _createValidator.Validate(request);

            _ = result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Create_WithNullDueDate_ShouldPass()
        {
            var request = new CreateTaskRequest("Valid title", null, Priority.Low, null);

            ValidationResult result = _createValidator.Validate(request);

            _ = result.IsValid.Should().BeTrue();
        }

        // -------------------------------------------------------------------------
        // UpdateTaskRequestValidator
        // -------------------------------------------------------------------------

        private readonly UpdateTaskRequestValidator _updateValidator = new();

        [Fact]
        public void Update_WithValidRequest_ShouldPass()
        {
            var request = new UpdateTaskRequest(
                Title: "Updated title",
                Description: "Updated description",
                Priority: Priority.High,
                DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1))
            );

            ValidationResult result = _updateValidator.Validate(request);

            _ = result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Update_WithEmptyTitle_ShouldFail()
        {
            var request = new UpdateTaskRequest("", null, Priority.Low, null);

            ValidationResult result = _updateValidator.Validate(request);

            _ = result.IsValid.Should().BeFalse();
            _ = result.Errors.Should().Contain(e => e.ErrorMessage == "Title is required.");
        }

        [Fact]
        public void Update_WithTitleExceeding200Chars_ShouldFail()
        {
            var request = new UpdateTaskRequest(
                Title: new string('a', 201),
                Description: null,
                Priority: Priority.Low,
                DueDate: null
            );

            ValidationResult result = _updateValidator.Validate(request);

            _ = result.IsValid.Should().BeFalse();
            _ = result.Errors.Should().Contain(e => e.ErrorMessage == "Title must be 200 characters or fewer.");
        }

        [Fact]
        public void Update_WithDescriptionExceeding2000Chars_ShouldFail()
        {
            var request = new UpdateTaskRequest(
                Title: "Valid title",
                Description: new string('a', 2001),
                Priority: Priority.Low,
                DueDate: null
            );

            ValidationResult result = _updateValidator.Validate(request);

            _ = result.IsValid.Should().BeFalse();
            _ = result.Errors.Should().Contain(e => e.ErrorMessage == "Description must be 2000 characters or fewer.");
        }

        [Fact]
        public void Update_WithPastDueDate_ShouldFail()
        {
            var request = new UpdateTaskRequest(
                Title: "Valid title",
                Description: null,
                Priority: Priority.Low,
                DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(-1))
            );

            ValidationResult result = _updateValidator.Validate(request);

            _ = result.IsValid.Should().BeFalse();
            _ = result.Errors.Should().Contain(e => e.ErrorMessage == "Due date must be today or in the future.");
        }

        [Fact]
        public void Update_WithNullDueDate_ShouldPass()
        {
            var request = new UpdateTaskRequest("Valid title", null, Priority.Low, null);

            ValidationResult result = _updateValidator.Validate(request);

            _ = result.IsValid.Should().BeTrue();
        }
    }
}
