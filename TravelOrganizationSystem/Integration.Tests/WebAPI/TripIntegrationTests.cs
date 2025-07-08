using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;
using FluentAssertions;
using System.Net.Http.Headers;

namespace Integration.Tests.WebAPI;

public class TripIntegrationTests : IntegrationTestBase
{
    public TripIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output) 
        : base(factory, output)
    {
    }

    private async Task<string> GetAuthTokenAsync(string email = "admin@example.com", string password = "admin123")
    {
        var loginRequest = new { Email = email, Password = password };
        var response = await Client.PostAsync("/api/auth/login", CreateJsonContent(loginRequest));
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        return result.GetProperty("token").GetString()!;
    }

    [Fact]
    public async Task GetTrips_WithoutAuth_ReturnsUnauthorized()
    {
        LogTestStart(nameof(GetTrips_WithoutAuth_ReturnsUnauthorized));

        try
        {
            // Act
            LogStep("Sending GET request to /api/trips without authorization");
            LogRequest("GET", "/api/trips");
            
            var response = await Client.GetAsync("/api/trips");
            var content = await response.Content.ReadAsStringAsync();
            
            LogResponse(response, content);

            // Assert
            LogStep("Verifying unauthorized response");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            LogTestEnd(nameof(GetTrips_WithoutAuth_ReturnsUnauthorized), true);
        }
        catch (Exception ex)
        {
            Output.WriteLine($"🚨 ERROR: {ex.Message}");
            LogTestEnd(nameof(GetTrips_WithoutAuth_ReturnsUnauthorized), false);
            throw;
        }
    }

    [Fact]
    public async Task GetTrips_WithAuth_ReturnsTrips()
    {
        LogTestStart(nameof(GetTrips_WithAuth_ReturnsTrips));

        try
        {
            // Arrange
            LogStep("Getting authentication token");
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            LogStep("Sending GET request to /api/trips with authorization");
            LogRequest("GET", "/api/trips");
            
            var response = await Client.GetAsync("/api/trips");
            var content = await response.Content.ReadAsStringAsync();
            
            LogResponse(response, content);

            // Assert
            LogStep("Verifying successful response and trip data");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().NotBeNullOrEmpty();

            var trips = JsonSerializer.Deserialize<JsonElement[]>(content);
            trips.Should().NotBeEmpty();
            trips.Should().HaveCountGreaterThan(0);

            LogTestEnd(nameof(GetTrips_WithAuth_ReturnsTrips), true);
        }
        catch (Exception ex)
        {
            Output.WriteLine($"🚨 ERROR: {ex.Message}");
            LogTestEnd(nameof(GetTrips_WithAuth_ReturnsTrips), false);
            throw;
        }
        finally
        {
            Client.DefaultRequestHeaders.Authorization = null;
        }
    }

    [Fact]
    public async Task GetTripById_WithValidId_ReturnsTrip()
    {
        LogTestStart(nameof(GetTripById_WithValidId_ReturnsTrip));

        try
        {
            // Arrange
            LogStep("Getting authentication token");
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            LogStep("Sending GET request to /api/trips/1");
            LogRequest("GET", "/api/trips/1");
            
            var response = await Client.GetAsync("/api/trips/1");
            var content = await response.Content.ReadAsStringAsync();
            
            LogResponse(response, content);

            // Assert
            LogStep("Verifying successful response and trip details");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var trip = JsonSerializer.Deserialize<JsonElement>(content);
            trip.GetProperty("id").GetInt32().Should().Be(1);
            trip.GetProperty("name").GetString().Should().Be("Test Trip");

            LogTestEnd(nameof(GetTripById_WithValidId_ReturnsTrip), true);
        }
        catch (Exception ex)
        {
            Output.WriteLine($"🚨 ERROR: {ex.Message}");
            LogTestEnd(nameof(GetTripById_WithValidId_ReturnsTrip), false);
            throw;
        }
        finally
        {
            Client.DefaultRequestHeaders.Authorization = null;
        }
    }

    [Fact]
    public async Task GetTripById_WithInvalidId_ReturnsNotFound()
    {
        LogTestStart(nameof(GetTripById_WithInvalidId_ReturnsNotFound));

        try
        {
            // Arrange
            LogStep("Getting authentication token");
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            LogStep("Sending GET request to /api/trips/999 (non-existent ID)");
            LogRequest("GET", "/api/trips/999");
            
            var response = await Client.GetAsync("/api/trips/999");
            var content = await response.Content.ReadAsStringAsync();
            
            LogResponse(response, content);

            // Assert
            LogStep("Verifying not found response");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            LogTestEnd(nameof(GetTripById_WithInvalidId_ReturnsNotFound), true);
        }
        catch (Exception ex)
        {
            Output.WriteLine($"🚨 ERROR: {ex.Message}");
            LogTestEnd(nameof(GetTripById_WithInvalidId_ReturnsNotFound), false);
            throw;
        }
        finally
        {
            Client.DefaultRequestHeaders.Authorization = null;
        }
    }

    [Fact]
    public async Task CreateTrip_AsAdmin_CreatesNewTrip()
    {
        LogTestStart(nameof(CreateTrip_AsAdmin_CreatesNewTrip));

        try
        {
            // Arrange
            LogStep("Getting admin authentication token");
            var token = await GetAuthTokenAsync("admin@example.com", "admin123");
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var newTrip = new
            {
                Name = "Integration Test Trip",
                Description = "A trip created during integration testing",
                StartDate = DateTime.UtcNow.AddDays(60).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                EndDate = DateTime.UtcNow.AddDays(67).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Price = 2500.00,
                MaxParticipants = 15
            };

            // Act
            LogStep("Sending POST request to /api/trips");
            LogRequest("POST", "/api/trips", newTrip);
            
            var response = await Client.PostAsync("/api/trips", CreateJsonContent(newTrip));
            var content = await response.Content.ReadAsStringAsync();
            
            LogResponse(response, content);

            // Assert
            LogStep("Verifying successful trip creation");
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var createdTrip = JsonSerializer.Deserialize<JsonElement>(content);
            createdTrip.GetProperty("name").GetString().Should().Be("Integration Test Trip");
            createdTrip.GetProperty("price").GetDecimal().Should().Be(2500.00m);

            LogTestEnd(nameof(CreateTrip_AsAdmin_CreatesNewTrip), true);
        }
        catch (Exception ex)
        {
            Output.WriteLine($"🚨 ERROR: {ex.Message}");
            LogTestEnd(nameof(CreateTrip_AsAdmin_CreatesNewTrip), false);
            throw;
        }
        finally
        {
            Client.DefaultRequestHeaders.Authorization = null;
        }
    }

    [Fact]
    public async Task CreateTrip_AsUser_ReturnsForbidden()
    {
        LogTestStart(nameof(CreateTrip_AsUser_ReturnsForbidden));

        try
        {
            // Arrange
            LogStep("Getting user authentication token (not admin)");
            var token = await GetAuthTokenAsync("test@example.com", "password123");
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var newTrip = new
            {
                Name = "Unauthorized Trip",
                Description = "This should fail",
                StartDate = DateTime.UtcNow.AddDays(60).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                EndDate = DateTime.UtcNow.AddDays(67).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Price = 1000.00,
                MaxParticipants = 10
            };

            // Act
            LogStep("Sending POST request to /api/trips as regular user");
            LogRequest("POST", "/api/trips", newTrip);
            
            var response = await Client.PostAsync("/api/trips", CreateJsonContent(newTrip));
            var content = await response.Content.ReadAsStringAsync();
            
            LogResponse(response, content);

            // Assert
            LogStep("Verifying forbidden response for non-admin user");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            LogTestEnd(nameof(CreateTrip_AsUser_ReturnsForbidden), true);
        }
        catch (Exception ex)
        {
            Output.WriteLine($"🚨 ERROR: {ex.Message}");
            LogTestEnd(nameof(CreateTrip_AsUser_ReturnsForbidden), false);
            throw;
        }
        finally
        {
            Client.DefaultRequestHeaders.Authorization = null;
        }
    }
}