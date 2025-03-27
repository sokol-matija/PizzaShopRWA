using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{
		}

		public DbSet<Destination> Destinations { get; set; }
		public DbSet<Trip> Trips { get; set; }
		public DbSet<Guide> Guides { get; set; }
		public DbSet<TripGuide> TripGuides { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<TripRegistration> TripRegistrations { get; set; }
		public DbSet<Log> Logs { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Configure table names to be singular (match database schema)
			modelBuilder.Entity<Destination>().ToTable("Destination");
			modelBuilder.Entity<Trip>().ToTable("Trip");
			modelBuilder.Entity<Guide>().ToTable("Guide");
			modelBuilder.Entity<TripGuide>().ToTable("TripGuide");
			modelBuilder.Entity<User>().ToTable("User");
			modelBuilder.Entity<TripRegistration>().ToTable("TripRegistration");
			modelBuilder.Entity<Log>().ToTable("Log");

			// Configure many-to-many relationship for Trip and Guide
			modelBuilder.Entity<TripGuide>()
				.HasKey(tg => new { tg.TripId, tg.GuideId });

			modelBuilder.Entity<TripGuide>()
				.HasOne(tg => tg.Trip)
				.WithMany(t => t.TripGuides)
				.HasForeignKey(tg => tg.TripId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<TripGuide>()
				.HasOne(tg => tg.Guide)
				.WithMany(g => g.TripGuides)
				.HasForeignKey(tg => tg.GuideId)
				.OnDelete(DeleteBehavior.Cascade);

			// Configure one-to-many relationship between Destination and Trip
			modelBuilder.Entity<Trip>()
				.HasOne(t => t.Destination)
				.WithMany(d => d.Trips)
				.HasForeignKey(t => t.DestinationId)
				.OnDelete(DeleteBehavior.Restrict);

			// Configure one-to-many relationship between User and TripRegistration
			modelBuilder.Entity<TripRegistration>()
				.HasOne(tr => tr.User)
				.WithMany(u => u.TripRegistrations)
				.HasForeignKey(tr => tr.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			// Configure one-to-many relationship between Trip and TripRegistration
			modelBuilder.Entity<TripRegistration>()
				.HasOne(tr => tr.Trip)
				.WithMany(t => t.TripRegistrations)
				.HasForeignKey(tr => tr.TripId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}