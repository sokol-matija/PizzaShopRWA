# DBeaver Setup Guide for Your Azure SQL Database

## ✅ DBeaver Installation Complete
DBeaver Community Edition has been installed/upgraded to version 25.1.1.

## Your Database Connection Details
- **Server**: `travel-sql-central-2024.database.windows.net`
- **Database**: `TravelOrganizationDB`
- **Username**: `sqladmin`
- **Password**: `TravelApp123!`
- **Port**: `1433`

## Step 1: Open DBeaver and Create Connection

1. **Launch DBeaver**: You can open it from Applications or run `dbeaver` in terminal
2. **Create New Connection**: Click the "New Database Connection" button (+ icon)
3. **Select Database Type**: Choose **SQL Server** from the list and click "Next"

## Step 2: Configure Connection Settings

### Main Tab:
- **Server Host**: `travel-sql-central-2024.database.windows.net`
- **Port**: `1433`
- **Database**: `TravelOrganizationDB`
- **Authentication**: SQL Server Authentication
- **Username**: `sqladmin`
- **Password**: `TravelApp123!` 

### Driver Properties Tab:
Add these important properties:
- `encrypt = true`
- `trustServerCertificate = false`
- `loginTimeout = 30`

## Step 3: Test and Connect

1. Click **Test Connection** to verify everything works
2. If successful, click **Finish** to save the connection
3. The connection will appear in your Database Navigator

## Step 4: Update Your Development Environment

Your `appsettings.Development.json` has been updated with:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp://travel-sql-central-2024.database.windows.net,1433;Initial Catalog=TravelOrganizationDB;User ID=sqladmin;Password=[YOUR_SQL_ADMIN_PASSWORD];MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

**⚠️ Important**: Replace `[YOUR_SQL_ADMIN_PASSWORD]` with your actual password!

## Step 5: Verify Database Tables

Once connected, you should see these tables in your `TravelOrganizationDB`:
- ✅ `Destination` (6 records)
- ✅ `Guide` (5 records)  
- ✅ `Trip` (16 records)
- ✅ `User` (3 records)
- ✅ `TripRegistration` (7 records)
- ✅ `TripGuide` (26 records)
- ✅ `Log` (for API logging)

## Firewall Status
✅ Your IP (`95.168.118.35`) is already allowed - the server has an "AllowAll" firewall rule.

## Next Steps

1. **Find your SQL admin password** (check your deployment notes or Azure portal)
2. **Update the connection string** in `appsettings.Development.json`
3. **Test your API** to ensure it connects to Azure database
4. **Run your application** in development mode

## Troubleshooting

### If Connection Fails:
1. Verify the password is correct
2. Check that `encrypt=true` is set in driver properties
3. Ensure you're using SQL Server Authentication (not Windows Auth)

### To Find Your Password:
- Check your deployment script output
- Look in Azure Portal → SQL Server → Reset Password if needed
- Check any password manager or notes from deployment

Your database is ready to use! 🎉 