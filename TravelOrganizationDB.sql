

--create database TravelOrganizationDB
--go

use TravelOrganizationDB
go

-- Create Destination table (1-to-N entity)
CREATE TABLE Destination (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL,
    Country NVARCHAR(100) NOT NULL,
    City NVARCHAR(100) NOT NULL
);

-- Create Trip table (primary entity)
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

-- Create Guide table (M-to-N entity)
CREATE TABLE Guide (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Bio NVARCHAR(500) NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Phone NVARCHAR(20) NULL,
    ImageUrl NVARCHAR(500) NULL,
    YearsOfExperience INT NULL
);

-- Create TripGuide table (bridge table)
CREATE TABLE TripGuide (
    TripId INT NOT NULL,
    GuideId INT NOT NULL,
    PRIMARY KEY (TripId, GuideId),
    FOREIGN KEY (TripId) REFERENCES Trip(Id) ON DELETE CASCADE,
    FOREIGN KEY (GuideId) REFERENCES Guide(Id) ON DELETE CASCADE
);

-- Create User table
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

-- Create TripRegistration table (User's M-to-N entity)
CREATE TABLE TripRegistration (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    TripId INT NOT NULL,
    RegistrationDate DATETIME NOT NULL DEFAULT GETDATE(),
    NumberOfParticipants INT NOT NULL DEFAULT 1,
    TotalPrice DECIMAL(10, 2) NOT NULL,
    Status NVARCHAR(50) NOT NULL, -- Pending, Confirmed, Cancelled
    FOREIGN KEY (UserId) REFERENCES [User](Id),
    FOREIGN KEY (TripId) REFERENCES Trip(Id),
    CONSTRAINT UC_UserTrip UNIQUE (UserId, TripId)
);

-- Create Log table for API logging
CREATE TABLE Log (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Timestamp DATETIME NOT NULL DEFAULT GETDATE(),
    Level NVARCHAR(50) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL
);

-- Insert sample data for Destination
INSERT INTO Destination (Name, Description, Country, City) VALUES 
('Paris', 'The City of Light', 'France', 'Paris'),
('Rome', 'The Eternal City', 'Italy', 'Rome'),
('Barcelona', 'Catalonia''s vibrant capital', 'Spain', 'Barcelona'),
('London', 'Historic metropolitan city', 'United Kingdom', 'London'),
('Tokyo', 'Japan''s bustling capital', 'Japan', 'Tokyo');

-- Insert sample data for Guide
INSERT INTO Guide (Name, Bio, Email, Phone, ImageUrl, YearsOfExperience) VALUES 
('John Smith', 'Specialized in European history and architecture', 'john.smith@guides.com', '+385-91-123-4567', 'john_smith.jpg', 8),
('Maria Garcia', 'Expert in Mediterranean cultures and cuisine', 'maria.garcia@guides.com', '+385-92-234-5678', 'maria_garcia.jpg', 5),
('Takashi Yamamoto', 'Specialized in Asian culture and traditions', 'takashi.yamamoto@guides.com', '+385-95-345-6789', 'takashi_yamamoto.jpg', 10),
('Emma Wilson', 'Art history expert with focus on European museums', 'emma.wilson@guides.com', '+385-98-456-7890', 'emma_wilson.jpg', 7),
('Carlos Rodriguez', 'Adventure travel expert with climbing experience', 'carlos.rodriguez@guides.com', '+385-99-567-8901', 'carlos_rodriguez.jpg', 9);

-- Insert sample data for Trip
INSERT INTO Trip (Name, Description, StartDate, EndDate, Price, ImageUrl, MaxParticipants, DestinationId) VALUES 
('Paris Art Tour', 'Explore the best museums and galleries of Paris', '2025-06-15', '2025-06-22', 1200.00, 'paris_art.jpg', 15, 1),
('Rome Historical Experience', 'Walk through the ancient Roman Empire', '2025-07-10', '2025-07-17', 1350.00, 'rome_historical.jpg', 12, 2),
('Barcelona Beach & Culture', 'Experience Barcelona''s beaches and architecture', '2025-08-05', '2025-08-12', 1150.00, 'barcelona_beach.jpg', 20, 3),
('London Theater Week', 'Enjoy the best plays and musicals in London', '2025-09-20', '2025-09-27', 1400.00, 'london_theater.jpg', 18, 4),
('Tokyo Technology Tour', 'Discover Japan''s technological innovations', '2025-10-15', '2025-10-25', 1800.00, 'tokyo_tech.jpg', 15, 5);

-- Insert sample data for TripGuide connections
INSERT INTO TripGuide (TripId, GuideId) VALUES 
(1, 1), -- John Smith guides Paris Art Tour
(1, 4), -- Emma Wilson also guides Paris Art Tour
(2, 1), -- John Smith guides Rome Historical Experience
(2, 2), -- Maria Garcia also guides Rome Historical Experience
(3, 2), -- Maria Garcia guides Barcelona Beach & Culture
(3, 5), -- Carlos Rodriguez also guides Barcelona Beach & Culture
(4, 4), -- Emma Wilson guides London Theater Week
(5, 3); -- Takashi Yamamoto guides Tokyo Technology Tour

-- Insert sample user (admin)
INSERT INTO [User] (Username, Email, PasswordHash, FirstName, LastName, PhoneNumber, Address, IsAdmin) VALUES 
('admin', 'admin@travelplanner.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'Admin', 'User', '123-456-7890', '123 Admin St', 1);

-- Insert sample user (regular user)
INSERT INTO [User] (Username, Email, PasswordHash, FirstName, LastName, PhoneNumber, Address, IsAdmin) VALUES 
('johndoe', 'john.doe@example.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'John', 'Doe', '234-567-8901', '456 Main St', 0),
('janesmith', 'jane.smith@example.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'Jane', 'Smith', '345-678-9012', '789 Oak St', 0);

-- Insert sample trip registrations
INSERT INTO TripRegistration (UserId, TripId, RegistrationDate, NumberOfParticipants, TotalPrice, Status) VALUES 
(2, 1, DATEADD(day, -10, GETDATE()), 2, 2400.00, 'Confirmed'), -- John Doe registered for Paris Art Tour with 2 participants
(2, 3, DATEADD(day, -5, GETDATE()), 1, 1150.00, 'Pending'),   -- John Doe registered for Barcelona Beach & Culture with 1 participant
(3, 2, DATEADD(day, -7, GETDATE()), 2, 2700.00, 'Confirmed'), -- Jane Smith registered for Rome Historical Experience with 2 participants
(3, 4, DATEADD(day, -3, GETDATE()), 1, 1400.00, 'Cancelled'); -- Jane Smith registered for London Theater Week but cancelled

select * from [User]

select * from [Destination]

select * from TripRegistration
