using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit.Abstractions;
using FluentAssertions;

namespace Integration.Tests.WebApp;

public class HomePageIntegrationTests : WebAppIntegrationTestBase
{
    public HomePageIntegrationTests(WebApplicationFactory<WebApp.Program> factory, ITestOutputHelper output) 
        : base(factory, output)
    {
    }

    [Fact]
    public async Task HomePage_Get_ReturnsSuccessAndCorrectContentType()
    {
        LogTestStart(nameof(HomePage_Get_ReturnsSuccessAndCorrectContentType));

        try
        {
            // Act
            LogStep("Sending GET request to home page");
            LogRequest("GET", "/");
            
            var response = await Client.GetAsync("/");
            var content = await response.Content.ReadAsStringAsync();
            
            LogResponse(response, content.Length);

            // Assert
            LogStep("Verifying successful response and HTML content");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType?.ToString().Should().StartWith("text/html");
            content.Should().Contain("Travel Organization System");

            LogTestEnd(nameof(HomePage_Get_ReturnsSuccessAndCorrectContentType), true);
        }
        catch (Exception ex)
        {
            Output.WriteLine($"🚨 ERROR: {ex.Message}");
            LogTestEnd(nameof(HomePage_Get_ReturnsSuccessAndCorrectContentType), false);
            throw;
        }
    }

    [Fact]
    public async Task AboutPage_Get_ReturnsSuccessWithContent()
    {
        LogTestStart(nameof(AboutPage_Get_ReturnsSuccessWithContent));

        try
        {
            // Act
            LogStep("Sending GET request to about page");
            LogRequest("GET", "/Home/About");
            
            var response = await Client.GetAsync("/Home/About");
            var content = await response.Content.ReadAsStringAsync();
            
            LogResponse(response, content.Length);

            // Assert
            LogStep("Verifying about page response");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("About");

            LogTestEnd(nameof(AboutPage_Get_ReturnsSuccessWithContent), true);
        }
        catch (Exception ex)
        {
            Output.WriteLine($"🚨 ERROR: {ex.Message}");
            LogTestEnd(nameof(AboutPage_Get_ReturnsSuccessWithContent), false);
            throw;
        }
    }

    [Fact]
    public async Task ContactPage_Get_ReturnsSuccessWithContent()
    {
        LogTestStart(nameof(ContactPage_Get_ReturnsSuccessWithContent));

        try
        {
            // Act
            LogStep("Sending GET request to contact page");
            LogRequest("GET", "/Home/Contact");
            
            var response = await Client.GetAsync("/Home/Contact");
            var content = await response.Content.ReadAsStringAsync();
            
            LogResponse(response, content.Length);

            // Assert
            LogStep("Verifying contact page response");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Contact");

            LogTestEnd(nameof(ContactPage_Get_ReturnsSuccessWithContent), true);
        }
        catch (Exception ex)
        {
            Output.WriteLine($"🚨 ERROR: {ex.Message}");
            LogTestEnd(nameof(ContactPage_Get_ReturnsSuccessWithContent), false);
            throw;
        }
    }

    [Fact]
    public async Task NonExistentPage_Get_ReturnsNotFound()
    {
        LogTestStart(nameof(NonExistentPage_Get_ReturnsNotFound));

        try
        {
            // Act
            LogStep("Sending GET request to non-existent page");
            LogRequest("GET", "/NonExistent/Page");
            
            var response = await Client.GetAsync("/NonExistent/Page");
            var content = await response.Content.ReadAsStringAsync();
            
            LogResponse(response, content.Length);

            // Assert
            LogStep("Verifying 404 not found response");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            LogTestEnd(nameof(NonExistentPage_Get_ReturnsNotFound), true);
        }
        catch (Exception ex)
        {
            Output.WriteLine($"🚨 ERROR: {ex.Message}");
            LogTestEnd(nameof(NonExistentPage_Get_ReturnsNotFound), false);
            throw;
        }
    }
}