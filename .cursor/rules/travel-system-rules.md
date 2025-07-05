---
description: Travel Organization System specific rules and patterns
globs: ["**/*.cs", "**/*.cshtml", "**/*.razor"]
alwaysApply: true
---

# Travel Organization System Rules

## Architecture Guidelines

### Project Structure
- **WebAPI**: Backend API with controllers, services, models, DTOs
- **WebApp**: Frontend Razor Pages application with MVC pattern
- Keep clear separation between frontend and backend concerns
- Use dependency injection for service registration

### Image Optimization Standards
- **ALWAYS use optimized images** for better performance
- Use `@Html.OptimizedImage()` helper for standard image rendering
- Use `<OptimizedImage>` component for advanced scenarios
- Prefer "small" size (400px) for card thumbnails
- Prefer "regular" size (800px) for detail views
- Always provide meaningful alt text for accessibility

### Image Usage Examples
```html
<!-- For trip/destination cards -->
@Html.OptimizedImage(
    imageUrl: trip.ImageUrl ?? $"{trip.DestinationName} travel destination",
    alt: trip.Title,
    size: "small",
    width: 400,
    height: 250
)

<!-- For detail pages -->
@Html.OptimizedImage(
    imageUrl: destination.ImageUrl,
    alt: destination.Name,
    size: "regular"
)
```

## Service Layer Patterns

### UnsplashService Usage
- Use caching for all Unsplash API calls (default 60 minutes)
- Always handle API failures gracefully
- Provide fallback search terms when ImageUrl is null
- Log errors appropriately without exposing sensitive data

### Error Handling
- Use try-catch blocks for external API calls
- Return meaningful error messages to users
- Log detailed errors for debugging
- Never expose internal system details to end users

## Frontend Guidelines

### Razor Pages Best Practices
- Keep page models focused and lightweight
- Use partial views for reusable components
- Implement proper model validation
- Use ViewData sparingly, prefer strongly-typed models

### CSS and Styling
- Use Bootstrap classes consistently
- Maintain dark theme styling with proper contrast
- Use CSS custom properties for theme colors
- Keep inline styles minimal, prefer CSS classes

### JavaScript Guidelines
- Keep JavaScript simple and focused
- Use vanilla JavaScript where possible
- Implement proper event handling
- Add loading states for async operations

## Data Access Patterns

### Entity Framework Usage
- Use async methods for all database operations
- Implement proper error handling for database operations
- Use Include() for eager loading when needed
- Keep DbContext usage scoped and dispose properly

### API Communication
- Use HttpClient with proper configuration
- Implement retry logic for transient failures
- Use cancellation tokens for long-running operations
- Cache responses when appropriate

## Security Considerations

### Authentication & Authorization
- Use proper role-based access control
- Validate user permissions on all admin operations
- Implement CSRF protection for forms
- Use HTTPS for all communications

### Input Validation
- Validate all user inputs on both client and server
- Sanitize data before database operations
- Use parameterized queries to prevent SQL injection
- Implement proper file upload validation

## Performance Guidelines

### Image Performance
- Always use lazy loading for images
- Implement proper caching strategies
- Use appropriate image sizes for context
- Optimize images for web delivery

### Database Performance
- Use appropriate indexes for frequently queried fields
- Implement pagination for large result sets
- Use projection to limit data transfer
- Monitor query performance and optimize as needed

### Caching Strategy
- Cache expensive operations (API calls, complex queries)
- Use appropriate cache expiration times
- Implement cache invalidation when data changes
- Consider memory usage when caching large objects 