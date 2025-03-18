using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Models;
using WebAPI.Services;
using Xunit;

namespace WebAPI.Tests.Services
{
	public class FoodServiceTests
	{
		private readonly ApplicationDbContext _context;
		private readonly Mock<ILogService> _mockLogService;
		private readonly FoodService _foodService;

		public FoodServiceTests()
		{
			// Set up in-memory database
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;

			_context = new ApplicationDbContext(options);

			// Set up mock log service
			_mockLogService = new Mock<ILogService>();

			// Create service with dependencies
			_foodService = new FoodService(_context, _mockLogService.Object);

			// Seed test data
			SeedTestData();
		}

		private void SeedTestData()
		{
			// Add test category
			var category = new FoodCategory { Id = 1, Name = "Test Category", Description = "Test Description" };
			_context.FoodCategories.Add(category);

			// Add test foods
			var foods = new List<Food>
			{
				new Food {
					Id = 1,
					Name = "Test Food 1",
					Description = "Test Description 1",
					Price = 10.99m,
					FoodCategoryId = 1
				},
				new Food {
					Id = 2,
					Name = "Test Food 2",
					Description = "Test Description 2",
					Price = 12.99m,
					FoodCategoryId = 1
				}
			};

			_context.Foods.AddRange(foods);
			_context.SaveChanges();
		}

		[Fact]
		public async Task GetByIdAsync_ExistingId_ReturnsFoodDTO()
		{
			// Arrange
			int foodId = 1;

			// Act
			var result = await _foodService.GetByIdAsync(foodId);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(foodId, result.Id);
			Assert.Equal("Test Food 1", result.Name);

			// Verify logging
			_mockLogService.Verify(x => x.LogInformationAsync(It.IsAny<string>()), Times.Once);
		}

		[Fact]
		public async Task GetByIdAsync_NonExistingId_ReturnsNull()
		{
			// Arrange
			int nonExistingFoodId = 999;

			// Act
			var result = await _foodService.GetByIdAsync(nonExistingFoodId);

			// Assert
			Assert.Null(result);

			// Verify logging
			_mockLogService.Verify(x => x.LogWarningAsync(It.IsAny<string>()), Times.Once);
		}

		[Fact]
		public async Task CreateAsync_ValidFood_ReturnsNewFoodDTO()
		{
			// Arrange
			var foodDto = new FoodCreateDTO
			{
				Name = "New Test Food",
				Description = "New Test Description",
				Price = 15.99m,
				FoodCategoryId = 1
			};

			// Act
			var result = await _foodService.CreateAsync(foodDto);

			// Assert
			Assert.NotNull(result);
			Assert.Equal("New Test Food", result.Name);
			Assert.Equal(15.99m, result.Price);

			// Verify it was saved to database
			var savedFood = await _context.Foods.FirstOrDefaultAsync(f => f.Name == "New Test Food");
			Assert.NotNull(savedFood);

			// Verify logging
			_mockLogService.Verify(x => x.LogInformationAsync(It.IsAny<string>()), Times.Once);
		}

		[Fact]
		public async Task CreateAsync_DuplicateName_ReturnsNull()
		{
			// Arrange
			var foodDto = new FoodCreateDTO
			{
				Name = "Test Food 1", // Already exists
				Description = "Duplicate Test",
				Price = 15.99m,
				FoodCategoryId = 1
			};

			// Act
			var result = await _foodService.CreateAsync(foodDto);

			// Assert
			Assert.Null(result);

			// Verify logging
			_mockLogService.Verify(x => x.LogWarningAsync(It.IsAny<string>()), Times.Once);
		}

		[Fact]
		public async Task DeleteAsync_UsedInOrder_ReturnsFalse()
		{
			// Arrange
			int foodId = 1;

			// Create order and order item referencing the food
			var order = new Order
			{
				Id = 1,
				UserId = 1,
				OrderDate = DateTime.Now,
				TotalAmount = 10.99m,
				DeliveryAddress = "Test",
				Status = "Pending"
			};

			_context.Orders.Add(order);
			await _context.SaveChangesAsync();

			var orderItem = new OrderItem
			{
				OrderId = 1,
				FoodId = foodId,
				Quantity = 1,
				Price = 10.99m
			};

			_context.OrderItems.Add(orderItem);
			await _context.SaveChangesAsync();

			// Act
			var result = await _foodService.DeleteAsync(foodId);

			// Assert
			Assert.False(result);

			// Verify food still exists
			var food = await _context.Foods.FindAsync(foodId);
			Assert.NotNull(food);

			// Verify logging
			_mockLogService.Verify(x => x.LogWarningAsync(It.IsAny<string>()), Times.Once);
		}
	}
}