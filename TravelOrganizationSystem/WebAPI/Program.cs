// Fixed SQL connection string - deploying to Azure endpoint: travel-api-matija.azurewebsites.net
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WebAPI.Data;
using WebAPI.Services;
using WebAPI.Swagger;

// All togehte now test deploy v222
Console.WriteLine("=== API STARTING ===");
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options => {
	options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
	options.JsonSerializerOptions.MaxDepth = 32;
});

// Configure CORS v1
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowWebApp", builder =>
	{
		builder.WithOrigins("http://localhost:17001", "https://localhost:17001", "https://*.vercel.app")
			   .AllowAnyMethod()
			   .AllowAnyHeader()
			   .AllowCredentials(); 
	});
	
	// Updated production policy to allow specific origins with credentials
	options.AddPolicy("AllowProduction", builder =>
	{
		builder.WithOrigins(
				"https://travel-web-matija.azurewebsites.net", // New frontend URL
				"https://travel-webapp-sokol-2024.azurewebsites.net", // Legacy URL for backwards compatibility
				"https://travel-webapp-sokol.azurewebsites.net", // Legacy URL for backwards compatibility
				"http://localhost:17001", // For local testing
				"https://localhost:17001" // For local testing with HTTPS
			   )
			   .AllowAnyMethod()
			   .AllowAnyHeader()
			   .AllowCredentials(); // Enable credentials for JWT authentication
	});
	
	// Fallback permissive policy for other scenarios
	options.AddPolicy("AllowAll", builder =>
	{
		builder.AllowAnyOrigin()
			   .AllowAnyMethod()
			   .AllowAnyHeader();
		// Note: Cannot use AllowCredentials() with AllowAnyOrigin()
	});
});

// Configure Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add application services
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IDestinationService, DestinationService>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<IGuideService, GuideService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ITripRegistrationService, TripRegistrationService>();

// Configure JWT authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]);

builder.Services.AddAuthentication(x =>
{
	x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
	x.RequireHttpsMetadata = false;
	x.SaveToken = true;
	x.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(key),
		ValidateIssuer = true,
		ValidIssuer = jwtSettings["Issuer"],
		ValidateAudience = true,
		ValidAudience = jwtSettings["Audience"],
		ValidateLifetime = true
	};
});

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { 
		Title = "Travel Organization API", 
		Version = "v1",
		Description = "API for Travel Organization System with authentication"
	});

	// Add JWT Authentication
	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
		Name = "Authorization",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.ApiKey,
		Scheme = "Bearer"
	});

	c.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			new string[] { }
		}
	});
	
	// Include XML comments
	var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
	var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
	c.IncludeXmlComments(xmlPath);
	
	// Add custom filters for Swagger documentation
	c.OperationFilter<AuthorizeCheckOperationFilter>();
	c.OperationFilter<OperationSummaryFilter>(); // Add operation summary filter to display summaries in list view
	
	// Use operation IDs in the URL path
	c.CustomOperationIds(e => $"{e.ActionDescriptor.RouteValues["action"]}");
});

// Add Security Requirement Operation Filter to mark endpoints that require JWT
builder.Services.AddSingleton<AuthorizeCheckOperationFilter>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger(c => 
	{
		// Customize the Swagger JSON to better show descriptions
		c.RouteTemplate = "swagger/{documentName}/swagger.json";
	});
	
	app.UseSwaggerUI(c => 
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "Travel Organization API v1");
		
		// Custom display options to show endpoint descriptions
		c.DefaultModelsExpandDepth(-1); // Hide the models by default
		c.DisplayRequestDuration(); // Show the request duration
		c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List); // Show endpoints as a list
		c.EnableFilter(); // Enable filtering
		c.EnableDeepLinking(); // Enable deep linking for navigation
		
		// Show operation description in the list view
		c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
		
		// Additional configuration to show descriptions
		c.ConfigObject.AdditionalItems.Add("tagsSorter", "alpha");
		c.ConfigObject.AdditionalItems.Add("operationsSorter", "alpha");
		c.ConfigObject.AdditionalItems.Add("displayOperationId", false); // Don't show operation ID
		c.ConfigObject.AdditionalItems.Add("showExtensions", true);
		c.ConfigObject.AdditionalItems.Add("showCommonExtensions", true);
        
        // Add custom CSS to enhance UI
        c.InjectStylesheet("/swagger-ui/custom.css");
	});
}

// Enable static files middleware to serve custom CSS
app.UseStaticFiles();

app.UseHttpsRedirection();

// Use different CORS policies based on environment
if (app.Environment.IsDevelopment())
{
	app.UseCors("AllowWebApp");
}
else
{
	app.UseCors("AllowProduction"); // Use the new production policy
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Make Program class accessible for integration testing
public partial class Program { }