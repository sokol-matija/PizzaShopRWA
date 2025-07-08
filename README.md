# Travel Organization System

A comprehensive web application for managing travel destinations, trips, guides, and user bookings built with .NET 8.

## Architecture

- **API**: ASP.NET Core Web API with JWT authentication, Entity Framework Core, SQL Server
- **WebApp**: ASP.NET Core Razor Pages with session-based authentication
- **Database**: SQL Server with comprehensive schema for trips, destinations, guides, and users

## Features

### For Users
- Browse travel destinations and trips
- User registration and authentication
- Book trips and manage bookings
- View trip details with guides and itineraries

### For Administrators
- Manage destinations, trips, and guides
- Assign guides to trips
- User management and system administration
- Comprehensive logging and monitoring

## Quick Start

### Local Development
```bash
# Start API (Terminal 1)
cd TravelOrganizationSystem/WebAPI
dotnet run

# Start WebApp (Terminal 2)  
cd TravelOrganizationSystem/WebApp
dotnet run
```

### Database Setup
1. Update connection strings in `appsettings.json`
2. Run database script: `Database/Database.sql`

## Deployment

### ⚡ Automated GitHub Actions Deployment (Recommended)
The application includes GitHub Actions workflows for automated deployment:
- API deployment workflow triggers on changes to `TravelOrganizationSystem/WebAPI/**`
- WebApp deployment workflow triggers on changes to `TravelOrganizationSystem/WebApp/**`

**Setup Requirements:**

1. **Configure Azure App Registration with Federated Credentials:**
   - Create an Azure App Registration
   - Add federated credentials for GitHub Actions
   - Subject identifier: `repo:YOUR_USERNAME/YOUR_REPO:ref:refs/heads/master`

2. **Required GitHub Secrets:**
   - `AZURE_CLIENT_ID` - Azure service principal client ID
   - `AZURE_TENANT_ID` - Azure tenant ID
   - `AZURE_SUBSCRIPTION_ID` - Azure subscription ID
   - `AZURE_API_APP_NAME` - Your Azure API app name
   - `AZURE_API_PUBLISH_PROFILE` - API app publish profile
   - `AZURE_WEBAPP_APP_NAME` - Your Azure webapp app name  
   - `AZURE_WEBAPP_PUBLISH_PROFILE` - Webapp publish profile

### ⚡ Quick Deployment (Alternative)
```powershell
# One-command deployment to Azure
.\deploy-both.ps1
```

### 📖 Deployment Documentation
- **GitHub Actions Workflows** - Automated CI/CD deployment (recommended)
- **[Simple Deployment Guide](SIMPLE_DEPLOYMENT_GUIDE.md)** - Direct Azure deployment
- **[Advanced Deployment Guide](DEPLOYMENT_GUIDE.md)** - CI/CD with GitHub Actions  
- **[Quick Deploy Script](deploy-both.ps1)** - Automated PowerShell deployment

## Project Documentation

- [Software Requirements Document (SRD)](Doc/Software%20Requirements%20Document%20(SRD).md)
- [Software Design Document (SDD)](Doc/Software%20Design%20Document%20(SDD).md) 
- [Requirements, Design, and Analysis (RDA)](Doc/Requirements,%20Design,%20and%20Analysis%20(RDA).md)
- [Project Verification Checklist](PROJECT_VERIFICATION_CHECKLIST.md)

## Technology Stack

- **.NET 8** - Core framework
- **ASP.NET Core** - Web API and Razor Pages
- **Entity Framework Core** - ORM
- **SQL Server** - Database
- **JWT** - API authentication
- **Bootstrap** - UI styling
- **Azure** - Cloud hosting

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration

### Destinations  
- `GET /api/destinations` - List destinations
- `POST /api/destinations` - Create destination (Admin)
- `PUT /api/destinations/{id}` - Update destination (Admin)

### Trips
- `GET /api/trips` - List trips
- `POST /api/trips` - Create trip (Admin)
- `GET /api/trips/{id}` - Trip details

### Bookings
- `POST /api/tripregistrations` - Book a trip
- `GET /api/tripregistrations/user/{userId}` - User's bookings

## Development Setup

1. **Prerequisites**
   - .NET 8 SDK
   - SQL Server (LocalDB or full)
   - Visual Studio Code or Visual Studio

2. **Clone and Setup**
   ```bash
   git clone [repository-url]
   cd travel-organization-system
   ```

3. **Configure Database**
   - Update connection strings in both projects
   - Run `Database/Database.sql` against your SQL Server

4. **Run Projects**
   ```bash
   # Terminal 1 - API
   cd TravelOrganizationSystem/WebAPI
   dotnet run

   # Terminal 2 - WebApp  
   cd TravelOrganizationSystem/WebApp
   dotnet run
   ```

## Production URLs

- **API**: https://travel-api-sokol-2024.azurewebsites.net
- **WebApp**: https://travel-webapp-sokol-2024.azurewebsites.net

## Important Notes

- **Swagger UI** is disabled in production for security
- **CORS** is configured for cross-origin requests
- **JWT tokens** expire after 2 hours
- **Session authentication** is used for the WebApp

## Support

For deployment issues or questions, see the [Simple Deployment Guide](SIMPLE_DEPLOYMENT_GUIDE.md) or check the Azure App Service logs:

```bash
az webapp log tail --name [app-name] --resource-group travel-org-rg
```
