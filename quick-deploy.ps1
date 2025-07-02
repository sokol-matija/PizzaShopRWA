# Quick Deploy Script for Travel Organization System
# This script will guide you through the entire deployment process

Write-Host "=== Travel Organization System - Quick Deploy ===" -ForegroundColor Cyan
Write-Host ""

# Check if Azure CLI is installed
try {
    az --version | Out-Null
} catch {
    Write-Host "Error: Azure CLI is not installed. Please install it first." -ForegroundColor Red
    Write-Host "Download from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Yellow
    exit 1
}

# Login to Azure
Write-Host "Logging into Azure..." -ForegroundColor Yellow
az login

# Get user inputs
Write-Host ""
Write-Host "Please provide the following information:" -ForegroundColor Green
Write-Host ""

$resourceGroup = Read-Host "Resource Group Name (e.g., travel-org-rg)"
$location = Read-Host "Azure Region (e.g., East US)"
$appName = Read-Host "App Service Name (must be globally unique, e.g., travel-api-123)"
$sqlServer = Read-Host "SQL Server Name (must be globally unique, e.g., travel-sql-123)"
$sqlUser = Read-Host "SQL Admin Username (e.g., sqladmin)"
$sqlPassword = Read-Host "SQL Admin Password" -AsSecureString
$sqlPasswordText = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($sqlPassword))

Write-Host ""
Write-Host "Summary of your deployment:" -ForegroundColor Cyan
Write-Host "- Resource Group: $resourceGroup" -ForegroundColor White
Write-Host "- Location: $location" -ForegroundColor White
Write-Host "- App Service: $appName" -ForegroundColor White
Write-Host "- SQL Server: $sqlServer" -ForegroundColor White
Write-Host "- SQL User: $sqlUser" -ForegroundColor White
Write-Host ""

$confirm = Read-Host "Continue with deployment? (y/n)"
if ($confirm -ne 'y' -and $confirm -ne 'Y') {
    Write-Host "Deployment cancelled." -ForegroundColor Yellow
    exit 0
}

# Run the main deployment script
Write-Host ""
Write-Host "Starting deployment..." -ForegroundColor Green
.\deploy-azure.ps1 -ResourceGroupName $resourceGroup -Location $location -AppServiceName $appName -SqlServerName $sqlServer -SqlAdminLogin $sqlUser -SqlAdminPassword $sqlPasswordText

# Update configuration files
Write-Host ""
Write-Host "Updating configuration files..." -ForegroundColor Yellow

# Update GitHub Actions workflow
$workflowPath = ".github/workflows/deploy-api.yml"
if (Test-Path $workflowPath) {
    $content = Get-Content $workflowPath -Raw
    $content = $content -replace "your-travel-api-app", $appName
    $content | Set-Content $workflowPath
    Write-Host "✓ Updated GitHub Actions workflow" -ForegroundColor Green
}

# Update WebApp production config
$webAppConfigPath = "TravelOrganizationSystem/WebApp/appsettings.Production.json"
if (Test-Path $webAppConfigPath) {
    $content = Get-Content $webAppConfigPath -Raw
    $content = $content -replace "your-travel-api-app", $appName
    $content | Set-Content $webAppConfigPath
    Write-Host "✓ Updated WebApp configuration" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== Deployment Complete! ===" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "1. Deploy database schema:" -ForegroundColor White
Write-Host "   - Connect to: $sqlServer.database.windows.net" -ForegroundColor Gray
Write-Host "   - Run: Database/Database.sql" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Deploy API code:" -ForegroundColor White
Write-Host "   - Run: cd TravelOrganizationSystem/WebAPI" -ForegroundColor Gray
Write-Host "   - Run: dotnet publish -c Release" -ForegroundColor Gray
Write-Host "   - Deploy via Azure Portal or GitHub Actions" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Setup GitHub Actions:" -ForegroundColor White
Write-Host "   - Get publish profile: az webapp deployment list-publishing-profiles --name $appName --resource-group $resourceGroup --xml" -ForegroundColor Gray
Write-Host "   - Add as GitHub secret: AZURE_WEBAPP_PUBLISH_PROFILE" -ForegroundColor Gray
Write-Host ""
Write-Host "4. Deploy to Vercel:" -ForegroundColor White
Write-Host "   - Import your repo to Vercel" -ForegroundColor Gray
Write-Host "   - Set root directory: TravelOrganizationSystem/WebApp" -ForegroundColor Gray
Write-Host "   - Add environment variable: ApiSettings__BaseUrl = https://$appName.azurewebsites.net/api/" -ForegroundColor Gray
Write-Host ""
Write-Host "API URL: https://$appName.azurewebsites.net" -ForegroundColor Yellow
Write-Host "Swagger: https://$appName.azurewebsites.net/swagger" -ForegroundColor Yellow
Write-Host ""
Write-Host "For detailed instructions, see DEPLOYMENT_GUIDE.md" -ForegroundColor Cyan 