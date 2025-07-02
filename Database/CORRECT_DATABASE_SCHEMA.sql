-- CORRECT DATABASE SCHEMA FOR TRAVEL ORGANIZATION SYSTEM
-- This script creates the database exactly as it exists in the local environment
-- Generated on: 2025-07-01

-- Create Destination table (1-to-N entity)
CREATE TABLE Destination (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL,
    Country NVARCHAR(100) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    ImageUrl NVARCHAR(500) NULL
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

-- Create TripGuide table (bridge table)
CREATE TABLE TripGuide (
    TripId INT NOT NULL,
    GuideId INT NOT NULL,
    PRIMARY KEY (TripId, GuideId),
    FOREIGN KEY (TripId) REFERENCES Trip(Id) ON DELETE CASCADE,
    FOREIGN KEY (GuideId) REFERENCES Guide(Id) ON DELETE CASCADE
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

-- =============================================
-- SAMPLE DATA (matching local database exactly)
-- =============================================

-- Insert sample data for Destination (6 destinations)
INSERT INTO Destination (Name, Description, Country, City, ImageUrl) VALUES 
('Paris', 'The City of Light', 'France', 'Paris', 'https://images.unsplash.com/photo-1566977309384-d145e7ab1615?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NDMxNjcwNjJ8&ixlib=rb-4.0.3&q=80&w=1080'),
('Rome', 'The Eternal City', 'Italy', 'Rome', 'https://images.unsplash.com/photo-1529154166925-574a0236a4f4?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NDMxNjcwNjN8&ixlib=rb-4.0.3&q=80&w=1080'),
('Barcelona', 'Catalonia''s vibrant capital', 'Spain', 'Barcelona', 'https://images.unsplash.com/photo-1736791418468-f822fad5fb7c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NDMxNjcwNjR8&ixlib=rb-4.0.3&q=80&w=1080'),
('London', 'Historic metropolitan city', 'United Kingdom', 'London', 'https://images.unsplash.com/photo-1500301111609-42f1aa6df72a?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NDMxNjcwNjR8&ixlib=rb-4.0.3&q=80&w=1080'),
('Tokyo', 'Japan''s bustling capital', 'Japan', 'Tokyo', 'https://images.unsplash.com/photo-1556923590-4a2473e29549?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NDMxNjcwNjV8&ixlib=rb-4.0.3&q=80&w=1080'),
('Zagreb Centar Nepar', 'Voltino', 'Croatia', 'Zagrbe', 'https://images.unsplash.com/photo-1658008193946-7b594ee5c0f1?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NTEyOTc0MDN8&ixlib=rb-4.1.0&q=80&w=1080');

-- Insert sample data for Guide (5 guides)
INSERT INTO Guide (Name, Bio, Email, Phone, ImageUrl, YearsOfExperience) VALUES 
('John Smith', 'Specialized in European history and architecture', 'john.smith@guides.com', '+385-91-123-4567', 'john_smith.jpg', 8),
('Maria Garcia', 'Expert in Mediterranean cultures and cuisine', 'maria.garcia@guides.com', '+385-92-234-5678', 'maria_garcia.jpg', 5),
('Takashi Yamamoto', 'Specialized in Asian culture and traditions', 'takashi.yamamoto@guides.com', '+385-95-345-6789', 'takashi_yamamoto.jpg', 10),
('Emma Wilson', 'Art history expert with focus on European museums', 'emma.wilson@guides.com', '+385-98-456-7890', 'emma_wilson.jpg', 7),
('Carlos Rodriguez', 'Adventure travel expert with climbing experience', 'carlos.rodriguez@guides.com', '+385-99-567-8901', 'carlos_rodriguez.jpg', 9);

-- Insert sample data for Trip (16 trips)
INSERT INTO Trip (Name, Description, StartDate, EndDate, Price, ImageUrl, MaxParticipants, DestinationId) VALUES 
('Paris Art Tour', 'Explore the best museums and galleries of Paris', '2025-06-15', '2025-06-22', 1200.00, 'https://images.unsplash.com/photo-1729687996499-149d33d87771?test=manual', 15, 1),
('Rome Historical Experience', 'Walk through the ancient Roman Empire', '2025-07-10', '2025-07-17', 1350.00, 'https://images.unsplash.com/photo-1515542622106-78bda8ba0e5b?w=1080&q=80&fit=max&auto=format', 12, 2),
('Barcelona Beach & Culture', 'Experience Barcelona''s beaches and architecture', '2025-08-05', '2025-08-12', 1150.00, 'https://images.unsplash.com/photo-1583422409516-2895a77efded?barcelona-sagrada', 20, 3),
('London Theater Week', 'Enjoy the best plays and musicals in London', '2025-09-20', '2025-09-27', 1400.00, 'https://images.unsplash.com/photo-1533929736458-ca588d08c8be?london-theater', 18, 4),
('Tokyo Technology Tour', 'Discover Japan''s technological innovations', '2025-10-15', '2025-10-25', 1800.00, 'https://images.unsplash.com/photo-1540959733332-eab4deabeeaf?w=1080&q=80&fit=max&auto=format', 15, 5),
('Paris Fashion & Shopping', 'Discover Parisian haute couture, boutique shopping on Champs-Élysées, and fashion district tours', '2025-07-07', '2025-07-30', 1300.00, 'https://images.unsplash.com/photo-1632742335890-5b8cb8c47585?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NTEzMDAwNDd8&ixlib=rb-4.1.0&q=80&w=1080', 12, 1),
('Rome Food & Wine Experience', 'Authentic Italian cooking classes, wine tastings in Trastevere, and local market tours', '2025-06-08', '2025-06-15', 1250.00, 'https://images.unsplash.com/photo-1414235077428-338989a2e8c0?rome-food-wine', 15, 2),
('Barcelona Architecture Walk', 'In-depth exploration of Gaudí masterpieces, Gothic Quarter, and modernist buildings', '2025-07-12', '2025-07-19', 1180.00, 'https://images.unsplash.com/photo-1523531294919-4bcd7c65e216?barcelona-gaudi', 18, 3),
('London Royal Heritage', 'Visit royal palaces, crown jewels, changing of the guard, and afternoon tea experiences', '2025-08-18', '2025-08-25', 1420.00, 'https://images.unsplash.com/photo-1513635269975-59663e0ac1ad?london-palace', 16, 4),
('Tokyo Modern Culture', 'Experience anime culture, gaming districts, modern art museums, and tech innovations', '2025-09-10', '2025-09-20', 1650.00, 'https://images.unsplash.com/photo-1542051841857-5f90071e7989?tokyo-modern', 14, 5),
('Paris Culinary Journey', 'Master French cuisine with professional chefs, visit local markets, bistro tours, and wine pairings in authentic Parisian neighborhoods', '2025-11-05', '2025-11-12', 1450.00, 'https://images.unsplash.com/photo-1564503022941-233d54adb4aa?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NTEzMDExMTR8&ixlib=rb-4.1.0&q=80&w=1080', 14, 1),
('Rome Countryside Escape', 'Explore Tuscan hills, visit ancient Roman villas, wine estates, olive groves, and charming medieval towns around Rome', '2025-12-01', '2025-12-08', 1380.00, 'https://images.unsplash.com/photo-1666259903467-f0fae966a21f?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NTEzMDEyMTF8&ixlib=rb-4.1.0&q=80&w=1080', 16, 2),
('Barcelona Nightlife & Music', 'Experience Barcelona''s vibrant music scene, flamenco shows, tapas crawls, rooftop bars, and underground music venues', '2025-08-25', '2025-09-01', 1220.00, 'https://images.unsplash.com/photo-1577264940948-8e4de22849b7?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NTEzMDEyMDV8&ixlib=rb-4.1.0&q=80&w=1080', 20, 3),
('London Literary Heritage', 'Follow the footsteps of Shakespeare, Dickens, and Sherlock Holmes through historic London with literary walking tours and historic pubs', '2025-10-08', '2025-10-15', 1350.00, 'https://images.unsplash.com/photo-1707358770118-dd35fc6ffedb?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NTEzMDEyMDB8&ixlib=rb-4.1.0&q=80&w=1080', 18, 4),
('Tokyo Traditional Culture', 'Discover ancient temples, tea ceremonies, traditional crafts, sumo wrestling, and authentic ryokan experiences in historic Tokyo districts', '2025-11-20', '2025-11-30', 1750.00, 'https://images.unsplash.com/photo-1507693595546-0512d61de389?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NTEzMDExOTN8&ixlib=rb-4.1.0&q=80&w=1080', 12, 5),
('Paris Seine River Cruise', 'Experience Paris from the water with a romantic Seine River cruise, including dinner, wine tasting, and views of iconic landmarks like Notre Dame and the Eiffel Tower', '2025-09-11', '2025-09-27', 1680.00, 'https://images.unsplash.com/photo-1743184345435-142722afa4f5?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NTEzMDExNzJ8&ixlib=rb-4.1.0&q=80&w=1080', 12, 1);

-- Insert sample data for User (3 users)
INSERT INTO [User] (Username, Email, PasswordHash, FirstName, LastName, PhoneNumber, Address, IsAdmin) VALUES 
('admin', 'admin@travel.com', 'hashed_password_here', 'Admin', 'User', '123-456-7890', '123 Admin St', 1),
('user1', 'user1@travel.com', 'hashed_password_here', 'John', 'Doe', '234-567-8901', '456 Main St', 0),
('user2', 'user2@travel.com', 'hashed_password_here', 'Jane', 'Smith', '345-678-9012', '789 Oak St', 0);

-- Insert sample data for TripRegistration (7 registrations)
INSERT INTO TripRegistration (UserId, TripId, RegistrationDate, NumberOfParticipants, TotalPrice, Status) VALUES 
(2, 1, '2025-03-18', 2, 2400.00, 'Confirmed'),
(2, 3, '2025-03-23', 1, 1150.00, 'Pending'),
(3, 2, '2025-03-21', 2, 2700.00, 'Confirmed'),
(3, 4, '2025-03-25', 1, 1400.00, 'Cancelled'),
(5, 1, '2025-06-29', 1, 1200.00, 'Pending'),
(5, 4, '2025-06-30', 1, 1400.00, 'Pending'),
(5, 5, '2025-06-30', 1, 1800.00, 'Pending');

-- Insert sample data for TripGuide connections (26 assignments)
INSERT INTO TripGuide (TripId, GuideId) VALUES 
(1, 4), (1, 5), (2, 1), (2, 2), (3, 2), (3, 5), (4, 4), (5, 3),
(6, 1), (6, 4), (7, 1), (7, 2), (8, 2), (8, 5), (9, 1), (9, 4),
(10, 3), (11, 2), (12, 1), (12, 5), (13, 2), (13, 5), (14, 1), (14, 4),
(15, 3), (16, 1);

-- =============================================
-- VERIFICATION QUERIES
-- =============================================

-- Verify data counts
SELECT 'Destination' as TableName, COUNT(*) as Count FROM Destination
UNION ALL
SELECT 'Guide', COUNT(*) FROM Guide
UNION ALL
SELECT 'Trip', COUNT(*) FROM Trip
UNION ALL
SELECT 'User', COUNT(*) FROM [User]
UNION ALL
SELECT 'TripRegistration', COUNT(*) FROM TripRegistration
UNION ALL
SELECT 'TripGuide', COUNT(*) FROM TripGuide; 