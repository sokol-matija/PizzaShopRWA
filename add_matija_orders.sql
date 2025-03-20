-- Add new orders for user matija (ID 6)
USE PizzaShopDB;
GO

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