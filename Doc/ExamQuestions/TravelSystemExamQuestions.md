# Travel Organization System - Comprehensive Exam Questions

## Backend API Architecture (Questions 1-15)

### 1. API Controllers & Structure
**Q1:** Explain the controller hierarchy in this project. What controllers exist and what is their primary responsibility?

**A1:** The project has 5 main controllers:
- `AuthController`: Handles authentication (login, register) and JWT token generation
- `TripController`: CRUD operations for trips, trip searching, and pagination
- `GuideController`: CRUD operations for guides and guide management
- `UserController`: User profile management and administrative user operations
- `DestinationController`: CRUD operations for destinations
- `TripRegistrationController`: Handles trip bookings and user registrations

---

**Q2:** What design pattern is used in the service layer architecture? How are dependencies injected?

**A2:** The project uses the **Dependency Injection pattern** with **Interface Segregation**. All services implement interfaces (IServiceName pattern):
- Services are registered in `Program.cs` using ASP.NET Core's built-in DI container
- Controllers receive services through constructor injection
- This enables loose coupling, testability, and adherence to SOLID principles

---

**Q3:** Describe the typical flow of an API request from controller to database. Include all layers involved.

**A3:** **Controller → Service → Entity Framework → Database**
1. **Controller** receives HTTP request, validates input, calls service
2. **Service** contains business logic, calls Entity Framework context
3. **Entity Framework** translates LINQ to SQL queries
4. **Database** executes query and returns data
5. **Response flows back**: Database → EF → Service → Controller → HTTP Response

---

### 2. Data Transfer Objects (DTOs)

**Q4:** Why does this project use DTOs instead of directly exposing entity models? Give 3 specific reasons.

**A4:** 
1. **Security**: Prevents over-posting attacks and hides internal model structure
2. **API Contract Stability**: Changes to internal models don't break API consumers
3. **Data Shaping**: Controls exactly what data is exposed (e.g., excluding PasswordHash from UserDTO)

---

**Q5:** Compare `TripDTO` and `Trip` entity. What fields are different and why?

**A5:** Key differences:
- `TripDTO` excludes navigation properties like `TripRegistrations` and `TripGuides`
- `TripDTO` may include computed fields like `GuideNames` for display
- `TripDTO` uses simpler data types (e.g., string for dates instead of DateTime?)
- Entity has EF-specific attributes, DTO has validation attributes

---

### 3. Authentication & JWT

**Q6:** How is JWT implemented in this project? Describe the token creation process step by step.

**A6:** JWT Implementation:
1. User submits credentials to `/api/auth/login`
2. `UserService` validates credentials using BCrypt
3. `JwtService.GenerateToken()` creates JWT with claims (UserId, Username, Email, Role)
4. Token signed with symmetric key (HMAC SHA256)
5. Token returned to client with expiration time (120 minutes)
6. Client includes token in `Authorization: Bearer <token>` header

---

**Q7:** What claims are included in the JWT token and why is each important?

**A7:** Claims included:
- **NameIdentifier**: User ID for identifying the authenticated user
- **Name**: Username for display purposes
- **Email**: User's email address
- **Role**: "Admin" or "User" for authorization decisions
- **Exp**: Expiration time for token validity
- **Iss/Aud**: Issuer and audience for token validation

---

**Q8:** Explain the difference between authentication and authorization in this project. How is each implemented?

**A8:** 
- **Authentication**: Verifies "who you are" using JWT tokens or cookies
- **Authorization**: Determines "what you can do" using role-based access

**Implementation:**
- Authentication: `[Authorize]` attribute validates JWT tokens
- Authorization: `[Authorize(Roles = "Admin")]` restricts access by role
- Web app uses cookie authentication, API uses JWT

---

## Database Design & Entity Framework (Questions 9-20)

### 4. Database Schema

**Q9:** Draw or describe the database schema relationships. Which relationships are one-to-many and which are many-to-many?

**A9:** **Relationships:**
- `Destination (1) → (N) Trip` - One-to-many
- `Trip (M) ↔ (N) Guide` - Many-to-many (via TripGuide bridge table)
- `User (1) → (N) TripRegistration` - One-to-many  
- `Trip (1) → (N) TripRegistration` - One-to-many

**Bridge table**: `TripGuide` with composite key (TripId, GuideId)

---

**Q10:** How are many-to-many relationships configured in Entity Framework? Show the specific configuration code.

**A10:** In `ApplicationDbContext.OnModelCreating()`:
```csharp
modelBuilder.Entity<TripGuide>()
    .HasKey(tg => new { tg.TripId, tg.GuideId }); // Composite primary key

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
```

---

**Q11:** What is the purpose of navigation properties? How does this project handle circular references?

**A11:** **Navigation Properties:**
- Enable traversing relationships (e.g., `trip.TripGuides`)
- Support LINQ queries across relationships
- Provide object-oriented access to related data

**Circular Reference Handling:**
- JSON serializer configured with `ReferenceHandler.Preserve`
- Lazy loading disabled to control data loading
- Uses explicit `.Include()` and `.ThenInclude()` for eager loading

---

### 5. Entity Framework Operations

**Q12:** How does the project prevent N+1 query problems? Give a specific example from the code.

**A12:** Uses **Eager Loading** with `.Include()`:
```csharp
var trips = await _context.Trips
    .Include(t => t.Destination)
    .Include(t => t.TripGuides)
        .ThenInclude(tg => tg.Guide)
    .Include(t => t.TripRegistrations)
    .ToListAsync();
```
This loads all related data in a single query instead of separate queries for each relationship.

---

**Q13:** Explain the difference between `Add()`, `Update()`, and `Attach()` in Entity Framework. When would you use each?

**A13:** 
- **Add()**: Marks entity as Added, EF will INSERT on SaveChanges()
- **Update()**: Marks all properties as Modified, EF will UPDATE all fields
- **Attach()**: Tells EF to track existing entity, used for entities loaded outside current context

**Usage:**
- Add(): Creating new entities
- Update(): Updating entire entities
- Attach(): Working with disconnected scenarios (like DTOs)

---

## Frontend Implementation (Questions 14-25)

### 6. Razor Pages vs MVC

**Q14:** This project uses both Razor Pages and MVC. Explain when each is used and why this hybrid approach was chosen.

**A14:** **Usage Pattern:**
- **Razor Pages**: Main application UI (`/Pages/Trips/Index.cshtml`) - page-focused scenarios
- **MVC Controllers**: API endpoints within web app (`TripsController` for AJAX calls)
- **Hybrid Benefits**: Razor Pages for traditional web pages, MVC for API-style operations

---

**Q15:** How do Razor Page models (PageModel classes) differ from MVC controllers?

**A15:** **Differences:**
- **Page Models**: Bound to specific pages, handle page-specific logic
- **MVC Controllers**: Handle multiple actions, more flexible routing
- **Page Models**: Use `OnGet()`, `OnPost()` methods
- **MVC Controllers**: Use action methods with `[HttpGet]`, `[HttpPost]`
- **Page Models**: Strongly typed to specific pages
- **MVC Controllers**: Return various view types

---

### 7. JavaScript & AJAX Implementation

**Q16:** Describe the AJAX implementation pattern used in `tripList.js`. How does it handle pagination without page reloads?

**A16:** **AJAX Pagination Pattern:**
```javascript
// Updates URL parameters without reload
const url = new URL(window.location);
url.searchParams.set('page', page);
history.pushState(null, '', url);

// Fetches new data
const response = await fetch(`/api/trips?page=${page}`);
const trips = await response.json();

// Updates DOM without reload
updateTripCards(trips);
```

**Benefits:** Better UX, faster navigation, bookmarkable URLs

---

**Q17:** How does the project implement progressive enhancement in its JavaScript?

**A17:** **Progressive Enhancement:**
- Pages work without JavaScript (server-side rendering)
- JavaScript enhances functionality (AJAX pagination, dynamic filtering)
- Graceful degradation when JS disabled
- Event listeners added after DOM ready
- Fallback to traditional form submissions

---

### 8. Unsplash API Integration

**Q18:** How is the Unsplash API integrated? Describe the service architecture and configuration.

**A18:** **Service Architecture:**
```csharp
// HttpClient Factory configuration
services.AddHttpClient<UnsplashService>(client => {
    client.BaseAddress = new Uri("https://api.unsplash.com/");
    client.DefaultRequestHeaders.Add("Accept-Version", "v1");
});

// Service registration
services.Configure<UnsplashSettings>(configuration.GetSection("Unsplash"));
services.AddSingleton<IUnsplashService, UnsplashService>();
```

**Features:** Caching (60 min), rate limiting compliance, error handling

---

**Q19:** Explain the image optimization features implemented in the `OptimizedImage` component.

**A19:** **Optimization Features:**
- **Lazy Loading**: `loading="lazy"` attribute
- **Responsive Images**: Generated `srcset` for multiple resolutions
- **URL Parameters**: Adds `?auto=format&fit=crop&q=80&w=800&h=600`
- **Size Presets**: thumb (200x150), small (400x300), regular (800x600), full
- **Fallback Handling**: Placeholder when images fail to load
- **Performance**: Reduces initial page load time

---

**Q20:** How does the project handle Unsplash API rate limiting and caching?

**A20:** **Rate Limiting & Caching:**
```csharp
// Memory caching with expiration
_cache.Set(cacheKey, photo, TimeSpan.FromMinutes(60));

// Rate limiting compliance
- Client-ID authentication (not OAuth for public access)
- Download tracking per Unsplash guidelines
- Error handling for API limits
- Fallback to cached/placeholder images
```

---

## Security & Configuration (Questions 21-30)

### 9. Security Implementation

**Q21:** How are passwords secured in this system? Describe the hashing implementation.

**A21:** **Password Security with BCrypt:**
```csharp
// Hashing during registration
user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

// Verification during login
bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
```

**Benefits:** Salt automatically generated, computationally expensive, resistance to rainbow table attacks

---

**Q22:** What CORS policies are implemented and why are they different between environments?

**A22:** **CORS Configuration:**
- **Development**: Restrictive - specific origins only
- **Production**: Permissive - broader access for deployment flexibility
- **Headers**: Allows Authorization header for JWT tokens
- **Methods**: GET, POST, PUT, DELETE for REST API operations

---

**Q23:** How does the project prevent common security vulnerabilities like SQL injection and XSS?

**A23:** **Security Measures:**
- **SQL Injection**: Entity Framework parameterized queries
- **XSS**: Razor automatic HTML encoding, Content Security Policy
- **CSRF**: Antiforgery tokens on forms
- **Over-posting**: DTOs limit field binding
- **Authentication**: JWT validation and cookie security

---

### 10. Configuration & Deployment

**Q24:** Explain the configuration management strategy. How are different environments handled?

**A24:** **Configuration Strategy:**
```json
// appsettings.json (base)
// appsettings.Development.json (dev overrides)
// appsettings.Production.json (prod overrides)
```

**Environment-specific:**
- Connection strings
- CORS policies  
- JWT settings
- API keys (Unsplash)
- Logging levels

---

**Q25:** How is dependency injection configured in `Program.cs`? List the main service registrations.

**A25:** **Service Registrations:**
```csharp
// Framework services
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>();

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);

// Custom services
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IJwtService, JwtService>();

// HttpClient
builder.Services.AddHttpClient<UnsplashService>();
```

---

## Advanced Architecture (Questions 26-35)

### 11. Design Patterns & Principles

**Q26:** What SOLID principles are demonstrated in this project? Give specific examples.

**A26:** **SOLID Examples:**
- **Single Responsibility**: Each service has one responsibility (UserService = user operations)
- **Open/Closed**: Services use interfaces, can extend without modifying
- **Liskov Substitution**: Any IUserService implementation can replace UserService
- **Interface Segregation**: Separate interfaces for different concerns
- **Dependency Inversion**: Controllers depend on abstractions (interfaces), not concrete classes

---

**Q27:** How does the project implement the Repository pattern? What are the benefits?

**A27:** **Repository Pattern via Entity Framework:**
- `ApplicationDbContext` acts as repository
- Service layer abstracts data access logic
- **Benefits**: Testability, separation of concerns, centralized data access logic
- **Note**: Direct EF usage instead of separate repository classes for simplicity

---

**Q28:** Explain the layered architecture. What is the responsibility of each layer?

**A28:** **Layered Architecture:**
1. **Presentation Layer**: Controllers, Razor Pages (handles HTTP requests/responses)
2. **Business Layer**: Services (contains business logic and rules)
3. **Data Access Layer**: Entity Framework, ApplicationDbContext (database operations)
4. **Database Layer**: SQL Server (data persistence)

**Separation ensures**: Maintainability, testability, loose coupling

---

### 12. Performance & Optimization

**Q29:** What caching strategies are implemented? How do they improve performance?

**A29:** **Caching Strategies:**
- **Memory Caching**: Unsplash API responses (60 minutes)
- **Browser Caching**: Static assets with proper headers
- **Database Optimization**: Eager loading to prevent N+1 queries
- **Image Optimization**: Multiple resolutions, lazy loading

**Performance Benefits:** Reduced API calls, faster page loads, better user experience

---

**Q30:** How does the AJAX implementation improve user experience? What are the performance benefits?

**A30:** **UX & Performance Benefits:**
- **No Page Reloads**: Faster navigation, maintains scroll position
- **Reduced Bandwidth**: Only data transferred, not full HTML
- **Better Responsiveness**: Immediate feedback, loading states
- **SEO Friendly**: URLs remain bookmarkable
- **Progressive Enhancement**: Works without JavaScript

---

## Integration & Testing (Questions 31-40)

### 13. API Integration

**Q31:** How does the web application consume its own API? Show the pattern used in controllers.

**A31:** **API Consumption Pattern:**
```csharp
public class TripsController : Controller
{
    private readonly HttpClient _httpClient;
    
    public async Task<IActionResult> GetTrips(int page = 1)
    {
        var response = await _httpClient.GetAsync($"/api/trips?page={page}");
        var trips = await response.Content.ReadFromJsonAsync<List<TripDTO>>();
        return Json(trips);
    }
}
```

**Benefits:** Consistent API usage, easier testing, separation of concerns

---

**Q32:** How are HTTP errors handled in both API and web application layers?

**A32:** **Error Handling Strategy:**
- **API Layer**: Try-catch blocks, specific HTTP status codes (400, 401, 404, 500)
- **Service Layer**: Business logic validation, logging via ILogService
- **Web Layer**: User-friendly error messages, fallback UI states
- **JavaScript**: Fetch API error handling, user notifications

---

**Q33:** What logging strategy is implemented? How are logs structured and stored?

**A33:** **Logging Implementation:**
```csharp
public interface ILogService
{
    Task LogAsync(string action, string message, string level = "Info");
}
```

**Features:**
- Database logging (Log table)
- Structured logging with action, message, level, timestamp
- Service-level logging for business operations
- ASP.NET Core built-in logging for framework events

---

### 14. Data Validation & Error Handling

**Q34:** How is data validation implemented across the different layers? Give examples of validation attributes used.

**A34:** **Multi-Layer Validation:**
```csharp
// DTO Level
public class RegisterDTO
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }
}

// Model Level  
public class User
{
    [Required]
    [StringLength(100)]
    public string Username { get; set; }
}

// Client-side validation via jQuery Validation
```

---

**Q35:** How does the project handle concurrency and ensure data consistency?

**A35:** **Concurrency Handling:**
- **Entity Framework**: Optimistic concurrency with timestamp fields
- **Database Transactions**: SaveChanges() wraps operations in transaction
- **Service Layer**: Business logic validation before database operations
- **JWT Tokens**: Stateless authentication reduces concurrency issues

---

## Deployment & DevOps (Questions 36-45)

### 15. Environment Configuration

**Q36:** How are connection strings managed across different environments? What security considerations are implemented?

**A36:** **Connection String Management:**
```json
// appsettings.Development.json
"ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TravelDB;..."
}

// appsettings.Production.json  
"ConnectionStrings": {
    "DefaultConnection": "Server=azure-server;Database=TravelDB;..."
}
```

**Security:** Production secrets in Azure App Service configuration, not in code

---

**Q37:** What deployment strategies are supported? How is the application configured for Azure deployment?

**A37:** **Deployment Configuration:**
- **Local Development**: SQL Server LocalDB
- **Azure Deployment**: Azure SQL Database with connection string in App Service
- **CI/CD**: GitHub Actions for automated deployment
- **Environment Variables**: Override appsettings values
- **Health Checks**: Built-in ASP.NET Core health check endpoints

---

### 16. Monitoring & Maintenance

**Q38:** How would you monitor this application in production? What metrics would be important?

**A38:** **Monitoring Strategy:**
- **Application Insights**: Request tracking, exception monitoring
- **Database Monitoring**: Query performance, connection pool usage
- **API Metrics**: Response times, error rates, authentication failures
- **Business Metrics**: User registrations, trip bookings, API usage
- **Custom Logging**: Business operation logging via ILogService

---

**Q39:** What maintenance tasks would be required for this system? How would you handle database migrations?

**A39:** **Maintenance Tasks:**
- **Database Migrations**: EF Core migrations for schema changes
- **Log Cleanup**: Archive old log entries
- **Image Cache**: Periodic cleanup of cached Unsplash images
- **Security Updates**: Regular package updates
- **Performance**: Query optimization, index maintenance

---

## System Design & Scalability (Questions 40-50)

### 17. Scalability Considerations

**Q40:** How would you scale this application to handle increased load? What components would need modification?

**A40:** **Scaling Strategies:**
- **Horizontal Scaling**: Multiple web server instances behind load balancer
- **Database Scaling**: Read replicas, connection pooling
- **Caching**: Redis for distributed caching instead of memory cache
- **CDN**: Static assets (CSS, JS, images) served from CDN
- **API Gateway**: Rate limiting, authentication at gateway level

---

**Q41:** What are the potential bottlenecks in the current architecture? How would you address them?

**A41:** **Bottlenecks & Solutions:**
- **Database**: N+1 queries → Use .Include() strategically
- **Images**: Unsplash API calls → Implement proper caching and CDN
- **Memory**: In-memory cache → Move to distributed cache (Redis)
- **Session State**: Server-side sessions → JWT tokens for stateless design
- **File I/O**: Large uploads → Implement async file processing

---

### 18. Architecture Evolution

**Q42:** How would you modify this architecture to support microservices? What services would you extract?

**A42:** **Microservices Breakdown:**
- **User Service**: Authentication, user management
- **Trip Service**: Trip CRUD, search, booking
- **Guide Service**: Guide management, assignments
- **Notification Service**: Email, SMS notifications
- **Image Service**: Unsplash integration, image processing
- **Gateway**: API routing, authentication, rate limiting

---

**Q43:** What changes would be needed to support real-time features (like live chat or notifications)?

**A43:** **Real-time Implementation:**
- **SignalR**: For real-time communication
- **WebSockets**: Persistent connections for live updates
- **Message Queue**: Azure Service Bus for async processing
- **Event-Driven**: Domain events for decoupled notifications
- **Caching**: Redis for session management across instances

---

### 19. Security Enhancements

**Q44:** What additional security measures would you implement for a production system?

**A44:** **Security Enhancements:**
- **HTTPS Enforcement**: Strict Transport Security headers
- **Rate Limiting**: Per-user and per-IP rate limits
- **Input Validation**: Comprehensive validation and sanitization
- **Audit Logging**: Track all data modifications
- **Secrets Management**: Azure Key Vault for sensitive configuration
- **Security Headers**: CSP, X-Frame-Options, etc.

---

**Q45:** How would you implement role-based permissions at a more granular level?

**A45:** **Granular Permissions:**
```csharp
public enum Permission
{
    CanCreateTrip,
    CanModifyTrip,
    CanDeleteTrip,
    CanViewAllBookings,
    CanModifyUserProfile
}

[Authorize(Policy = "CanCreateTrip")]
public async Task<IActionResult> CreateTrip(TripDTO tripDto)
```

**Implementation:** Claims-based authorization with permission claims in JWT

---

## Final Advanced Questions (Questions 46-50)

**Q46:** Compare the trade-offs between using Entity Framework versus a micro-ORM like Dapper for this project.

**A46:** **EF Core vs Dapper:**
- **EF Core Pros**: Rich mapping, change tracking, migrations, LINQ
- **EF Core Cons**: Performance overhead, complexity for simple queries
- **Dapper Pros**: Faster execution, direct SQL control, minimal overhead
- **Dapper Cons**: Manual mapping, no change tracking, SQL injection risk
- **Decision**: EF Core chosen for rapid development and maintenance ease

---

**Q47:** How would you implement soft deletes in this system? What entities would benefit from this approach?

**A47:** **Soft Delete Implementation:**
```csharp
public abstract class BaseEntity
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string DeletedBy { get; set; }
}

// Global query filter
modelBuilder.Entity<Trip>().HasQueryFilter(t => !t.IsDeleted);
```

**Entities**: Trips, Users, Guides (for audit trail and data recovery)

---

**Q48:** Explain how you would implement an event-driven architecture for this travel system. What events would you publish?

**A48:** **Event-Driven Events:**
- `TripCreated`: Trigger email notifications to subscribers
- `TripBooked`: Update availability, send confirmation
- `PaymentProcessed`: Update booking status, generate receipt
- `TripCancelled`: Refund processing, notify affected users
- `UserRegistered`: Welcome email, onboarding process

**Implementation:** Domain events, MediatR, or message queues

---

**Q49:** How would you implement API versioning for this system? What strategies would you use?

**A49:** **API Versioning Strategies:**
```csharp
// URL Versioning
[Route("api/v1/trips")]
[Route("api/v2/trips")]

// Header Versioning
[ApiVersion("1.0")]
[ApiVersion("2.0")]

// Query Parameter
/api/trips?version=1.0
```

**Recommendation:** Header versioning for clean URLs, backward compatibility support

---

**Q50:** Design a comprehensive testing strategy for this application. What types of tests would you implement and what would they cover?

**A50:** **Testing Strategy:**

**Unit Tests:**
- Service layer business logic
- JWT token generation/validation
- DTO mapping and validation

**Integration Tests:**
- API endpoints with in-memory database
- Entity Framework database operations
- Authentication and authorization flows

**End-to-End Tests:**
- Complete user workflows (registration → booking)
- Browser automation testing
- API contract testing

**Performance Tests:**
- Load testing for concurrent users
- Database query performance
- API response time benchmarks

**Tools:** xUnit, TestServer, Selenium, NBomber for load testing

---

## Summary

This comprehensive exam covers:
- **Backend Architecture**: APIs, services, dependency injection
- **Authentication & Security**: JWT, authorization, password hashing
- **Database Design**: EF Core, relationships, performance
- **Frontend Implementation**: Razor Pages, JavaScript, AJAX
- **Third-party Integration**: Unsplash API, caching strategies
- **Architecture Patterns**: Layered architecture, SOLID principles
- **Production Concerns**: Deployment, monitoring, scalability

Each question tests practical knowledge of real-world ASP.NET Core development, focusing on the actual implementation details found in your Travel Organization System.