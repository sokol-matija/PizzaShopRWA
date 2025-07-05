# RWA Exam Questions - Backend API Architecture

## 🔧 **API Architecture & Design Patterns**

### **Question 1: API Architecture Overview**
**Q:** Explain the overall architecture of your Web API. What design pattern did you implement and why?

**A:** We implemented a **layered architecture** with the following components:
- **Controllers**: Handle HTTP requests and responses (AuthController, TripController, etc.)
- **Services**: Business logic layer (TripService, UserService, JwtService)
- **Data Access**: Entity Framework with ApplicationDbContext
- **DTOs**: Data Transfer Objects for API communication
- **Models**: Domain entities (Trip, User, Destination, etc.)

This follows the **Repository/Service pattern** for separation of concerns, making the code testable, maintainable, and following SOLID principles.

### **Question 2: Dependency Injection Configuration**
**Q:** How did you configure dependency injection in your API? Show the registration pattern.

**A:** In `Program.cs`, we register services using the built-in DI container:
```csharp
// Add application services
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IDestinationService, DestinationService>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<IGuideService, GuideService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
```
We use **AddScoped** for per-request lifetime, ensuring each HTTP request gets its own service instance.

### **Question 3: CORS Configuration**
**Q:** How did you handle Cross-Origin Resource Sharing (CORS) in your API?

**A:** We configured CORS policies for different environments:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", builder =>
    {
        builder.WithOrigins("http://localhost:17001", "https://localhost:17001")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});
```
This allows our frontend (running on port 17001) to communicate with the API while maintaining security.

---

## 🗄️ **Database & Entity Framework**

### **Question 4: Database Connection & Configuration**
**Q:** How is the database configured in your application? What connection string pattern do you use?

**A:** Database is configured in `Program.cs`:
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```
Connection string in `appsettings.json`:
```json
"ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=TravelOrganizationDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

### **Question 5: Entity Relationships**
**Q:** Describe the database relationships in your system. What types of relationships exist?

**A:** Our system has several relationship types:
- **One-to-Many**: Destination → Trips, User → TripRegistrations
- **Many-to-Many**: Trip ↔ Guide (through TripGuide junction table)
- **Self-contained**: Logs (no foreign keys)

Key entities: User, Destination, Trip, Guide, TripGuide, TripRegistration, Log

### **Question 6: Entity Framework Queries**
**Q:** Show how you handle complex queries with Entity Framework. Give an example.

**A:** Example from TripService getting trips with destinations:
```csharp
public async Task<IEnumerable<Trip>> GetAllTripsAsync()
{
    return await _context.Trips
        .Include(t => t.Destination)
        .Include(t => t.TripGuides)
            .ThenInclude(tg => tg.Guide)
        .ToListAsync();
}
```
We use `Include()` for eager loading to avoid N+1 queries and get related data in a single database call.

---

## 📋 **DTOs & Data Transfer**

### **Question 7: What are DTOs and why do you need them?**
**Q:** Explain the purpose of DTOs in your API. Why not use domain models directly?

**A:** **DTOs (Data Transfer Objects)** serve several purposes:
1. **Security**: Hide internal model structure, prevent over-posting
2. **Versioning**: API can evolve without breaking existing clients
3. **Validation**: Specific validation rules for API operations
4. **Performance**: Transfer only needed data, reduce payload size
5. **Separation**: Decouple API contract from internal domain models

Example: `CreateTripDTO` vs `Trip` entity - DTO only contains fields needed for creation.

### **Question 8: DTO Mapping Strategy**
**Q:** How do you map between DTOs and domain models? Show an example.

**A:** We use **manual mapping** for better control and performance:
```csharp
// In DestinationController.cs
var destination = new Destination
{
    Name = destinationDto.Name,
    Description = destinationDto.Description,
    Country = destinationDto.Country,
    City = destinationDto.City,
    ImageUrl = destinationDto.ImageUrl
};
```
This gives us explicit control over what gets mapped and allows for custom logic during mapping.

### **Question 9: DTO Validation**
**Q:** How do you implement validation in your DTOs? Show examples.

**A:** We use **Data Annotations** for validation:
```csharp
public class CreateTripDTO
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "MaxParticipants must be greater than 0")]
    public int MaxParticipants { get; set; }
}
```

---

## 🔐 **JWT Authentication & Security**

### **Question 10: JWT Implementation**
**Q:** How is JWT authentication implemented in your API? Explain the complete flow.

**A:** JWT implementation involves:

1. **Configuration** (Program.cs):
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true
    };
});
```

2. **Token Generation** (JwtService):
```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Name, user.Username),
    new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
};
```

### **Question 11: JWT Security Configuration**
**Q:** What JWT settings do you use and why? How do you ensure security?

**A:** JWT settings in `appsettings.json`:
```json
"JwtSettings": {
    "Secret": "YourSuperSecretKeyWithAtLeast32Characters",
    "Issuer": "TravelOrganizationAPI",
    "Audience": "TravelOrganizationClient",
    "ExpiryInMinutes": 120
}
```
- **Secret**: 32+ character key for HMAC-SHA256 signing
- **Issuer/Audience**: Validate token origin and intended recipient
- **Expiry**: 2-hour lifetime for security vs usability balance

### **Question 12: Authorization Levels**
**Q:** How do you implement different authorization levels in your API?

**A:** We use **role-based authorization**:
```csharp
[Authorize(Roles = "Admin")]  // Admin only
[Authorize]                   // Any authenticated user
// No attribute = Public access
```

Examples:
- **Public**: GET destinations, trips
- **Authenticated**: User profile, change password
- **Admin**: CRUD operations, logs access

### **Question 13: Password Security**
**Q:** How do you handle password hashing and verification?

**A:** We use **ASP.NET Core's PasswordHasher**:
```csharp
private string HashPassword(string password)
{
    var hasher = new PasswordHasher<User>();
    return hasher.HashPassword(null, password);
}

private bool VerifyPasswordHash(string password, string storedHash)
{
    var hasher = new PasswordHasher<User>();
    var result = hasher.VerifyHashedPassword(null, storedHash, password);
    return result == PasswordVerificationResult.Success;
}
```
This provides secure bcrypt-based hashing with salt.

---

## 🎛️ **Controllers & Endpoints**

### **Question 14: RESTful API Design**
**Q:** How do you follow RESTful principles in your API design? Give examples.

**A:** We follow REST conventions:
- **GET** `/api/trip` - Get all trips
- **GET** `/api/trip/{id}` - Get specific trip
- **POST** `/api/trip` - Create new trip
- **PUT** `/api/trip/{id}` - Update trip
- **DELETE** `/api/trip/{id}` - Delete trip

HTTP status codes:
- **200**: Success
- **201**: Created
- **400**: Bad Request
- **401**: Unauthorized
- **404**: Not Found
- **500**: Server Error

### **Question 15: Error Handling Strategy**
**Q:** How do you handle errors in your API controllers?

**A:** We use try-catch blocks with appropriate HTTP status codes:
```csharp
try
{
    var result = await _tripService.CreateTripAsync(trip);
    return CreatedAtAction(nameof(GetTrip), new { id = result.Id }, result);
}
catch (ValidationException ex)
{
    return BadRequest(ex.Message);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error creating trip");
    return StatusCode(500, "Internal server error");
}
```

### **Question 16: Model Validation in Controllers**
**Q:** How do you handle model validation in your controllers?

**A:** We check `ModelState.IsValid`:
```csharp
[HttpPost]
public async Task<IActionResult> CreateTrip(CreateTripDTO tripDto)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
    
    // Process valid model...
}
```
This automatically validates data annotations and returns detailed error messages.

---

## 📊 **Logging & Monitoring**

### **Question 17: Logging Implementation**
**Q:** How is logging implemented in your API? What gets logged?

**A:** We have a custom `LogService` that logs to database:
```csharp
public async Task LogInformationAsync(string message)
{
    await AddLogAsync("Information", message);
}

private async Task AddLogAsync(string level, string message)
{
    var log = new Log
    {
        Timestamp = DateTime.Now,
        Level = level,
        Message = message
    };
    _context.Logs.Add(log);
    await _context.SaveChangesAsync();
}
```

We log:
- Authentication attempts
- CRUD operations
- Errors and exceptions
- User actions

### **Question 18: Log Endpoints**
**Q:** How do you expose logs through your API? What security measures are in place?

**A:** Logs are exposed through protected endpoints:
```csharp
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class LogsController : ControllerBase
{
    [HttpGet("get/{count}")]
    public async Task<IActionResult> Get(int count)
    {
        var logs = await _logService.GetLogsAsync(count);
        return Ok(logs);
    }
}
```
Only **Admin** users can access logs for security.

---

## 🔧 **Services & Business Logic**

### **Question 19: Service Layer Design**
**Q:** Explain the purpose and design of your service layer. Why separate services from controllers?

**A:** Services contain **business logic** and **data access**:

**Benefits:**
- **Separation of Concerns**: Controllers handle HTTP, services handle business logic
- **Testability**: Services can be unit tested independently
- **Reusability**: Same service can be used by multiple controllers
- **Maintainability**: Business logic centralized in one place

Example interface:
```csharp
public interface ITripService
{
    Task<IEnumerable<Trip>> GetAllTripsAsync();
    Task<Trip> GetTripByIdAsync(int id);
    Task<Trip> CreateTripAsync(Trip trip);
    Task<Trip> UpdateTripAsync(int id, Trip trip);
    Task<bool> DeleteTripAsync(int id);
}
```

### **Question 20: Async/Await Pattern**
**Q:** Why do you use async/await throughout your API? What are the benefits?

**A:** **Async/await** provides:
1. **Scalability**: Non-blocking I/O operations
2. **Responsiveness**: Server can handle more concurrent requests
3. **Resource Efficiency**: Threads not blocked during database calls
4. **Performance**: Better throughput under load

Example:
```csharp
public async Task<Trip> GetTripByIdAsync(int id)
{
    return await _context.Trips
        .Include(t => t.Destination)
        .FirstOrDefaultAsync(t => t.Id == id);
}
```

---

## 📚 **Swagger & Documentation**

### **Question 21: Swagger Configuration**
**Q:** How did you configure Swagger for your API? What features did you implement?

**A:** Swagger configuration in `Program.cs`:
```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "Travel Organization API", 
        Version = "v1"
    });
    
    // JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
});
```

Features:
- JWT authentication support
- XML documentation comments
- Operation filters for auth requirements
- Custom operation IDs

### **Question 22: API Documentation**
**Q:** How do you document your API endpoints? Show examples.

**A:** We use **XML documentation comments**:
```csharp
/// <summary>
/// Create a new trip
/// </summary>
/// <param name="tripDto">The trip details to create</param>
/// <remarks>
/// This endpoint requires Admin role access
/// </remarks>
/// <returns>The newly created trip</returns>
[HttpPost]
[Authorize(Roles = "Admin")]
public async Task<ActionResult<Trip>> CreateTrip(CreateTripDTO tripDto)
```

This generates comprehensive Swagger documentation with descriptions, parameters, and security requirements. 