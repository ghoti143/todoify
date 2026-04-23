using FluentValidation;

namespace Todoify.Api.DTOs.Auth
{
    /// <summary>
    /// Represents the request body for registering a new user account.
    /// </summary>
    /// <param name="Email">The user's email address. Must be a valid email format.</param>
    /// <param name="Password">The user's password. Must be at least 6 characters.</param>
    /// <param name="DisplayName">An optional display name for the user. Maximum 100 characters.</param>
    public record RegisterRequest(
        string Email,
        string Password,
        string? DisplayName
    );

    /// <summary>
    /// Represents the request body for authenticating an existing user.
    /// </summary>
    /// <param name="Email">The user's email address.</param>
    /// <param name="Password">The user's password.</param>
    public record LoginRequest(string Email, string Password);

    /// <summary>
    /// Represents the response returned after a successful authentication or registration.
    /// </summary>
    /// <param name="Token">The JWT bearer token to use for authenticated requests.</param>
    /// <param name="Email">The authenticated user's email address.</param>
    /// <param name="DisplayName">The authenticated user's display name, if set.</param>
    /// <param name="ExpiresAt">The UTC date and time at which the token expires.</param>
    public record AuthResponse(
        string Token,
        string Email,
        string? DisplayName,
        DateTime ExpiresAt
    );

    // --- Validators ---

    /// <summary>
    /// FluentValidation validator for <see cref="RegisterRequest"/>.
    /// </summary>
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RegisterRequestValidator"/> with validation rules.
        /// </summary>
        public RegisterRequestValidator()
        {
            _ = RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress().WithMessage("A valid email address is required.");

            _ = RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

            _ = RuleFor(x => x.DisplayName)
                .MaximumLength(100).WithMessage("Display name must be 100 characters or fewer.")
                .When(x => x.DisplayName is not null);
        }
    }

    /// <summary>
    /// FluentValidation validator for <see cref="LoginRequest"/>.
    /// </summary>
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="LoginRequestValidator"/> with validation rules.
        /// </summary>
        public LoginRequestValidator()
        {
            _ = RuleFor(x => x.Email).NotEmpty().EmailAddress();
            _ = RuleFor(x => x.Password).NotEmpty();
        }
    }
}
