# Proper Data Sync Script
# This will create correctly formatted SQL INSERT statements

Write-Host "Creating proper data sync script..." -ForegroundColor Green

$outputFile = "Database\AzureSync\Proper_Data_Sync.sql"

# Clear the file
"" | Out-File -FilePath $outputFile -Encoding UTF8

# Add header
"-- Proper Data Sync for Azure SQL Database" | Out-File -FilePath $outputFile -Append -Encoding UTF8
"-- Generated on $(Get-Date)" | Out-File -FilePath $outputFile -Append -Encoding UTF8
"USE [TravelOrganizationDB]" | Out-File -FilePath $outputFile -Append -Encoding UTF8
"GO" | Out-File -FilePath $outputFile -Append -Encoding UTF8
"" | Out-File -FilePath $outputFile -Append -Encoding UTF8

# Function to properly format values
function Format-SqlValue {
    param($value)
    
    if ($value -eq $null -or $value -eq "NULL") {
        return "NULL"
    }
    elseif ($value -match "^\d+(\.\d+)?$") {
        return $value
    }
    else {
        return "'$($value -replace "'", "''")'"
    }
}

# Extract and format Destination data
Write-Host "Processing Destinations..." -ForegroundColor Yellow
$destinations = sqlcmd -S "." -d "TravelOrganizationDB" -Q "SELECT Id, Name, Description, Country, City, ImageUrl FROM Destination ORDER BY Id" -h -1

"-- Destination Data" | Out-File -FilePath $outputFile -Append -Encoding UTF8
"GO" | Out-File -FilePath $outputFile -Append -Encoding UTF8

foreach ($line in $destinations) {
    if ($line.Trim() -ne "" -and $line -notmatch "^---" -and $line -notmatch "^\([0-9]+ rows affected\)") {
        $parts = $line -split '\s{2,}' | Where-Object { $_.Trim() -ne "" }
        if ($parts.Count -ge 6) {
            $id = $parts[0]
            $name = $parts[1]
            $desc = $parts[2]
            $country = $parts[3]
            $city = $parts[4]
            $imageUrl = $parts[5]
            
            $insert = "INSERT INTO Destination (Id, Name, Description, Country, City, ImageUrl) VALUES ($(Format-SqlValue $id), $(Format-SqlValue $name), $(Format-SqlValue $desc), $(Format-SqlValue $country), $(Format-SqlValue $city), $(Format-SqlValue $imageUrl));"
            $insert | Out-File -FilePath $outputFile -Append -Encoding UTF8
        }
    }
}

"" | Out-File -FilePath $outputFile -Append -Encoding UTF8

# Extract and format Guide data
Write-Host "Processing Guides..." -ForegroundColor Yellow
$guides = sqlcmd -S "." -d "TravelOrganizationDB" -Q "SELECT Id, Name, Bio, Email, Phone, ImageUrl FROM Guide ORDER BY Id" -h -1

"-- Guide Data" | Out-File -FilePath $outputFile -Append -Encoding UTF8
"GO" | Out-File -FilePath $outputFile -Append -Encoding UTF8

foreach ($line in $guides) {
    if ($line.Trim() -ne "" -and $line -notmatch "^---" -and $line -notmatch "^\([0-9]+ rows affected\)") {
        $parts = $line -split '\s{2,}' | Where-Object { $_.Trim() -ne "" }
        if ($parts.Count -ge 6) {
            $id = $parts[0]
            $name = $parts[1]
            $bio = $parts[2]
            $email = $parts[3]
            $phone = $parts[4]
            $imageUrl = $parts[5]
            
            $insert = "INSERT INTO Guide (Id, Name, Bio, Email, Phone, ImageUrl) VALUES ($(Format-SqlValue $id), $(Format-SqlValue $name), $(Format-SqlValue $bio), $(Format-SqlValue $email), $(Format-SqlValue $phone), $(Format-SqlValue $imageUrl));"
            $insert | Out-File -FilePath $outputFile -Append -Encoding UTF8
        }
    }
}

"" | Out-File -FilePath $outputFile -Append -Encoding UTF8

# Extract and format Trip data
Write-Host "Processing Trips..." -ForegroundColor Yellow
$trips = sqlcmd -S "." -d "TravelOrganizationDB" -Q "SELECT Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId FROM Trip ORDER BY Id" -h -1

"-- Trip Data" | Out-File -FilePath $outputFile -Append -Encoding UTF8
"GO" | Out-File -FilePath $outputFile -Append -Encoding UTF8

foreach ($line in $trips) {
    if ($line.Trim() -ne "" -and $line -notmatch "^---" -and $line -notmatch "^\([0-9]+ rows affected\)") {
        $parts = $line -split '\s{2,}' | Where-Object { $_.Trim() -ne "" }
        if ($parts.Count -ge 8) {
            $id = $parts[0]
            $name = $parts[1]
            $desc = $parts[2]
            $startDate = $parts[3]
            $endDate = $parts[4]
            $price = $parts[5]
            $maxParticipants = $parts[6]
            $destinationId = $parts[7]
            
            $insert = "INSERT INTO Trip (Id, Name, Description, StartDate, EndDate, Price, MaxParticipants, DestinationId) VALUES ($(Format-SqlValue $id), $(Format-SqlValue $name), $(Format-SqlValue $desc), $(Format-SqlValue $startDate), $(Format-SqlValue $endDate), $(Format-SqlValue $price), $(Format-SqlValue $maxParticipants), $(Format-SqlValue $destinationId));"
            $insert | Out-File -FilePath $outputFile -Append -Encoding UTF8
        }
    }
}

"" | Out-File -FilePath $outputFile -Append -Encoding UTF8

# Extract and format User data
Write-Host "Processing Users..." -ForegroundColor Yellow
$users = sqlcmd -S "." -d "TravelOrganizationDB" -Q "SELECT Id, Username, Email, PasswordHash, IsAdmin, CreatedAt FROM [User] ORDER BY Id" -h -1

"-- User Data" | Out-File -FilePath $outputFile -Append -Encoding UTF8
"GO" | Out-File -FilePath $outputFile -Append -Encoding UTF8

foreach ($line in $users) {
    if ($line.Trim() -ne "" -and $line -notmatch "^---" -and $line -notmatch "^\([0-9]+ rows affected\)") {
        $parts = $line -split '\s{2,}' | Where-Object { $_.Trim() -ne "" }
        if ($parts.Count -ge 6) {
            $id = $parts[0]
            $username = $parts[1]
            $email = $parts[2]
            $passwordHash = $parts[3]
            $isAdmin = $parts[4]
            $createdAt = $parts[5]
            
            $insert = "INSERT INTO [User] (Id, Username, Email, PasswordHash, IsAdmin, CreatedAt) VALUES ($(Format-SqlValue $id), $(Format-SqlValue $username), $(Format-SqlValue $email), $(Format-SqlValue $passwordHash), $(Format-SqlValue $isAdmin), $(Format-SqlValue $createdAt));"
            $insert | Out-File -FilePath $outputFile -Append -Encoding UTF8
        }
    }
}

"" | Out-File -FilePath $outputFile -Append -Encoding UTF8

# Extract and format TripRegistration data
Write-Host "Processing Trip Registrations..." -ForegroundColor Yellow
$registrations = sqlcmd -S "." -d "TravelOrganizationDB" -Q "SELECT Id, UserId, TripId, RegistrationDate, Status FROM TripRegistration ORDER BY Id" -h -1

"-- TripRegistration Data" | Out-File -FilePath $outputFile -Append -Encoding UTF8
"GO" | Out-File -FilePath $outputFile -Append -Encoding UTF8

foreach ($line in $registrations) {
    if ($line.Trim() -ne "" -and $line -notmatch "^---" -and $line -notmatch "^\([0-9]+ rows affected\)") {
        $parts = $line -split '\s{2,}' | Where-Object { $_.Trim() -ne "" }
        if ($parts.Count -ge 5) {
            $id = $parts[0]
            $userId = $parts[1]
            $tripId = $parts[2]
            $registrationDate = $parts[3]
            $status = $parts[4]
            
            $insert = "INSERT INTO TripRegistration (Id, UserId, TripId, RegistrationDate, Status) VALUES ($(Format-SqlValue $id), $(Format-SqlValue $userId), $(Format-SqlValue $tripId), $(Format-SqlValue $registrationDate), $(Format-SqlValue $status));"
            $insert | Out-File -FilePath $outputFile -Append -Encoding UTF8
        }
    }
}

"" | Out-File -FilePath $outputFile -Append -Encoding UTF8

# Extract and format TripGuide data
Write-Host "Processing Trip Guides..." -ForegroundColor Yellow
$tripGuides = sqlcmd -S "." -d "TravelOrganizationDB" -Q "SELECT TripId, GuideId FROM TripGuide ORDER BY TripId, GuideId" -h -1

"-- TripGuide Data" | Out-File -FilePath $outputFile -Append -Encoding UTF8
"GO" | Out-File -FilePath $outputFile -Append -Encoding UTF8

foreach ($line in $tripGuides) {
    if ($line.Trim() -ne "" -and $line -notmatch "^---" -and $line -notmatch "^\([0-9]+ rows affected\)") {
        $parts = $line -split '\s{2,}' | Where-Object { $_.Trim() -ne "" }
        if ($parts.Count -ge 2) {
            $tripId = $parts[0]
            $guideId = $parts[1]
            
            $insert = "INSERT INTO TripGuide (TripId, GuideId) VALUES ($(Format-SqlValue $tripId), $(Format-SqlValue $guideId));"
            $insert | Out-File -FilePath $outputFile -Append -Encoding UTF8
        }
    }
}

Write-Host ""
Write-Host "Proper data sync script created!" -ForegroundColor Green
Write-Host "File: $outputFile" -ForegroundColor Cyan
Write-Host ""
Write-Host "To sync to Azure, run:" -ForegroundColor Yellow
Write-Host "sqlcmd -S 'travel-sql-central-2024.database.windows.net' -U 'sqladmin' -P 'TravelApp123!' -d 'TravelOrganizationDB' -i '$outputFile'" -ForegroundColor White 