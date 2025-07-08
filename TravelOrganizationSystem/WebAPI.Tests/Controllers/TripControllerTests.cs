using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using WebAPI.Controllers;
using WebAPI.DTOs;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Tests.Controllers;

public class TripControllerTests
{
    private readonly Mock<ITripService> _mockTripService;
    private readonly Mock<ILogger<TripController>> _mockLogger;
    private readonly TripController _controller;

    public TripControllerTests()
    {
        _mockTripService = new Mock<ITripService>();
        _mockLogger = new Mock<ILogger<TripController>>();
        _controller = new TripController(_mockTripService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllTrips_ReturnsOkResultWithTripDtos()
    {
        // Arrange
        var trips = new List<Trip>
        {
            new Trip
            {
                Id = 1,
                Name = "Trip 1",
                Description = "Description 1",
                StartDate = DateTime.UtcNow.AddDays(30),
                EndDate = DateTime.UtcNow.AddDays(37),
                MaxParticipants = 20,
                Price = 1000,
                Destination = new Destination { Id = 1, Name = "Destination 1" },
                TripGuides = new List<TripGuide>()
            },
            new Trip
            {
                Id = 2,
                Name = "Trip 2",
                Description = "Description 2",
                StartDate = DateTime.UtcNow.AddDays(60),
                EndDate = DateTime.UtcNow.AddDays(67),
                MaxParticipants = 15,
                Price = 1500,
                Destination = new Destination { Id = 2, Name = "Destination 2" },
                TripGuides = new List<TripGuide>()
            }
        };

        _mockTripService.Setup(s => s.GetAllTripsAsync())
            .ReturnsAsync(trips);

        // Act
        var result = await _controller.GetAllTrips();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var tripDtos = okResult.Value as List<TripDTO>;
        
        tripDtos.Should().NotBeNull();
        tripDtos.Should().HaveCount(2);
        tripDtos[0].Name.Should().Be("Trip 1");
        tripDtos[1].Name.Should().Be("Trip 2");
    }

    [Fact]
    public async Task GetTrip_WithValidId_ReturnsOkResultWithTripDto()
    {
        // Arrange
        var tripId = 1;
        var trip = new Trip
        {
            Id = tripId,
            Name = "Test Trip",
            Description = "Test Description",
            StartDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow.AddDays(37),
            MaxParticipants = 20,
            Price = 1000,
            Destination = new Destination { Id = 1, Name = "Test Destination" },
            TripGuides = new List<TripGuide>()
        };

        _mockTripService.Setup(s => s.GetTripByIdAsync(tripId))
            .ReturnsAsync(trip);

        // Act
        var result = await _controller.GetTrip(tripId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var tripDto = okResult.Value as TripDTO;
        
        tripDto.Should().NotBeNull();
        tripDto.Id.Should().Be(tripId);
        tripDto.Name.Should().Be("Test Trip");
    }

    [Fact]
    public async Task GetTrip_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var tripId = 999;
        _mockTripService.Setup(s => s.GetTripByIdAsync(tripId))
            .ReturnsAsync((Trip)null);

        // Act
        var result = await _controller.GetTrip(tripId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetTripsByDestination_WithValidDestinationId_ReturnsOkResultWithTripDtos()
    {
        // Arrange
        var destinationId = 1;
        var trips = new List<Trip>
        {
            new Trip
            {
                Id = 1,
                Name = "Trip 1",
                Description = "Description 1",
                StartDate = DateTime.UtcNow.AddDays(30),
                EndDate = DateTime.UtcNow.AddDays(37),
                MaxParticipants = 20,
                Price = 1000,
                DestinationId = destinationId,
                Destination = new Destination { Id = destinationId, Name = "Test Destination" },
                TripGuides = new List<TripGuide>()
            }
        };

        _mockTripService.Setup(s => s.GetTripsByDestinationAsync(destinationId))
            .ReturnsAsync(trips);

        // Act
        var result = await _controller.GetTripsByDestination(destinationId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var tripDtos = okResult.Value as List<TripDTO>;
        
        tripDtos.Should().NotBeNull();
        tripDtos.Should().HaveCount(1);
        tripDtos[0].DestinationId.Should().Be(destinationId);
    }

    [Fact]
    public async Task GetTripsByDestination_WithInvalidDestinationId_ReturnsEmptyList()
    {
        // Arrange
        var destinationId = 999;
        var trips = new List<Trip>();

        _mockTripService.Setup(s => s.GetTripsByDestinationAsync(destinationId))
            .ReturnsAsync(trips);

        // Act
        var result = await _controller.GetTripsByDestination(destinationId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var tripDtos = okResult.Value as List<TripDTO>;
        
        tripDtos.Should().NotBeNull();
        tripDtos.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchTrips_WithValidParameters_ReturnsOkResultWithTripDtos()
    {
        // Arrange
        var searchName = "Beach";
        var searchDescription = "Relaxing";
        var page = 1;
        var count = 10;
        
        var trips = new List<Trip>
        {
            new Trip
            {
                Id = 1,
                Name = "Beach Paradise",
                Description = "Relaxing beach vacation",
                StartDate = DateTime.UtcNow.AddDays(30),
                EndDate = DateTime.UtcNow.AddDays(37),
                MaxParticipants = 20,
                Price = 1000,
                Destination = new Destination { Id = 1, Name = "Beach Destination" },
                TripGuides = new List<TripGuide>()
            }
        };

        _mockTripService.Setup(s => s.SearchTripsAsync(searchName, searchDescription, page, count))
            .ReturnsAsync(trips);

        // Act
        var result = await _controller.SearchTrips(searchName, searchDescription, page, count);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var tripDtos = okResult.Value as List<TripDTO>;
        
        tripDtos.Should().NotBeNull();
        tripDtos.Should().HaveCount(1);
        tripDtos[0].Name.Should().Be("Beach Paradise");
    }

    [Fact]
    public async Task SearchTrips_WithInvalidPageNumber_ReturnsBadRequest()
    {
        // Arrange
        var page = 0; // Invalid page number
        var count = 10;

        // Act
        var result = await _controller.SearchTrips(null, null, page, count);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SearchTrips_WithInvalidCount_ReturnsBadRequest()
    {
        // Arrange
        var page = 1;
        var count = 0; // Invalid count

        // Act
        var result = await _controller.SearchTrips(null, null, page, count);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SearchTrips_WithDefaultParameters_ReturnsOkResult()
    {
        // Arrange
        var trips = new List<Trip>();
        _mockTripService.Setup(s => s.SearchTripsAsync(null, null, 1, 10))
            .ReturnsAsync(trips);

        // Act
        var result = await _controller.SearchTrips(null, null, 1, 10);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _mockTripService.Verify(s => s.SearchTripsAsync(null, null, 1, 10), Times.Once);
    }
}