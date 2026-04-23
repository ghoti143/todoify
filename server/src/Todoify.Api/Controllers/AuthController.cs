using Microsoft.AspNetCore.Mvc;
using Todoify.Api.DTOs.Auth;
using Todoify.Api.Services;

namespace Todoify.Api.Controllers
{
    /// <summary>
    /// Handles authentication operations including user registration and login.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="request">The registration details including email, password, and optional display name.</param>
        /// <returns>
        /// <see cref="OkObjectResult"/> with an <see cref="AuthResponse"/> on success, or
        /// <see cref="BadRequestObjectResult"/> with a list of validation errors on failure.
        /// </returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            (AuthResponse? response, IEnumerable<string>? errors) = await authService.RegisterAsync(request);
            return response is null ? BadRequest(new { errors }) : Ok(response);
        }

        /// <summary>
        /// Authenticates an existing user and returns an auth token.
        /// </summary>
        /// <param name="request">The login credentials including email and password.</param>
        /// <returns>
        /// <see cref="OkObjectResult"/> with an <see cref="AuthResponse"/> on success, or
        /// <see cref="UnauthorizedObjectResult"/> if the credentials are invalid.
        /// </returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            AuthResponse? result = await authService.LoginAsync(request);
            return result is null ? Unauthorized(new { message = "Invalid email or password." }) : Ok(result);
        }
    }
}
