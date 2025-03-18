using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{
		}

		public DbSet<Food> Foods { get; set; }
		public DbSet<FoodCategory> FoodCategories { get; set; }
		public DbSet<Allergen> Allergens { get; set; }
		public DbSet<FoodAllergen> FoodAllergens { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<Order> Orders { get; set; }
		public DbSet<OrderItem> OrderItems { get; set; }
		public DbSet<Log> Logs { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Configure table names to be singular (match database schema)
			modelBuilder.Entity<Food>().ToTable("Food");
			modelBuilder.Entity<FoodCategory>().ToTable("FoodCategory");
			modelBuilder.Entity<Allergen>().ToTable("Allergen");
			modelBuilder.Entity<FoodAllergen>().ToTable("FoodAllergen");
			modelBuilder.Entity<User>().ToTable("User");
			modelBuilder.Entity<Order>().ToTable("Order");
			modelBuilder.Entity<OrderItem>().ToTable("OrderItem");
			modelBuilder.Entity<Log>().ToTable("Log");

			// Configure many-to-many relationship for Food and Allergen
			modelBuilder.Entity<FoodAllergen>()
				.HasKey(fa => new { fa.FoodId, fa.AllergenId });

			modelBuilder.Entity<FoodAllergen>()
				.HasOne(fa => fa.Food)
				.WithMany(f => f.FoodAllergens)
				.HasForeignKey(fa => fa.FoodId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<FoodAllergen>()
				.HasOne(fa => fa.Allergen)
				.WithMany(a => a.FoodAllergens)
				.HasForeignKey(fa => fa.AllergenId)
				.OnDelete(DeleteBehavior.Cascade);

			// Configure one-to-many relationship between FoodCategory and Food
			modelBuilder.Entity<Food>()
				.HasOne(f => f.FoodCategory)
				.WithMany(c => c.Foods)
				.HasForeignKey(f => f.FoodCategoryId)
				.OnDelete(DeleteBehavior.Restrict);

			// Configure one-to-many relationship between User and Order
			modelBuilder.Entity<Order>()
				.HasOne(o => o.User)
				.WithMany(u => u.Orders)
				.HasForeignKey(o => o.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			// Configure one-to-many relationship between Order and OrderItem
			modelBuilder.Entity<OrderItem>()
				.HasOne(oi => oi.Order)
				.WithMany(o => o.OrderItems)
				.HasForeignKey(oi => oi.OrderId)
				.OnDelete(DeleteBehavior.Cascade);

			// Configure one-to-many relationship between Food and OrderItem
			modelBuilder.Entity<OrderItem>()
				.HasOne(oi => oi.Food)
				.WithMany(f => f.OrderItems)
				.HasForeignKey(oi => oi.FoodId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}