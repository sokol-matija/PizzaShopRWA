# Model Validation Analysis - Travel Organization System

## Overview

This document provides a comprehensive analysis of model validation patterns used throughout the Travel Organization System, covering data annotations, validation strategies, error handling, business rule enforcement, and validation best practices across both entity models and DTOs.

## Validation Architecture Summary

The Travel Organization System implements **comprehensive validation patterns** with:
- **Data Annotations** - Declarative validation rules
- **Business Rule Validation** - Custom validation logic
- **Client-Side Validation** - JavaScript validation for user experience
- **Server-Side Validation** - Security and data integrity
- **Error Handling** - Consistent error responses
- **Validation Layers** - Multiple validation checkpoints

## Validation Strategy Overview

### 1. **Multi-Layer Validation Approach**

```
Client-Side → DTO Validation → Business Logic → Entity Validation → Database Constraints
```

#### **Validation Layers**
1. **Client-Side**: JavaScript validation for immediate feedback
2. **DTO Validation**: API input validation with data annotations
3. **Business Logic**: Service layer business rule validation
4. **Entity Validation**: Model-level validation rules
5. **Database**: Database constraints as final safety net

### 2. **Validation Types Used**

#### **Input Validation**
- **Format Validation**: Email, phone, URL formats
- **Length Validation**: String length constraints
- **Range Validation**: Numeric range constraints
- **Required Validation**: Mandatory field enforcement

#### **Business Rule Validation**
- **Custom Logic**: Complex business rules
- **Cross-Field Validation**: Field interdependencies
- **Conditional Validation**: Context-dependent rules
- **Entity Relationships**: Foreign key validation

---

## Data Annotation Patterns

### 1. **Basic Validation Annotations**

#### **Required Fields**
```csharp
[Required]
public string Username { get; set; }

[Required(ErrorMessage = "Email address is required")]
public string Email { get; set; }
```

#### **String Length Validation**
```csharp
[StringLength(100)]
public string Name { get; set; }

[StringLength(100, MinimumLength = 3)]
public string Username { get; set; }

[StringLength(500)]
public string Description { get; set; }
```

#### **Format Validation**
```csharp
[EmailAddress]
public string Email { get; set; }

[Phone]
public string PhoneNumber { get; set; }

[Url]
public string ImageUrl { get; set; }
```

#### **Numeric Range Validation**
```csharp
[Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
public decimal Price { get; set; }

[Range(1, int.MaxValue, ErrorMessage = "MaxParticipants must be greater than 0")]
public int MaxParticipants { get; set; }
```

### 2. **Advanced Validation Patterns**

#### **Custom Error Messages**
```csharp
[Required(ErrorMessage = "Trip name is required")]
[StringLength(100, ErrorMessage = "Trip name cannot exceed 100 characters")]
public string Name { get; set; }

[Range(1, int.MaxValue, ErrorMessage = "Number of participants must be greater than 0")]
public int NumberOfParticipants { get; set; }
```

#### **Data Type Hints**
```csharp
[DataType(DataType.Password)]
public string Password { get; set; }

[DataType(DataType.Date)]
public DateTime StartDate { get; set; }

[DataType(DataType.Currency)]
public decimal Price { get; set; }
```

#### **Comparison Validation**
```csharp
[Required]
[DataType(DataType.Password)]
public string Password { get; set; }

[Required]
[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
[DataType(DataType.Password)]
public string ConfirmPassword { get; set; }
```

---

## Entity Model Validation

### 1. **User Entity Validation**

```csharp
public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Username { get; set; }

    [Required]
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(500)]
    public string PasswordHash { get; set; }

    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    [StringLength(20)]
    [Phone]
    public string? PhoneNumber { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    public bool IsAdmin { get; set; }
}
```

#### **Validation Features**
- **Required Fields**: Username, email, password hash
- **Format Validation**: Email address and phone number
- **Length Constraints**: Prevent database overflow
- **Optional Fields**: Personal information (nullable)
- **Security**: Password hash instead of plain password

### 2. **Trip Entity Validation**

```csharp
public class Trip
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "MaxParticipants must be greater than 0")]
    public int MaxParticipants { get; set; }

    [Required]
    public int DestinationId { get; set; }
}
```

#### **Business Rule Validation**
- **Price Validation**: Must be positive value
- **Capacity Validation**: Must allow at least 1 participant
- **Date Validation**: Required start and end dates
- **Foreign Key**: Valid destination reference

### 3. **TripRegistration Entity Validation**

```csharp
public class TripRegistration
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int TripId { get; set; }

    [Required]
    public DateTime RegistrationDate { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Number of participants must be greater than 0")]
    public int NumberOfParticipants { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Total price must be greater than 0")]
    public decimal TotalPrice { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Confirmed";
}
```

#### **Complex Validation Rules**
- **Participant Count**: Must be positive
- **Price Validation**: Must be positive (calculated field)
- **Status Management**: Limited status values
- **Relationship Validation**: Valid user and trip references

---

## DTO Validation Patterns

### 1. **Registration DTO Validation**

```csharp
public class RegisterDTO
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Username { get; set; }

    [Required]
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; }

    [StringLength(100)]
    public string FirstName { get; set; }

    [StringLength(100)]
    public string LastName { get; set; }

    [StringLength(20)]
    [Phone]
    public string PhoneNumber { get; set; }

    [StringLength(200)]
    public string Address { get; set; }
}
```

#### **Comprehensive Validation**
- **Username**: Required, 3-100 characters
- **Email**: Required, valid email format
- **Password**: Required, minimum 6 characters
- **Confirmation**: Must match password
- **Optional Fields**: Personal information

### 2. **Trip Creation DTO Validation**

```csharp
public class CreateTripDTO
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [StringLength(500)]
    [Url]
    public string ImageUrl { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "MaxParticipants must be greater than 0")]
    public int MaxParticipants { get; set; }

    [Required]
    public int DestinationId { get; set; }

    public List<int> GuideIds { get; set; } = new List<int>();
}
```

#### **Business Logic Validation**
- **Date Requirements**: Start and end dates required
- **Business Rules**: Price and capacity must be positive
- **URL Validation**: Image URL format validation
- **Relationships**: Valid destination and guide references

---

## Business Logic Validation

### 1. **Service Layer Validation**

#### **Trip Registration Validation**
```csharp
public async Task<TripRegistration> CreateRegistrationAsync(TripRegistration registration)
{
    // Validate trip exists
    var trip = await _context.Trips.FindAsync(registration.TripId);
    if (trip == null)
        return null;

    // Business Rule: Check capacity
    var currentParticipants = await _context.TripRegistrations
        .Where(tr => tr.TripId == registration.TripId)
        .SumAsync(tr => tr.NumberOfParticipants);

    if (currentParticipants + registration.NumberOfParticipants > trip.MaxParticipants)
        return null; // Trip is full

    // Business Rule: Calculate price
    if (registration.TotalPrice <= 0)
        registration.TotalPrice = trip.Price * registration.NumberOfParticipants;

    // Set defaults
    if (registration.RegistrationDate == default)
        registration.RegistrationDate = DateTime.Now;

    _context.TripRegistrations.Add(registration);
    await _context.SaveChangesAsync();
    return registration;
}
```

#### **User Registration Validation**
```csharp
public async Task<User> RegisterAsync(RegisterDTO model)
{
    // Business Rule: Unique username
    if (await _context.Users.AnyAsync(u => u.Username == model.Username))
        return null;

    // Business Rule: Unique email
    if (await _context.Users.AnyAsync(u => u.Email == model.Email))
        return null;

    // Create user with validation
    var user = new User
    {
        Username = model.Username,
        Email = model.Email,
        PasswordHash = HashPassword(model.Password),
        FirstName = model.FirstName,
        LastName = model.LastName,
        PhoneNumber = model.PhoneNumber,
        Address = model.Address,
        IsAdmin = false // Security: New users are not admins
    };

    await _context.Users.AddAsync(user);
    await _context.SaveChangesAsync();
    return user;
}
```

### 2. **Trip Deletion Validation**

```csharp
public async Task<bool> DeleteTripAsync(int id)
{
    var trip = await _context.Trips.FindAsync(id);
    if (trip == null)
        return false;

    // Business Rule: Cannot delete trips with registrations
    bool hasRegistrations = await _context.TripRegistrations.AnyAsync(tr => tr.TripId == id);
    if (hasRegistrations)
        return false; // Prevent deletion

    // Clean up related data
    var tripGuides = await _context.TripGuides.Where(tg => tg.TripId == id).ToListAsync();
    _context.TripGuides.RemoveRange(tripGuides);

    _context.Trips.Remove(trip);
    await _context.SaveChangesAsync();
    return true;
}
```

---

## Controller Validation Handling

### 1. **ModelState Validation**

```csharp
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterDTO model)
{
    // Check model validation
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    var user = await _userService.RegisterAsync(model);
    if (user == null)
        return BadRequest("Username or email already exists");

    return Ok(new { message = "Registration successful" });
}
```

### 2. **Custom Validation Responses**

```csharp
[HttpPost]
public async Task<ActionResult<TripRegistrationDTO>> CreateRegistration(CreateTripRegistrationDTO registrationDto)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    // Additional validation
    if (registrationDto.NumberOfParticipants <= 0)
        return BadRequest("Number of participants must be greater than 0");

    var registration = new TripRegistration
    {
        TripId = registrationDto.TripId,
        NumberOfParticipants = registrationDto.NumberOfParticipants,
        RegistrationDate = DateTime.Now,
        Status = "Pending"
    };

    var createdRegistration = await _registrationService.CreateRegistrationAsync(registration);
    if (createdRegistration == null)
        return BadRequest("Unable to create registration. The trip may be full or not exist.");

    return CreatedAtAction(nameof(GetRegistration), new { id = createdRegistration.Id }, 
        MapRegistrationToDto(createdRegistration));
}
```

### 3. **Parameter Validation**

```csharp
[HttpGet("search")]
public async Task<ActionResult<IEnumerable<TripDTO>>> SearchTrips(
    [FromQuery] string? name,
    [FromQuery] string? description,
    [FromQuery] int page = 1,
    [FromQuery] int count = 10)
{
    // Validate pagination parameters
    if (page < 1)
        return BadRequest("Page number must be 1 or greater");
    
    if (count < 1 || count > 100)
        return BadRequest("Count must be between 1 and 100");

    var trips = await _tripService.SearchTripsAsync(name, description, page, count);
    return Ok(trips.Select(MapTripToDto));
}
```

---

## Error Handling Patterns

### 1. **Validation Error Responses**

#### **ModelState Error Response**
```csharp
if (!ModelState.IsValid)
{
    var errors = ModelState
        .Where(x => x.Value.Errors.Count > 0)
        .ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
        );
    
    return BadRequest(new { errors });
}
```

#### **Custom Error Response Format**
```json
{
  "errors": {
    "Username": ["Username is required"],
    "Email": ["Email address is not valid"],
    "Password": ["Password must be at least 6 characters"]
  }
}
```

### 2. **Business Logic Error Responses**

```csharp
public async Task<IActionResult> CreateTrip(CreateTripDTO tripDto)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    // Business validation
    if (tripDto.StartDate >= tripDto.EndDate)
        return BadRequest("End date must be after start date");

    if (tripDto.StartDate < DateTime.Today)
        return BadRequest("Trip cannot start in the past");

    var trip = await _tripService.CreateTripAsync(MapDtoToEntity(tripDto));
    return CreatedAtAction(nameof(GetTrip), new { id = trip.Id }, MapTripToDto(trip));
}
```

### 3. **Centralized Error Handling**

```csharp
public class ValidationExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = "Validation failed",
                details = ex.Message
            }));
        }
        catch (BusinessRuleException ex)
        {
            context.Response.StatusCode = 422;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = "Business rule violation",
                details = ex.Message
            }));
        }
    }
}
```

---

## Custom Validation Attributes

### 1. **Date Range Validation**

```csharp
public class DateRangeAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value is DateTime date)
        {
            return date >= DateTime.Today;
        }
        return false;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"{name} cannot be in the past";
    }
}
```

#### **Usage**
```csharp
public class CreateTripDTO
{
    [Required]
    [DateRange]
    public DateTime StartDate { get; set; }
}
```

### 2. **Business Rule Validation**

```csharp
public class TripCapacityAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is CreateTripDTO trip)
        {
            if (trip.MaxParticipants < 1)
            {
                return new ValidationResult("Trip must allow at least 1 participant");
            }
            
            if (trip.MaxParticipants > 1000)
            {
                return new ValidationResult("Trip capacity cannot exceed 1000 participants");
            }
        }
        
        return ValidationResult.Success;
    }
}
```

### 3. **Cross-Field Validation**

```csharp
public class TripDateValidationAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is CreateTripDTO trip)
        {
            if (trip.EndDate <= trip.StartDate)
            {
                return new ValidationResult("End date must be after start date");
            }
            
            if (trip.StartDate < DateTime.Today)
            {
                return new ValidationResult("Trip cannot start in the past");
            }
            
            var duration = (trip.EndDate - trip.StartDate).Days;
            if (duration > 365)
            {
                return new ValidationResult("Trip duration cannot exceed 1 year");
            }
        }
        
        return ValidationResult.Success;
    }
}
```

---

## Client-Side Validation Integration

### 1. **Razor Pages Validation**

```html
@model RegisterViewModel

<form asp-action="Register" method="post">
    <div asp-validation-summary="All" class="text-danger"></div>
    
    <div class="form-group">
        <label asp-for="Username"></label>
        <input asp-for="Username" class="form-control" />
        <span asp-validation-for="Username" class="text-danger"></span>
    </div>
    
    <div class="form-group">
        <label asp-for="Email"></label>
        <input asp-for="Email" class="form-control" />
        <span asp-validation-for="Email" class="text-danger"></span>
    </div>
    
    <div class="form-group">
        <label asp-for="Password"></label>
        <input asp-for="Password" type="password" class="form-control" />
        <span asp-validation-for="Password" class="text-danger"></span>
    </div>
    
    <button type="submit" class="btn btn-primary">Register</button>
</form>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

### 2. **JavaScript Validation**

```javascript
// Custom validation rules
$.validator.addMethod("futuredate", function(value, element) {
    var today = new Date();
    var inputDate = new Date(value);
    return inputDate > today;
}, "Date must be in the future");

// Apply validation
$("#tripForm").validate({
    rules: {
        startDate: {
            required: true,
            futuredate: true
        },
        endDate: {
            required: true,
            futuredate: true
        },
        price: {
            required: true,
            min: 0.01
        }
    },
    messages: {
        startDate: {
            required: "Start date is required",
            futuredate: "Trip cannot start in the past"
        },
        price: {
            min: "Price must be greater than 0"
        }
    }
});
```

---

## ELI5: Explain Like I'm 5 🧒

### Validation is like Having Multiple Safety Checks

Imagine you're going on a **field trip** and there are lots of people checking to make sure everything is safe and correct!

#### 🎫 **The Permission Slip (Data Annotations)**

##### **Basic Checks**
- **Name Required**: You must write your name (can't be empty)
- **Parent Signature**: Must have a real signature (format validation)
- **Emergency Phone**: Must be a real phone number (phone validation)
- **Age Range**: Must be between 5 and 18 years old (range validation)

##### **Smart Checks**
- **Matching Passwords**: Password and "confirm password" must be the same
- **Email Format**: Must look like a real email (someone@somewhere.com)
- **Not Too Long**: Name can't be 1000 characters long

#### 🏫 **Multiple Checkpoints (Validation Layers)**

##### **1. Teacher Check (Client-Side)**
- **Quick Check**: Teacher looks at your form right away
- **Immediate Help**: "Oops, you forgot to write your name!"
- **No Waiting**: Fixes problems before you submit

##### **2. Office Check (DTO Validation)**
- **Detailed Review**: Office staff checks everything carefully
- **Format Rules**: Makes sure everything is filled out correctly
- **Safety Rules**: Checks if you're old enough for the trip

##### **3. Principal Check (Business Logic)**
- **Smart Rules**: Principal knows special rules
- **Trip Capacity**: "Sorry, the bus is full"
- **Date Logic**: "This trip is in the past, that's impossible!"

##### **4. Final Check (Database)**
- **Last Safety**: Computer double-checks everything
- **No Duplicates**: "You already signed up for this trip"
- **Real Things**: "This destination doesn't exist"

#### 🚨 **Error Messages (User-Friendly Feedback)**

##### **Helpful Messages**
- **Clear Problems**: "Your password is too short" (not "Error 123")
- **How to Fix**: "Password must be at least 6 characters"
- **Multiple Issues**: Shows all problems at once, not one by one

##### **Different Types**
- **Red Alerts**: Something is wrong and must be fixed
- **Yellow Warnings**: Something might be better a different way
- **Green Success**: Everything looks great!

#### 🎯 **Why So Many Checks?**

1. **Safety First**: Make sure nothing dangerous happens
2. **Good Experience**: Fix problems quickly and clearly
3. **No Confusion**: Clear messages about what's wrong
4. **Smart System**: Computer knows the rules and helps you follow them

### The Magic Validation Process

```
You fill out form → Teacher checks quickly → 
Office checks carefully → Principal applies rules → 
Computer saves safely → Trip approved! 🎉
```

#### 🏆 **Smart Features**

- **Automatic**: Most checks happen automatically
- **Immediate**: Problems shown right away
- **Clear**: Easy to understand what's wrong
- **Helpful**: Tells you exactly how to fix things
- **Safe**: Multiple layers prevent any problems

---

## Validation Best Practices

### 1. **Comprehensive Validation Strategy**

#### **Defense in Depth**
```csharp
// Multiple validation layers
[Required]                                    // Basic validation
[StringLength(100, MinimumLength = 3)]       // Length validation
[RegularExpression(@"^[a-zA-Z0-9_]+$")]     // Format validation
public string Username { get; set; }
```

#### **Consistent Error Messages**
```csharp
public static class ValidationMessages
{
    public const string Required = "{0} is required";
    public const string StringLength = "{0} must be between {2} and {1} characters";
    public const string Email = "{0} must be a valid email address";
    public const string Range = "{0} must be between {1} and {2}";
}
```

### 2. **Performance Considerations**

#### **Efficient Validation**
```csharp
// Avoid expensive operations in validation
[Required]
public string Username { get; set; }

// Validate uniqueness in service layer, not attribute
public async Task<bool> IsUsernameUniqueAsync(string username)
{
    return !await _context.Users.AnyAsync(u => u.Username == username);
}
```

#### **Caching Validation Results**
```csharp
public class CachedValidationService
{
    private readonly IMemoryCache _cache;
    
    public async Task<bool> ValidateDestinationExistsAsync(int destinationId)
    {
        var cacheKey = $"destination_exists_{destinationId}";
        if (_cache.TryGetValue(cacheKey, out bool exists))
            return exists;
            
        exists = await _context.Destinations.AnyAsync(d => d.Id == destinationId);
        _cache.Set(cacheKey, exists, TimeSpan.FromMinutes(5));
        return exists;
    }
}
```

### 3. **Security Considerations**

#### **Input Sanitization**
```csharp
public class SanitizedStringAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is string str)
        {
            // Check for potentially dangerous content
            if (str.Contains("<script>") || str.Contains("javascript:"))
            {
                return new ValidationResult("Input contains potentially dangerous content");
            }
        }
        return ValidationResult.Success;
    }
}
```

#### **Rate Limiting**
```csharp
public class RateLimitAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Implement rate limiting for validation-heavy endpoints
        var key = $"rate_limit_{context.HttpContext.Connection.RemoteIpAddress}";
        // Check rate limit logic
    }
}
```

---

## Conclusion

The Travel Organization System's **validation architecture** demonstrates **comprehensive input validation and business rule enforcement** with:

### **Multi-Layer Validation Strategy**
- **Client-Side**: Immediate user feedback with JavaScript validation
- **DTO Layer**: API input validation with data annotations
- **Service Layer**: Business logic and rule enforcement
- **Entity Layer**: Model-level validation constraints
- **Database Layer**: Final data integrity constraints

### **Comprehensive Validation Patterns**
- **Data Annotations**: Declarative validation rules
- **Custom Attributes**: Business-specific validation logic
- **Cross-Field Validation**: Complex field interdependencies
- **Business Rules**: Domain-specific validation logic
- **Error Handling**: Consistent, user-friendly error responses

### **Security & Performance Benefits**
- **Input Sanitization**: Protection against malicious input
- **Business Rule Enforcement**: Data integrity and consistency
- **Performance Optimization**: Efficient validation strategies
- **User Experience**: Clear, actionable error messages
- **Maintainability**: Consistent validation patterns

### **Best Practices Demonstrated**
- **Defense in Depth**: Multiple validation checkpoints
- **Separation of Concerns**: Different validation types at appropriate layers
- **Consistent Messaging**: Standardized error message formats
- **Performance Awareness**: Efficient validation implementation
- **Security Focus**: Protection against common vulnerabilities

The validation implementation provides a **robust foundation** for data integrity, user experience, and security while following industry best practices for comprehensive input validation and business rule enforcement.

---

*Document created: January 2025*  
*Project: Travel Organization System*  
*Pattern: Comprehensive Multi-Layer Validation Architecture*  
*Technology: ASP.NET Core with Data Annotations and Custom Validation* 