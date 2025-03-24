--create database PizzaShopDB
go



use PizzaShopDB
go

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
('Margherita Pizza', 'Classic pizza with tomato sauce, mozzarella, and basil', 10.99, 'margherita.jpg', 20, 1),
('Pepperoni Pizza', 'Pizza with tomato sauce, mozzarella, and pepperoni', 12.99, 'pepperoni.jpg', 20, 1),
('Cheeseburger', 'Beef patty with cheese, lettuce, tomato, and special sauce', 9.99, 'cheeseburger.jpg', 15, 2),
('Vegetarian Burger', 'Plant-based patty with lettuce, tomato, and vegan sauce', 10.99, 'vegburger.jpg', 15, 2),
('Spaghetti Bolognese', 'Spaghetti with beef and tomato sauce', 11.99, 'spaghetti.jpg', 25, 3),
('Caesar Salad', 'Romaine lettuce with Caesar dressing, croutons, and parmesan', 8.99, 'caesar.jpg', 10, 4),
('Chocolate Cake', 'Rich chocolate cake with chocolate ganache', 5.99, 'choccake.jpg', 5, 5);

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

-- Insert sample user (admin)
INSERT INTO [User] (Username, Email, PasswordHash, FirstName, LastName, PhoneNumber, Address, IsAdmin) VALUES 
('admin', 'admin@pizzeria.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'Admin', 'User', '123-456-7890', '123 Admin St', 1);

-- Insert sample user (regular user)
INSERT INTO [User] (Username, Email, PasswordHash, FirstName, LastName, PhoneNumber, Address, IsAdmin) VALUES 
('john.doe', 'john.doe@example.com', 'AQAAAAEAACcQAAAAEL8fRMH8G2l7ORzB8DeGP8MkHe8MuKYZVK2f5vhO6QSdikSPAKSN0CWnEL4Dwn05HA==', 'John', 'Doe', '123-456-7890', '456 Main St', 0);

-- Insert sample order
INSERT INTO [Order] (UserId, OrderDate, TotalAmount, DeliveryAddress, Status) VALUES 
(2, GETDATE(), 23.98, '456 Main St', 'Delivered');

-- Insert sample order items
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
(1, 1, 1, 10.99), -- One Margherita Pizza
(1, 6, 1, 8.99),  -- One Caesar Salad
(1, 7, 1, 5.99);  -- One Chocolate Cake




select * from dbo.Food
select * from [User]

UPDATE [User]
SET IsAdmin = 1
WHERE Username = 'newadmin';


--{
--  "username": "newadmin",
--  "email": "newadmin@example.com",
--  "password": "Admin123!",
--  "confirmPassword": "Admin123!",
--  "firstName": "New",
--  "lastName": "Admin"
--}

select * from food
select * from dbo.Allergen
select * from dbo.[Order]

-- Insert 15 sample orders with different statuses and dates
INSERT INTO [Order] (UserId, OrderDate, TotalAmount, DeliveryAddress, Status) VALUES 
-- Orders for user 2 (john.doe)
(2, DATEADD(day, -5, GETDATE()), 35.97, '456 Main St', 'Delivered'),
(2, DATEADD(day, -4, GETDATE()), 22.98, '456 Main St', 'Delivered'),
(2, DATEADD(day, -3, GETDATE()), 18.98, '456 Main St', 'In Progress'),
(2, DATEADD(day, -2, GETDATE()), 45.96, '456 Main St', 'Accepted'),
(2, DATEADD(day, -1, GETDATE()), 15.98, '456 Main St', 'Pending'),
(2, GETDATE(), 28.97, '456 Main St', 'Pending'),
(2, DATEADD(day, 1, GETDATE()), 33.97, '456 Main St', 'Pending'),
(2, DATEADD(day, 2, GETDATE()), 19.98, '456 Main St', 'Pending'),
(2, DATEADD(day, 3, GETDATE()), 42.96, '456 Main St', 'Pending'),
(2, DATEADD(day, 4, GETDATE()), 25.98, '456 Main St', 'Pending'),

-- Orders for user 1 (admin)
(1, DATEADD(day, -5, GETDATE()), 38.97, '123 Admin St', 'Delivered'),
(1, DATEADD(day, -4, GETDATE()), 29.98, '123 Admin St', 'Delivered'),
(1, DATEADD(day, -3, GETDATE()), 21.98, '123 Admin St', 'Cancelled'),
(1, DATEADD(day, -2, GETDATE()), 47.96, '123 Admin St', 'Accepted'),
(1, DATEADD(day, -1, GETDATE()), 17.98, '123 Admin St', 'In Progress');

-- Insert order items for all orders
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
-- Order 2 (john.doe)
(2, 1, 2, 10.99),  -- Two Margherita Pizzas
(2, 7, 1, 5.99),   -- One Chocolate Cake
(2, 6, 1, 8.99),   -- One Caesar Salad

-- Order 3 (john.doe)
(3, 3, 1, 9.99),   -- One Cheeseburger
(3, 6, 1, 8.99),   -- One Caesar Salad

-- Order 4 (john.doe)
(4, 2, 2, 12.99),  -- Two Pepperoni Pizzas
(4, 7, 2, 5.99),   -- Two Chocolate Cakes
(4, 6, 1, 8.99),   -- One Caesar Salad

-- Order 5 (john.doe)
(5, 4, 1, 10.99),  -- One Vegetarian Burger
(5, 6, 1, 8.99),   -- One Caesar Salad

-- Order 6 (john.doe)
(6, 5, 1, 11.99),  -- One Spaghetti Bolognese
(6, 7, 1, 5.99),   -- One Chocolate Cake
(6, 6, 1, 8.99),   -- One Caesar Salad

-- Order 7 (john.doe)
(7, 1, 2, 10.99),  -- Two Margherita Pizzas
(7, 3, 1, 9.99),   -- One Cheeseburger

-- Order 8 (john.doe)
(8, 4, 1, 10.99),  -- One Vegetarian Burger
(8, 6, 1, 8.99),   -- One Caesar Salad

-- Order 9 (john.doe)
(9, 2, 2, 12.99),  -- Two Pepperoni Pizzas
(9, 5, 1, 11.99),  -- One Spaghetti Bolognese

-- Order 10 (john.doe)
(10, 1, 1, 10.99), -- One Margherita Pizza
(10, 3, 1, 9.99),  -- One Cheeseburger
(10, 7, 1, 5.99),  -- One Chocolate Cake

-- Order 11 (admin)
(11, 2, 2, 12.99), -- Two Pepperoni Pizzas
(11, 7, 1, 5.99),  -- One Chocolate Cake
(11, 6, 1, 8.99),  -- One Caesar Salad

-- Order 12 (admin)
(12, 5, 1, 11.99), -- One Spaghetti Bolognese
(12, 3, 1, 9.99),  -- One Cheeseburger
(12, 7, 1, 5.99),  -- One Chocolate Cake

-- Order 13 (admin)
(13, 1, 1, 10.99), -- One Margherita Pizza
(13, 6, 1, 8.99),  -- One Caesar Salad

-- Order 14 (admin)
(14, 2, 2, 12.99), -- Two Pepperoni Pizzas
(14, 4, 1, 10.99), -- One Vegetarian Burger
(14, 7, 1, 5.99),  -- One Chocolate Cake

-- Order 15 (admin)
(15, 3, 1, 9.99),  -- One Cheeseburger
(15, 6, 1, 8.99);  -- One Caesar Salad

-- Insert more sample orders with different statuses and dates
INSERT INTO [Order] (UserId, OrderDate, TotalAmount, DeliveryAddress, Status) VALUES 
-- Orders for user 2 (john.doe) - Today's orders
(2, DATEADD(hour, -2, GETDATE()), 35.97, '456 Main St', 'Delivered'),
(2, DATEADD(hour, -1, GETDATE()), 22.98, '456 Main St', 'In Progress'),
(2, GETDATE(), 18.98, '456 Main St', 'Pending'),

-- Orders for user 1 (admin) - Today's orders
(1, DATEADD(hour, -3, GETDATE()), 38.97, '123 Admin St', 'Delivered'),
(1, DATEADD(hour, -1, GETDATE()), 29.98, '123 Admin St', 'In Progress'),
(1, GETDATE(), 21.98, '123 Admin St', 'Pending'),

-- Orders for user 2 (john.doe) - Yesterday's orders
(2, DATEADD(day, -1, DATEADD(hour, -2, GETDATE())), 35.97, '456 Main St', 'Delivered'),
(2, DATEADD(day, -1, DATEADD(hour, -1, GETDATE())), 22.98, '456 Main St', 'Delivered'),
(2, DATEADD(day, -1, GETDATE()), 18.98, '456 Main St', 'Delivered'),

-- Orders for user 1 (admin) - Yesterday's orders
(1, DATEADD(day, -1, DATEADD(hour, -3, GETDATE())), 38.97, '123 Admin St', 'Delivered'),
(1, DATEADD(day, -1, DATEADD(hour, -1, GETDATE())), 29.98, '123 Admin St', 'Delivered'),
(1, DATEADD(day, -1, GETDATE()), 21.98, '123 Admin St', 'Delivered');

-- Insert order items for the new orders
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
-- Today's orders for user 2
(16, 1, 2, 10.99),  -- Two Margherita Pizzas
(16, 7, 1, 5.99),   -- One Chocolate Cake
(16, 6, 1, 8.99),   -- One Caesar Salad

(17, 3, 1, 9.99),   -- One Cheeseburger
(17, 6, 1, 8.99),   -- One Caesar Salad

(18, 4, 1, 10.99),  -- One Vegetarian Burger
(18, 6, 1, 8.99),   -- One Caesar Salad

-- Today's orders for user 1
(19, 2, 2, 12.99),  -- Two Pepperoni Pizzas
(19, 7, 1, 5.99),   -- One Chocolate Cake
(19, 6, 1, 8.99),   -- One Caesar Salad

(20, 5, 1, 11.99),  -- One Spaghetti Bolognese
(20, 3, 1, 9.99),   -- One Cheeseburger
(20, 7, 1, 5.99),   -- One Chocolate Cake

(21, 1, 1, 10.99),  -- One Margherita Pizza
(21, 6, 1, 8.99),   -- One Caesar Salad

-- Yesterday's orders for user 2
(22, 2, 2, 12.99),  -- Two Pepperoni Pizzas
(22, 7, 1, 5.99),   -- One Chocolate Cake
(22, 6, 1, 8.99),   -- One Caesar Salad

(23, 5, 1, 11.99),  -- One Spaghetti Bolognese
(23, 3, 1, 9.99),   -- One Cheeseburger
(23, 7, 1, 5.99),   -- One Chocolate Cake

(24, 1, 1, 10.99),  -- One Margherita Pizza
(24, 6, 1, 8.99),   -- One Caesar Salad

-- Yesterday's orders for user 1
(25, 3, 2, 9.99),   -- Two Cheeseburgers
(25, 6, 1, 8.99),   -- One Caesar Salad
(25, 7, 1, 5.99),   -- One Chocolate Cake

(26, 4, 1, 10.99),  -- One Vegetarian Burger
(26, 6, 1, 8.99),   -- One Caesar Salad
(26, 7, 1, 5.99),   -- One Chocolate Cake

(27, 2, 1, 12.99),  -- One Pepperoni Pizza
(27, 6, 1, 8.99);   -- One Caesar Salad

-- Insert new orders with different statuses and dates
INSERT INTO [Order] (UserId, OrderDate, TotalAmount, DeliveryAddress, Status) VALUES 
-- Today's orders
(6, GETDATE(), 34.97, 'Matija Home Address', 'Pending'),
(6, DATEADD(hour, -2, GETDATE()), 23.98, 'Matija Work Address', 'In Progress'),
-- Yesterday's orders
(6, DATEADD(day, -1, GETDATE()), 40.96, 'Matija Home Address', 'Delivered'),
-- Orders from earlier this week
(6, DATEADD(day, -3, GETDATE()), 19.98, 'Matija Home Address', 'Delivered'),
(6, DATEADD(day, -5, GETDATE()), 29.97, 'Matija Work Address', 'Cancelled');

-- Get the IDs of the newly inserted orders
DECLARE @Order1 INT = SCOPE_IDENTITY();
DECLARE @Order2 INT = @Order1 - 1;
DECLARE @Order3 INT = @Order1 - 2;
DECLARE @Order4 INT = @Order1 - 3;
DECLARE @Order5 INT = @Order1 - 4;

-- Insert order items for the new orders
-- Today's pending order
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
(@Order1, 1, 2, 10.99),  -- Two Margherita Pizzas
(@Order1, 7, 1, 5.99),   -- One Chocolate Cake
(@Order1, 6, 1, 8.99);   -- One Caesar Salad

-- Today's in-progress order
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
(@Order2, 3, 1, 9.99),   -- One Cheeseburger
(@Order2, 5, 1, 11.99),  -- One Spaghetti Bolognese
(@Order2, 7, 1, 5.99);   -- One Chocolate Cake

-- Yesterday's delivered order
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
(@Order3, 2, 2, 12.99),  -- Two Pepperoni Pizzas
(@Order3, 4, 1, 10.99),  -- One Vegetarian Burger
(@Order3, 6, 1, 8.99);   -- One Caesar Salad

-- 3 days ago delivered order
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
(@Order4, 4, 1, 10.99),  -- One Vegetarian Burger
(@Order4, 6, 1, 8.99);   -- One Caesar Salad

-- 5 days ago cancelled order
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
(@Order5, 1, 1, 10.99),  -- One Margherita Pizza
(@Order5, 3, 1, 9.99),   -- One Cheeseburger
(@Order5, 6, 1, 8.99);   -- One Caesar Salad

-- Verify the orders were created
SELECT * FROM [Order] WHERE UserId = 6 ORDER BY OrderDate DESC;

-- Verify the order items were created
SELECT o.Id AS OrderId, o.Status, o.OrderDate, f.Name AS FoodName, oi.Quantity, oi.Price
FROM [Order] o
JOIN OrderItem oi ON o.Id = oi.OrderId
JOIN Food f ON oi.FoodId = f.Id
WHERE o.UserId = 6
ORDER BY o.OrderDate DESC, o.Id; 

-- Insert 20 new users
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

-- Get the ID of the first inserted user to use as a reference for new orders
DECLARE @FirstNewUserId INT = (SELECT MIN(Id) FROM [User] WHERE Username = 'emma.smith');

-- Insert approximately 70 new orders across multiple users with different dates and statuses
-- We'll distribute these across the 20 new users (about 3-4 orders per user)

-- Helper variables for order creation
DECLARE @OrderIndex INT = 1;
DECLARE @CurrentUserId INT;
DECLARE @OrderDate DATETIME;
DECLARE @OrderStatus NVARCHAR(50);
DECLARE @TotalAmount DECIMAL(10, 2);
DECLARE @DeliveryAddress NVARCHAR(200);
DECLARE @OrderId INT;

-- Create orders with various dates and statuses
WHILE @OrderIndex <= 70
BEGIN
    -- Determine which user gets this order (cycle through the 20 new users)
    SET @CurrentUserId = @FirstNewUserId + ((@OrderIndex - 1) % 20);
    
    -- Determine order date (scatter across 2024 and 2025 up to now)
    SET @OrderDate = DATEADD(day, -CAST((@OrderIndex * 7) % 450 AS INT), GETDATE()); -- Spread over ~450 days (~ Jan 2024 to now)
    
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

-- Create additional orders for today and yesterday to ensure recent activity
-- These will be distributed among the first 10 new users

-- Today's orders
INSERT INTO [Order] (UserId, OrderDate, TotalAmount, DeliveryAddress, Status)
VALUES 
(@FirstNewUserId, DATEADD(hour, -1, GETDATE()), 23.98, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId), 'Pending'),
(@FirstNewUserId + 1, DATEADD(hour, -2, GETDATE()), 35.97, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 1), 'Accepted'),
(@FirstNewUserId + 2, DATEADD(hour, -3, GETDATE()), 28.97, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 2), 'In Progress'),
(@FirstNewUserId + 3, DATEADD(hour, -4, GETDATE()), 42.96, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 3), 'In Progress'),
(@FirstNewUserId + 4, DATEADD(hour, -5, GETDATE()), 16.98, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 4), 'Delivered');

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

-- Order 4 (In Progress)
SET @OrderId = @OrderId - 1;
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
(@OrderId, 2, 2, 12.99), -- Two Pepperoni Pizzas
(@OrderId, 4, 1, 10.99), -- One Vegetarian Burger
(@OrderId, 7, 1, 5.99);  -- One Chocolate Cake

-- Order 5 (Delivered)
SET @OrderId = @OrderId - 1;
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
(@OrderId, 4, 1, 10.99), -- One Vegetarian Burger
(@OrderId, 6, 1, 8.99);  -- One Caesar Salad

-- Add some recent orders (last few days)
INSERT INTO [Order] (UserId, OrderDate, TotalAmount, DeliveryAddress, Status)
VALUES 
(@FirstNewUserId + 5, DATEADD(day, -1, GETDATE()), 31.97, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 5), 'Delivered'),
(@FirstNewUserId + 6, DATEADD(day, -1, GETDATE()), 23.98, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 6), 'Delivered'),
(@FirstNewUserId + 7, DATEADD(day, -2, GETDATE()), 33.97, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 7), 'Delivered'),
(@FirstNewUserId + 8, DATEADD(day, -2, GETDATE()), 19.98, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 8), 'Cancelled'),
(@FirstNewUserId + 9, DATEADD(day, -3, GETDATE()), 25.98, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 9), 'Delivered');

-- Add some seasonal orders (holidays, special dates)
INSERT INTO [Order] (UserId, OrderDate, TotalAmount, DeliveryAddress, Status)
VALUES 
-- Valentine's Day 2024
(@FirstNewUserId + 10, '2024-02-14 18:30:00', 45.96, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 10), 'Delivered'),
(@FirstNewUserId + 11, '2024-02-14 19:15:00', 39.97, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 11), 'Delivered'),

-- Christmas 2024
(@FirstNewUserId + 12, '2024-12-24 17:00:00', 52.95, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 12), 'Delivered'),
(@FirstNewUserId + 13, '2024-12-24 18:30:00', 48.96, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 13), 'Delivered'),
(@FirstNewUserId + 14, '2024-12-25 12:00:00', 56.94, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 14), 'Delivered'),

-- New Year's Eve/Day 2024/2025
(@FirstNewUserId + 15, '2024-12-31 20:00:00', 62.93, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 15), 'Delivered'),
(@FirstNewUserId + 16, '2025-01-01 13:30:00', 37.97, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 16), 'Delivered'),

-- Super Bowl February 2025 (approximate date)
(@FirstNewUserId + 17, '2025-02-09 17:00:00', 73.92, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 17), 'Delivered'),
(@FirstNewUserId + 18, '2025-02-09 16:45:00', 68.94, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 18), 'Delivered'),
(@FirstNewUserId + 19, '2025-02-09 17:15:00', 65.95, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 19), 'Delivered');

-- Add order items for recent orders (last few days)
-- Order 1 (Delivered - yesterday)
SET @OrderId = SCOPE_IDENTITY();
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
(@OrderId, 1, 1, 10.99), -- One Margherita Pizza
(@OrderId, 3, 1, 9.99),  -- One Cheeseburger
(@OrderId, 7, 1, 5.99);  -- One Chocolate Cake

-- Order 2 (Delivered - yesterday)
SET @OrderId = @OrderId - 1;
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
(@OrderId, 4, 1, 10.99), -- One Vegetarian Burger
(@OrderId, 6, 1, 8.99),  -- One Caesar Salad
(@OrderId, 7, 1, 5.99);  -- One Chocolate Cake

-- Order 3 (Delivered - 2 days ago)
SET @OrderId = @OrderId - 1;
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
(@OrderId, 2, 1, 12.99), -- One Pepperoni Pizza
(@OrderId, 3, 1, 9.99),  -- One Cheeseburger
(@OrderId, 7, 2, 5.99);  -- Two Chocolate Cakes

-- Order 4 (Cancelled - 2 days ago)
SET @OrderId = @OrderId - 1;
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
(@OrderId, 4, 1, 10.99), -- One Vegetarian Burger
(@OrderId, 6, 1, 8.99);  -- One Caesar Salad

-- Order 5 (Delivered - 3 days ago)
SET @OrderId = @OrderId - 1;
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
(@OrderId, 3, 1, 9.99),  -- One Cheeseburger
(@OrderId, 5, 1, 11.99), -- One Spaghetti Bolognese
(@OrderId, 7, 1, 5.99);  -- One Chocolate Cake

-- Add items for Valentine's Day orders
-- Valentine's Order 1
SET @OrderId = @OrderId - 1;
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
(@OrderId, 1, 2, 10.99), -- Two Margherita Pizzas
(@OrderId, 5, 1, 11.99), -- One Spaghetti Bolognese
(@OrderId, 7, 2, 5.99);  -- Two Chocolate Cakes

-- Valentine's Order 2
SET @OrderId = @OrderId - 1;
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
(@OrderId, 2, 1, 12.99), -- One Pepperoni Pizza
(@OrderId, 5, 1, 11.99), -- One Spaghetti Bolognese
(@OrderId, 6, 1, 8.99),  -- One Caesar Salad
(@OrderId, 7, 1, 5.99);  -- One Chocolate Cake

-- Christmas Eve Order 1
SET @OrderId = @OrderId - 1;
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
(@OrderId, 1, 2, 10.99), -- Two Margherita Pizzas
(@OrderId, 2, 1, 12.99), -- One Pepperoni Pizza
(@OrderId, 6, 1, 8.99),  -- One Caesar Salad
(@OrderId, 7, 2, 5.99);  -- Two Chocolate Cakes

-- Christmas Eve Order 2
SET @OrderId = @OrderId - 1;
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
(@OrderId, 2, 2, 12.99), -- Two Pepperoni Pizzas
(@OrderId, 3, 1, 9.99),  -- One Cheeseburger
(@OrderId, 6, 1, 8.99),  -- One Caesar Salad
(@OrderId, 7, 1, 5.99);  -- One Chocolate Cake

-- Christmas Day Order
SET @OrderId = @OrderId - 1;
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
(@OrderId, 1, 2, 10.99), -- Two Margherita Pizzas
(@OrderId, 2, 1, 12.99), -- One Pepperoni Pizza
(@OrderId, 3, 1, 9.99),  -- One Cheeseburger
(@OrderId, 6, 1, 8.99),  -- One Caesar Salad
(@OrderId, 7, 2, 5.99);  -- Two Chocolate Cakes

-- New Year's Eve Order
SET @OrderId = @OrderId - 1;
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
(@OrderId, 1, 2, 10.99), -- Two Margherita Pizzas
(@OrderId, 2, 2, 12.99), -- Two Pepperoni Pizzas
(@OrderId, 6, 1, 8.99),  -- One Caesar Salad
(@OrderId, 7, 3, 5.99);  -- Three Chocolate Cakes

-- New Year's Day Order
SET @OrderId = @OrderId - 1;
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
(@OrderId, 1, 1, 10.99), -- One Margherita Pizza
(@OrderId, 3, 1, 9.99),  -- One Cheeseburger
(@OrderId, 4, 1, 10.99), -- One Vegetarian Burger
(@OrderId, 7, 1, 5.99);  -- One Chocolate Cake

-- Super Bowl Order 1
SET @OrderId = @OrderId - 1;
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
(@OrderId, 1, 2, 10.99), -- Two Margherita Pizzas
(@OrderId, 2, 3, 12.99), -- Three Pepperoni Pizzas
(@OrderId, 6, 2, 8.99),  -- Two Caesar Salads
(@OrderId, 7, 2, 5.99);  -- Two Chocolate Cakes

-- Super Bowl Order 2
SET @OrderId = @OrderId - 1;
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
(@OrderId, 2, 3, 12.99), -- Three Pepperoni Pizzas
(@OrderId, 3, 2, 9.99),  -- Two Cheeseburgers
(@OrderId, 6, 1, 8.99),  -- One Caesar Salad
(@OrderId, 7, 2, 5.99);  -- Two Chocolate Cakes

-- Super Bowl Order 3
SET @OrderId = @OrderId - 1;
INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
(@OrderId, 1, 2, 10.99), -- Two Margherita Pizzas
(@OrderId, 2, 2, 12.99), -- Two Pepperoni Pizzas
(@OrderId, 3, 1, 9.99),  -- One Cheeseburger
(@OrderId, 7, 3, 5.99);  -- Three Chocolate Cakes

-- Add rush hour orders
INSERT INTO [Order] (UserId, OrderDate, TotalAmount, DeliveryAddress, Status)
VALUES 
-- Lunch rush (11:30am - 1:30pm)
(@FirstNewUserId + 1, DATEADD(day, -15, DATEADD(hour, 12, GETDATE())), 21.98, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 1), 'Delivered'),
(@FirstNewUserId + 3, DATEADD(day, -15, DATEADD(hour, 12, DATEADD(minute, 15, GETDATE()))), 23.97, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 3), 'Delivered'),
(@FirstNewUserId + 5, DATEADD(day, -15, DATEADD(hour, 12, DATEADD(minute, 30, GETDATE()))), 19.98, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 5), 'Delivered'),
(@FirstNewUserId + 7, DATEADD(day, -15, DATEADD(hour, 12, DATEADD(minute, 45, GETDATE()))), 25.98, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 7), 'Delivered'),
(@FirstNewUserId + 9, DATEADD(day, -15, DATEADD(hour, 13, GETDATE())), 29.97, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 9), 'Delivered');

-- Dinner rush (6:00pm - 8:00pm) - separate INSERT statement
INSERT INTO [Order] (UserId, OrderDate, TotalAmount, DeliveryAddress, Status)
VALUES 
(@FirstNewUserId + 2, DATEADD(day, -15, DATEADD(hour, 18, GETDATE())), 34.97, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 2), 'Delivered'),
(@FirstNewUserId + 4, DATEADD(day, -15, DATEADD(hour, 18, DATEADD(minute, 15, GETDATE()))), 36.96, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 4), 'Delivered'),
(@FirstNewUserId + 6, DATEADD(day, -15, DATEADD(hour, 18, DATEADD(minute, 30, GETDATE()))), 39.96, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 6), 'Delivered'),
(@FirstNewUserId + 8, DATEADD(day, -15, DATEADD(hour, 19, GETDATE())), 43.95, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 8), 'Delivered'),
(@FirstNewUserId + 10, DATEADD(day, -15, DATEADD(hour, 19, DATEADD(minute, 30, GETDATE()))), 31.97, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 10), 'Delivered');
-- Add items for these rush hour orders (simplified for brevity - just adding common patterns)
SET @OrderId = SCOPE_IDENTITY();

-- Add the order items for the 10 rush hour orders
DECLARE @RushOrderIndex INT = 0;
WHILE @RushOrderIndex < 10
BEGIN
    -- Create different order patterns based on index
    IF @RushOrderIndex % 3 = 0
    BEGIN
        -- Pizza focused order
        INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
        (@OrderId - @RushOrderIndex, 1, 1, 10.99), -- One Margherita Pizza
        (@OrderId - @RushOrderIndex, 6, 1, 8.99);  -- One Caesar Salad
    END
    ELSE IF @RushOrderIndex % 3 = 1
    BEGIN
        -- Burger focused order
        INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
        (@OrderId - @RushOrderIndex, 3, 1, 9.99),  -- One Cheeseburger
        (@OrderId - @RushOrderIndex, 6, 1, 8.99),  -- One Caesar Salad
        (@OrderId - @RushOrderIndex, 7, 1, 5.99);  -- One Chocolate Cake
    END
    ELSE
    BEGIN
        -- Family style order
        INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
        (@OrderId - @RushOrderIndex, 2, 2, 12.99), -- Two Pepperoni Pizzas
        (@OrderId - @RushOrderIndex, 6, 1, 8.99),  -- One Caesar Salad
        (@OrderId - @RushOrderIndex, 7, 1, 5.99);  -- One Chocolate Cake
    END
    
    SET @RushOrderIndex = @RushOrderIndex + 1;
END;

-- Add weekend orders (Friday night, Saturday lunch/dinner, Sunday lunch)
INSERT INTO [Order] (UserId, OrderDate, TotalAmount, DeliveryAddress, Status)
VALUES 
-- Friday night (busy time - 7pm to 10pm)
(@FirstNewUserId + 2, DATEADD(day, -8, DATEADD(hour, 19, GETDATE())), 33.97, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 2), 'Delivered'),
(@FirstNewUserId + 4, DATEADD(day, -8, DATEADD(hour, 19, DATEADD(minute, 30, GETDATE()))), 45.96, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 4), 'Delivered'),
(@FirstNewUserId + 6, DATEADD(day, -8, DATEADD(hour, 20, GETDATE())), 37.97, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 6), 'Delivered'),
(@FirstNewUserId + 8, DATEADD(day, -8, DATEADD(hour, 20, DATEADD(minute, 30, GETDATE()))), 42.96, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 8), 'Delivered'),
(@FirstNewUserId + 10, DATEADD(day, -8, DATEADD(hour, 21, GETDATE())), 39.97, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 10), 'Delivered'),

-- Saturday lunch (12pm to 2pm)
(@FirstNewUserId + 1, DATEADD(day, -9, DATEADD(hour, 12, GETDATE())), 28.97, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 1), 'Delivered'),
(@FirstNewUserId + 3, DATEADD(day, -9, DATEADD(hour, 12, DATEADD(minute, 30, GETDATE()))), 32.97, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 3), 'Delivered'),
(@FirstNewUserId + 5, DATEADD(day, -9, DATEADD(hour, 13, GETDATE())), 25.98, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 5), 'Delivered'),
(@FirstNewUserId + 7, DATEADD(day, -9, DATEADD(hour, 13, DATEADD(minute, 30, GETDATE()))), 29.97, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 7), 'Delivered'),

-- Saturday dinner (6pm to 9pm - busiest time of week)
(@FirstNewUserId + 9, DATEADD(day, -9, DATEADD(hour, 18, GETDATE())), 44.96, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 9), 'Delivered'),
(@FirstNewUserId + 11, DATEADD(day, -9, DATEADD(hour, 18, DATEADD(minute, 15, GETDATE()))), 49.95, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 11), 'Delivered'),
(@FirstNewUserId + 13, DATEADD(day, -9, DATEADD(hour, 18, DATEADD(minute, 30, GETDATE()))), 52.95, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 13), 'Delivered'),
(@FirstNewUserId + 15, DATEADD(day, -9, DATEADD(hour, 19, GETDATE())), 47.96, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 15), 'Delivered'),
(@FirstNewUserId + 17, DATEADD(day, -9, DATEADD(hour, 19, DATEADD(minute, 30, GETDATE()))), 38.97, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 17), 'Delivered'),
(@FirstNewUserId + 19, DATEADD(day, -9, DATEADD(hour, 20, GETDATE())), 41.97, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 19), 'Delivered'),

-- Sunday lunch/early dinner (1pm to 6pm)
(@FirstNewUserId, DATEADD(day, -10, DATEADD(hour, 13, GETDATE())), 31.97, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId), 'Delivered'),
(@FirstNewUserId + 2, DATEADD(day, -10, DATEADD(hour, 14, GETDATE())), 35.97, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 2), 'Delivered'),
(@FirstNewUserId + 4, DATEADD(day, -10, DATEADD(hour, 15, GETDATE())), 29.97, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 4), 'Delivered'),
(@FirstNewUserId + 6, DATEADD(day, -10, DATEADD(hour, 16, GETDATE())), 26.98, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 6), 'Delivered'),
(@FirstNewUserId + 8, DATEADD(day, -10, DATEADD(hour, 17, GETDATE())), 33.97, (SELECT Address FROM [User] WHERE Id = @FirstNewUserId + 8), 'Delivered');

-- Add order items for weekend orders (just add basic patterns to save time and avoid repetition)
SET @OrderId = SCOPE_IDENTITY();

-- Add the order items for the 20 weekend orders
DECLARE @WeekendOrderIndex INT = 0;
WHILE @WeekendOrderIndex < 20
BEGIN
    -- Create different order patterns based on index
    IF @WeekendOrderIndex % 4 = 0
    BEGIN
        -- Pizza focused order
        INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
        (@OrderId - @WeekendOrderIndex, 1, 2, 10.99), -- Two Margherita Pizzas
        (@OrderId - @WeekendOrderIndex, 6, 1, 8.99),  -- One Caesar Salad
        (@OrderId - @WeekendOrderIndex, 7, 1, 5.99);  -- One Chocolate Cake
    END
    ELSE IF @WeekendOrderIndex % 4 = 1
    BEGIN
        -- Burger focused order
        INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
        (@OrderId - @WeekendOrderIndex, 3, 2, 9.99),  -- Two Cheeseburgers
        (@OrderId - @WeekendOrderIndex, 4, 1, 10.99), -- One Vegetarian Burger
        (@OrderId - @WeekendOrderIndex, 6, 1, 8.99);  -- One Caesar Salad
    END
    ELSE IF @WeekendOrderIndex % 4 = 2
    BEGIN
        -- Family style order
        INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
        (@OrderId - @WeekendOrderIndex, 2, 2, 12.99), -- Two Pepperoni Pizzas
        (@OrderId - @WeekendOrderIndex, 5, 1, 11.99), -- One Spaghetti Bolognese
        (@OrderId - @WeekendOrderIndex, 6, 1, 8.99),  -- One Caesar Salad
        (@OrderId - @WeekendOrderIndex, 7, 2, 5.99);  -- Two Chocolate Cakes
    END
    ELSE
    BEGIN
        -- Mixed order
        INSERT INTO OrderItem (OrderId, FoodId, Quantity, Price) VALUES 
        (@OrderId - @WeekendOrderIndex, 1, 1, 10.99), -- One Margherita Pizza
        (@OrderId - @WeekendOrderIndex, 3, 1, 9.99),  -- One Cheeseburger
        (@OrderId - @WeekendOrderIndex, 6, 1, 8.99),  -- One Caesar Salad
        (@OrderId - @WeekendOrderIndex, 7, 1, 5.99);  -- One Chocolate Cake
    END
    
    SET @WeekendOrderIndex = @WeekendOrderIndex + 1;
END;

-- Print confirmation message
PRINT 'Successfully inserted 20 new users and approximately 120 new orders with order items.';
PRINT 'Orders are scattered throughout 2024 and 2025, including special dates, rush hours, and weekends.';
PRINT 'Total orders should now be ~120 or more in the database.';

-- Optional: Verify the total number of orders
-- SELECT COUNT(*) AS TotalOrders FROM [Order];

-- Update image paths for all food items to use correct paths and .png extension
UPDATE Food
SET ImageUrl = CASE Id
    WHEN 1 THEN '/images/food/margherita.png'
    WHEN 2 THEN '/images/food/pepperoni.png'
    WHEN 3 THEN '/images/food/cheeseburger.png'
    WHEN 4 THEN '/images/food/vegburger.png'
    WHEN 5 THEN '/images/food/spaghetti.png'
    WHEN 6 THEN '/images/food/caesar.png'
    WHEN 7 THEN '/images/food/choccake.png'
END
WHERE Id IN (1, 2, 3, 4, 5, 6, 7);

-- Verify the updates
SELECT Id, Name, ImageUrl FROM Food WHERE Id IN (1, 2, 3, 4, 5, 6, 7);



select * from Allergen

select * from [Order]

select * from [User]

select * from food 

select * from dbo.FoodCategory

select * from [Order]

SELECT * FROM [Order]
ORDER BY OrderDate DESC;  -- Assuming there is a column named OrderDate to sort by date




