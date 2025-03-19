using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PizzaShopWebApp
{
    public class ApiTester
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private string? _authToken;

        public ApiTester(string baseUrl)
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
        }

        public async Task RunTests()
        {
            Console.WriteLine("=== Pizza Shop API Tester ===");
            
            // Login first to get token
            if (!await Login("admin", "admin"))
            {
                Console.WriteLine("Login failed! Cannot continue with tests.");
                return;
            }

            Console.WriteLine("\n=== Testing Food Endpoints ===");
            await GetAllFood();
            await GetFoodCategories();
            await GetAllergens();
            
            Console.WriteLine("\n=== Testing Order Endpoints ===");
            await GetUserOrders();
            await GetAllOrders();
            
            Console.WriteLine("\nAll tests completed!");
        }

        private async Task<bool> Login(string username, string password)
        {
            Console.WriteLine($"Logging in as {username}...");
            
            try
            {
                var loginData = new { Username = username, Password = password };
                var response = await _httpClient.PostAsJsonAsync("/api/auth/login", loginData);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Login Response: {FormatJson(content)}");
                    
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (tokenResponse?.Token != null)
                    {
                        _authToken = tokenResponse.Token;
                        _httpClient.DefaultRequestHeaders.Authorization = 
                            new AuthenticationHeaderValue("Bearer", _authToken);
                        
                        Console.WriteLine("Login successful!");
                        return true;
                    }
                }
                
                Console.WriteLine($"Login failed. Status: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return false;
            }
        }

        private async Task GetAllFood()
        {
            Console.WriteLine("\nGetting all food items...");
            await GetAndShowJson("/api/food");
        }

        private async Task GetFoodCategories()
        {
            Console.WriteLine("\nGetting all food categories...");
            await GetAndShowJson("/api/foodcategory");
        }

        private async Task GetAllergens()
        {
            Console.WriteLine("\nGetting all allergens...");
            await GetAndShowJson("/api/allergen");
        }

        private async Task GetUserOrders()
        {
            Console.WriteLine("\nGetting user orders...");
            await GetAndShowJson("/api/order");
        }

        private async Task GetAllOrders()
        {
            Console.WriteLine("\nGetting all orders (admin only)...");
            await GetAndShowJson("/api/order/all");
        }

        private async Task GetAndShowJson(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Response from {endpoint}:");
                    Console.WriteLine(FormatJson(content));
                }
                else
                {
                    Console.WriteLine($"Failed to get data from {endpoint}. Status: {response.StatusCode}");
                    Console.WriteLine($"Response: {content}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing {endpoint}: {ex.Message}");
            }
        }

        private string FormatJson(string json)
        {
            try
            {
                // Parse and format JSON for better readability
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
                return JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions { WriteIndented = true });
            }
            catch
            {
                // If parsing fails, return the original JSON
                return json;
            }
        }
    }

    public class TokenResponse
    {
        public string? Token { get; set; }
        public string? Username { get; set; }
        public bool IsAdmin { get; set; }
        public string? ExpiresAt { get; set; }
    }

    // You can run this from Program.cs with:
    // var tester = new ApiTester("http://localhost:5156");
    // await tester.RunTests();
} 