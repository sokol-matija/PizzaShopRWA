# Extract Local Database Data and Generate SQL INSERT Statements
# This script will extract all data from your local database and create SQL scripts for Azure

Write-Host "Extracting data from local database..." -ForegroundColor Green

# Create output directory
$outputDir = "Database\AzureSync"
if (!(Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force
}

# Function to extract data and generate INSERT statements
function Extract-TableData {
    param($TableName, $Columns)
    
    Write-Host "Extracting data from $TableName..." -ForegroundColor Yellow
    
    # Get data from local database
    $query = "SELECT $Columns FROM $TableName"
    $result = sqlcmd -S "." -d "TravelOrganizationDB" -Q $query -h -1
    
    if ($result) {
        $outputFile = "$outputDir\${TableName}_Data.sql"
        "GO" | Out-File -FilePath $outputFile -Encoding UTF8
        "-- Data for $TableName" | Out-File -FilePath $outputFile -Append -Encoding UTF8
        "GO" | Out-File -FilePath $outputFile -Append -Encoding UTF8
        
        # Process each row and create INSERT statements
        foreach ($line in $result) {
            if ($line.Trim() -ne "" -and $line -notmatch "^---" -and $line -notmatch "^\([0-9]+ rows affected\)") {
                # This is a simplified approach - you might need to handle specific data types
                $values = $line -split '\s{2,}' | Where-Object { $_.Trim() -ne "" }
                if ($values.Count -gt 0) {
                    $insertStatement = "INSERT INTO $TableName VALUES ($($values -join ', '));"
                    $insertStatement | Out-File -FilePath $outputFile -Append -Encoding UTF8
                }
            }
        }
        
        Write-Host "✓ Generated $outputFile" -ForegroundColor Green
    }
}

# Extract data for each table
Write-Host "Starting data extraction..." -ForegroundColor Cyan

# Destination table
Extract-TableData -TableName "Destination" -Columns "Id, Name, Description, Country, City, ImageUrl"

# Guide table  
Extract-TableData -TableName "Guide" -Columns "Id, Name, Bio, Email, Phone, ImageUrl"

# Trip table
Extract-TableData -TableName "Trip" -Columns "Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId"

# User table
Extract-TableData -TableName "User" -Columns "Id, Username, Email, PasswordHash, IsAdmin, CreatedAt"

# TripRegistration table
Extract-TableData -TableName "TripRegistration" -Columns "Id, UserId, TripId, RegistrationDate, Status"

# TripGuide table
Extract-TableData -TableName "TripGuide" -Columns "TripId, GuideId"

Write-Host ""
Write-Host "Data extraction completed!" -ForegroundColor Green
Write-Host "SQL files generated in: $outputDir" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Review the generated SQL files" -ForegroundColor White
Write-Host "2. Run them against your Azure SQL Database" -ForegroundColor White
Write-Host "3. Test the API endpoints" -ForegroundColor White 