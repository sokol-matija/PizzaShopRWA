
-- Final Azure Data Sync Script
-- Generated on 07/01/2025 16:26:54
USE [TravelOrganizationDB]
GO

SET IDENTITY_INSERT Destination ON;
SET IDENTITY_INSERT Guide ON;
SET IDENTITY_INSERT Trip ON;
SET IDENTITY_INSERT [User] ON;
SET IDENTITY_INSERT TripRegistration ON;

-- Destination Data
GO
INSERT INTO Destination (Id, Name, Description, Country, City) VALUES (1, 'Paris', 'The City of Light', 'France', 'Paris');
INSERT INTO Destination (Id, Name, Description, Country, City) VALUES (2, 'Rome', 'The Eternal City', 'Italy', 'Rome');
INSERT INTO Destination (Id, Name, Description, Country, City) VALUES (3, 'Barcelona', 'Catalonia''s vibrant capital', 'Spain', 'Barcelona');
INSERT INTO Destination (Id, Name, Description, Country, City) VALUES (4, 'London', 'Historic metropolitan city', 'United Kingdom', 'London');
INSERT INTO Destination (Id, Name, Description, Country, City) VALUES (5, 'Tokyo', 'Japan''s bustling capital', 'Japan', 'Tokyo');
INSERT INTO Destination (Id, Name, Description, Country, City) VALUES (7, 'Zagreb Centar Nepar', 'Voltino', 'Croatia', 'Zagrbe');

-- Guide Data
GO
INSERT INTO Guide (Id, Name, Bio, Email, Phone) VALUES (1, 'John Smith', 'Specialized in European history and architecture', 'john.smith@guides.com', '+385-91-123-4567');
INSERT INTO Guide (Id, Name, Bio, Email, Phone) VALUES (2, 'Maria Garcia', 'Expert in Mediterranean cultures and cuisine', 'maria.garcia@guides.com', '+385-92-234-5678');
INSERT INTO Guide (Id, Name, Bio, Email, Phone) VALUES (3, 'Takashi Yamamoto', 'Specialized in Asian culture and traditions', 'takashi.yamamoto@guides.com', '+385-95-345-6789');
INSERT INTO Guide (Id, Name, Bio, Email, Phone) VALUES (4, 'Emma Wilson', 'Art history expert with focus on European museums', 'emma.wilson@guides.com', '+385-98-456-7890');
INSERT INTO Guide (Id, Name, Bio, Email, Phone) VALUES (5, 'Carlos Rodriguez', 'Adventure travel expert with climbing experience', 'carlos.rodriguez@guides.com', '+385-99-567-8901');

-- Trip Data
GO
INSERT INTO Trip (Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId) VALUES (1, 'Paris Art Tour', 'Explore the best museums and galleries of Paris', '2025-06-15', '2025-06-22', 1200.00, 15, 1);
INSERT INTO Trip (Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId) VALUES (2, 'Rome Historical Experience', 'Walk through the ancient Roman Empire', '2025-07-10', '2025-07-17', 1350.00, 12, 2);
INSERT INTO Trip (Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId) VALUES (3, 'Barcelona Beach & Culture', 'Experience Barcelona''s beaches and architecture', '2025-08-05', '2025-08-12', 1150.00, 20, 3);
INSERT INTO Trip (Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId) VALUES (4, 'London Theater Week', 'Enjoy the best plays and musicals in London', '2025-09-20', '2025-09-27', 1400.00, 18, 4);
INSERT INTO Trip (Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId) VALUES (5, 'Tokyo Technology Tour', 'Discover Japan''s technological innovations', '2025-10-15', '2025-10-25', 1800.00, 15, 5);
INSERT INTO Trip (Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId) VALUES (6, 'Paris Fashion & Shopping', 'Discover Parisian haute couture, boutique shopping on Champs-Élysées, and fashion district tours', '2025-07-07', '2025-07-30', 1300.00, 12, 1);
INSERT INTO Trip (Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId) VALUES (7, 'Rome Food & Wine Experience', 'Authentic Italian cooking classes, wine tastings in Trastevere, and local market tours', '2025-06-08', '2025-06-15', 1250.00, 15, 2);
INSERT INTO Trip (Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId) VALUES (8, 'Barcelona Architecture Walk', 'In-depth exploration of Gaudí masterpieces, Gothic Quarter, and modernist buildings', '2025-07-12', '2025-07-19', 1180.00, 18, 3);
INSERT INTO Trip (Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId) VALUES (9, 'London Royal Heritage', 'Visit royal palaces, crown jewels, changing of the guard, and afternoon tea experiences', '2025-08-18', '2025-08-25', 1420.00, 16, 4);
INSERT INTO Trip (Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId) VALUES (10, 'Tokyo Modern Culture', 'Experience anime culture, gaming districts, modern art museums, and tech innovations', '2025-09-10', '2025-09-20', 1650.00, 14, 5);
INSERT INTO Trip (Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId) VALUES (11, 'Paris Culinary Journey', 'Master French cuisine with professional chefs, visit local markets, bistro tours, and wine pairings in authentic Parisian neighborhoods', '2025-11-05', '2025-11-12', 1450.00, 14, 1);
INSERT INTO Trip (Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId) VALUES (12, 'Rome Countryside Escape', 'Explore Tuscan hills, visit ancient Roman villas, wine estates, olive groves, and charming medieval towns around Rome', '2025-12-01', '2025-12-08', 1380.00, 16, 2);
INSERT INTO Trip (Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId) VALUES (13, 'Barcelona Nightlife & Music', 'Experience Barcelona''s vibrant music scene, flamenco shows, tapas crawls, rooftop bars, and underground music venues', '2025-08-25', '2025-09-01', 1220.00, 20, 3);
INSERT INTO Trip (Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId) VALUES (14, 'London Literary Heritage', 'Follow the footsteps of Shakespeare, Dickens, and Sherlock Holmes through historic London with literary walking tours and historic pubs', '2025-10-08', '2025-10-15', 1350.00, 18, 4);
INSERT INTO Trip (Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId) VALUES (15, 'Tokyo Traditional Culture', 'Discover ancient temples, tea ceremonies, traditional crafts, sumo wrestling, and authentic ryokan experiences in historic Tokyo districts', '2025-11-20', '2025-11-30', 1750.00, 12, 5);
INSERT INTO Trip (Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId) VALUES (16, 'Paris Seine River Cruise', 'Experience Paris from the water with a romantic Seine River cruise, including dinner, wine tasting, and views of iconic landmarks like Notre Dame and the Eiffel Tower', '2025-09-11', '2025-09-27', 1680.00, 12, 1);

-- User Data
GO
INSERT INTO [User] (Id, Username, Email, PasswordHash, IsAdmin) VALUES (1, 'admin', 'admin@travel.com', 'hashed_password_here', 1);
INSERT INTO [User] (Id, Username, Email, PasswordHash, IsAdmin) VALUES (2, 'user1', 'user1@travel.com', 'hashed_password_here', 0);
INSERT INTO [User] (Id, Username, Email, PasswordHash, IsAdmin) VALUES (3, 'user2', 'user2@travel.com', 'hashed_password_here', 0);

-- TripRegistration Data
GO
INSERT INTO TripRegistration (Id, UserId, TripId, RegistrationDate, Status) VALUES (1, 2, 1, '2025-03-18', 'Confirmed');
INSERT INTO TripRegistration (Id, UserId, TripId, RegistrationDate, Status) VALUES (2, 2, 3, '2025-03-23', 'Pending');
INSERT INTO TripRegistration (Id, UserId, TripId, RegistrationDate, Status) VALUES (3, 3, 2, '2025-03-21', 'Confirmed');
INSERT INTO TripRegistration (Id, UserId, TripId, RegistrationDate, Status) VALUES (4, 3, 4, '2025-03-25', 'Cancelled');
INSERT INTO TripRegistration (Id, UserId, TripId, RegistrationDate, Status) VALUES (5, 5, 1, '2025-06-29', 'Pending');
INSERT INTO TripRegistration (Id, UserId, TripId, RegistrationDate, Status) VALUES (6, 5, 4, '2025-06-30', 'Pending');
INSERT INTO TripRegistration (Id, UserId, TripId, RegistrationDate, Status) VALUES (8, 5, 5, '2025-06-30', 'Pending');

-- TripGuide Data
GO
INSERT INTO TripGuide (TripId, GuideId) VALUES (1, 4);
INSERT INTO TripGuide (TripId, GuideId) VALUES (1, 5);
INSERT INTO TripGuide (TripId, GuideId) VALUES (2, 1);
INSERT INTO TripGuide (TripId, GuideId) VALUES (2, 2);
INSERT INTO TripGuide (TripId, GuideId) VALUES (3, 2);
INSERT INTO TripGuide (TripId, GuideId) VALUES (3, 5);
INSERT INTO TripGuide (TripId, GuideId) VALUES (4, 4);
INSERT INTO TripGuide (TripId, GuideId) VALUES (5, 3);
INSERT INTO TripGuide (TripId, GuideId) VALUES (6, 1);
INSERT INTO TripGuide (TripId, GuideId) VALUES (6, 4);
INSERT INTO TripGuide (TripId, GuideId) VALUES (7, 1);
INSERT INTO TripGuide (TripId, GuideId) VALUES (7, 2);
INSERT INTO TripGuide (TripId, GuideId) VALUES (8, 2);
INSERT INTO TripGuide (TripId, GuideId) VALUES (8, 5);
INSERT INTO TripGuide (TripId, GuideId) VALUES (9, 1);
INSERT INTO TripGuide (TripId, GuideId) VALUES (9, 4);
INSERT INTO TripGuide (TripId, GuideId) VALUES (10, 3);
INSERT INTO TripGuide (TripId, GuideId) VALUES (11, 2);
INSERT INTO TripGuide (TripId, GuideId) VALUES (12, 1);
INSERT INTO TripGuide (TripId, GuideId) VALUES (12, 5);
INSERT INTO TripGuide (TripId, GuideId) VALUES (13, 2);
INSERT INTO TripGuide (TripId, GuideId) VALUES (13, 5);
INSERT INTO TripGuide (TripId, GuideId) VALUES (14, 1);
INSERT INTO TripGuide (TripId, GuideId) VALUES (14, 4);
INSERT INTO TripGuide (TripId, GuideId) VALUES (15, 3);
INSERT INTO TripGuide (TripId, GuideId) VALUES (16, 1);

SET IDENTITY_INSERT Destination OFF;
SET IDENTITY_INSERT Guide OFF;
SET IDENTITY_INSERT Trip OFF;
SET IDENTITY_INSERT [User] OFF;
SET IDENTITY_INSERT TripRegistration OFF;
