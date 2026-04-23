using FluentAssertions;
using FluentValidation.Results;
using Todoify.Api.DTOs.Auth;

namespace Todoify.Api.UnitTests.DTOs.Auth
{
    public class AuthValidatorTests
    {
        // -------------------------------------------------------------------------
        // RegisterRequestValidator
        // -------------------------------------------------------------------------

        private readonly RegisterRequestValidator _registerValidator = new();

        [Fact]
        public void Register_WithValidRequest_ShouldPass()
        {
            var request = new RegisterRequest(
                Email: "test@todoify.com",
                Password: "password123",
                DisplayName: "Test User"
            );

            ValidationResult result = _registerValidator.Validate(request);

            _ = result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Register_WithNullDisplayName_ShouldPass()
        {
            var request = new RegisterRequest("test@todoify.com", "password123", null);

            ValidationResult result = _registerValidator.Validate(request);

            _ = result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("notanemail")]
        [InlineData("missing@")]
        [InlineData("@nodomain.com")]
        public void Register_WithInvalidEmail_ShouldFail(string email)
        {
            var request = new RegisterRequest(email, "password123", null);

            ValidationResult result = _registerValidator.Validate(request);

            _ = result.IsValid.Should().BeFalse();
            _ = result.Errors.Should().Contain(e => e.ErrorMessage == "A valid email address is required.");
        }

        [Fact]
        public void Register_WithEmptyPassword_ShouldFail()
        {
            var request = new RegisterRequest("test@todoify.com", "", null);

            ValidationResult result = _registerValidator.Validate(request);

            _ = result.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData("a")]
        [InlineData("abc")]
        [InlineData("12345")]
        public void Register_WithPasswordUnder6Chars_ShouldFail(string password)
        {
            var request = new RegisterRequest("test@todoify.com", password, null);

            ValidationResult result = _registerValidator.Validate(request);

            _ = result.IsValid.Should().BeFalse();
            _ = result.Errors.Should().Contain(e => e.ErrorMessage == "Password must be at least 6 characters.");
        }

        [Fact]
        public void Register_WithPasswordExactly6Chars_ShouldPass()
        {
            var request = new RegisterRequest("test@todoify.com", "abc123", null);

            ValidationResult result = _registerValidator.Validate(request);

            _ = result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Register_WithDisplayNameExceeding100Chars_ShouldFail()
        {
            var request = new RegisterRequest(
                Email: "test@todoify.com",
                Password: "password123",
                DisplayName: new string('a', 101)
            );

            ValidationResult result = _registerValidator.Validate(request);

            _ = result.IsValid.Should().BeFalse();
            _ = result.Errors.Should().Contain(e => e.ErrorMessage == "Display name must be 100 characters or fewer.");
        }

        [Fact]
        public void Register_WithDisplayNameExactly100Chars_ShouldPass()
        {
            var request = new RegisterRequest(
                Email: "test@todoify.com",
                Password: "password123",
                DisplayName: new string('a', 100)
            );

            ValidationResult result = _registerValidator.Validate(request);

            _ = result.IsValid.Should().BeTrue();
        }

        // -------------------------------------------------------------------------
        // LoginRequestValidator
        // -------------------------------------------------------------------------

        private readonly LoginRequestValidator _loginValidator = new();

        [Fact]
        public void Login_WithValidRequest_ShouldPass()
        {
            var request = new LoginRequest("test@todoify.com", "password123");

            ValidationResult result = _loginValidator.Validate(request);

            _ = result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Login_WithEmptyEmail_ShouldFail()
        {
            var request = new LoginRequest("", "password123");

            ValidationResult result = _loginValidator.Validate(request);

            _ = result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Login_WithInvalidEmail_ShouldFail()
        {
            var request = new LoginRequest("notanemail", "password123");

            ValidationResult result = _loginValidator.Validate(request);

            _ = result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Login_WithEmptyPassword_ShouldFail()
        {
            var request = new LoginRequest("test@todoify.com", "");

            ValidationResult result = _loginValidator.Validate(request);

            _ = result.IsValid.Should().BeFalse();
        }
    }
}
