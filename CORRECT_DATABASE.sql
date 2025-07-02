-- CORRECT DATABASE SCHEMA FOR TRAVEL ORGANIZATION SYSTEM
-- This matches the local database exactly

-- Create Destination table (with ImageUrl)
CREATE TABLE Destination (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL,
    Country NVARCHAR(100) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    ImageUrl NVARCHAR(500) NULL
);

-- Create Guide table (with ImageUrl and YearsOfExperience)
CREATE TABLE Guide (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Bio NVARCHAR(500) NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Phone NVARCHAR(20) NULL,
    ImageUrl NVARCHAR(500) NULL,
    YearsOfExperience INT NULL
);

-- Create Trip table (with ImageUrl)
CREATE TABLE Trip (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL,
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    Price DECIMAL(10, 2) NOT NULL,
    ImageUrl NVARCHAR(500) NULL,
    MaxParticipants INT NOT NULL,
    DestinationId INT NOT NULL,
    FOREIGN KEY (DestinationId) REFERENCES Destination(Id)
);

-- Create User table (without CreatedAt)
CREATE TABLE [User] (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    FirstName NVARCHAR(100) NULL,
    LastName NVARCHAR(100) NULL,
    PhoneNumber NVARCHAR(20) NULL,
    Address NVARCHAR(200) NULL,
    IsAdmin BIT NOT NULL DEFAULT 0
);

-- Create TripGuide table
CREATE TABLE TripGuide (
    TripId INT NOT NULL,
    GuideId INT NOT NULL,
    PRIMARY KEY (TripId, GuideId),
    FOREIGN KEY (TripId) REFERENCES Trip(Id) ON DELETE CASCADE,
    FOREIGN KEY (GuideId) REFERENCES Guide(Id) ON DELETE CASCADE
);

-- Create TripRegistration table
CREATE TABLE TripRegistration (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    TripId INT NOT NULL,
    RegistrationDate DATETIME NOT NULL DEFAULT GETDATE(),
    NumberOfParticipants INT NOT NULL DEFAULT 1,
    TotalPrice DECIMAL(10, 2) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    FOREIGN KEY (UserId) REFERENCES [User](Id),
    FOREIGN KEY (TripId) REFERENCES Trip(Id),
    CONSTRAINT UC_UserTrip UNIQUE (UserId, TripId)
);

-- Create Log table
CREATE TABLE Log (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Timestamp DATETIME NOT NULL DEFAULT GETDATE(),
    Level NVARCHAR(50) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL
); 