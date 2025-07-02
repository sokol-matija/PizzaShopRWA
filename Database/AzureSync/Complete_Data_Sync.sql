USE [TravelOrganizationDB]
GO

-- Complete Data Sync for Azure SQL Database
-- Generated on 07/01/2025 16:22:05

-- Destination Data
GO
INSERT INTO Destination VALUES ('1 Paris', 'The City of Light', 'France', 'Paris', 'https://images.unsplash.com/photo-1566977309384-d145e7ab1615?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NDMxNjcwNjJ8&ixlib=rb-4.0.3&q=80&w=1080');
INSERT INTO Destination VALUES ('2 Rome', 'The Eternal City', 'Italy', 'Rome', 'https://images.unsplash.com/photo-1529154166925-574a0236a4f4?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NDMxNjcwNjN8&ixlib=rb-4.0.3&q=80&w=1080');
INSERT INTO Destination VALUES ('3 Barcelona', 'Catalonia''s vibrant capital', 'Spain', 'Barcelona', 'https://images.unsplash.com/photo-1736791418468-f822fad5fb7c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NDMxNjcwNjR8&ixlib=rb-4.0.3&q=80&w=1080');
INSERT INTO Destination VALUES ('4 London', 'Historic metropolitan city', 'United Kingdom', 'London', 'https://images.unsplash.com/photo-1500301111609-42f1aa6df72a?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NDMxNjcwNjR8&ixlib=rb-4.0.3&q=80&w=1080');
INSERT INTO Destination VALUES ('5 Tokyo', 'Japan''s bustling capital', 'Japan', 'Tokyo', 'https://images.unsplash.com/photo-1556923590-4a2473e29549?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NDMxNjcwNjV8&ixlib=rb-4.0.3&q=80&w=1080');
INSERT INTO Destination VALUES ('7 Zagreb Centar Nepar', 'Voltino', 'Croatia', 'Zagrbe', 'https://images.unsplash.com/photo-1658008193946-7b594ee5c0f1?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NTEyOTc0MDN8&ixlib=rb-4.1.0&q=80&w=1080');

-- Guide Data
GO
INSERT INTO Guide VALUES ('1 John Smith', 'Specialized in European history and architecture', 'john.smith@guides.com', '+385-91-123-4567', 'john_smith.jpg', 8);
INSERT INTO Guide VALUES ('2 Maria Garcia', 'Expert in Mediterranean cultures and cuisine', 'maria.garcia@guides.com', '+385-92-234-5678', 'maria_garcia.jpg', 5);
INSERT INTO Guide VALUES ('3 Takashi Yamamoto', 'Specialized in Asian culture and traditions', 'takashi.yamamoto@guides.com', '+385-95-345-6789', 'takashi_yamamoto.jpg', 10);
INSERT INTO Guide VALUES ('4 Emma Wilson', 'Art history expert with focus on European museums', 'emma.wilson@guides.com', '+385-98-456-7890', 'emma_wilson.jpg', 7);
INSERT INTO Guide VALUES ('5 Carlos Rodriguez', 'Adventure travel expert with climbing experience', 'carlos.rodriguez@guides.com', '+385-99-567-8901', 'carlos_rodriguez.jpg', 9);

-- Trip Data
GO
INSERT INTO Trip VALUES ('1 Paris Art Tour', 'Explore the best museums and galleries of Paris', '2025-06-15 00:00:00.000 2025-06-22 00:00:00.000', '1200.00 https://images.unsplash.com/photo-1729687996499-149d33d87771?test=manual', 15, 1);
INSERT INTO Trip VALUES ('2 Rome Historical Experience', 'Walk through the ancient Roman Empire', '2025-07-10 00:00:00.000 2025-07-17 00:00:00.000', '1350.00 https://images.unsplash.com/photo-1515542622106-78bda8ba0e5b?w=1080&q=80&fit=max&auto=format', 12, 2);
INSERT INTO Trip VALUES ('3 Barcelona Beach & Culture', 'Experience Barcelona''s beaches and architecture', '2025-08-05 00:00:00.000 2025-08-12 00:00:00.000', '1150.00 https://images.unsplash.com/photo-1583422409516-2895a77efded?barcelona-sagrada', 20, 3);
INSERT INTO Trip VALUES ('4 London Theater Week', 'Enjoy the best plays and musicals in London', '2025-09-20 00:00:00.000 2025-09-27 00:00:00.000', '1400.00 https://images.unsplash.com/photo-1533929736458-ca588d08c8be?london-theater', 18, 4);
INSERT INTO Trip VALUES ('5 Tokyo Technology Tour', 'Discover Japan''s technological innovations', '2025-10-15 00:00:00.000 2025-10-25 00:00:00.000', '1800.00 https://images.unsplash.com/photo-1540959733332-eab4deabeeaf?w=1080&q=80&fit=max&auto=format', 15, 5);
INSERT INTO Trip VALUES ('6 Paris Fashion & Shopping', 'Discover Parisian haute couture, boutique shopping on Champs-Élysées, and fashion district tours', '2025-07-07 00:00:00.000 2025-07-30 00:00:00.000', '1300.00 https://images.unsplash.com/photo-1632742335890-5b8cb8c47585?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NTEzMDAwNDd8&ixlib=rb-4.1.0&q=80&w=1080', 12, 1);
INSERT INTO Trip VALUES ('7 Rome Food & Wine Experience', 'Authentic Italian cooking classes, wine tastings in Trastevere, and local market tours', '2025-06-08 00:00:00.000 2025-06-15 00:00:00.000', '1250.00 https://images.unsplash.com/photo-1414235077428-338989a2e8c0?rome-food-wine', 15, 2);
INSERT INTO Trip VALUES ('8 Barcelona Architecture Walk', 'In-depth exploration of Gaudí masterpieces, Gothic Quarter, and modernist buildings', '2025-07-12 00:00:00.000 2025-07-19 00:00:00.000', '1180.00 https://images.unsplash.com/photo-1523531294919-4bcd7c65e216?barcelona-gaudi', 18, 3);
INSERT INTO Trip VALUES ('9 London Royal Heritage', 'Visit royal palaces, crown jewels, changing of the guard, and afternoon tea experiences', '2025-08-18 00:00:00.000 2025-08-25 00:00:00.000', '1420.00 https://images.unsplash.com/photo-1513635269975-59663e0ac1ad?london-palace', 16, 4);
INSERT INTO Trip VALUES ('10 Tokyo Modern Culture', 'Experience anime culture, gaming districts, modern art museums, and tech innovations', '2025-09-10 00:00:00.000 2025-09-20 00:00:00.000', '1650.00 https://images.unsplash.com/photo-1542051841857-5f90071e7989?tokyo-modern', 14, 5);
INSERT INTO Trip VALUES ('11 Paris Culinary Journey', 'Master French cuisine with professional chefs, visit local markets, bistro tours, and wine pairings in authentic Parisian neighborhoods', '2025-11-05 00:00:00.000 2025-11-12 00:00:00.000', '1450.00 https://images.unsplash.com/photo-1564503022941-233d54adb4aa?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NTEzMDExMTR8&ixlib=rb-4.1.0&q=80&w=1080', 14, 1);
INSERT INTO Trip VALUES ('12 Rome Countryside Escape', 'Explore Tuscan hills, visit ancient Roman villas, wine estates, olive groves, and charming medieval towns around Rome', '2025-12-01 00:00:00.000 2025-12-08 00:00:00.000', '1380.00 https://images.unsplash.com/photo-1666259903467-f0fae966a21f?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NTEzMDEyMTF8&ixlib=rb-4.1.0&q=80&w=1080', 16, 2);
INSERT INTO Trip VALUES ('13 Barcelona Nightlife & Music', 'Experience Barcelona''s vibrant music scene, flamenco shows, tapas crawls, rooftop bars, and underground music venues', '2025-08-25 00:00:00.000 2025-09-01 00:00:00.000', '1220.00 https://images.unsplash.com/photo-1577264940948-8e4de22849b7?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NTEzMDEyMDV8&ixlib=rb-4.1.0&q=80&w=1080', 20, 3);
INSERT INTO Trip VALUES ('14 London Literary Heritage', 'Follow the footsteps of Shakespeare, Dickens, and Sherlock Holmes through historic London with literary walking tours and historic pubs', '2025-10-08 00:00:00.000 2025-10-15 00:00:00.000', '1350.00 https://images.unsplash.com/photo-1707358770118-dd35fc6ffedb?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NTEzMDEyMDB8&ixlib=rb-4.1.0&q=80&w=1080', 18, 4);
INSERT INTO Trip VALUES ('15 Tokyo Traditional Culture', 'Discover ancient temples, tea ceremonies, traditional crafts, sumo wrestling, and authentic ryokan experiences in historic Tokyo districts', '2025-11-20 00:00:00.000 2025-11-30 00:00:00.000', '1750.00 https://images.unsplash.com/photo-1507693595546-0512d61de389?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NTEzMDExOTN8&ixlib=rb-4.1.0&q=80&w=1080', 12, 5);
INSERT INTO Trip VALUES ('16 Paris Seine River Cruise', 'Experience Paris from the water with a romantic Seine River cruise, including dinner, wine tasting, and views of iconic landmarks like Notre Dame and the Eiffel Tower', '2025-09-11 00:00:00.000 2025-09-27 00:00:00.000', '1680.00 https://images.unsplash.com/photo-1743184345435-142722afa4f5?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NTEzMDExNzJ8&ixlib=rb-4.1.0&q=80&w=1080', 12, 1);

-- User Data
GO
INSERT INTO User VALUES ('Msg 156, Level 15, State 1, Server sokol, Line 1');
INSERT INTO User VALUES ('Incorrect syntax near the keyword ''User''.');

-- TripRegistration Data
GO
INSERT INTO TripRegistration VALUES (1, 2, '1 2025-03-18 13:57:22.107', 2, '2400.00 Confirmed');
INSERT INTO TripRegistration VALUES (2, 2, '3 2025-03-23 13:57:22.107', 1, '1150.00 Pending');
INSERT INTO TripRegistration VALUES (3, 3, '2 2025-03-21 13:57:22.107', 2, '2700.00 Confirmed');
INSERT INTO TripRegistration VALUES (4, 3, '4 2025-03-25 13:57:22.107', 1, '1400.00 Cancelled');
INSERT INTO TripRegistration VALUES (5, 5, '1 2025-06-29 02:56:36.977', 1, '1200.00 Pending');
INSERT INTO TripRegistration VALUES (6, 5, '4 2025-06-30 12:12:03.277', 1, '1400.00 Pending');
INSERT INTO TripRegistration VALUES (8, 5, '5 2025-06-30 14:05:27.573', 1, '1800.00 Pending');

-- TripGuide Data
GO
INSERT INTO TripGuide VALUES (1, 4);
INSERT INTO TripGuide VALUES (1, 5);
INSERT INTO TripGuide VALUES (2, 1);
INSERT INTO TripGuide VALUES (2, 2);
INSERT INTO TripGuide VALUES (3, 2);
INSERT INTO TripGuide VALUES (3, 5);
INSERT INTO TripGuide VALUES (4, 4);
INSERT INTO TripGuide VALUES (5, 3);
INSERT INTO TripGuide VALUES (6, 1);
INSERT INTO TripGuide VALUES (6, 4);
INSERT INTO TripGuide VALUES (7, 1);
INSERT INTO TripGuide VALUES (7, 2);
INSERT INTO TripGuide VALUES (8, 2);
INSERT INTO TripGuide VALUES (8, 5);
INSERT INTO TripGuide VALUES (9, 1);
INSERT INTO TripGuide VALUES (9, 4);
INSERT INTO TripGuide VALUES (10, 3);
INSERT INTO TripGuide VALUES (11, 2);
INSERT INTO TripGuide VALUES (12, 1);
INSERT INTO TripGuide VALUES (12, 5);
INSERT INTO TripGuide VALUES (13, 2);
INSERT INTO TripGuide VALUES (13, 5);
INSERT INTO TripGuide VALUES (14, 1);
INSERT INTO TripGuide VALUES (14, 4);
INSERT INTO TripGuide VALUES (15, 3);
INSERT INTO TripGuide VALUES (16, 1);

