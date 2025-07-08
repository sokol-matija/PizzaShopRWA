using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using WebAPI.Controllers;
using WebAPI.DTOs;
using WebAPI.Models;
using WebAPI.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Xunit.Abstractions;

namespace WebAPI.Tests.Controllers;

public class AuthControllerTests : TestBase
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<ILogService> _mockLogService;
    private readonly AuthController _controller;

    public AuthControllerTests(ITestOutputHelper output) : base(output)
    {
        LogInfo("Initializing AuthController test dependencies");
        _mockUserService = new Mock<IUserService>();
        _mockJwtService = new Mock<IJwtService>();
        _mockLogService = new Mock<ILogService>();
        _controller = new AuthController(_mockUserService.Object, _mockJwtService.Object, _mockLogService.Object);
        LogInfo("AuthController test setup completed");
    }

    [Fact]
    public async Task Register_WithValidModel_ReturnsOkResult()
    {
        LogTestStart(nameof(Register_WithValidModel_ReturnsOkResult));

        try
        {
            // Arrange
            LogTestStep("Setting up test data for valid user registration");
            var registerDto = new RegisterDTO
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "password123",
                ConfirmPassword = "password123",
                FirstName = "Test",
                LastName = "User"
            };
            LogTestStep("Created RegisterDTO", new { registerDto.Username, registerDto.Email, registerDto.FirstName, registerDto.LastName });

            var user = new User { Id = 1, Username = "testuser", Email = "test@example.com" };
            LogTestStep("Setting up UserService mock to return successful registration");
            _mockUserService.Setup(s => s.RegisterAsync(It.IsAny<RegisterDTO>()))
                .ReturnsAsync(user);

            // Act
            LogTestStep("Calling Register endpoint with valid data");
            var result = await _controller.Register(registerDto);
            LogTestStep("Register endpoint call completed");

            // Assert
            LogTestStep("Verifying response is OkObjectResult");
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            LogTestStep("Verifying response contains success message");
            okResult.Value.Should().BeEquivalentTo(new { message = "Registration successful" });
            
            LogTestResult("User registration test completed successfully");
            LogTestEnd(nameof(Register_WithValidModel_ReturnsOkResult), true);
        }
        catch (Exception ex)
        {
            LogError("Test failed with exception", ex);
            LogTestEnd(nameof(Register_WithValidModel_ReturnsOkResult), false);
            throw;
        }
    }

    [Fact]
    public async Task Register_WhenUserServiceReturnsNull_ReturnsBadRequest()
    {
        // Arrange
        var registerDto = new RegisterDTO
        {
            Username = "existinguser",
            Email = "existing@example.com",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        _mockUserService.Setup(s => s.RegisterAsync(It.IsAny<RegisterDTO>()))
            .ReturnsAsync((User)null);

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Value.Should().Be("Username or email already exists");
    }

    [Fact]
    public async Task Register_WithInvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var registerDto = new RegisterDTO(); // Invalid model
        _controller.ModelState.AddModelError("Username", "Username is required");

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        LogTestStart(nameof(Login_WithValidCredentials_ReturnsOkWithToken));

        try
        {
            // Arrange
            LogTestStep("Setting up login test data");
            var loginDto = new LoginDTO
            {
                Username = "testuser",
                Password = "password123"
            };
            LogTestStep("Created LoginDTO", new { loginDto.Username, PasswordLength = loginDto.Password.Length });

            var user = new User 
            { 
                Id = 1, 
                Username = "testuser", 
                Email = "test@example.com",
                IsAdmin = false
            };
            LogTestStep("Created test user", new { user.Id, user.Username, user.Email, user.IsAdmin });

            var token = "generated-jwt-token";
            LogTestStep("Setting up service mocks for authentication");

            _mockUserService.Setup(s => s.AuthenticateAsync(loginDto.Username, loginDto.Password))
                .ReturnsAsync(user);
            _mockJwtService.Setup(s => s.GenerateToken(user))
                .Returns(token);
            LogTestStep("Service mocks configured successfully");

            // Act
            LogTestStep("Calling Login endpoint");
            var result = await _controller.Login(loginDto);
            LogTestStep("Login endpoint call completed");

            // Assert
            LogTestStep("Verifying response type is OkObjectResult");
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult.Value as TokenResponseDTO;
            
            LogTestStep("Verifying token response structure");
            response.Should().NotBeNull();
            LogTestStep("Verifying token content", new { ExpectedToken = token, ActualToken = response.Token });
            response.Token.Should().Be(token);
            response.Username.Should().Be(user.Username);
            response.IsAdmin.Should().Be(user.IsAdmin);
            response.ExpiresAt.Should().NotBeNullOrEmpty();
            
            LogTestResult("Login authentication test completed successfully");
            LogTestEnd(nameof(Login_WithValidCredentials_ReturnsOkWithToken), true);
        }
        catch (Exception ex)
        {
            LogError("Login test failed with exception", ex);
            LogTestEnd(nameof(Login_WithValidCredentials_ReturnsOkWithToken), false);
            throw;
        }
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsBadRequest()
    {
        // Arrange
        var loginDto = new LoginDTO
        {
            Username = "testuser",
            Password = "wrongpassword"
        };

        _mockUserService.Setup(s => s.AuthenticateAsync(loginDto.Username, loginDto.Password))
            .ReturnsAsync((User)null);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Value.Should().Be("Username or password is incorrect");
    }

    [Fact]
    public async Task Login_WithInvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var loginDto = new LoginDTO(); // Invalid model
        _controller.ModelState.AddModelError("Username", "Username is required");

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ChangePassword_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var changePasswordDto = new ChangePasswordDTO
        {
            CurrentPassword = "currentpass",
            NewPassword = "newpass123",
            ConfirmNewPassword = "newpass123"
        };

        var userId = 1;
        var userClaims = new[]
        {
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", userId.ToString())
        };

        var identity = new ClaimsIdentity(userClaims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _mockUserService.Setup(s => s.ChangePasswordAsync(userId, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ChangePassword(changePasswordDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(new { message = "Password changed successfully" });
    }

    [Fact]
    public async Task ChangePassword_WithIncorrectCurrentPassword_ReturnsBadRequest()
    {
        // Arrange
        var changePasswordDto = new ChangePasswordDTO
        {
            CurrentPassword = "wrongcurrentpass",
            NewPassword = "newpass123",
            ConfirmNewPassword = "newpass123"
        };

        var userId = 1;
        var userClaims = new[]
        {
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", userId.ToString())
        };

        var identity = new ClaimsIdentity(userClaims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _mockUserService.Setup(s => s.ChangePasswordAsync(userId, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ChangePassword(changePasswordDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Value.Should().Be("Current password is incorrect");
    }

    [Fact]
    public async Task ChangePassword_WithoutUserClaim_ReturnsUnauthorized()
    {
        // Arrange
        var changePasswordDto = new ChangePasswordDTO
        {
            CurrentPassword = "currentpass",
            NewPassword = "newpass123",
            ConfirmNewPassword = "newpass123"
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act
        var result = await _controller.ChangePassword(changePasswordDto);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task ChangePassword_WithInvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var changePasswordDto = new ChangePasswordDTO(); // Invalid model
        _controller.ModelState.AddModelError("CurrentPassword", "Current password is required");

        // Act
        var result = await _controller.ChangePassword(changePasswordDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}