using FluentValidation;

namespace Todoify.Api.DTOs.Auth;

public record RegisterRequest(
    string Email,
    string Password,
    string? DisplayName
);

public record LoginRequest(string Email, string Password);

public record AuthResponse(
    string Token,
    string Email,
    string? DisplayName,
    DateTime ExpiresAt
);

// --- Validators ---

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

        RuleFor(x => x.DisplayName)
            .MaximumLength(100).WithMessage("Display name must be 100 characters or fewer.")
            .When(x => x.DisplayName is not null);
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}