using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using Xunit.Abstractions;
using FluentAssertions;
using System.Net.Http.Headers;

namespace Integration.Tests.EndToEnd;

public class FullFlowIntegrationTests : IntegrationTestBase
{
    public FullFlowIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output) 
        : base(factory, output)
    {
    }

    [Fact]
    public async Task FullUserJourney_RegisterLoginCreateBooking_CompletesSuccessfully()
    {
        LogTestStart(nameof(FullUserJourney_RegisterLoginCreateBooking_CompletesSuccessfully));

        try
        {
            // Step 1: Register a new user
            LogStep("🟢 STEP 1: Registering new user");
            var registerRequest = new
            {
                FirstName = "John",
                LastName = "Traveler",
                Email = $"johntraveler{DateTime.UtcNow.Ticks}@example.com", // Unique email
                Password = "JohnPassword123!"
            };

            LogRequest("POST", "/api/auth/register", registerRequest);
            var registerResponse = await Client.PostAsync("/api/auth/register", CreateJsonContent(registerRequest));
            var registerContent = await registerResponse.Content.ReadAsStringAsync();
            LogResponse(registerResponse, registerContent);

            registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            Output.WriteLine("✅ User registration successful");

            // Step 2: Login with the new user
            LogStep("🟢 STEP 2: Logging in with new user credentials");
            var loginRequest = new
            {
                Email = registerRequest.Email,
                Password = registerRequest.Password
            };

            LogRequest("POST", "/api/auth/login", loginRequest);
            var loginResponse = await Client.PostAsync("/api/auth/login", CreateJsonContent(loginRequest));
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            LogResponse(loginResponse, loginContent);

            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var loginResult = JsonSerializer.Deserialize<JsonElement>(loginContent);
            var userToken = loginResult.GetProperty("token").GetString();
            userToken.Should().NotBeNullOrEmpty();
            Output.WriteLine("✅ User login successful");

            // Step 3: Get admin token to create a trip
            LogStep("🟢 STEP 3: Getting admin token to create a test trip");
            var adminLoginRequest = new { Email = "admin@example.com", Password = "admin123" };
            var adminLoginResponse = await Client.PostAsync("/api/auth/login", CreateJsonContent(adminLoginRequest));
            var adminLoginContent = await adminLoginResponse.Content.ReadAsStringAsync();
            var adminLoginResult = JsonSerializer.Deserialize<JsonElement>(adminLoginContent);
            var adminToken = adminLoginResult.GetProperty("token").GetString();

            // Step 4: Create a trip as admin
            LogStep("🟢 STEP 4: Creating a test trip as admin");
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            var newTrip = new
            {
                Name = "End-to-End Test Adventure",
                Description = "A complete journey testing trip created for integration testing",
                StartDate = DateTime.UtcNow.AddDays(45).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                EndDate = DateTime.UtcNow.AddDays(52).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Price = 3500.00,
                MaxParticipants = 12
            };

            LogRequest("POST", "/api/trips", newTrip);
            var createTripResponse = await Client.PostAsync("/api/trips", CreateJsonContent(newTrip));
            var createTripContent = await createTripResponse.Content.ReadAsStringAsync();
            LogResponse(createTripResponse, createTripContent);

            createTripResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdTrip = JsonSerializer.Deserialize<JsonElement>(createTripContent);
            var tripId = createdTrip.GetProperty("id").GetInt32();
            Output.WriteLine($"✅ Trip created successfully with ID: {tripId}");

            // Step 5: Switch back to user token and view trips
            LogStep("🟢 STEP 5: Switching to user token and viewing available trips");
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

            LogRequest("GET", "/api/trips");
            var getTripsResponse = await Client.GetAsync("/api/trips");
            var getTripsContent = await getTripsResponse.Content.ReadAsStringAsync();
            LogResponse(getTripsResponse, getTripsContent);

            getTripsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var trips = JsonSerializer.Deserialize<JsonElement[]>(getTripsContent);
            trips.Should().Contain(trip => trip.GetProperty("id").GetInt32() == tripId);
            Output.WriteLine($"✅ User can view trips including the new trip (ID: {tripId})");

            // Step 6: Get specific trip details
            LogStep("🟢 STEP 6: Getting detailed information about the test trip");
            LogRequest("GET", $"/api/trips/{tripId}");
            var getTripResponse = await Client.GetAsync($"/api/trips/{tripId}");
            var getTripContent = await getTripResponse.Content.ReadAsStringAsync();
            LogResponse(getTripResponse, getTripContent);

            getTripResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var tripDetails = JsonSerializer.Deserialize<JsonElement>(getTripContent);
            tripDetails.GetProperty("name").GetString().Should().Be("End-to-End Test Adventure");
            tripDetails.GetProperty("price").GetDecimal().Should().Be(3500.00m);
            Output.WriteLine("✅ Trip details retrieved successfully");

            // Step 7: Test unauthorized access (try to create trip as user)
            LogStep("🟢 STEP 7: Testing authorization - user trying to create trip (should fail)");
            var unauthorizedTrip = new
            {
                Name = "Unauthorized Trip",
                Description = "This should fail",
                StartDate = DateTime.UtcNow.AddDays(60).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                EndDate = DateTime.UtcNow.AddDays(67).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Price = 1000.00,
                MaxParticipants = 5
            };

            LogRequest("POST", "/api/trips", unauthorizedTrip);
            var unauthorizedResponse = await Client.PostAsync("/api/trips", CreateJsonContent(unauthorizedTrip));
            LogResponse(unauthorizedResponse);

            unauthorizedResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            Output.WriteLine("✅ Authorization working correctly - user cannot create trips");

            LogTestEnd(nameof(FullUserJourney_RegisterLoginCreateBooking_CompletesSuccessfully), true);
        }
        catch (Exception ex)
        {
            Output.WriteLine($"🚨 ERROR: {ex.Message}");
            Output.WriteLine($"🚨 STACK TRACE: {ex.StackTrace}");
            LogTestEnd(nameof(FullUserJourney_RegisterLoginCreateBooking_CompletesSuccessfully), false);
            throw;
        }
        finally
        {
            Client.DefaultRequestHeaders.Authorization = null;
        }
    }

    [Fact]
    public async Task DatabaseConsistency_MultipleOperations_MaintainsDataIntegrity()
    {
        LogTestStart(nameof(DatabaseConsistency_MultipleOperations_MaintainsDataIntegrity));

        try
        {
            // Get admin token
            LogStep("🟢 STEP 1: Getting admin authentication");
            var adminLoginRequest = new { Email = "admin@example.com", Password = "admin123" };
            var adminLoginResponse = await Client.PostAsync("/api/auth/login", CreateJsonContent(adminLoginRequest));
            var adminLoginContent = await adminLoginResponse.Content.ReadAsStringAsync();
            var adminResult = JsonSerializer.Deserialize<JsonElement>(adminLoginContent);
            var adminToken = adminResult.GetProperty("token").GetString();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

            // Create multiple trips
            LogStep("🟢 STEP 2: Creating multiple trips to test data consistency");
            var tripIds = new List<int>();

            for (int i = 1; i <= 3; i++)
            {
                var trip = new
                {
                    Name = $"Data Consistency Test Trip {i}",
                    Description = $"Trip {i} for testing database consistency",
                    StartDate = DateTime.UtcNow.AddDays(30 + i * 7).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    EndDate = DateTime.UtcNow.AddDays(37 + i * 7).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    Price = 1000.00m + (i * 500),
                    MaxParticipants = 10 + i
                };

                var response = await Client.PostAsync("/api/trips", CreateJsonContent(trip));
                response.StatusCode.Should().Be(HttpStatusCode.Created);
                
                var content = await response.Content.ReadAsStringAsync();
                var createdTrip = JsonSerializer.Deserialize<JsonElement>(content);
                tripIds.Add(createdTrip.GetProperty("id").GetInt32());
                
                Output.WriteLine($"✅ Created trip {i} with ID: {tripIds.Last()}");
            }

            // Verify all trips exist
            LogStep("🟢 STEP 3: Verifying all created trips exist in database");
            var getTripsResponse = await Client.GetAsync("/api/trips");
            getTripsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var allTripsContent = await getTripsResponse.Content.ReadAsStringAsync();
            var allTrips = JsonSerializer.Deserialize<JsonElement[]>(allTripsContent);
            
            foreach (var tripId in tripIds)
            {
                allTrips.Should().Contain(trip => trip.GetProperty("id").GetInt32() == tripId);
                Output.WriteLine($"✅ Trip ID {tripId} found in trip list");
            }

            // Test individual trip retrieval
            LogStep("🟢 STEP 4: Testing individual trip retrieval for data integrity");
            foreach (var tripId in tripIds)
            {
                var getTripResponse = await Client.GetAsync($"/api/trips/{tripId}");
                getTripResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                
                var tripContent = await getTripResponse.Content.ReadAsStringAsync();
                var trip = JsonSerializer.Deserialize<JsonElement>(tripContent);
                trip.GetProperty("id").GetInt32().Should().Be(tripId);
                trip.GetProperty("name").GetString().Should().Contain("Data Consistency Test Trip");
                
                Output.WriteLine($"✅ Trip ID {tripId} individual retrieval successful");
            }

            Output.WriteLine($"✅ Database consistency verified for {tripIds.Count} trips");

            LogTestEnd(nameof(DatabaseConsistency_MultipleOperations_MaintainsDataIntegrity), true);
        }
        catch (Exception ex)
        {
            Output.WriteLine($"🚨 ERROR: {ex.Message}");
            LogTestEnd(nameof(DatabaseConsistency_MultipleOperations_MaintainsDataIntegrity), false);
            throw;
        }
        finally
        {
            Client.DefaultRequestHeaders.Authorization = null;
        }
    }
}