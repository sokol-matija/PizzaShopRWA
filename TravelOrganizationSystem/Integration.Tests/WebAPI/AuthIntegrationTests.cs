using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;
using FluentAssertions;

namespace Integration.Tests.WebAPI;

public class AuthIntegrationTests : IntegrationTestBase
{
    public AuthIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output) 
        : base(factory, output)
    {
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsJwtToken()
    {
        LogTestStart(nameof(Login_WithValidCredentials_ReturnsJwtToken));

        try
        {
            // Arrange
            LogStep("Preparing login request with valid credentials");
            var loginRequest = new
            {
                Email = "test@example.com",
                Password = "password123"
            };

            // Act
            LogStep("Sending POST request to /api/auth/login");
            LogRequest("POST", "/api/auth/login", loginRequest);
            
            var response = await Client.PostAsync("/api/auth/login", CreateJsonContent(loginRequest));
            var content = await response.Content.ReadAsStringAsync();
            
            LogResponse(response, content);

            // Assert
            LogStep("Verifying response status and JWT token");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().NotBeNullOrEmpty();

            var result = JsonSerializer.Deserialize<JsonElement>(content);
            result.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
            result.GetProperty("user").GetProperty("email").GetString().Should().Be("test@example.com");

            LogTestEnd(nameof(Login_WithValidCredentials_ReturnsJwtToken), true);
        }
        catch (Exception ex)
        {
            Output.WriteLine($"🚨 ERROR: {ex.Message}");
            LogTestEnd(nameof(Login_WithValidCredentials_ReturnsJwtToken), false);
            throw;
        }
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        LogTestStart(nameof(Login_WithInvalidCredentials_ReturnsUnauthorized));

        try
        {
            // Arrange
            LogStep("Preparing login request with invalid credentials");
            var loginRequest = new
            {
                Email = "test@example.com",
                Password = "wrongpassword"
            };

            // Act
            LogStep("Sending POST request to /api/auth/login with wrong password");
            LogRequest("POST", "/api/auth/login", loginRequest);
            
            var response = await Client.PostAsync("/api/auth/login", CreateJsonContent(loginRequest));
            var content = await response.Content.ReadAsStringAsync();
            
            LogResponse(response, content);

            // Assert
            LogStep("Verifying unauthorized response");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            LogTestEnd(nameof(Login_WithInvalidCredentials_ReturnsUnauthorized), true);
        }
        catch (Exception ex)
        {
            Output.WriteLine($"🚨 ERROR: {ex.Message}");
            LogTestEnd(nameof(Login_WithInvalidCredentials_ReturnsUnauthorized), false);
            throw;
        }
    }

    [Fact]
    public async Task Register_WithValidData_CreatesNewUser()
    {
        LogTestStart(nameof(Register_WithValidData_CreatesNewUser));

        try
        {
            // Arrange
            LogStep("Preparing registration request");
            var registerRequest = new
            {
                FirstName = "New",
                LastName = "User",
                Email = "newuser@example.com",
                Password = "newpassword123"
            };

            // Act
            LogStep("Sending POST request to /api/auth/register");
            LogRequest("POST", "/api/auth/register", registerRequest);
            
            var response = await Client.PostAsync("/api/auth/register", CreateJsonContent(registerRequest));
            var content = await response.Content.ReadAsStringAsync();
            
            LogResponse(response, content);

            // Assert
            LogStep("Verifying successful registration");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = JsonSerializer.Deserialize<JsonElement>(content);
            result.GetProperty("message").GetString().Should().Be("User registered successfully");

            LogTestEnd(nameof(Register_WithValidData_CreatesNewUser), true);
        }
        catch (Exception ex)
        {
            Output.WriteLine($"🚨 ERROR: {ex.Message}");
            LogTestEnd(nameof(Register_WithValidData_CreatesNewUser), false);
            throw;
        }
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsBadRequest()
    {
        LogTestStart(nameof(Register_WithExistingEmail_ReturnsBadRequest));

        try
        {
            // Arrange
            LogStep("Preparing registration request with existing email");
            var registerRequest = new
            {
                FirstName = "Another",
                LastName = "User",
                Email = "test@example.com", // This email already exists in test data
                Password = "password123"
            };

            // Act
            LogStep("Sending POST request to /api/auth/register with existing email");
            LogRequest("POST", "/api/auth/register", registerRequest);
            
            var response = await Client.PostAsync("/api/auth/register", CreateJsonContent(registerRequest));
            var content = await response.Content.ReadAsStringAsync();
            
            LogResponse(response, content);

            // Assert
            LogStep("Verifying bad request response");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            LogTestEnd(nameof(Register_WithExistingEmail_ReturnsBadRequest), true);
        }
        catch (Exception ex)
        {
            Output.WriteLine($"🚨 ERROR: {ex.Message}");
            LogTestEnd(nameof(Register_WithExistingEmail_ReturnsBadRequest), false);
            throw;
        }
    }
}