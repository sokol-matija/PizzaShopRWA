-- Fix Azure Database Schema
-- Add missing ImageUrl column to Destination table

USE [TravelOrganizationDB]
GO

-- Add ImageUrl column to Destination table
ALTER TABLE Destination ADD ImageUrl NVARCHAR(500) NULL;
GO

-- Update existing destinations with ImageUrl values
UPDATE Destination SET ImageUrl = 'https://images.unsplash.com/photo-1566977309384-d145e7ab1615?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NDMxNjcwNjJ8&ixlib=rb-4.0.3&q=80&w=1080' WHERE Id = 1;
UPDATE Destination SET ImageUrl = 'https://images.unsplash.com/photo-1529154166925-574a0236a4f4?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NDMxNjcwNjN8&ixlib=rb-4.0.3&q=80&w=1080' WHERE Id = 2;
UPDATE Destination SET ImageUrl = 'https://images.unsplash.com/photo-1736791418468-f822fad5fb7c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NDMxNjcwNjR8&ixlib=rb-4.0.3&q=80&w=1080' WHERE Id = 3;
UPDATE Destination SET ImageUrl = 'https://images.unsplash.com/photo-1500301111609-42f1aa6df72a?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NDMxNjcwNjR8&ixlib=rb-4.0.3&q=80&w=1080' WHERE Id = 4;
UPDATE Destination SET ImageUrl = 'https://images.unsplash.com/photo-1556923590-4a2473e29549?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NDMxNjcwNjV8&ixlib=rb-4.0.3&q=80&w=1080' WHERE Id = 5;
UPDATE Destination SET ImageUrl = 'https://images.unsplash.com/photo-1658008193946-7b594ee5c0f1?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&ixid=M3w3Mjk2ODd8MHwxfHJhbmRvbXx8fHx8fHx8fDE3NTEyOTc0MDN8&ixlib=rb-4.1.0&q=80&w=1080' WHERE Id = 7;
GO

-- Verify the fix
SELECT Id, Name, ImageUrl FROM Destination ORDER BY Id;
GO 