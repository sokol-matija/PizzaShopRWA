$sourceDir = "S:\2-Repos\PizzaShop\WebApi"
$destDir = "S:\2-Repos\PizzaShop\ClaudeApi"

# Create destination if needed
if (-not (Test-Path -Path $destDir)) {
    New-Item -Path $destDir -ItemType Directory -Force
}

# Get all files matching extensions and not in bin/obj
Get-ChildItem -Path $sourceDir -Recurse -File -Include *.http,*.csproj,*.json,*.cs | 
    Where-Object { $_.FullName -notmatch '\\bin\\' -and $_.FullName -notmatch '\\obj\\' } | 
    Copy-Item -Destination $destDir -Force

Write-Host "Files copied successfully to $destDir"