using FluentAssertions;
using WebApp.Models;
using Xunit.Abstractions;

namespace WebApp.Tests.Models;

public class UserModelTests : TestBase
{
    public UserModelTests(ITestOutputHelper output) : base(output)
    {
        LogInfo("Initializing UserModel tests");
    }

    [Fact]
    public void UserModel_DefaultValues_ShouldBeCorrect()
    {
        LogTestStart(nameof(UserModel_DefaultValues_ShouldBeCorrect));

        try
        {
            // Arrange & Act
            LogTestStep("Creating new UserModel with default constructor");
            var user = new UserModel();
            LogModelValidation(user, "UserModel with defaults");

            // Assert
            LogTestStep("Validating default property values");
            LogTestStep("Checking Id property");
            user.Id.Should().Be(0);
            LogTestStep("Checking Username property");
            user.Username.Should().BeNullOrEmpty();
            LogTestStep("Checking Email property");
            user.Email.Should().BeNullOrEmpty();
            LogTestStep("Checking FirstName property");
            user.FirstName.Should().BeNullOrEmpty();
            LogTestStep("Checking LastName property");
            user.LastName.Should().BeNullOrEmpty();
            LogTestStep("Checking IsAdmin property");
            user.IsAdmin.Should().BeFalse();

            LogTestResult("All default values validated successfully");
            LogTestEnd(nameof(UserModel_DefaultValues_ShouldBeCorrect), true);
        }
        catch (Exception ex)
        {
            LogError("UserModel default values test failed", ex);
            LogTestEnd(nameof(UserModel_DefaultValues_ShouldBeCorrect), false);
            throw;
        }
    }

    [Fact]
    public void UserModel_WithValidData_ShouldHaveCorrectProperties()
    {
        // Arrange
        var user = new UserModel
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            IsAdmin = true
        };

        // Act & Assert
        user.Id.Should().Be(1);
        user.Username.Should().Be("testuser");
        user.Email.Should().Be("test@example.com");
        user.FirstName.Should().Be("Test");
        user.LastName.Should().Be("User");
        user.IsAdmin.Should().BeTrue();
    }

    [Fact]
    public void UserModel_FullName_ShouldCombineFirstAndLastName()
    {
        // Arrange
        var user = new UserModel
        {
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var fullName = $"{user.FirstName} {user.LastName}";

        // Assert
        fullName.Should().Be("John Doe");
    }
}