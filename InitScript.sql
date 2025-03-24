-- PizzaShopDB Initialization Script
-- This script creates and populates the PizzaShopDB database with sample data

-- Create database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'PizzaShopDB')
BEGIN
    CREATE DATABASE PizzaShopDB;
END
GO

USE PizzaShopDB;
GO

-- Drop tables if they exist (in reverse order of creation to handle foreign key constraints)
IF OBJECT_ID('OrderItem', 'U') IS NOT NULL DROP TABLE OrderItem;
IF OBJECT_ID('[Order]', 'U') IS NOT NULL DROP TABLE [Order];
IF OBJECT_ID('[User]', 'U') IS NOT NULL DROP TABLE [User];
IF OBJECT_ID('FoodAllergen', 'U') IS NOT NULL DROP TABLE FoodAllergen;
IF OBJECT_ID('Allergen', 'U') IS NOT NULL DROP TABLE Allergen;
IF OBJECT_ID('Food', 'U') IS NOT NULL DROP TABLE Food;
IF OBJECT_ID('FoodCategory', 'U') IS NOT NULL DROP TABLE FoodCategory;
IF OBJECT_ID('Log', 'U') IS NOT NULL DROP TABLE Log;
GO

-- =============================================
-- TABLE CREATION
-- =============================================

-- Create FoodCategory table (1-to-N entity)
CREATE TABLE FoodCategory (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL
);

-- Create Food table (primary entity)
CREATE TABLE Food (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL,
    Price DECIMAL(10, 2) NOT NULL,
    ImageUrl NVARCHAR(500) NULL,
    PreparationTime INT NULL, -- in minutes
    FoodCategoryId INT NOT NULL,
    FOREIGN KEY (FoodCategoryId) REFERENCES FoodCategory(Id)
);

-- Create Allergen table (M-to-N entity)
CREATE TABLE Allergen (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL
);

-- Create FoodAllergen table (bridge table)
CREATE TABLE FoodAllergen (
    FoodId INT NOT NULL,
    AllergenId INT NOT NULL,
    PRIMARY KEY (FoodId, AllergenId),
    FOREIGN KEY (FoodId) REFERENCES Food(Id) ON DELETE CASCADE,
    FOREIGN KEY (AllergenId) REFERENCES Allergen(Id) ON DELETE CASCADE
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

-- Create Order table (User's M-to-N entity)
CREATE TABLE [Order] (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    TotalAmount DECIMAL(10, 2) NOT NULL,
    DeliveryAddress NVARCHAR(200) NOT NULL,
    Status NVARCHAR(50) NOT NULL, -- Pending, Accepted, Delivered, Cancelled
    FOREIGN KEY (UserId) REFERENCES [User](Id)
);

-- Create OrderItem table (order details)
CREATE TABLE OrderItem (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    FoodId INT NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(10, 2) NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES [Order](Id) ON DELETE CASCADE,
    FOREIGN KEY (FoodId) REFERENCES Food(Id)
);

-- Create Log table for API logging
CREATE TABLE Log (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Timestamp DATETIME NOT NULL DEFAULT GETDATE(),
    Level NVARCHAR(50) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL
);

-- =============================================
-- SEED DATA INSERTION
-- =============================================

-- Insert sample data for FoodCategory
INSERT INTO FoodCategory (Name, Description) VALUES 
('Pizza', 'Traditional and specialty pizzas'),
('Burger', 'Gourmet burgers with various toppings'),
('Pasta', 'Italian pasta dishes'),
('Salad', 'Fresh and healthy salads'),
('Dessert', 'Sweet treats to finish your meal');

-- Insert sample data for Allergen
INSERT INTO Allergen (Name, Description) VALUES 
('Gluten', 'Found in wheat and other grains'),
('Dairy', 'Includes milk, cheese, yogurt, and other milk products'),
('Nuts', 'Tree nuts and peanuts'),
('Eggs', 'Chicken eggs and egg products'),
('Soy', 'Soybeans and soy products'),
('Shellfish', 'Includes shrimp, crab, and lobster');

-- Insert sample data for Food
INSERT INTO Food (Name, Description, Price, ImageUrl, PreparationTime, FoodCategoryId) VALUES 
('Margherita Pizza', 'Classic pizza with tomato sauce, mozzarella, and basil', 10.99, '/images/food/margherita.png', 20, 1),
('Pepperoni Pizza', 'Pizza with tomato sauce, mozzarella, and pepperoni', 12.99, '/images/food/pepperoni.png', 20, 1),
('Cheeseburger', 'Beef patty with cheese, lettuce, tomato, and special sauce', 9.99, '/images/food/cheeseburger.png', 15, 2),
('Vegetarian Burger', 'Plant-based patty with lettuce, tomato, and vegan sauce', 10.99, '/images/food/vegburger.png', 15, 2),
('Spaghetti Bolognese', 'Spaghetti with beef and tomato sauce', 11.99, '/images/food/spaghetti.png', 25, 3),
('Caesar Salad', 'Romaine lettuce with Caesar dressing, croutons, and parmesan', 8.99, '/images/food/caesar.png', 10, 4),
('Chocolate Cake', 'Rich chocolate cake with chocolate ganache', 5.99, '/images/food/choccake.png', 5, 5);

-- Insert sample data for FoodAllergen
INSERT INTO FoodAllergen (FoodId, AllergenId) VALUES 
(1, 1), -- Margherita Pizza contains Gluten
(1, 2), -- Margherita Pizza contains Dairy
(2, 1), -- Pepperoni Pizza contains Gluten
(2, 2), -- Pepperoni Pizza contains Dairy
(3, 1), -- Cheeseburger contains Gluten
(3, 2), -- Cheeseburger contains Dairy
(3, 4), -- Cheeseburger contains Eggs
(4, 1), -- Vegetarian Burger contains Gluten
(4, 5), -- Vegetarian Burger contains Soy
(5, 1), -- Spaghetti Bolognese contains Gluten
(5, 4), -- Spaghetti Bolognese contains Eggs
(6, 2), -- Caesar Salad contains Dairy
(6, 4), -- Caesar Salad contains Eggs
(7, 1), -- Chocolate Cake contains Gluten
(7, 2), -- Chocolate Cake contains Dairy
(7, 4); -- Chocolate Cake contains Eggs

-- Insert sample users
INSERT INTO [User] (Username, Email, PasswordHash, FirstName, LastName, PhoneNumber, Address, IsAdmin) VALUES 
('admin', 'admin@pizzeria.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'Admin', 'User', '123-456-7890', '123 Admin St', 1),
('john.doe', 'john.doe@example.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'John', 'Doe', '123-456-7890', '456 Main St', 0);

-- Insert sample order
INSERT INTO [Order] (UserId, OrderDate, TotalAmount, DeliveryAddress, Status) VALUES 
(2, GETDATE(), 23.98, '456 Main St', 'Delivered');

-- Insert sample order items
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
(1, 1, 1, 10.99), -- One Margherita Pizza
(1, 6, 1, 8.99),  -- One Caesar Salad
(1, 7, 1, 5.99);  -- One Chocolate Cake

-- =============================================
-- ADDITIONAL USERS 
-- =============================================

-- Insert 20 more sample users
INSERT INTO [User] (Username, Email, PasswordHash, FirstName, LastName, PhoneNumber, Address, IsAdmin) VALUES 
('emma.smith', 'emma.smith@example.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'Emma', 'Smith', '555-111-2222', '123 Elm St', 0),
('noah.johnson', 'noah.johnson@example.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'Noah', 'Johnson', '555-222-3333', '456 Oak St', 0),
('olivia.williams', 'olivia.williams@example.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'Olivia', 'Williams', '555-333-4444', '789 Pine St', 0),
('liam.brown', 'liam.brown@example.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'Liam', 'Brown', '555-444-5555', '101 Maple Ave', 0),
('ava.jones', 'ava.jones@example.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'Ava', 'Jones', '555-555-6666', '202 Cedar Ln', 0),
('william.garcia', 'william.garcia@example.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'William', 'Garcia', '555-666-7777', '303 Birch Rd', 0),
('sophia.miller', 'sophia.miller@example.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'Sophia', 'Miller', '555-777-8888', '404 Spruce Dr', 0),
('james.davis', 'james.davis@example.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'James', 'Davis', '555-888-9999', '505 Fir Blvd', 0),
('isabella.rodriguez', 'isabella.rodriguez@example.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'Isabella', 'Rodriguez', '555-999-0000', '606 Redwood St', 0),
('benjamin.martinez', 'benjamin.martinez@example.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'Benjamin', 'Martinez', '555-111-0000', '707 Sequoia Ter', 0),
('mia.hernandez', 'mia.hernandez@example.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'Mia', 'Hernandez', '555-222-0000', '808 Aspen Way', 0),
('ethan.lopez', 'ethan.lopez@example.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'Ethan', 'Lopez', '555-333-0000', '909 Willow Pl', 0),
('charlotte.gonzalez', 'charlotte.gonzalez@example.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'Charlotte', 'Gonzalez', '555-444-0000', '110 Juniper Ct', 0),
('alexander.wilson', 'alexander.wilson@example.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'Alexander', 'Wilson', '555-555-0000', '211 Locust Ave', 0),
('amelia.anderson', 'amelia.anderson@example.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'Amelia', 'Anderson', '555-666-0000', '312 Poplar St', 0),
('henry.thomas', 'henry.thomas@example.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'Henry', 'Thomas', '555-777-0000', '413 Sycamore Rd', 0),
('lily.taylor', 'lily.taylor@example.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'Lily', 'Taylor', '555-888-0000', '514 Chestnut Dr', 0),
('michael.moore', 'michael.moore@example.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'Michael', 'Moore', '555-999-1111', '615 Walnut Blvd', 0),
('evelyn.jackson', 'evelyn.jackson@example.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'Evelyn', 'Jackson', '555-000-1111', '716 Beech St', 0),
('daniel.martin', 'daniel.martin@example.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'Daniel', 'Martin', '555-000-2222', '817 Alder Ln', 0);

-- =============================================
-- OPTIONAL DATA GENERATION SECTION
-- =============================================

-- This section contains code to generate additional sample data.
-- It is commented out by default since you might not want to 
-- generate a large amount of sample data in every environment.

/*
-- =============================================
-- CREATE SAMPLE ORDERS GENERATION PROCEDURE
-- =============================================

-- Create stored procedure for generating orders
IF OBJECT_ID('GenerateOrders', 'P') IS NOT NULL
    DROP PROCEDURE GenerateOrders;
GO

CREATE PROCEDURE GenerateOrders
AS
BEGIN
    -- Helper variables for order creation
    DECLARE @OrderIndex INT = 1;
    DECLARE @CurrentUserId INT;
    DECLARE @OrderDate DATETIME;
    DECLARE @OrderStatus NVARCHAR(50);
    DECLARE @TotalAmount DECIMAL(10, 2);
    DECLARE @DeliveryAddress NVARCHAR(200);
    DECLARE @OrderId INT;
    DECLARE @FirstNewUserId INT = (SELECT MIN(Id) FROM [User] WHERE Username = 'emma.smith');
    
    -- Create orders with various dates and statuses
    WHILE @OrderIndex <= 70
    BEGIN
        -- Determine which user gets this order (cycle through the 20 new users)
        SET @CurrentUserId = @FirstNewUserId + ((@OrderIndex - 1) % 20);
        
        -- Determine order date (scatter across 2024 and 2025 up to now)
        SET @OrderDate = DATEADD(day, -CAST((@OrderIndex * 7) % 450 AS INT), GETDATE());
        
        -- Determine order status (mix of different statuses)
        IF @OrderIndex % 5 = 0 
            SET @OrderStatus = 'Pending';
        ELSE IF @OrderIndex % 5 = 1 
            SET @OrderStatus = 'Accepted';
        ELSE IF @OrderIndex % 5 = 2 
            SET @OrderStatus = 'In Progress';
        ELSE IF @OrderIndex % 5 = 3 
            SET @OrderStatus = 'Delivered';
        ELSE
            SET @OrderStatus = 'Cancelled';
        
        -- For delivered orders older than 15 days, make sure they're delivered or cancelled
        IF DATEDIFF(day, @OrderDate, GETDATE()) > 15
           AND @OrderStatus IN ('Pending', 'Accepted', 'In Progress')
        BEGIN
            SET @OrderStatus = 'Delivered';
        END;
        
        -- Randomize total amount (will be correctly calculated based on items)
        IF @OrderIndex % 4 = 0
            SET @TotalAmount = 22.98;  -- Small order
        ELSE IF @OrderIndex % 4 = 1
            SET @TotalAmount = 35.97;  -- Medium order
        ELSE IF @OrderIndex % 4 = 2
            SET @TotalAmount = 45.96;  -- Large order
        ELSE
            SET @TotalAmount = 29.97;  -- Medium order
        
        -- Get delivery address from user
        SET @DeliveryAddress = (SELECT Address FROM [User] WHERE Id = @CurrentUserId);
        
        -- Insert the order
        INSERT INTO [Order] (UserId, OrderDate, TotalAmount, DeliveryAddress, Status)
        VALUES (@CurrentUserId, @OrderDate, @TotalAmount, @DeliveryAddress, @OrderStatus);
        
        -- Get the ID of the inserted order
        SET @OrderId = SCOPE_IDENTITY();
        
        -- Insert order items based on the pattern of the order index
        IF @OrderIndex % 4 = 0 -- Small order
        BEGIN
            INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
            (@OrderId, 1, 1, 10.99), -- One Margherita Pizza
            (@OrderId, 6, 1, 8.99),  -- One Caesar Salad
            (@OrderId, 7, 1, 5.99);  -- One Chocolate Cake
        END
        ELSE IF @OrderIndex % 4 = 1 -- Medium order with pizza focus
        BEGIN
            INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
            (@OrderId, 1, 2, 10.99), -- Two Margherita Pizzas
            (@OrderId, 2, 1, 12.99), -- One Pepperoni Pizza
            (@OrderId, 7, 1, 5.99);  -- One Chocolate Cake
        END
        ELSE IF @OrderIndex % 4 = 2 -- Large order
        BEGIN
            INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
            (@OrderId, 2, 2, 12.99), -- Two Pepperoni Pizzas
            (@OrderId, 3, 1, 9.99),  -- One Cheeseburger
            (@OrderId, 5, 1, 11.99), -- One Spaghetti Bolognese
            (@OrderId, 6, 1, 8.99);  -- One Caesar Salad
        END
        ELSE -- Medium order with mixed items
        BEGIN
            INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
            (@OrderId, 3, 1, 9.99),  -- One Cheeseburger
            (@OrderId, 4, 1, 10.99), -- One Vegetarian Burger
            (@OrderId, 6, 1, 8.99);  -- One Caesar Salad
        END
        
        -- Move to next order
        SET @OrderIndex = @OrderIndex + 1;
    END;
    
    -- Create additional orders for special dates (holidays, etc.)
    -- Today's orders
    INSERT INTO [Order] (UserId, OrderDate, TotalAmount, DeliveryAddress, Status)
    VALUES 
    (@FirstNewUserId, DATEADD(hour, -1, GETDATE()), 23.98, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId), 'Pending'),
    (@FirstNewUserId + 1, DATEADD(hour, -2, GETDATE()), 35.97, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 1), 'Accepted'),
    (@FirstNewUserId + 2, DATEADD(hour, -3, GETDATE()), 28.97, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 2), 'In Progress');

    -- Add order items for today's orders
    -- Order 1 (Pending)
    SET @OrderId = SCOPE_IDENTITY();
    INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
    (@OrderId, 1, 1, 10.99), -- One Margherita Pizza
    (@OrderId, 6, 1, 8.99),  -- One Caesar Salad
    (@OrderId, 7, 1, 5.99);  -- One Chocolate Cake

    -- Order 2 (Accepted)
    SET @OrderId = @OrderId - 1;
    INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
    (@OrderId, 2, 2, 12.99), -- Two Pepperoni Pizzas
    (@OrderId, 7, 1, 5.99),  -- One Chocolate Cake
    (@OrderId, 6, 1, 8.99);  -- One Caesar Salad

    -- Order 3 (In Progress)
    SET @OrderId = @OrderId - 1;
    INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
    (@OrderId, 5, 1, 11.99), -- One Spaghetti Bolognese
    (@OrderId, 3, 1, 9.99),  -- One Cheeseburger
    (@OrderId, 7, 1, 5.99);  -- One Chocolate Cake
    
    PRINT 'Generated 70+ sample orders with order items';
END;
GO

-- Run the procedure to generate sample orders
-- Uncomment the line below to execute:
-- EXEC GenerateOrders;

-- Clean up (optional)
-- Uncomment to drop the procedure:
-- DROP PROCEDURE IF EXISTS GenerateOrders;
*/

-- =============================================
-- VERIFICATION QUERIES
-- =============================================

-- Verify data was properly inserted
SELECT 'Food Categories:' AS TableName, COUNT(*) AS RowCount FROM FoodCategory;
SELECT 'Foods:' AS TableName, COUNT(*) AS RowCount FROM Food;
SELECT 'Allergens:' AS TableName, COUNT(*) AS RowCount FROM Allergen;
SELECT 'Users:' AS TableName, COUNT(*) AS RowCount FROM [User];
SELECT 'Orders:' AS TableName, COUNT(*) AS RowCount FROM [Order];
SELECT 'Order Items:' AS TableName, COUNT(*) AS RowCount FROM OrderItem;

-- Completed database setup
PRINT 'PizzaShopDB initialized successfully.';
GO
</rewritten_file>