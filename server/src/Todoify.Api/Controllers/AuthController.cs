using Microsoft.AspNetCore.Mvc;
using Todoify.Api.DTOs.Auth;
using Todoify.Api.Services;

namespace Todoify.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var (response, errors) = await authService.RegisterAsync(request);
        if (response is null)
            return BadRequest(new { errors });

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        if (result is null)
            return Unauthorized(new { message = "Invalid email or password." });

        return Ok(result);
    }
}