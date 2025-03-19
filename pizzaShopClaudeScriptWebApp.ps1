$sourceDir = "S:\2-Repos\PizzaShop\PizzaShopWebApp"
$destDir = "S:\2-Repos\PizzaShop\ClaudeWebApp"

# Create destination if needed
if (-not (Test-Path -Path $destDir)) {
    New-Item -Path $destDir -ItemType Directory -Force
}

# Get all files matching extensions and excluding specified folders
Get-ChildItem -Path $sourceDir -Recurse -File -Include *.cs,*.csproj,*.user,*.json,*.js,*.css,*.cshtml | 
    Where-Object { 
        $_.FullName -notmatch '\\bin\\' -and 
        $_.FullName -notmatch '\\obj\\' -and 
        $_.FullName -notmatch '\\wwwroot\\lib\\'
    } | 
    Copy-Item -Destination $destDir -Force

Write-Host "Files copied successfully to $destDir"