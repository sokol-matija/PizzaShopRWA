# Travel Organization System - Deployment Guide

This guide will help you deploy your Travel Organization System with:
- **Database + API**: Azure (SQL Database + App Service)
- **Frontend**: Vercel
- **CI/CD**: GitHub Actions

## Prerequisites

1. **Azure CLI** installed and logged in
2. **Azure Subscription** with sufficient credits
3. **GitHub Account** with your repository
4. **Vercel Account** connected to GitHub

## Step 1: Deploy to Azure (Database + API)

### 1.1 Login to Azure CLI
```powershell
az login
az account set --subscription "your-subscription-id"
```

### 1.2 Run the deployment script
```powershell
.\deploy-azure.ps1 -ResourceGroupName "travel-org-rg" -Location "East US" -AppServiceName "your-travel-api-app" -SqlServerName "your-travel-sql-server" -SqlAdminLogin "sqladmin" -SqlAdminPassword "YourSecurePassword123!"
```

**Important**: Replace the parameter values with your own:
- `your-travel-api-app`: Must be globally unique
- `your-travel-sql-server`: Must be globally unique
- Use a strong password for SQL admin

### 1.3 Deploy the database schema
1. Open SQL Server Management Studio (SSMS)
2. Connect to your Azure SQL Server: `your-travel-sql-server.database.windows.net`
3. Run the `Database/Database.sql` script

### 1.4 Deploy the API code
```powershell
# Navigate to the WebAPI folder
cd TravelOrganizationSystem/WebAPI

# Build and publish
dotnet publish -c Release -o ./publish

# Create a zip file for deployment
Compress-Archive -Path ./publish/* -DestinationPath api-deployment.zip

# Deploy using Azure CLI
az webapp deployment source config-zip --resource-group "travel-org-rg" --name "your-travel-api-app" --src api-deployment.zip
```

## Step 2: Setup GitHub Actions (CI/CD)

### 2.1 Get Azure publish profile
```powershell
az webapp deployment list-publishing-profiles --name "your-travel-api-app" --resource-group "travel-org-rg" --xml
```

### 2.2 Add GitHub Secrets
1. Go to your GitHub repository
2. Settings → Secrets and variables → Actions
3. Add a new secret:
   - **Name**: `AZURE_WEBAPP_PUBLISH_PROFILE`
   - **Value**: The XML content from step 2.1

### 2.3 Update workflow file
Edit `.github/workflows/deploy-api.yml`:
- Replace `your-travel-api-app` with your actual app name

### 2.4 Test the workflow
Push any change to the WebAPI folder to trigger automatic deployment.

## Step 3: Deploy Frontend to Vercel

### 3.1 Update API URL
Edit `TravelOrganizationSystem/WebApp/appsettings.Production.json`:
- Replace `your-travel-api-app` with your actual Azure app name

### 3.2 Deploy to Vercel
1. Go to [vercel.com](https://vercel.com)
2. Sign in with GitHub
3. Click "New Project"
4. Import your repository
5. Set the root directory to: `TravelOrganizationSystem/WebApp`
6. Click "Deploy"

### 3.3 Configure environment variables in Vercel
In Vercel dashboard:
1. Go to your project settings
2. Environment Variables tab
3. Add these variables:
   - `ASPNETCORE_ENVIRONMENT`: `Production`
   - `ApiSettings__BaseUrl`: `https://your-travel-api-app.azurewebsites.net/api/`

## Step 4: Update CORS for Production

Your API is already configured to allow CORS from any origin in production. The configuration automatically:
- Uses restrictive CORS in development (localhost only)
- Uses permissive CORS in production (all origins)

## Step 5: Test Your Deployment

### 5.1 Test API
Visit: `https://your-travel-api-app.azurewebsites.net/swagger`

### 5.2 Test Frontend
Visit your Vercel URL (provided after deployment)

### 5.3 Test Integration
- Register a new user on the frontend
- Login and test trip booking functionality
- Verify data is stored in Azure SQL Database

## Troubleshooting

### Common Issues

1. **CORS Errors**
   - Ensure your Azure API app is running
   - Check that CORS is configured correctly
   - Verify the API URL in your frontend configuration

2. **Database Connection Issues**
   - Check Azure SQL Server firewall rules
   - Verify connection string format
   - Ensure SQL Database is running

3. **GitHub Actions Failing**
   - Verify publish profile secret is correct
   - Check app name in workflow file
   - Ensure .NET version matches your project

4. **Vercel Deployment Issues**
   - Check build logs in Vercel dashboard
   - Verify all environment variables are set
   - Ensure .NET runtime is compatible

### Monitoring and Logs

- **Azure App Service Logs**: Azure Portal → App Service → Monitoring → Log stream
- **Vercel Logs**: Vercel Dashboard → Project → Functions tab
- **GitHub Actions**: Repository → Actions tab

## Security Recommendations

1. **Restrict SQL Server firewall** to specific IP ranges after testing
2. **Use Azure Key Vault** for sensitive configuration
3. **Enable HTTPS only** for all services
4. **Set up monitoring** and alerts
5. **Regular security updates** for dependencies

## Costs Optimization

- **Azure**: Use Basic tier for development, scale up for production
- **Vercel**: Free tier sufficient for small applications
- **Monitor usage** regularly to avoid unexpected costs

---

## Support

If you encounter issues:
1. Check the troubleshooting section above
2. Review Azure/Vercel documentation
3. Check GitHub Issues for known problems
4. Contact support if needed

Remember to replace all placeholder values with your actual configuration! 