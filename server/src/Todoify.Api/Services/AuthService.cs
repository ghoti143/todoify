using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Todoify.Api.DTOs.Auth;
using Todoify.Api.Models;

namespace Todoify.Api.Services;

public interface IAuthService
{
    Task<(AuthResponse?, IEnumerable<string> Errors)> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
}

public class AuthService(
    UserManager<AppUser> userManager,
    IConfiguration config) : IAuthService
{
    public async Task<(AuthResponse?, IEnumerable<string> Errors)> RegisterAsync(RegisterRequest request)
    {
        // Check for existing user before attempting create
        if (await userManager.FindByEmailAsync(request.Email) is not null)
            return (null, ["Email is already in use"]);

        var user = new AppUser
        {
            Email = request.Email,
            UserName = request.Email,
            DisplayName = request.DisplayName
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return (null, result.Errors.Select(e => e.Description));

        return (GenerateToken(user), []);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null) return null;

        var valid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!valid) return null;

        return GenerateToken(user);
    }

    private AuthResponse GenerateToken(AppUser user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddHours(
            config.GetValue<int>("Jwt:ExpiryHours", 24));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds
        );

        return new AuthResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            user.Email!,
            user.DisplayName,
            expiry
        );
    }
}