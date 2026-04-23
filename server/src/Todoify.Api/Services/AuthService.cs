using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Todoify.Api.DTOs.Auth;
using Todoify.Api.Models;

namespace Todoify.Api.Services
{
    /// <summary>
    /// Defines authentication operations for the application.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="request">The registration details including email, password, and optional display name.</param>
        /// <returns>
        /// A tuple containing an <see cref="AuthResponse"/> on success, or <c>null</c> with a list of error messages on failure.
        /// </returns>
        Task<(AuthResponse?, IEnumerable<string> Errors)> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// Authenticates an existing user by email and password.
        /// </summary>
        /// <param name="request">The login credentials.</param>
        /// <returns>
        /// An <see cref="AuthResponse"/> on success, or <c>null</c> if the credentials are invalid.
        /// </returns>
        Task<AuthResponse?> LoginAsync(LoginRequest request);
    }

    /// <summary>
    /// Provides JWT-based authentication services including user registration and login.
    /// </summary>
    public class AuthService(
        UserManager<AppUser> userManager,
        IConfiguration config) : IAuthService
    {
        /// <inheritdoc/>
        public async Task<(AuthResponse?, IEnumerable<string> Errors)> RegisterAsync(RegisterRequest request)
        {
            // Check for existing user before attempting create
            if (await userManager.FindByEmailAsync(request.Email) is not null)
            {
                return (null, ["Email is already in use"]);
            }

            var user = new AppUser
            {
                Email = request.Email,
                UserName = request.Email,
                DisplayName = request.DisplayName
            };

            IdentityResult result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return (null, result.Errors.Select(e => e.Description));
            }

            return (GenerateToken(user), []);
        }

        /// <inheritdoc/>
        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            AppUser? user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return null;
            }

            bool valid = await userManager.CheckPasswordAsync(user, request.Password);
            return !valid ? null : GenerateToken(user);
        }

        /// <summary>
        /// Generates a signed JWT and wraps it in an <see cref="AuthResponse"/>.
        /// </summary>
        /// <param name="user">The authenticated user for whom the token is generated.</param>
        /// <returns>An <see cref="AuthResponse"/> containing the token, user details, and expiry time.</returns>
        private AuthResponse GenerateToken(AppUser user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            DateTime expiry = DateTime.UtcNow.AddHours(
                config.GetValue("Jwt:ExpiryHours", 24));

            Claim[] claims =
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ];

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
}
