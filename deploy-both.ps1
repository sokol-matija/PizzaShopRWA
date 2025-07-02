# Deploy Both API and WebApp to Azure
# Quick deployment script for Travel Organization System

param(
    [string]$ResourceGroup = "travel-org-rg",
    [string]$ApiAppName = "travel-api-sokol-2024",
    [string]$WebAppName = "travel-webapp-sokol-2024"
)

Write-Host "=== Azure Deployment Script ===" -ForegroundColor Cyan
Write-Host "API: $ApiAppName" -ForegroundColor Yellow
Write-Host "WebApp: $WebAppName" -ForegroundColor Yellow
Write-Host "Resource Group: $ResourceGroup" -ForegroundColor Yellow
Write-Host ""

# Check Azure login
Write-Host "Checking Azure login..." -ForegroundColor Green
try {
    az account show --output none
    Write-Host "✓ Azure CLI authenticated" -ForegroundColor Green
} catch {
    Write-Host "✗ Not logged into Azure. Run: az login" -ForegroundColor Red
    exit 1
}

# Function to deploy a project
function Deploy-Project {
    param(
        [string]$ProjectPath,
        [string]$ProjectName,
        [string]$AppServiceName,
        [string]$ZipName
    )
    
    Write-Host "Deploying $ProjectName..." -ForegroundColor Yellow
    
    # Navigate to project
    Push-Location $ProjectPath
    
    try {
        # Clean and build
        Write-Host "  Building $ProjectName..." -ForegroundColor Gray
        dotnet clean --configuration Release --verbosity quiet
        dotnet build --configuration Release --verbosity quiet
        
        if ($LASTEXITCODE -ne 0) {
            throw "Build failed for $ProjectName"
        }
        
        # Publish
        Write-Host "  Publishing $ProjectName..." -ForegroundColor Gray
        dotnet publish --configuration Release --output ./publish --verbosity quiet
        
        if ($LASTEXITCODE -ne 0) {
            throw "Publish failed for $ProjectName"
        }
        
        # Create zip
        Write-Host "  Creating deployment package..." -ForegroundColor Gray
        if (Test-Path $ZipName) {
            Remove-Item $ZipName
        }
        Compress-Archive -Path "./publish/*" -DestinationPath $ZipName -Force
        
        # Deploy to Azure
        Write-Host "  Uploading to Azure..." -ForegroundColor Gray
        az webapp deployment source config-zip --resource-group $ResourceGroup --name $AppServiceName --src $ZipName --output none
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ $ProjectName deployed successfully" -ForegroundColor Green
            return $true
        } else {
            throw "Azure deployment failed for $ProjectName"
        }
        
    } catch {
        Write-Host "✗ Failed to deploy $ProjectName" -ForegroundColor Red
        Write-Host "  Error: $_" -ForegroundColor Red
        return $false
    } finally {
        Pop-Location
    }
}

$startTime = Get-Date

# Deploy API
$apiSuccess = Deploy-Project -ProjectPath "TravelOrganizationSystem/WebAPI" -ProjectName "API" -AppServiceName $ApiAppName -ZipName "api-deployment.zip"

# Deploy WebApp
$webAppSuccess = Deploy-Project -ProjectPath "TravelOrganizationSystem/WebApp" -ProjectName "WebApp" -AppServiceName $WebAppName -ZipName "webapp-deployment.zip"

# Summary
Write-Host ""
Write-Host "=== Deployment Summary ===" -ForegroundColor Cyan
$endTime = Get-Date
$duration = $endTime - $startTime
Write-Host "Duration: $($duration.Minutes)m $($duration.Seconds)s" -ForegroundColor Gray

if ($apiSuccess) {
    Write-Host "✓ API: https://$ApiAppName.azurewebsites.net" -ForegroundColor Green
} else {
    Write-Host "✗ API: Deployment failed" -ForegroundColor Red
}

if ($webAppSuccess) {
    Write-Host "✓ WebApp: https://$WebAppName.azurewebsites.net" -ForegroundColor Green
} else {
    Write-Host "✗ WebApp: Deployment failed" -ForegroundColor Red
}

Write-Host ""

if ($apiSuccess -and $webAppSuccess) {
    Write-Host "🎉 All deployments successful!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "• Wait 2-3 minutes for apps to start" -ForegroundColor White
    Write-Host "• Test API: curl -I https://$ApiAppName.azurewebsites.net/api/destinations" -ForegroundColor White
    Write-Host "• Test WebApp: https://$WebAppName.azurewebsites.net" -ForegroundColor White
    Write-Host "• Remember: Swagger is disabled in production for security" -ForegroundColor Yellow
} else {
    Write-Host "❌ Some deployments failed. Check errors above." -ForegroundColor Red
    Write-Host "For logs: az webapp log tail --name [app-name] --resource-group $ResourceGroup" -ForegroundColor Yellow
    exit 1
} 