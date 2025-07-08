# Travel Organization System - API Documentation

## Overview

The Travel Organization System API provides RESTful endpoints for managing travel-related data including trips, destinations, guides, users, and bookings. The API supports JWT authentication and comprehensive logging.

## Base URL

- **Development**: `http://localhost:16000/api/`
- **Production**: `https://travel-api-sokol-2024.azurewebsites.net/api/`

## Authentication

### JWT Token Authentication

Most endpoints require JWT authentication. Include the token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

### Authentication Endpoints

#### Register User
```http
POST /api/auth/register
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "password": "SecurePassword123!",
  "confirmPassword": "SecurePassword123!"
}
```

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "john.doe@example.com",
  "password": "SecurePassword123!"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "role": "User"
  }
}
```

#### Change Password
```http
POST /api/auth/changepassword
Authorization: Bearer <token>
Content-Type: application/json

{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewPassword123!",
  "confirmPassword": "NewPassword123!"
}
```

## Core Entities

### Trip Endpoints

#### Get All Trips
```http
GET /api/trip
```

**Response:**
```json
{
  "$values": [
    {
      "id": 1,
      "name": "Amazing Paris Adventure",
      "description": "Explore the City of Light",
      "startDate": "2024-06-01T00:00:00",
      "endDate": "2024-06-07T00:00:00",
      "price": 1299.99,
      "maxParticipants": 20,
      "destinationId": 1,
      "destinationName": "Paris",
      "country": "France",
      "city": "Paris",
      "imageUrl": "https://images.unsplash.com/photo-123...",
      "availableSpots": 15,
      "guides": []
    }
  ]
}
```

#### Get Trip by ID
```http
GET /api/trip/{id}
```

#### Create Trip
```http
POST /api/trip
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "name": "New Adventure",
  "description": "Exciting new trip",
  "startDate": "2024-07-01T00:00:00",
  "endDate": "2024-07-07T00:00:00",
  "price": 999.99,
  "maxParticipants": 15,
  "destinationId": 2,
  "imageUrl": "https://images.unsplash.com/photo-456..."
}
```

#### Update Trip
```http
PUT /api/trip/{id}
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "id": 1,
  "name": "Updated Adventure",
  "description": "Updated description",
  "startDate": "2024-07-01T00:00:00",
  "endDate": "2024-07-07T00:00:00",
  "price": 1199.99,
  "maxParticipants": 18,
  "destinationId": 2,
  "imageUrl": "https://images.unsplash.com/photo-789..."
}
```

#### Delete Trip
```http
DELETE /api/trip/{id}
Authorization: Bearer <admin-token>
```

#### Search Trips
```http
GET /api/trip/search
```

**Query Parameters:**
- `name` (string): Search in trip name
- `description` (string): Search in trip description
- `page` (int): Page number (default: 1)
- `count` (int): Items per page (default: 10, max: 100)

### Destination Endpoints

#### Get All Destinations
```http
GET /api/destination
```

**Response:**
```json
{
  "$values": [
    {
      "id": 1,
      "name": "Paris",
      "country": "France",
      "description": "The City of Light",
      "imageUrl": "https://images.unsplash.com/photo-paris...",
      "location": "Paris, France",
      "tagline": "Romance and Culture"
    }
  ]
}
```

#### Get Destination by ID
```http
GET /api/destination/{id}
```

#### Create Destination
```http
POST /api/destination
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "name": "Tokyo",
  "country": "Japan",
  "description": "Modern metropolis with traditional culture",
  "imageUrl": "https://images.unsplash.com/photo-tokyo...",
  "tagline": "Where tradition meets innovation"
}
```

#### Update Destination
```http
PUT /api/destination/{id}
Authorization: Bearer <admin-token>
```

#### Delete Destination
```http
DELETE /api/destination/{id}
Authorization: Bearer <admin-token>
```

### Guide Endpoints

#### Get All Guides
```http
GET /api/guide
```

**Response:**
```json
{
  "$values": [
    {
      "id": 1,
      "firstName": "Marie",
      "lastName": "Dubois",
      "email": "marie.dubois@example.com",
      "phone": "+33 1 23 45 67 89",
      "specialization": "Art History",
      "experience": 8,
      "languages": "French, English, Spanish",
      "bio": "Passionate art historian with extensive knowledge of European culture."
    }
  ]
}
```

#### Get Guide by ID
```http
GET /api/guide/{id}
```

#### Create Guide
```http
POST /api/guide
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Smith",
  "email": "john.smith@example.com",
  "phone": "+1 555 123 4567",
  "specialization": "Adventure Sports",
  "experience": 5,
  "languages": "English, Spanish",
  "bio": "Adventure sports enthusiast with 5 years of guiding experience."
}
```

#### Update Guide
```http
PUT /api/guide/{id}
Authorization: Bearer <admin-token>
```

#### Delete Guide
```http
DELETE /api/guide/{id}
Authorization: Bearer <admin-token>
```

### Trip Registration Endpoints

#### Get User's Bookings
```http
GET /api/tripregistration/user/{userId}
Authorization: Bearer <token>
```

#### Book Trip
```http
POST /api/tripregistration
Authorization: Bearer <token>
Content-Type: application/json

{
  "tripId": 1,
  "userId": 1,
  "numberOfParticipants": 2,
  "specialRequests": "Vegetarian meals please"
}
```

#### Cancel Booking
```http
DELETE /api/tripregistration/{id}
Authorization: Bearer <token>
```

### User Management Endpoints

#### Get All Users (Admin Only)
```http
GET /api/user
Authorization: Bearer <admin-token>
```

#### Get User by ID
```http
GET /api/user/{id}
Authorization: Bearer <token>
```

#### Update User Profile
```http
PUT /api/user/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "id": 1,
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phone": "+1 555 123 4567"
}
```

## Logging Endpoints

### Get Recent Logs
```http
GET /api/logs/get/{count}
Authorization: Bearer <admin-token>
```

**Parameters:**
- `count` (int): Number of recent logs to retrieve

**Response:**
```json
{
  "$values": [
    {
      "id": 1,
      "timestamp": "2024-01-15T10:30:00",
      "level": "Information",
      "message": "Trip with id=1 was created successfully"
    }
  ]
}
```

### Get Log Count
```http
GET /api/logs/count
Authorization: Bearer <admin-token>
```

**Response:**
```json
{
  "count": 1250
}
```

## Error Handling

The API returns standard HTTP status codes:

- **200 OK**: Success
- **201 Created**: Resource created successfully
- **400 Bad Request**: Invalid request data
- **401 Unauthorized**: Authentication required
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Resource not found
- **500 Internal Server Error**: Server error

### Error Response Format
```json
{
  "error": {
    "message": "Validation failed",
    "details": [
      "Title is required",
      "Price must be greater than 0"
    ]
  }
}
```

## Rate Limiting

- **Authenticated requests**: 1000 requests per hour
- **Unauthenticated requests**: 100 requests per hour

## Pagination

Most list endpoints support pagination:

**Query Parameters:**
- `page`: Page number (1-based)
- `pageSize`: Items per page (max 100)

**Response Headers:**
- `X-Total-Count`: Total number of items
- `X-Page-Count`: Total number of pages

## Data Validation

### Trip Validation
- `title`: Required, max 200 characters
- `description`: Required, max 1000 characters
- `startDate`: Required, must be future date
- `endDate`: Required, must be after start date
- `price`: Required, must be positive
- `maxParticipants`: Required, must be positive

### User Validation
- `firstName`: Required, max 50 characters
- `lastName`: Required, max 50 characters
- `email`: Required, valid email format
- `password`: Required, min 8 characters, must contain uppercase, lowercase, digit, and special character

## Swagger Documentation

Interactive API documentation is available at:
- **Development**: `http://localhost:16000/swagger`
- **Production**: `https://travel-api-sokol-2024.azurewebsites.net/swagger`

## Testing

### Example cURL Commands

**Get all trips:**
```bash
curl -X GET "http://localhost:16000/api/trip" \
  -H "accept: application/json"
```

**Login:**
```bash
curl -X POST "http://localhost:16000/api/auth/login" \
  -H "accept: application/json" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "Admin123!"
  }'
```

**Create trip (authenticated):**
```bash
curl -X POST "http://localhost:16000/api/trip" \
  -H "accept: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "New Adventure",
    "description": "Exciting trip",
    "startDate": "2024-07-01T00:00:00",
    "endDate": "2024-07-07T00:00:00",
    "price": 999.99,
    "maxParticipants": 15,
    "destinationId": 1
  }'
```

## Security Considerations

1. **JWT Tokens**: Expire after 24 hours
2. **Password Hashing**: Uses bcrypt with salt
3. **HTTPS**: Required in production
4. **CORS**: Configured for web application domain
5. **Input Validation**: All inputs are validated and sanitized
6. **SQL Injection Protection**: Using parameterized queries

## Monitoring and Logging

- All API requests are logged
- Performance metrics are tracked
- Error rates are monitored
- Database queries are logged for debugging

## Support

For API support and questions:
- **Email**: support@travelorganization.com
- **Documentation**: Available in Swagger UI
- **Status Page**: Monitor API health and uptime 