using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Xunit.Abstractions;
using WebAPI.Data;

namespace Integration.Tests.WebApp;

public abstract class WebAppIntegrationTestBase : IClassFixture<WebApplicationFactory<WebApp.Program>>
{
    protected readonly WebApplicationFactory<WebApp.Program> Factory;
    protected readonly HttpClient Client;
    protected readonly ITestOutputHelper Output;

    protected WebAppIntegrationTestBase(WebApplicationFactory<WebApp.Program> factory, ITestOutputHelper output)
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
                    options.UseInMemoryDatabase("WebAppIntegrationTestDb");
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
        Output.WriteLine($"🌐 WEBAPP INTEGRATION TEST: {testName}");
        Output.WriteLine($"⏰ Started at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC");
        Output.WriteLine("=".PadRight(80, '='));
    }

    protected void LogStep(string step)
    {
        Output.WriteLine($"📋 STEP: {step}");
    }

    protected void LogRequest(string method, string url)
    {
        Output.WriteLine($"🌐 {method} {url}");
    }

    protected void LogResponse(HttpResponseMessage response, int contentLength = 0)
    {
        Output.WriteLine($"📨 Response: {(int)response.StatusCode} {response.StatusCode}");
        if (contentLength > 0)
        {
            Output.WriteLine($"📊 Content Length: {contentLength} bytes");
        }
    }

    protected void LogTestEnd(string testName, bool success)
    {
        Output.WriteLine("=".PadRight(80, '='));
        Output.WriteLine($"{(success ? "✅" : "❌")} WEBAPP TEST {(success ? "PASSED" : "FAILED")}: {testName}");
        Output.WriteLine($"⏰ Completed at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC");
        Output.WriteLine("=".PadRight(80, '='));
    }

    private void SeedTestData(TravelContext context)
    {
        // Clear existing data
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Seed test data for WebApp
        var testUser = new WebAPI.Models.User
        {
            Id = 1,
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("password123"),
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
            Name = "WebApp Test Trip",
            Description = "A test trip for WebApp integration testing",
            StartDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow.AddDays(37),
            Price = 1800.00m,
            MaxParticipants = 25,
            UserId = 2
        };

        context.Trips.Add(testTrip);
        context.SaveChanges();
    }
}