using Microsoft.Extensions.Configuration;
using Moq;
using FluentAssertions;
using WebAPI.Services;
using WebAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WebAPI.Tests.Services;

public class JwtServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IConfigurationSection> _mockJwtSection;
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockJwtSection = new Mock<IConfigurationSection>();
        
        // Setup JWT configuration
        _mockJwtSection.Setup(x => x["Secret"]).Returns("ThisIsASecretKeyForJwtTokenGenerationThatIsAtLeast32Characters");
        _mockJwtSection.Setup(x => x["Issuer"]).Returns("TravelOrganizationSystem");
        _mockJwtSection.Setup(x => x["Audience"]).Returns("TravelOrganizationSystem");
        _mockJwtSection.Setup(x => x["ExpiryInMinutes"]).Returns("120");
        
        _mockConfiguration.Setup(x => x.GetSection("JwtSettings")).Returns(_mockJwtSection.Object);
        
        _jwtService = new JwtService(_mockConfiguration.Object);
    }

    [Fact]
    public void GenerateToken_WithValidUser_ReturnsValidJwtToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            IsAdmin = false
        };

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        // Verify the token is valid JWT format
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        
        jwtToken.Claims.Should().Contain(c => c.Type == "nameid" && c.Value == user.Id.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == user.Username);
        jwtToken.Claims.Should().Contain(c => c.Type == "email" && c.Value == user.Email);
        jwtToken.Claims.Should().Contain(c => c.Type == "role" && c.Value == "User");
    }

    [Fact]
    public void GenerateToken_WithAdminUser_ReturnsTokenWithAdminRole()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "admin",
            Email = "admin@example.com",
            IsAdmin = true
        };

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        
        jwtToken.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Admin");
    }

    [Fact]
    public void GenerateToken_WithValidUser_TokenHasCorrectExpiration()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            IsAdmin = false
        };

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        
        var expirationTime = jwtToken.ValidTo;
        var expectedExpiration = DateTime.UtcNow.AddMinutes(120);
        
        // Allow for a small time difference due to execution time
        expirationTime.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void GenerateToken_WithValidUser_TokenHasCorrectIssuerAndAudience()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            IsAdmin = false
        };

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        
        jwtToken.Issuer.Should().Be("TravelOrganizationSystem");
        jwtToken.Audiences.Should().Contain("TravelOrganizationSystem");
    }

    [Fact]
    public void GenerateToken_MultipleCalls_ReturnsDifferentTokens()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            IsAdmin = false
        };

        // Act
        var token1 = _jwtService.GenerateToken(user);
        Thread.Sleep(1000); // Ensure different timestamps
        var token2 = _jwtService.GenerateToken(user);

        // Assert
        token1.Should().NotBeNullOrEmpty();
        token2.Should().NotBeNullOrEmpty();
        token1.Should().NotBe(token2);
    }
}