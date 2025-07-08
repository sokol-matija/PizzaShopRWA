using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Xunit.Abstractions;
using WebAPI.Data;

namespace Integration.Tests;

public abstract class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly ITestOutputHelper Output;

    protected IntegrationTestBase(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        Output = output;
        Factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real database context
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TravelContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<TravelContext>(options =>
                {
                    options.UseInMemoryDatabase("IntegrationTestDb");
                });

                // Build the service provider to seed data
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<TravelContext>();
                SeedTestData(context);
            });

            builder.UseEnvironment("Testing");
        });

        Client = Factory.CreateClient();
    }

    protected void LogTestStart(string testName)
    {
        Output.WriteLine("=".PadRight(80, '='));
        Output.WriteLine($"🧪 INTEGRATION TEST: {testName}");
        Output.WriteLine($"⏰ Started at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC");
        Output.WriteLine("=".PadRight(80, '='));
    }

    protected void LogStep(string step)
    {
        Output.WriteLine($"📋 STEP: {step}");
    }

    protected void LogRequest(string method, string url, object? body = null)
    {
        Output.WriteLine($"🌐 {method} {url}");
        if (body != null)
        {
            Output.WriteLine($"📊 Request Body: {JsonSerializer.Serialize(body, new JsonSerializerOptions { WriteIndented = true })}");
        }
    }

    protected void LogResponse(HttpResponseMessage response, string? content = null)
    {
        Output.WriteLine($"📨 Response: {(int)response.StatusCode} {response.StatusCode}");
        if (!string.IsNullOrEmpty(content))
        {
            Output.WriteLine($"📊 Response Body: {content}");
        }
    }

    protected StringContent CreateJsonContent(object obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    protected async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(content))
            return default;

        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    private void SeedTestData(TravelContext context)
    {
        // Clear existing data
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Seed test data
        var testUser = new WebAPI.Models.User
        {
            Id = 1,
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("password123"), // Hash the password
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        var adminUser = new WebAPI.Models.User
        {
            Id = 2,
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        };

        context.Users.AddRange(testUser, adminUser);

        var testTrip = new WebAPI.Models.Trip
        {
            Id = 1,
            Name = "Test Trip",
            Description = "A test trip for integration testing",
            StartDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow.AddDays(37),
            Price = 1500.00m,
            MaxParticipants = 20,
            UserId = 2 // Admin user
        };

        context.Trips.Add(testTrip);
        context.SaveChanges();
    }

    protected void LogTestEnd(string testName, bool success)
    {
        Output.WriteLine("=".PadRight(80, '='));
        Output.WriteLine($"{(success ? "✅" : "❌")} TEST {(success ? "PASSED" : "FAILED")}: {testName}");
        Output.WriteLine($"⏰ Completed at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC");
        Output.WriteLine("=".PadRight(80, '='));
    }
}