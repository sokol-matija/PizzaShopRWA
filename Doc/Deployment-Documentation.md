# Travel Organization System - Deployment Documentation

## Overview

This document provides comprehensive deployment instructions for the Travel Organization System, covering local development, staging, and production environments. The system consists of two main components: a Web API backend and an MVC frontend application.

## System Requirements

### Development Environment
- **Operating System**: Windows 10/11, macOS 10.15+, or Linux (Ubuntu 18.04+)
- **.NET SDK**: .NET 6.0 or higher
- **Database**: SQL Server 2019+, SQL Server Express, or SQL Server LocalDB
- **IDE**: Visual Studio 2022, Visual Studio Code, or JetBrains Rider
- **Web Browser**: Chrome 90+, Firefox 88+, Safari 14+, or Edge 90+

### Production Environment
- **Server OS**: Windows Server 2019+ or Linux (Ubuntu 20.04+)
- **Runtime**: ASP.NET Core 6.0 Runtime
- **Database**: SQL Server 2019+ or Azure SQL Database
- **Web Server**: IIS 10+ or Nginx
- **SSL Certificate**: Required for HTTPS
- **Memory**: Minimum 4GB RAM (8GB recommended)
- **Storage**: Minimum 20GB available space

## Project Structure

```
TravelOrganizationSystem/
├── TravelOrganizationSystem.sln
├── WebAPI/                    # Backend API
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   ├── Data/
│   ├── appsettings.json
│   ├── appsettings.Production.json
│   └── WebApi.csproj
├── WebApp/                    # Frontend MVC
│   ├── Pages/
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   ├── wwwroot/
│   ├── appsettings.json
│   ├── appsettings.Production.json
│   └── WebApp.csproj
└── Database/
    └── Database.sql           # Database creation script
```

## Local Development Setup

### 1. Prerequisites Installation

#### Install .NET SDK
```bash
# Windows (using winget)
winget install Microsoft.DotNet.SDK.6

# macOS (using Homebrew)
brew install --cask dotnet

# Linux (Ubuntu)
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-6.0
```

#### Install SQL Server
```bash
# Windows: Download SQL Server Express from Microsoft
# macOS/Linux: Use Docker
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123!" \
   -p 1433:1433 --name sqlserver \
   -d mcr.microsoft.com/mssql/server:2019-latest
```

### 2. Database Setup

#### Create Database
```sql
-- Connect to SQL Server and run Database.sql script
-- Or use SQL Server Management Studio to execute the script
```

#### Update Connection Strings
```json
// WebAPI/appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TravelOrganizationDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}

// WebApp/appsettings.json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:16000/api/"
  }
}
```

### 3. Build and Run

#### Using Visual Studio
1. Open `TravelOrganizationSystem.sln`
2. Set multiple startup projects: WebAPI and WebApp
3. Press F5 to run both projects

#### Using Command Line
```bash
# Clone repository
git clone <repository-url>
cd TravelOrganizationSystem

# Restore packages
dotnet restore

# Build solution
dotnet build

# Run WebAPI (Terminal 1)
cd WebAPI
dotnet run

# Run WebApp (Terminal 2)
cd WebApp
dotnet run
```

### 4. Verify Installation

#### Check API
- Navigate to: `http://localhost:16000/swagger`
- Verify Swagger UI loads with all endpoints

#### Check Web Application
- Navigate to: `http://localhost:5000`
- Verify homepage loads correctly
- Test user registration and login

## Production Deployment

### Azure Deployment (Recommended)

#### 1. Azure Resources Setup

```powershell
# Azure CLI commands
az login
az group create --name travel-system-rg --location "East US"

# Create SQL Database
az sql server create --name travel-sql-server --resource-group travel-system-rg \
  --location "East US" --admin-user sqladmin --admin-password "YourPassword123!"

az sql db create --resource-group travel-system-rg --server travel-sql-server \
  --name TravelOrganizationDB --service-objective Basic

# Create App Service Plans
az appservice plan create --name travel-api-plan --resource-group travel-system-rg \
  --sku B1 --is-linux

az appservice plan create --name travel-web-plan --resource-group travel-system-rg \
  --sku B1 --is-linux

# Create Web Apps
az webapp create --resource-group travel-system-rg --plan travel-api-plan \
  --name travel-api-sokol-2024 --runtime "DOTNETCORE|6.0"

az webapp create --resource-group travel-system-rg --plan travel-web-plan \
  --name travel-web-sokol-2024 --runtime "DOTNETCORE|6.0"
```

#### 2. Database Deployment

```sql
-- Connect to Azure SQL Database
-- Run Database.sql script to create tables and sample data
-- Update connection string in Azure App Service configuration
```

#### 3. Application Configuration

```json
// Production configuration (Azure App Service Application Settings)
{
  "ConnectionStrings__DefaultConnection": "Server=tcp:travel-sql-server.database.windows.net,1433;Initial Catalog=TravelOrganizationDB;Persist Security Info=False;User ID=sqladmin;Password=YourPassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
  "ApiSettings__BaseUrl": "https://travel-api-sokol-2024.azurewebsites.net/api/",
  "UnsplashSettings__AccessKey": "your-unsplash-access-key",
  "UnsplashSettings__SecretKey": "your-unsplash-secret-key",
  "UnsplashSettings__CacheDurationMinutes": "60"
}
```

#### 4. Deployment Scripts

```powershell
# deploy-azure.ps1
param(
    [string]$ResourceGroupName = "travel-system-rg",
    [string]$ApiAppName = "travel-api-sokol-2024",
    [string]$WebAppName = "travel-web-sokol-2024"
)

Write-Host "Building applications..." -ForegroundColor Green

# Build and publish WebAPI
dotnet publish WebAPI/WebApi.csproj -c Release -o WebAPI/publish

# Build and publish WebApp
dotnet publish WebApp/WebApp.csproj -c Release -o WebApp/publish

Write-Host "Deploying to Azure..." -ForegroundColor Green

# Deploy WebAPI
az webapp deployment source config-zip --resource-group $ResourceGroupName \
  --name $ApiAppName --src WebAPI/publish.zip

# Deploy WebApp
az webapp deployment source config-zip --resource-group $ResourceGroupName \
  --name $WebAppName --src WebApp/publish.zip

Write-Host "Deployment completed successfully!" -ForegroundColor Green
```

### IIS Deployment (Windows Server)

#### 1. Server Prerequisites

```powershell
# Install IIS and ASP.NET Core Module
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServer
Enable-WindowsOptionalFeature -Online -FeatureName IIS-CommonHttpFeatures
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpErrors
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpLogging
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpRedirect
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ApplicationDevelopment
Enable-WindowsOptionalFeature -Online -FeatureName IIS-NetFxExtensibility45
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HealthAndDiagnostics
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpCompressionStatic
Enable-WindowsOptionalFeature -Online -FeatureName IIS-Security
Enable-WindowsOptionalFeature -Online -FeatureName IIS-RequestFiltering

# Download and install ASP.NET Core Runtime
# https://dotnet.microsoft.com/download/dotnet/6.0
```

#### 2. Application Deployment

```powershell
# Build applications for production
dotnet publish WebAPI/WebApi.csproj -c Release -o C:\inetpub\wwwroot\TravelAPI
dotnet publish WebApp/WebApp.csproj -c Release -o C:\inetpub\wwwroot\TravelWeb

# Create IIS Application Pools
New-WebAppPool -Name "TravelAPI" -Force
New-WebAppPool -Name "TravelWeb" -Force

# Set .NET Core runtime
Set-ItemProperty -Path "IIS:\AppPools\TravelAPI" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty -Path "IIS:\AppPools\TravelWeb" -Name "managedRuntimeVersion" -Value ""

# Create IIS Websites
New-Website -Name "TravelAPI" -Port 8080 -PhysicalPath "C:\inetpub\wwwroot\TravelAPI" -ApplicationPool "TravelAPI"
New-Website -Name "TravelWeb" -Port 80 -PhysicalPath "C:\inetpub\wwwroot\TravelWeb" -ApplicationPool "TravelWeb"
```

#### 3. SSL Configuration

```powershell
# Install SSL certificate
Import-Certificate -FilePath "certificate.pfx" -CertStoreLocation Cert:\LocalMachine\My -Password (ConvertTo-SecureString "password" -AsPlainText -Force)

# Bind SSL to websites
New-WebBinding -Name "TravelAPI" -Protocol https -Port 443 -SslFlags 1
New-WebBinding -Name "TravelWeb" -Protocol https -Port 443 -SslFlags 1
```

### Docker Deployment

#### 1. Dockerfile Creation

```dockerfile
# WebAPI/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WebAPI/WebApi.csproj", "WebAPI/"]
RUN dotnet restore "WebAPI/WebApi.csproj"
COPY . .
WORKDIR "/src/WebAPI"
RUN dotnet build "WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApi.dll"]
```

```dockerfile
# WebApp/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WebApp/WebApp.csproj", "WebApp/"]
RUN dotnet restore "WebApp/WebApp.csproj"
COPY . .
WORKDIR "/src/WebApp"
RUN dotnet build "WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApp.dll"]
```

#### 2. Docker Compose

```yaml
# docker-compose.yml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2019-latest
    environment:
      SA_PASSWORD: "YourPassword123!"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"
    volumes:
      - sql_data:/var/opt/mssql

  travel-api:
    build:
      context: .
      dockerfile: WebAPI/Dockerfile
    ports:
      - "8080:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=TravelOrganizationDB;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true
    depends_on:
      - sqlserver

  travel-web:
    build:
      context: .
      dockerfile: WebApp/Dockerfile
    ports:
      - "80:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ApiSettings__BaseUrl=http://travel-api:80/api/
    depends_on:
      - travel-api

volumes:
  sql_data:
```

#### 3. Docker Commands

```bash
# Build and run with Docker Compose
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Rebuild and restart
docker-compose up -d --build
```

## Environment Configuration

### Development
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TravelOrganizationDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "ApiSettings": {
    "BaseUrl": "http://localhost:16000/api/"
  },
  "UnsplashSettings": {
    "AccessKey": "development-key",
    "CacheDurationMinutes": 5
  }
}
```

### Production
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:travel-sql-server.database.windows.net,1433;Initial Catalog=TravelOrganizationDB;Persist Security Info=False;User ID=sqladmin;Password=YourPassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  },
  "ApiSettings": {
    "BaseUrl": "https://travel-api-sokol-2024.azurewebsites.net/api/"
  },
  "UnsplashSettings": {
    "AccessKey": "production-key",
    "CacheDurationMinutes": 60
  }
}
```

## Security Configuration

### HTTPS Setup

#### Development
```csharp
// Program.cs
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
```

#### Production
```csharp
// Program.cs
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHsts();
app.UseHttpsRedirection();
```

### CORS Configuration

```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp",
        builder =>
        {
            builder.WithOrigins("https://travel-web-sokol-2024.azurewebsites.net")
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials();
        });
});

app.UseCors("AllowWebApp");
```

### Authentication

```csharp
// JWT Configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
```

## Monitoring and Logging

### Application Insights (Azure)

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

```json
// appsettings.json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-instrumentation-key"
  }
}
```

### Custom Logging

```csharp
// LogService implementation
public class LogService : ILogService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LogService> _logger;

    public async Task LogAsync(string level, string message, Exception exception = null, int? userId = null)
    {
        var log = new Log
        {
            Timestamp = DateTime.UtcNow,
            Level = level,
            Message = message,
            Exception = exception?.ToString(),
            UserId = userId
        };

        _context.Logs.Add(log);
        await _context.SaveChangesAsync();
    }
}
```

### Health Checks

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddDbContext<ApplicationDbContext>()
    .AddUrlGroup(new Uri("https://api.unsplash.com/"), "Unsplash API");

app.MapHealthChecks("/health");
```

## Performance Optimization

### Caching

```csharp
// Program.cs
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

app.UseResponseCaching();
```

### Compression

```csharp
// Program.cs
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});

app.UseResponseCompression();
```

### Static Files

```csharp
// Program.cs
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
    }
});
```

## Backup and Recovery

### Database Backup

```sql
-- Automated backup script
BACKUP DATABASE TravelOrganizationDB 
TO DISK = 'C:\Backups\TravelOrganizationDB_Full.bak'
WITH FORMAT, INIT, COMPRESSION;

-- Differential backup
BACKUP DATABASE TravelOrganizationDB 
TO DISK = 'C:\Backups\TravelOrganizationDB_Diff.bak'
WITH DIFFERENTIAL, COMPRESSION;

-- Transaction log backup
BACKUP LOG TravelOrganizationDB 
TO DISK = 'C:\Backups\TravelOrganizationDB_Log.trn';
```

### Application Backup

```powershell
# Backup script
$BackupPath = "C:\Backups\TravelSystem_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
New-Item -ItemType Directory -Path $BackupPath

# Backup application files
Copy-Item -Path "C:\inetpub\wwwroot\TravelAPI" -Destination "$BackupPath\API" -Recurse
Copy-Item -Path "C:\inetpub\wwwroot\TravelWeb" -Destination "$BackupPath\Web" -Recurse

# Backup configuration
Copy-Item -Path "appsettings.Production.json" -Destination "$BackupPath\Config"
```

## Troubleshooting

### Common Issues

#### Connection String Issues
```bash
# Test database connection
sqlcmd -S server -d database -U username -P password -Q "SELECT 1"
```

#### Port Conflicts
```bash
# Check port usage
netstat -an | findstr :80
netstat -an | findstr :443
```

#### SSL Certificate Issues
```powershell
# Check certificate
Get-ChildItem -Path Cert:\LocalMachine\My | Where-Object {$_.Subject -like "*travel*"}
```

### Logging and Diagnostics

```csharp
// Enhanced error handling
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        
        logger.LogError(exceptionHandlerPathFeature?.Error, 
            "Unhandled exception occurred. Path: {Path}", 
            exceptionHandlerPathFeature?.Path);

        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("An error occurred");
    });
});
```

## Maintenance

### Regular Tasks

1. **Database Maintenance**
   - Weekly index rebuilding
   - Daily backup verification
   - Monthly statistics updates

2. **Application Updates**
   - Security patches
   - Dependency updates
   - Performance optimizations

3. **Monitoring**
   - Error rate monitoring
   - Performance metrics
   - Resource utilization

### Update Procedures

```powershell
# Application update script
# 1. Create backup
.\backup-system.ps1

# 2. Stop services
Stop-Website -Name "TravelAPI"
Stop-Website -Name "TravelWeb"

# 3. Deploy new version
.\deploy-update.ps1

# 4. Run database migrations
dotnet ef database update --project WebAPI

# 5. Start services
Start-Website -Name "TravelAPI"
Start-Website -Name "TravelWeb"

# 6. Verify deployment
.\verify-deployment.ps1
```

## Support and Maintenance

### Contact Information
- **Technical Support**: support@travelorganization.com
- **Emergency Contact**: +1-555-123-4567
- **Documentation**: Available in `/Doc` folder

### Maintenance Windows
- **Regular Maintenance**: Sundays 2:00 AM - 4:00 AM UTC
- **Emergency Maintenance**: As needed with 2-hour notice
- **Major Updates**: Quarterly with 1-week notice 