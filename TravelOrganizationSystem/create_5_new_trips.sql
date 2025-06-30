-- SQL for 5 new trips (Trip IDs 11-15)

-- Trip 11: Paris Culinary Journey (complement Art Tour & Fashion Shopping)
INSERT INTO Trip (Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId, ImageUrl)
VALUES (11, 'Paris Culinary Journey', 'Master French cuisine with professional chefs, visit local markets, bistro tours, and wine pairings in authentic Parisian neighborhoods', '2025-11-05', '2025-11-12', 1450.00, 14, 1, NULL);

-- Trip 12: Rome Countryside Escape (complement Historical & Food/Wine)
INSERT INTO Trip (Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId, ImageUrl)
VALUES (12, 'Rome Countryside Escape', 'Explore Tuscan hills, visit ancient Roman villas, wine estates, olive groves, and charming medieval towns around Rome', '2025-12-01', '2025-12-08', 1380.00, 16, 2, NULL);

-- Trip 13: Barcelona Nightlife & Music (complement Beach/Culture & Architecture)
INSERT INTO Trip (Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId, ImageUrl)
VALUES (13, 'Barcelona Nightlife & Music', 'Experience Barcelona''s vibrant music scene, flamenco shows, tapas crawls, rooftop bars, and underground music venues', '2025-08-25', '2025-09-01', 1220.00, 20, 3, NULL);

-- Trip 14: London Literary Heritage (complement Theater & Royal Heritage)
INSERT INTO Trip (Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId, ImageUrl)
VALUES (14, 'London Literary Heritage', 'Follow the footsteps of Shakespeare, Dickens, and Sherlock Holmes through historic London with literary walking tours and historic pubs', '2025-10-08', '2025-10-15', 1350.00, 18, 4, NULL);

-- Trip 15: Tokyo Traditional Culture (complement Technology & Modern Culture)
INSERT INTO Trip (Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId, ImageUrl)
VALUES (15, 'Tokyo Traditional Culture', 'Discover ancient temples, tea ceremonies, traditional crafts, sumo wrestling, and authentic ryokan experiences in historic Tokyo districts', '2025-11-20', '2025-11-30', 1750.00, 12, 5, NULL);
