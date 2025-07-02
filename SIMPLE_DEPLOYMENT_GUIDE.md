# Simple Azure Deployment Guide
*Direct deployment without CI/CD pipelines*

## Overview
This guide shows how to deploy both the API and WebApp directly to Azure App Services using the "old school" method:
1. Build → Publish → Zip → Upload to Azure
2. Simple, reliable, and works every time

## Prerequisites
- Azure CLI installed and logged in
- Existing Azure App Services (API and WebApp)
- .NET 8 SDK installed

## Quick Deployment Steps

### Step 1: Check Azure Connection
```powershell
# Verify you're logged into Azure
az account show

# List your existing App Services
az webapp list --output table
```

### Step 2: Deploy API

```powershell
# Navigate to API project
cd TravelOrganizationSystem/WebAPI

# Clean, build, and publish
dotnet clean && dotnet build --configuration Release
dotnet publish --configuration Release --output ./publish

# Create deployment zip
Compress-Archive -Path "./publish/*" -DestinationPath "api-deployment.zip" -Force

# Deploy to Azure (replace with your API service name)
az webapp deployment source config-zip --resource-group "travel-org-rg" --name "travel-api-sokol-2024" --src "api-deployment.zip"
```

### Step 3: Deploy WebApp

```powershell
# Navigate to WebApp project
cd ../WebApp

# Clean, build, and publish
dotnet clean && dotnet build --configuration Release
dotnet publish --configuration Release --output ./publish

# Create deployment zip
Compress-Archive -Path "./publish/*" -DestinationPath "webapp-deployment.zip" -Force

# Deploy to Azure (replace with your WebApp service name)
az webapp deployment source config-zip --resource-group "travel-org-rg" --name "travel-webapp-sokol-2024" --src "webapp-deployment.zip"
```

### Step 4: Verify Deployment

```powershell
# Check API (note: Swagger is disabled in production for security)
curl -I https://your-api-name.azurewebsites.net/api/destinations

# Check WebApp
curl -I https://your-webapp-name.azurewebsites.net

# Check logs if needed
az webapp log tail --name "your-api-name" --resource-group "travel-org-rg"
```

## Important Notes

### Swagger in Production
- Swagger UI is **intentionally disabled** in production for security
- This is correct behavior - don't try to access `/swagger` in production
- Test API endpoints directly: `/api/destinations`, `/api/auth/login`, etc.

### Configuration
- Azure App Services should already have these settings configured:
  - `ConnectionStrings__DefaultConnection`
  - `JwtSettings__Secret`
  - `JwtSettings__Issuer`
  - `JwtSettings__Audience`
  - `ASPNETCORE_ENVIRONMENT=Production`

### CORS Settings
- API automatically uses permissive CORS in production
- This allows WebApp to communicate with API from different domains

## Troubleshooting

### 404 Errors on API
1. **Wait 2-3 minutes** after deployment (app needs time to start)
2. Check Azure App Service is running in Azure Portal
3. Verify app settings are configured correctly
4. Check logs: `az webapp log tail --name "your-api-name" --resource-group "travel-org-rg"`

### Connection Issues
1. Verify database connection string is correct
2. Check SQL Server firewall allows Azure services
3. Ensure JWT secret is properly configured

### Build Warnings
- Nullable reference warnings are normal and don't affect deployment
- File locking warnings occur when local development server is running

## Deployment Checklist

- [ ] Azure CLI logged in
- [ ] Both projects build without errors
- [ ] API deployed successfully 
- [ ] WebApp deployed successfully
- [ ] API responds to test endpoint (not /swagger)
- [ ] WebApp loads correctly
- [ ] No critical errors in logs

## Time Estimates
- API deployment: ~2-3 minutes
- WebApp deployment: ~2-3 minutes  
- Total process: ~10 minutes including verification

---

**Why This Method Works:**
- No complex CI/CD setup required
- Direct control over what gets deployed
- Easy to troubleshoot
- Reproducible and reliable
- Works for quick updates and hotfixes 