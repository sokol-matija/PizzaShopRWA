-- Test Trip: Create new trip for Paris with John Smith as guide
-- This trip will test the image population and fallback mechanism

USE TravelOrganizationDB;
GO

-- Enable IDENTITY_INSERT to allow explicit ID insertion
SET IDENTITY_INSERT Trip ON;

-- Trip 16: Paris Seine River Cruise (new test trip for destination Paris)
INSERT INTO Trip (Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId, ImageUrl)
VALUES (16, 'Paris Seine River Cruise', 'Experience Paris from the water with a romantic Seine River cruise, including dinner, wine tasting, and views of iconic landmarks like Notre Dame and the Eiffel Tower', '2025-05-20', '2025-05-27', 1680.00, 12, 1, NULL);

-- Disable IDENTITY_INSERT 
SET IDENTITY_INSERT Trip OFF;

-- Assign John Smith (Guide ID 1) to this trip
INSERT INTO TripGuide (TripId, GuideId)
VALUES (16, 1);

-- Verify the insertion - show the newly created trip with its guide and destination info
SELECT 
    t.Id as TripId,
    t.Name as TripName,
    t.ImageUrl as TripImageUrl,
    d.Name as DestinationName,
    d.ImageUrl as DestinationImageUrl,
    g.Name as GuideName
FROM Trip t
LEFT JOIN Destination d ON t.DestinationId = d.Id
LEFT JOIN TripGuide tg ON t.Id = tg.TripId
LEFT JOIN Guide g ON tg.GuideId = g.Id
WHERE t.Id = 16;

PRINT 'Trip 16 created successfully!';
PRINT 'Now you can test the image population and fallback mechanism.';

-- Show summary of all trips that need images
SELECT 
    COUNT(*) as TripsNeedingImages
FROM Trip 
WHERE ImageUrl IS NULL; 