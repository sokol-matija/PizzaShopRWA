-- Assign guides to the 5 new trips

-- Trip 11: Paris Culinary Journey - Maria Garcia (cuisine expert)
INSERT INTO TripGuide (TripId, GuideId) VALUES (11, 2);

-- Trip 12: Rome Countryside Escape - John Smith (European history) + Carlos Rodriguez (adventure)
INSERT INTO TripGuide (TripId, GuideId) VALUES (12, 1);
INSERT INTO TripGuide (TripId, GuideId) VALUES (12, 5);

-- Trip 13: Barcelona Nightlife & Music - Maria Garcia (Mediterranean cultures) + Carlos Rodriguez (adventure)
INSERT INTO TripGuide (TripId, GuideId) VALUES (13, 2);
INSERT INTO TripGuide (TripId, GuideId) VALUES (13, 5);

-- Trip 14: London Literary Heritage - Emma Wilson (art/history) + John Smith (European history)
INSERT INTO TripGuide (TripId, GuideId) VALUES (14, 4);
INSERT INTO TripGuide (TripId, GuideId) VALUES (14, 1);

-- Trip 15: Tokyo Traditional Culture - Takashi Yamamoto (Asian culture specialist)
INSERT INTO TripGuide (TripId, GuideId) VALUES (15, 3);
