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


select * from Allergen

select * from [Order]

select * from [User]

select * from food 

select * from dbo.FoodCategory

select * from [Order]

SELECT * FROM [Order]
ORDER BY OrderDate DESC;  -- Assuming there is a column named OrderDate to sort by date

