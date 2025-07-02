# Azure Deployment Script for Travel Organization System
# Make sure you're logged into Azure CLI before running this script

param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$Location = "East US",
    
    [Parameter(Mandatory=$true)]
    [string]$AppServiceName,
    
    [Parameter(Mandatory=$true)]
    [string]$SqlServerName,
    
    [Parameter(Mandatory=$true)]
    [string]$DatabaseName = "TravelOrganizationDB",
    
    [Parameter(Mandatory=$true)]
    [string]$SqlAdminLogin,
    
    [Parameter(Mandatory=$true)]
    [string]$SqlAdminPassword
)

Write-Host "Starting Azure deployment..." -ForegroundColor Green

# Create Resource Group
Write-Host "Creating resource group: $ResourceGroupName" -ForegroundColor Yellow
az group create --name $ResourceGroupName --location $Location

# Create SQL Server
Write-Host "Creating SQL Server: $SqlServerName" -ForegroundColor Yellow
az sql server create `
    --name $SqlServerName `
    --resource-group $ResourceGroupName `
    --location $Location `
    --admin-user $SqlAdminLogin `
    --admin-password $SqlAdminPassword

# Configure SQL Server firewall to allow Azure services
Write-Host "Configuring SQL Server firewall..." -ForegroundColor Yellow
az sql server firewall-rule create `
    --resource-group $ResourceGroupName `
    --server $SqlServerName `
    --name "AllowAzureServices" `
    --start-ip-address 0.0.0.0 `
    --end-ip-address 0.0.0.0

# Allow all IPs for now (you can restrict this later)
az sql server firewall-rule create `
    --resource-group $ResourceGroupName `
    --server $SqlServerName `
    --name "AllowAll" `
    --start-ip-address 0.0.0.0 `
    --end-ip-address 255.255.255.255

# Create SQL Database
Write-Host "Creating SQL Database: $DatabaseName" -ForegroundColor Yellow
az sql db create `
    --resource-group $ResourceGroupName `
    --server $SqlServerName `
    --name $DatabaseName `
    --service-objective Basic

# Create App Service Plan
Write-Host "Creating App Service Plan..." -ForegroundColor Yellow
az appservice plan create `
    --name "$AppServiceName-plan" `
    --resource-group $ResourceGroupName `
    --location $Location `
    --sku B1 `
    --is-linux

# Create App Service
Write-Host "Creating App Service: $AppServiceName" -ForegroundColor Yellow
az webapp create `
    --resource-group $ResourceGroupName `
    --plan "$AppServiceName-plan" `
    --name $AppServiceName `
    --deployment-container-image-name "mcr.microsoft.com/dotnet/aspnet:8.0"

# Configure App Settings
Write-Host "Configuring app settings..." -ForegroundColor Yellow
$connectionString = "Server=tcp:$SqlServerName.database.windows.net,1433;Initial Catalog=$DatabaseName;Persist Security Info=False;User ID=$SqlAdminLogin;Password=$SqlAdminPassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
$jwtSecret = [System.Guid]::NewGuid().ToString() + [System.Guid]::NewGuid().ToString()

az webapp config appsettings set `
    --resource-group $ResourceGroupName `
    --name $AppServiceName `
    --settings `
        "ConnectionStrings__DefaultConnection=$connectionString" `
        "JwtSettings__Secret=$jwtSecret" `
        "JwtSettings__Issuer=TravelOrganizationAPI" `
        "JwtSettings__Audience=TravelOrganizationClient" `
        "JwtSettings__ExpiryInMinutes=120" `
        "ASPNETCORE_ENVIRONMENT=Production"

# Apply database schema
Write-Host "Applying database schema..." -ForegroundColor Yellow
Write-Host "Please run the Database.sql script against your Azure SQL Database manually or using SQL Server Management Studio" -ForegroundColor Red

Write-Host "Deployment completed!" -ForegroundColor Green
Write-Host "App Service URL: https://$AppServiceName.azurewebsites.net" -ForegroundColor Green
Write-Host "SQL Server: $SqlServerName.database.windows.net" -ForegroundColor Green
Write-Host "Database: $DatabaseName" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Deploy your API code to the App Service" -ForegroundColor White
Write-Host "2. Run the Database.sql script against your Azure SQL Database" -ForegroundColor White
Write-Host "3. Update your frontend configuration to use the new API URL" -ForegroundColor White 