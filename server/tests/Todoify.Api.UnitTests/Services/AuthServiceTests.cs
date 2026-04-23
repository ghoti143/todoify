using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Todoify.Api.DTOs.Auth;
using Todoify.Api.Models;
using Todoify.Api.Services;

namespace Todoify.Api.UnitTests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<AppUser>> _userManagerMock;
        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            _userManagerMock = new Mock<UserManager<AppUser>>(
                Mock.Of<IUserStore<AppUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);

            IConfigurationRoot config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "super-secret-key-that-is-long-enough-for-hmac",
                    ["Jwt:Issuer"] = "todoify",
                    ["Jwt:Audience"] = "todoify",
                    ["Jwt:ExpiryHours"] = "24"
                })
                .Build();

            _sut = new AuthService(_userManagerMock.Object, config);
        }

        [Fact]
        public async Task RegisterAsync_DuplicateEmail_ReturnsEmailInUseError()
        {
            _ = _userManagerMock
                .Setup(m => m.FindByEmailAsync("x@test.com"))
                .ReturnsAsync(new AppUser());

            (AuthResponse? response, IEnumerable<string>? errors) = await _sut.RegisterAsync(
                new RegisterRequest("x@test.com", "password123", null));

            _ = response.Should().BeNull();
            _ = errors.Should().ContainSingle(e => e == "Email is already in use");
        }

        [Fact]
        public async Task RegisterAsync_Success_ReturnsTokenWithCorrectClaims()
        {
            var user = new AppUser { Id = "user-123", Email = "x@test.com", DisplayName = "Test" };

            _ = _userManagerMock
                .Setup(m => m.FindByEmailAsync(user.Email))
                .ReturnsAsync((AppUser?)null);
            _ = _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            (AuthResponse? response, IEnumerable<string> _) = await _sut.RegisterAsync(
                new RegisterRequest(user.Email, "password123", user.DisplayName));

            JwtSecurityToken jwt = new JwtSecurityTokenHandler().ReadJwtToken(response!.Token);
            _ = jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
            _ = response.Email.Should().Be(user.Email);
            _ = response.DisplayName.Should().Be(user.DisplayName);
        }

        [Fact]
        public async Task RegisterAsync_Success_TokenExpiryMatchesConfig()
        {
            _ = _userManagerMock
                .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((AppUser?)null);
            _ = _userManagerMock
                .Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            DateTime before = DateTime.UtcNow;
            (AuthResponse? response, IEnumerable<string> _) = await _sut.RegisterAsync(
                new RegisterRequest("x@test.com", "password123", null));

            _ = response!.ExpiresAt.Should().BeCloseTo(before.AddHours(24), precision: TimeSpan.FromSeconds(5));
        }
    }
}
