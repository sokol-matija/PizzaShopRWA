// Testing deployment to new Azure endpoint: travel-web-matija.azurewebsites.net
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using WebApp.Services;
using WebApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
if (builder.Environment.IsDevelopment())
{
    // Enable runtime compilation for hot reload in development
    builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
} // Trigger build 2
else
{
    builder.Services.AddRazorPages();
}
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();

// Add HttpClient factory
builder.Services.AddHttpClient();

// Add HttpClient for Unsplash with proper configuration
builder.Services.AddHttpClient<UnsplashService>(client =>
{
    client.BaseAddress = new Uri("https://api.unsplash.com/");
    client.DefaultRequestHeaders.Add("Accept-Version", "v1");
    // Auth header will be added in the service
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add Memory Cache
builder.Services.AddMemoryCache();

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<IDestinationService, DestinationService>();
builder.Services.AddScoped<ITripRegistrationService, TripRegistrationService>();
builder.Services.AddScoped<IGuideService, GuideService>();
builder.Services.AddScoped<IUnsplashService, UnsplashService>();
builder.Services.AddScoped<ILogService, LogService>();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure cookie-based authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.SlidingExpiration = true;
    });

// Configure Unsplash settings
builder.Services.Configure<UnsplashSettings>(
    builder.Configuration.GetSection("UnsplashSettings"));

// Register UnsplashSettings as a singleton
builder.Services.AddSingleton(sp => 
    sp.GetRequiredService<IOptions<UnsplashSettings>>().Value);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable session before authentication and authorization
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers(); // Map API controllers

// Map Blazor components
app.Run();

// All togehte now test deploy
// Make Program class accessible for integration testing
public partial class Program { }
