using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using WebApp.Models;

namespace WebApp.Controllers
{
    /// <summary>
    /// Controller for handling profile operations via AJAX
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProfileController> _logger;
        private readonly string _apiBaseUrl;

        public ProfileController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ProfileController> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _logger = logger;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7066/api/";
        }

        /// <summary>
        /// Update user profile information
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                // Get token from session (same pattern as AuthService)
                var token = HttpContext.Session.GetString("Token");
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Profile update failed: No token found in session");
                    return Unauthorized(new { message = "User not authenticated" });
                }

                // Set the authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                // Create the profile update request (matching WebAPI DTO)
                var profileUpdateData = new
                {
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address
                };

                var json = JsonSerializer.Serialize(profileUpdateData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending profile update to {ApiUrl}", $"{_apiBaseUrl}User/profile");

                // Make the call to WebAPI
                var response = await _httpClient.PutAsync($"{_apiBaseUrl}User/profile", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("Profile update response: Status={StatusCode}, Content={Content}", 
                    response.StatusCode, responseContent);
                
                if (response.IsSuccessStatusCode)
                {
                    var updatedUser = JsonSerializer.Deserialize<UserModel>(responseContent, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });
                    
                    if (updatedUser != null)
                    {
                        return Ok(new
                        {
                            id = updatedUser.Id,
                            username = updatedUser.Username,
                            email = updatedUser.Email,
                            firstName = updatedUser.FirstName,
                            lastName = updatedUser.LastName,
                            phoneNumber = updatedUser.PhoneNumber,
                            address = updatedUser.Address,
                            isAdmin = updatedUser.IsAdmin
                        });
                    }
                }
                
                return BadRequest(new { message = "Failed to update profile", details = responseContent });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
    }

    /// <summary>
    /// Request model for profile updates
    /// </summary>
    public class UpdateProfileRequest
    {
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
    }
} 