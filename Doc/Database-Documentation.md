# Travel Organization System - Database Documentation

## Overview

The Travel Organization System uses a relational database design following the database-first approach with Entity Framework Core. The database supports a complete travel booking system with user management, trip organization, and comprehensive logging.

## Database Schema

### Entity Relationship Diagram

```
Users (1) -----> (N) TripRegistrations (N) <----- (1) Trips
  |                                                    |
  |                                                    |
  v                                                    v
UserRoles                                         Destinations (1)
                                                       |
                                                       v
                                                  TripGuides (N) <----- (1) Guides
                                                       |
                                                       v
                                                    Logs
```

## Tables

### 1. Users Table

**Purpose**: Stores user account information for authentication and profile management.

```sql
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    PhoneNumber NVARCHAR(20),
    Address NVARCHAR(200),
    IsAdmin BIT NOT NULL DEFAULT 0
);
```

**Columns:**
- `Id`: Primary key, auto-increment
- `Username`: Unique username for login (required)
- `Email`: Unique email address (required)
- `PasswordHash`: Hashed password using ASP.NET Core PasswordHasher (required)
- `FirstName`: User's first name (optional)
- `LastName`: User's last name (optional)
- `PhoneNumber`: Optional phone number
- `Address`: Optional address
- `IsAdmin`: Boolean flag for admin role (default: false)

**Indexes:**
- `IX_Users_Username`: Unique index on username
- `IX_Users_Email`: Unique index on email

### 2. Destinations Table

**Purpose**: Stores travel destinations with location and description information.

```sql
CREATE TABLE Destinations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Country NVARCHAR(50) NOT NULL,
    Description NVARCHAR(1000),
    ImageUrl NVARCHAR(500),
    Tagline NVARCHAR(200),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
```

**Columns:**
- `Id`: Primary key, auto-increment
- `Name`: Destination name (required)
- `Country`: Country name (required)
- `Description`: Detailed destination description
- `ImageUrl`: URL to destination image
- `Tagline`: Marketing tagline
- `CreatedAt`: Creation timestamp
- `UpdatedAt`: Last update timestamp

**Indexes:**
- `IX_Destinations_Country`: Index on country for filtering
- `IX_Destinations_Name`: Index on name for searching

### 3. Trips Table

**Purpose**: Stores trip information including dates, pricing, and capacity.

```sql
CREATE TABLE Trips (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    ImageUrl NVARCHAR(500),
    MaxParticipants INT NOT NULL,
    DestinationId INT NOT NULL,
    
    CONSTRAINT FK_Trips_Destinations 
        FOREIGN KEY (DestinationId) REFERENCES Destinations(Id)
);
```

**Columns:**
- `Id`: Primary key, auto-increment
- `Name`: Trip name (required)
- `Description`: Detailed trip description (optional)
- `StartDate`: Trip start date and time (required)
- `EndDate`: Trip end date and time (required)
- `Price`: Trip price per person (required)
- `ImageUrl`: URL to trip image (optional)
- `MaxParticipants`: Maximum number of participants (required)
- `DestinationId`: Foreign key to Destinations table (required)

**Indexes:**
- `IX_Trips_DestinationId`: Index on destination for filtering
- `IX_Trips_StartDate`: Index on start date for date queries
- `IX_Trips_Price`: Index on price for price range queries

**Constraints:**
- `CK_Trips_EndDate`: CHECK (EndDate > StartDate)
- `CK_Trips_Price`: CHECK (Price > 0)
- `CK_Trips_MaxParticipants`: CHECK (MaxParticipants > 0)

### 4. Guides Table

**Purpose**: Stores tour guide information and qualifications.

```sql
CREATE TABLE Guides (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Bio NVARCHAR(500),
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Phone NVARCHAR(20),
    ImageUrl NVARCHAR(500),
    YearsOfExperience INT
);
```

**Columns:**
- `Id`: Primary key, auto-increment
- `Name`: Guide's full name (required)
- `Bio`: Professional biography (optional)
- `Email`: Unique email address (required)
- `Phone`: Contact phone number (optional)
- `ImageUrl`: URL to guide's profile image (optional)
- `YearsOfExperience`: Years of experience (optional)

**Indexes:**
- `IX_Guides_Email`: Unique index on email

### 5. TripGuides Table (M:N Bridge)

**Purpose**: Many-to-many relationship between trips and guides.

```sql
CREATE TABLE TripGuides (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TripId INT NOT NULL,
    GuideId INT NOT NULL,
    AssignedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT FK_TripGuides_Trips 
        FOREIGN KEY (TripId) REFERENCES Trips(Id) ON DELETE CASCADE,
    CONSTRAINT FK_TripGuides_Guides 
        FOREIGN KEY (GuideId) REFERENCES Guides(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_TripGuides_TripId_GuideId 
        UNIQUE (TripId, GuideId)
);
```

**Columns:**
- `Id`: Primary key, auto-increment
- `TripId`: Foreign key to Trips table
- `GuideId`: Foreign key to Guides table
- `AssignedAt`: Assignment timestamp

**Indexes:**
- `IX_TripGuides_TripId`: Index on trip ID
- `IX_TripGuides_GuideId`: Index on guide ID

### 6. TripRegistrations Table

**Purpose**: Stores user trip bookings and registration details.

```sql
CREATE TABLE TripRegistrations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    TripId INT NOT NULL,
    RegistrationDate DATETIME2 NOT NULL,
    NumberOfParticipants INT NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Confirmed',
    
    CONSTRAINT FK_TripRegistrations_Trips 
        FOREIGN KEY (TripId) REFERENCES Trips(Id) ON DELETE CASCADE,
    CONSTRAINT FK_TripRegistrations_Users 
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
```

**Columns:**
- `Id`: Primary key, auto-increment
- `UserId`: Foreign key to Users table (required)
- `TripId`: Foreign key to Trips table (required)
- `RegistrationDate`: Booking timestamp (required)
- `NumberOfParticipants`: Number of people in booking (required)
- `TotalPrice`: Total price for the registration (required)
- `Status`: Booking status (default: 'Confirmed')

**Indexes:**
- `IX_TripRegistrations_TripId`: Index on trip ID
- `IX_TripRegistrations_UserId`: Index on user ID
- `IX_TripRegistrations_Status`: Index on status

**Constraints:**
- `CK_TripRegistrations_Participants`: CHECK (NumberOfParticipants > 0)
- `CK_TripRegistrations_TotalPrice`: CHECK (TotalPrice > 0)

### 7. Logs Table

**Purpose**: Stores application logs for monitoring and debugging.

```sql
CREATE TABLE Logs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Timestamp DATETIME2 NOT NULL DEFAULT GETDATE(),
    Level NVARCHAR(20) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Exception NVARCHAR(MAX),
    UserId INT,
    
    CONSTRAINT FK_Logs_Users 
        FOREIGN KEY (UserId) REFERENCES Users(Id)
);
```

**Columns:**
- `Id`: Primary key, auto-increment
- `Timestamp`: Log entry timestamp
- `Level`: Log level (Information, Warning, Error)
- `Message`: Log message
- `Exception`: Exception details (if applicable)
- `UserId`: Associated user ID (optional)

**Indexes:**
- `IX_Logs_Timestamp`: Index on timestamp for time-based queries
- `IX_Logs_Level`: Index on level for filtering
- `IX_Logs_UserId`: Index on user ID

## Data Relationships

### One-to-Many Relationships

1. **Destinations → Trips**
   - One destination can have multiple trips
   - Foreign key: `Trips.DestinationId`
   - Cascade: Restrict (prevent destination deletion if trips exist)

2. **Users → TripRegistrations**
   - One user can have multiple trip registrations
   - Foreign key: `TripRegistrations.UserId`
   - Cascade: Cascade (delete registrations when user is deleted)

3. **Trips → TripRegistrations**
   - One trip can have multiple registrations
   - Foreign key: `TripRegistrations.TripId`
   - Cascade: Cascade (delete registrations when trip is deleted)

4. **Users → Logs**
   - One user can have multiple log entries
   - Foreign key: `Logs.UserId`
   - Cascade: Set null (keep logs when user is deleted)

### Many-to-Many Relationships

1. **Trips ↔ Guides** (via TripGuides)
   - One trip can have multiple guides
   - One guide can be assigned to multiple trips
   - Bridge table: `TripGuides`

## Sample Data

### Users
```sql
INSERT INTO Users (FirstName, LastName, Email, PasswordHash, Role) VALUES
('Admin', 'User', 'admin@example.com', '$2a$11$...', 'Admin'),
('John', 'Doe', 'john.doe@example.com', '$2a$11$...', 'User'),
('Jane', 'Smith', 'jane.smith@example.com', '$2a$11$...', 'User');
```

### Destinations
```sql
INSERT INTO Destinations (Name, Country, Description, Tagline) VALUES
('Paris', 'France', 'The City of Light with iconic landmarks', 'Romance and Culture'),
('Tokyo', 'Japan', 'Modern metropolis with traditional roots', 'Where Tradition Meets Innovation'),
('Barcelona', 'Spain', 'Vibrant city with stunning architecture', 'Art, Culture, and Mediterranean Charm');
```

### Trips
```sql
INSERT INTO Trips (Title, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId) VALUES
('Amazing Paris Adventure', 'Explore the best of Paris in 7 days', '2024-06-01', '2024-06-07', 1299.99, 20, 1),
('Tokyo Discovery Tour', 'Experience modern and traditional Tokyo', '2024-07-15', '2024-07-22', 1899.99, 15, 2),
('Barcelona Art & Culture', 'Immerse yourself in Barcelona culture', '2024-08-10', '2024-08-17', 1199.99, 18, 3);
```

### Guides
```sql
INSERT INTO Guides (FirstName, LastName, Email, Specialization, Experience, Languages) VALUES
('Marie', 'Dubois', 'marie.dubois@example.com', 'Art History', 8, 'French, English, Spanish'),
('Hiroshi', 'Tanaka', 'hiroshi.tanaka@example.com', 'Cultural Tours', 12, 'Japanese, English, Mandarin'),
('Carlos', 'Rodriguez', 'carlos.rodriguez@example.com', 'Architecture', 6, 'Spanish, English, French');
```

## Database Configuration

### Connection String
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TravelOrganizationDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### Entity Framework Configuration

#### DbContext
```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

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
        ConfigureTripEntity(modelBuilder);
        ConfigureGuideEntity(modelBuilder);
        ConfigureTripGuideEntity(modelBuilder);
        ConfigureTripRegistrationEntity(modelBuilder);
        ConfigureLogEntity(modelBuilder);
    }
}
```

#### Model Configurations
```csharp
private void ConfigureTripEntity(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Trip>()
        .HasOne(t => t.Destination)
        .WithMany(d => d.Trips)
        .HasForeignKey(t => t.DestinationId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<Trip>()
        .Property(t => t.Price)
        .HasPrecision(10, 2);

    modelBuilder.Entity<Trip>()
        .HasCheckConstraint("CK_Trips_EndDate", "EndDate > StartDate");
}
```

## Performance Considerations

### Indexing Strategy

1. **Primary Keys**: Clustered indexes on all primary keys
2. **Foreign Keys**: Non-clustered indexes on all foreign keys
3. **Search Columns**: Indexes on frequently searched columns
4. **Composite Indexes**: Multi-column indexes for complex queries

### Query Optimization

#### Common Queries
```sql
-- Get trips with destination info
SELECT t.*, d.Name as DestinationName, d.Country
FROM Trips t
INNER JOIN Destinations d ON t.DestinationId = d.Id
WHERE t.IsActive = 1
ORDER BY t.StartDate;

-- Get user bookings with trip details
SELECT tr.*, t.Title, t.StartDate, t.EndDate, d.Name as DestinationName
FROM TripRegistrations tr
INNER JOIN Trips t ON tr.TripId = t.Id
INNER JOIN Destinations d ON t.DestinationId = d.Id
WHERE tr.UserId = @UserId
ORDER BY tr.RegistrationDate DESC;

-- Get trip capacity information
SELECT t.Id, t.Title, t.MaxParticipants,
       COALESCE(SUM(tr.NumberOfParticipants), 0) as BookedParticipants,
       t.MaxParticipants - COALESCE(SUM(tr.NumberOfParticipants), 0) as AvailableSpots
FROM Trips t
LEFT JOIN TripRegistrations tr ON t.Id = tr.TripId AND tr.Status = 'Confirmed'
GROUP BY t.Id, t.Title, t.MaxParticipants;
```

### Caching Strategy

1. **Application Level**: Cache frequently accessed data
2. **Query Results**: Cache complex query results
3. **Static Data**: Cache destinations and guides
4. **User Sessions**: Cache user authentication data

## Security Considerations

### Data Protection

1. **Password Hashing**: bcrypt with salt
2. **Sensitive Data**: Encrypt PII where required
3. **Access Control**: Role-based permissions
4. **Audit Trail**: Comprehensive logging

### SQL Injection Prevention

1. **Parameterized Queries**: Always use parameters
2. **Input Validation**: Validate all user inputs
3. **Stored Procedures**: Use for complex operations
4. **Least Privilege**: Minimal database permissions

## Backup and Recovery

### Backup Strategy

1. **Full Backups**: Weekly full database backups
2. **Differential Backups**: Daily differential backups
3. **Transaction Log Backups**: Hourly log backups
4. **Point-in-Time Recovery**: Ability to restore to specific time

### Recovery Procedures

1. **Disaster Recovery**: Documented recovery procedures
2. **Testing**: Regular backup restore testing
3. **Monitoring**: Automated backup monitoring
4. **Documentation**: Recovery runbooks

## Monitoring and Maintenance

### Performance Monitoring

1. **Query Performance**: Monitor slow queries
2. **Index Usage**: Track index effectiveness
3. **Deadlock Detection**: Monitor for deadlocks
4. **Resource Usage**: CPU, memory, disk monitoring

### Maintenance Tasks

1. **Index Maintenance**: Regular index rebuilding
2. **Statistics Updates**: Keep query statistics current
3. **Data Archiving**: Archive old log data
4. **Cleanup Jobs**: Remove temporary data

## Migration Scripts

### Database Creation Script

The complete database creation script is available in `/Database/Database.sql` and includes:

1. **Table Creation**: All tables with constraints
2. **Index Creation**: Performance indexes
3. **Sample Data**: Initial data for testing
4. **Stored Procedures**: Common operations
5. **Views**: Frequently used data combinations

### Version Control

1. **Schema Versioning**: Track database schema changes
2. **Migration Scripts**: Incremental update scripts
3. **Rollback Scripts**: Ability to revert changes
4. **Documentation**: Change log maintenance

## Troubleshooting

### Common Issues

1. **Connection Timeouts**: Check connection pooling
2. **Deadlocks**: Analyze transaction isolation
3. **Performance Issues**: Review query plans
4. **Data Integrity**: Check constraint violations

### Diagnostic Queries

```sql
-- Check database size
SELECT 
    name,
    size * 8 / 1024 as SizeMB,
    max_size * 8 / 1024 as MaxSizeMB
FROM sys.database_files;

-- Check table sizes
SELECT 
    t.name AS TableName,
    p.rows AS RowCounts,
    (SUM(a.total_pages) * 8) / 1024 AS TotalSpaceMB
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
GROUP BY t.name, p.rows
ORDER BY TotalSpaceMB DESC;

-- Check active connections
SELECT 
    session_id,
    login_name,
    host_name,
    program_name,
    status,
    last_request_start_time
FROM sys.dm_exec_sessions
WHERE is_user_process = 1;
``` 