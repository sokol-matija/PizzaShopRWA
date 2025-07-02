# Better Data Extraction Script
# This will properly extract all data from your local database

Write-Host "Extracting ALL data from local database..." -ForegroundColor Green

# Create output directory
$outputDir = "Database\AzureSync"
if (!(Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force
}

# Create a comprehensive SQL file
$sqlFile = "$outputDir\Complete_Data_Sync.sql"
"USE [TravelOrganizationDB]" | Out-File -FilePath $sqlFile -Encoding UTF8
"GO" | Out-File -FilePath $sqlFile -Append -Encoding UTF8
"" | Out-File -FilePath $sqlFile -Append -Encoding UTF8
"-- Complete Data Sync for Azure SQL Database" | Out-File -FilePath $sqlFile -Append -Encoding UTF8
"-- Generated on $(Get-Date)" | Out-File -FilePath $sqlFile -Append -Encoding UTF8
"" | Out-File -FilePath $sqlFile -Append -Encoding UTF8

# Function to get data and format it properly
function Get-TableData {
    param($TableName)
    
    Write-Host "Processing $TableName..." -ForegroundColor Yellow
    
    # Get column information
    $columnsQuery = "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '$TableName' ORDER BY ORDINAL_POSITION"
    $columns = sqlcmd -S "." -d "TravelOrganizationDB" -Q $columnsQuery -h -1
    
    # Get data
    $dataQuery = "SELECT * FROM $TableName"
    $data = sqlcmd -S "." -d "TravelOrganizationDB" -Q $dataQuery -h -1
    
    if ($data) {
        "-- $TableName Data" | Out-File -FilePath $sqlFile -Append -Encoding UTF8
        "GO" | Out-File -FilePath $sqlFile -Append -Encoding UTF8
        
        # Process each row
        foreach ($row in $data) {
            if ($row.Trim() -ne "" -and $row -notmatch "^---" -and $row -notmatch "^\([0-9]+ rows affected\)") {
                # Split the row and format values properly
                $values = $row -split '\s{2,}' | Where-Object { $_.Trim() -ne "" }
                if ($values.Count -gt 0) {
                    # Format values with proper quotes and handling
                    $formattedValues = @()
                    foreach ($value in $values) {
                        if ($value -eq "NULL") {
                            $formattedValues += "NULL"
                        } elseif ($value -match "^\d+$") {
                            $formattedValues += $value
                        } else {
                            $formattedValues += "'$($value -replace "'", "''")'"
                        }
                    }
                    
                    $insertStatement = "INSERT INTO $TableName VALUES ($($formattedValues -join ', '));"
                    $insertStatement | Out-File -FilePath $sqlFile -Append -Encoding UTF8
                }
            }
        }
        
        "" | Out-File -FilePath $sqlFile -Append -Encoding UTF8
        Write-Host "✓ Processed $TableName" -ForegroundColor Green
    }
}

# Process all tables
$tables = @("Destination", "Guide", "Trip", "User", "TripRegistration", "TripGuide")

foreach ($table in $tables) {
    Get-TableData -TableName $table
}

Write-Host ""
Write-Host "Complete data extraction finished!" -ForegroundColor Green
Write-Host "File generated: $sqlFile" -ForegroundColor Cyan
Write-Host ""
Write-Host "To sync to Azure, run:" -ForegroundColor Yellow
Write-Host "sqlcmd -S 'travel-sql-central-2024.database.windows.net' -U 'sqladmin' -P 'TravelApp123!' -d 'TravelOrganizationDB' -i '$sqlFile'" -ForegroundColor White 