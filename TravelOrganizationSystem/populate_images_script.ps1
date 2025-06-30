# PowerShell script to populate images for trips without images
# Run this after starting both WebAPI and WebApp

Write-Host "🚀 Starting image population for trips..." -ForegroundColor Green

# Wait for user to confirm both services are running
Write-Host "📋 Please ensure both services are running:" -ForegroundColor Yellow
Write-Host "   1. WebAPI on http://localhost:5000" -ForegroundColor White
Write-Host "   2. WebApp on http://localhost:5001" -ForegroundColor White
Write-Host ""
$confirm = Read-Host "Are both services running? (y/N)"

if ($confirm -ne "y" -and $confirm -ne "Y") {
    Write-Host "❌ Please start both services first:" -ForegroundColor Red
    Write-Host "   Terminal 1: cd TravelOrganizationSystem && dotnet run --project WebAPI" -ForegroundColor White
    Write-Host "   Terminal 2: cd TravelOrganizationSystem && dotnet run --project WebApp" -ForegroundColor White
    exit 1
}

try {
    Write-Host "🔍 Checking current trips without images..." -ForegroundColor Cyan
    
    # First check which trips need images
    $tripsResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/Trip" -Method GET
    $tripsNeedingImages = $tripsResponse | Where-Object { [string]::IsNullOrEmpty($_.imageUrl) }
    
    Write-Host "📊 Found $($tripsNeedingImages.Count) trips without images" -ForegroundColor Yellow
    
    if ($tripsNeedingImages.Count -eq 0) {
        Write-Host "✅ All trips already have images!" -ForegroundColor Green
        exit 0
    }
    
    # List the trips that need images
    Write-Host "🎯 Trips that need images:" -ForegroundColor Cyan
    foreach ($trip in $tripsNeedingImages) {
        Write-Host "   - Trip $($trip.id): $($trip.name)" -ForegroundColor White
    }
    
    Write-Host ""
    $proceed = Read-Host "Proceed with image population? (y/N)"
    
    if ($proceed -ne "y" -and $proceed -ne "Y") {
        Write-Host "❌ Operation cancelled" -ForegroundColor Red
        exit 1
    }
    
    # Call the populate endpoint
    Write-Host "🖼️ Starting image population..." -ForegroundColor Green
    
    $populateResponse = Invoke-RestMethod -Uri "http://localhost:5001/api/unsplash/populate-trip-images" -Method POST -ContentType "application/json"
    
    Write-Host "✅ Image population completed!" -ForegroundColor Green
    Write-Host "📈 Results:" -ForegroundColor Cyan
    Write-Host "   - Total processed: $($populateResponse.totalProcessed)" -ForegroundColor White
    Write-Host "   - Successful updates: $($populateResponse.results | Where-Object { $_.status -like "*SUCCESS*" } | Measure-Object).Count" -ForegroundColor Green
    Write-Host "   - Failed updates: $($populateResponse.results | Where-Object { $_.status -like "*ERROR*" -or $_.status -like "*EXCEPTION*" } | Measure-Object).Count" -ForegroundColor Red
    
    # Show detailed results
    if ($populateResponse.results) {
        Write-Host ""
        Write-Host "📋 Detailed Results:" -ForegroundColor Cyan
        foreach ($result in $populateResponse.results) {
            $color = if ($result.status -like "*SUCCESS*") { "Green" } 
                    elseif ($result.status -like "*ALREADY*") { "Yellow" }
                    else { "Red" }
            Write-Host "   $($result.status) - Trip $($result.tripId): $($result.tripName)" -ForegroundColor $color
            if ($result.error) {
                Write-Host "     Error: $($result.error)" -ForegroundColor Red
            }
        }
    }
    
    Write-Host ""
    Write-Host "🎉 Image population process completed! Check your trips page to see the results." -ForegroundColor Green
    
} catch {
    Write-Host "❌ Error during image population:" -ForegroundColor Red
    Write-Host "   $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Troubleshooting:" -ForegroundColor Yellow
    Write-Host "   1. Ensure both WebAPI and WebApp are running" -ForegroundColor White
    Write-Host "   2. Check that ports 5000 and 5001 are not blocked" -ForegroundColor White
    Write-Host "   3. Verify Unsplash API key is valid" -ForegroundColor White
    exit 1
} 