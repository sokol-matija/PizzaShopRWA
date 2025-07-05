# RWA Exam Questions - Database & Entity Framework

## 🗄️ **Database Design & Architecture**

### **Question 1: Database Schema Overview**
**Q:** Describe your database schema. What entities do you have and how are they related?

**A:** Our database has **7 main entities** with the following relationships:

**Entities:**
1. **Users** - System users (admin/regular)
2. **Destinations** - Travel destinations
3. **Trips** - Travel packages
4. **Guides** - Tour guides
5. **TripGuides** - Many-to-many junction table
6. **TripRegistrations** - User bookings
7. **Logs** - System activity logs

**Relationships:**
- **Destination → Trips** (One-to-Many)
- **User → TripRegistrations** (One-to-Many)
- **Trip → TripRegistrations** (One-to-Many)
- **Trip ↔ Guide** (Many-to-Many via TripGuides)
- **Logs** (Independent, no foreign keys)

### **Question 2: Entity Relationship Design**
**Q:** Explain the many-to-many relationship between Trips and Guides. Why did you design it this way?

**A:** We implemented **explicit many-to-many** with a junction table:

**TripGuide Entity:**
```csharp
public class TripGuide
{
    public int TripId { get; set; }
    public Trip Trip { get; set; }
    
    public int GuideId { get; set; }
    public Guide Guide { get; set; }
}
```

**Configuration:**
```csharp
modelBuilder.Entity<TripGuide>()
    .HasKey(tg => new { tg.TripId, tg.GuideId });

modelBuilder.Entity<TripGuide>()
    .HasOne(tg => tg.Trip)
    .WithMany(t => t.TripGuides)
    .HasForeignKey(tg => tg.TripId);
```

**Why explicit junction table?**
- **Future extensibility**: Can add fields like AssignedDate, Role
- **Better control**: Explicit relationship management
- **Performance**: Better for complex queries
- **Clarity**: Obvious relationship structure

### **Question 3: Primary Keys & Indexes**
**Q:** How do you handle primary keys and what indexing strategy do you use?

**A:** **Identity-based primary keys** with strategic indexing:

**Primary Keys:**
```csharp
public class Trip
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
}
```

**Indexes for Performance:**
```csharp
// Unique constraints
modelBuilder.Entity<User>()
    .HasIndex(u => u.Username)
    .IsUnique();

modelBuilder.Entity<User>()
    .HasIndex(u => u.Email)
    .IsUnique();

// Foreign key indexes (automatic)
// - Trip.DestinationId
// - TripRegistration.UserId
// - TripRegistration.TripId
```

**Why this approach?**
- **Identity columns**: Auto-incrementing, guaranteed unique
- **Unique indexes**: Prevent duplicate usernames/emails
- **Foreign key indexes**: Faster joins and lookups

---

## 🔧 **Entity Framework Configuration**

### **Question 4: DbContext Configuration**
**Q:** How did you configure your Entity Framework DbContext? Show the setup.

**A:** **ApplicationDbContext** with explicit configuration:

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Destination> Destinations { get; set; }
    public DbSet<Trip> Trips { get; set; }
    public DbSet<Guide> Guides { get; set; }
    public DbSet<TripGuide> TripGuides { get; set; }
    public DbSet<TripRegistration> TripRegistrations { get; set; }
    public DbSet<Log> Logs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships and constraints
        ConfigureUserEntity(modelBuilder);
        ConfigureTripGuideEntity(modelBuilder);
        // ... other configurations
    }
}
```

**Registration in Program.cs:**
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### **Question 5: Connection String Management**
**Q:** How do you manage database connections across different environments?

**A:** **Environment-specific connection strings**:

**Development (appsettings.json):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=TravelOrganizationDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

**Production (appsettings.Production.json):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "#{AZURE_SQL_CONNECTION_STRING}#"
  }
}
```

**Benefits:**
- **Security**: Production secrets not in source code
- **Flexibility**: Different databases per environment
- **Deployment**: Easy environment switching
- **Local development**: Simple local SQL Server setup

### **Question 6: Entity Relationships Configuration**
**Q:** Show how you configure entity relationships in Entity Framework.

**A:** **Fluent API configuration** for explicit control:

**One-to-Many (Destination → Trips):**
```csharp
modelBuilder.Entity<Trip>()
    .HasOne(t => t.Destination)
    .WithMany(d => d.Trips)
    .HasForeignKey(t => t.DestinationId)
    .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete
```

**Many-to-Many (Trip ↔ Guide):**
```csharp
modelBuilder.Entity<TripGuide>()
    .HasKey(tg => new { tg.TripId, tg.GuideId });

modelBuilder.Entity<TripGuide>()
    .HasOne(tg => tg.Trip)
    .WithMany(t => t.TripGuides)
    .HasForeignKey(tg => tg.TripId);
```

**Unique Constraints:**
```csharp
modelBuilder.Entity<User>()
    .HasIndex(u => u.Username)
    .IsUnique();
```

---

## 📊 **Data Access Patterns**

### **Question 7: Repository vs Service Pattern**
**Q:** What data access pattern did you use? Why did you choose this approach?

**A:** We used the **Service Pattern** directly with DbContext:

**Service Implementation:**
```csharp
public class TripService : ITripService
{
    private readonly ApplicationDbContext _context;
    
    public async Task<IEnumerable<Trip>> GetAllTripsAsync()
    {
        return await _context.Trips
            .Include(t => t.Destination)
            .Include(t => t.TripGuides)
                .ThenInclude(tg => tg.Guide)
            .ToListAsync();
    }
}
```

**Why Service Pattern over Repository?**
- **Simplicity**: Fewer abstraction layers
- **Entity Framework**: Already provides repository-like functionality
- **Performance**: Direct control over queries
- **Flexibility**: Can easily add business logic
- **Modern approach**: EF Core is the abstraction layer

### **Question 8: Async/Await in Data Access**
**Q:** Why do you use async/await for all database operations? Show examples.

**A:** **Async operations** for better scalability:

**Async Query Examples:**
```csharp
// Single entity
public async Task<Trip> GetTripByIdAsync(int id)
{
    return await _context.Trips
        .Include(t => t.Destination)
        .FirstOrDefaultAsync(t => t.Id == id);
}

// Collection
public async Task<List<User>> GetAllUsersAsync()
{
    return await _context.Users.ToListAsync();
}

// Create operation
public async Task<Trip> CreateTripAsync(Trip trip)
{
    _context.Trips.Add(trip);
    await _context.SaveChangesAsync();
    return trip;
}
```

**Benefits:**
- **Scalability**: Non-blocking I/O operations
- **Performance**: Server can handle more concurrent requests
- **Resource efficiency**: Threads not blocked during DB calls
- **User experience**: Responsive application under load

### **Question 9: Eager Loading vs Lazy Loading**
**Q:** How do you handle related data loading? What strategy do you use and why?

**A:** We use **explicit eager loading** with `Include()`:

**Eager Loading Examples:**
```csharp
// Load trip with destination and guides
var trip = await _context.Trips
    .Include(t => t.Destination)
    .Include(t => t.TripGuides)
        .ThenInclude(tg => tg.Guide)
    .FirstOrDefaultAsync(t => t.Id == id);

// Load user with their registrations
var user = await _context.Users
    .Include(u => u.TripRegistrations)
        .ThenInclude(tr => tr.Trip)
    .FirstOrDefaultAsync(u => u.Id == userId);
```

**Why Eager Loading?**
- **Performance**: Avoid N+1 query problems
- **Predictability**: Know exactly what data is loaded
- **Control**: Explicit about what relationships to load
- **Efficiency**: Single database round trip

**When to use what:**
- **Eager Loading**: When you know you need related data
- **Explicit Loading**: For conditional loading
- **Lazy Loading**: Avoided due to unpredictable behavior

---

## 🔍 **Querying & Performance**

### **Question 10: Complex Queries**
**Q:** Show examples of complex queries in your application. How do you optimize them?

**A:** **Optimized queries** with strategic loading:

**Complex Trip Query:**
```csharp
public async Task<(IEnumerable<Trip> trips, int totalCount)> GetTripsAsync(
    int page, int pageSize, int? destinationId = null)
{
    var query = _context.Trips
        .Include(t => t.Destination)
        .Include(t => t.TripGuides)
            .ThenInclude(tg => tg.Guide)
        .AsQueryable();

    // Apply filtering
    if (destinationId.HasValue)
    {
        query = query.Where(t => t.DestinationId == destinationId.Value);
    }

    // Get total count before pagination
    var totalCount = await query.CountAsync();

    // Apply pagination
    var trips = await query
        .OrderBy(t => t.StartDate)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return (trips, totalCount);
}
```

**Search with Multiple Criteria:**
```csharp
public async Task<IEnumerable<Guide>> SearchGuidesAsync(string searchTerm)
{
    return await _context.Guides
        .Where(g => 
            g.FirstName.Contains(searchTerm) ||
            g.LastName.Contains(searchTerm) ||
            g.Email.Contains(searchTerm))
        .OrderBy(g => g.FirstName)
        .ToListAsync();
}
```

### **Question 11: Pagination Implementation**
**Q:** How do you implement pagination in your database queries?

**A:** **Efficient pagination** with count optimization:

**Pagination Pattern:**
```csharp
public async Task<(IEnumerable<T> items, int totalCount)> GetPagedAsync<T>(
    IQueryable<T> query, int page, int pageSize)
{
    // Get total count (expensive operation)
    var totalCount = await query.CountAsync();
    
    // Get paged results
    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return (items, totalCount);
}
```

**Smart Pagination (only when needed):**
```csharp
// Only show pagination if more than one page
@if (Model.TotalPages > 1)
{
    <nav aria-label="Page navigation">
        <!-- Pagination controls -->
    </nav>
}
```

**Benefits:**
- **Performance**: Only load needed records
- **Memory**: Reduced memory usage
- **UX**: Faster page loads
- **Scalability**: Handles large datasets

### **Question 12: Transaction Management**
**Q:** How do you handle database transactions? When do you use them?

**A:** **Automatic and explicit transaction management**:

**Automatic Transactions (SaveChanges):**
```csharp
// Single operation - automatic transaction
public async Task<Trip> CreateTripAsync(Trip trip)
{
    _context.Trips.Add(trip);
    await _context.SaveChangesAsync(); // Automatic transaction
    return trip;
}
```

**Explicit Transactions (Multiple operations):**
```csharp
public async Task<bool> TransferTripRegistrationAsync(int fromUserId, int toUserId, int tripId)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // Cancel old registration
        var oldRegistration = await _context.TripRegistrations
            .FirstOrDefaultAsync(tr => tr.UserId == fromUserId && tr.TripId == tripId);
        oldRegistration.Status = "Cancelled";
        
        // Create new registration
        var newRegistration = new TripRegistration
        {
            UserId = toUserId,
            TripId = tripId,
            Status = "Confirmed"
        };
        _context.TripRegistrations.Add(newRegistration);
        
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }
    catch
    {
        await transaction.RollbackAsync();
        return false;
    }
}
```

**When to use transactions:**
- **Multiple related operations** that must succeed together
- **Data consistency** requirements
- **Complex business operations** spanning multiple entities

---

## 🛡️ **Data Validation & Constraints**

### **Question 13: Entity Validation**
**Q:** How do you implement validation at the database level? Show examples.

**A:** **Multi-layer validation** approach:

**Data Annotations:**
```csharp
public class User
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Username { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    [StringLength(100)]
    public string? FirstName { get; set; }

    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
}
```

**Fluent API Constraints:**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<User>()
        .HasIndex(u => u.Username)
        .IsUnique();
        
    modelBuilder.Entity<User>()
        .HasIndex(u => u.Email)
        .IsUnique();
        
    modelBuilder.Entity<Trip>()
        .Property(t => t.Price)
        .HasColumnType("decimal(18,2)");
}
```

**Business Logic Validation:**
```csharp
public async Task<Trip> CreateTripAsync(Trip trip)
{
    // Business rule validation
    if (trip.StartDate <= DateTime.Now)
        throw new ValidationException("Start date must be in the future");
        
    if (trip.EndDate <= trip.StartDate)
        throw new ValidationException("End date must be after start date");
        
    _context.Trips.Add(trip);
    await _context.SaveChangesAsync();
    return trip;
}
```

### **Question 14: Handling Constraint Violations**
**Q:** How do you handle database constraint violations and provide user-friendly error messages?

**A:** **Exception handling** with user-friendly messages:

**Service-Level Handling:**
```csharp
public async Task<User> RegisterAsync(RegisterDTO model)
{
    try
    {
        // Check if username exists
        if (await _context.Users.AnyAsync(u => u.Username == model.Username))
        {
            throw new ValidationException("Username already exists");
        }
        
        // Check if email exists
        if (await _context.Users.AnyAsync(u => u.Email == model.Email))
        {
            throw new ValidationException("Email already exists");
        }
        
        var user = new User { /* ... */ };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
    catch (DbUpdateException ex)
    {
        // Handle database-level constraint violations
        if (ex.InnerException?.Message.Contains("UNIQUE constraint") == true)
        {
            throw new ValidationException("Username or email already exists");
        }
        throw;
    }
}
```

**Controller-Level Handling:**
```csharp
try
{
    var user = await _userService.RegisterAsync(model);
    return Ok(new { message = "Registration successful" });
}
catch (ValidationException ex)
{
    return BadRequest(ex.Message);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Registration error");
    return StatusCode(500, "Internal server error");
}
```

---

## 📈 **Performance & Optimization**

### **Question 15: Query Performance Optimization**
**Q:** What strategies do you use to optimize database query performance?

**A:** **Multiple optimization strategies**:

**1. Proper Indexing:**
```csharp
// Unique indexes for lookups
modelBuilder.Entity<User>()
    .HasIndex(u => u.Username)
    .IsUnique();

// Composite indexes for common queries
modelBuilder.Entity<TripRegistration>()
    .HasIndex(tr => new { tr.UserId, tr.Status });
```

**2. Selective Loading:**
```csharp
// Only load what you need
public async Task<IEnumerable<TripSummary>> GetTripSummariesAsync()
{
    return await _context.Trips
        .Select(t => new TripSummary
        {
            Id = t.Id,
            Name = t.Name,
            Price = t.Price,
            DestinationName = t.Destination.Name
        })
        .ToListAsync();
}
```

**3. Efficient Counting:**
```csharp
// Use Any() instead of Count() for existence checks
var hasBookings = await _context.TripRegistrations
    .AnyAsync(tr => tr.UserId == userId);
```

**4. Batch Operations:**
```csharp
// Bulk operations where possible
_context.TripRegistrations.AddRange(registrations);
await _context.SaveChangesAsync();
```

### **Question 16: Memory Management**
**Q:** How do you handle memory management and prevent memory leaks in EF Core?

**A:** **Proper disposal and scoped lifetimes**:

**Dependency Injection (Scoped):**
```csharp
// Services registered as Scoped
builder.Services.AddScoped<ITripService, TripService>();
```
**Benefits:** DbContext automatically disposed at end of request

**Explicit Disposal (when needed):**
```csharp
public async Task<List<Trip>> GetTripsWithCustomContextAsync()
{
    using var context = new ApplicationDbContext(options);
    return await context.Trips.ToListAsync();
} // Context automatically disposed
```

**Avoiding Memory Leaks:**
```csharp
// Don't hold references to entities outside of context scope
public async Task<TripModel> GetTripModelAsync(int id)
{
    var trip = await _context.Trips.FindAsync(id);
    
    // Map to model (no EF tracking)
    return new TripModel
    {
        Id = trip.Id,
        Name = trip.Name,
        // ... other properties
    };
}
```

### **Question 17: Database Migration Strategy**
**Q:** How do you handle database migrations and schema changes?

**A:** **Code-First migrations** with version control:

**Creating Migrations:**
```bash
# Add new migration
dotnet ef migrations add AddTripImageUrl

# Update database
dotnet ef database update
```

**Migration Files:**
```csharp
public partial class AddTripImageUrl : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ImageUrl",
            table: "Trips",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true);
    }
    
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ImageUrl",
            table: "Trips");
    }
}
```

**Production Deployment:**
```csharp
// Apply migrations on startup (development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}
```

**Benefits:**
- **Version control**: Migrations tracked in source control
- **Rollback capability**: Down methods for reversing changes
- **Team collaboration**: Consistent schema across environments
- **Automated deployment**: Can be applied during deployment 