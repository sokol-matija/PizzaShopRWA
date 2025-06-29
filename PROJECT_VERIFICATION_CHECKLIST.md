# Travel Organization System - Project Verification Checklist

## Project Theme: Travel Organization System
**Main Entity**: Trip (putovanje) - title, description, etc.  
**1-N Entity**: Destination (odredišna lokacija) - e.g., city  
**M-N Entity**: Guide (vodič)  
**User M-N Entity**: Trip Registration (prijava na putovanje)

---

## 🏗️ **Project Structure Requirements**

### Archive Structure ✅/❌
- [ ] ZIP format (not RAR or 7z)
- [ ] Correct naming: `{LastName}-{FirstName}-{ProjectName}.zip`
- [ ] ProjectTask folder structure:
  - [ ] `Database/Database.sql` (single SQL file)
  - [ ] `SolutionName/` folder
    - [ ] `SolutionName.sln` file
    - [ ] `WebAPI/` project folder
    - [ ] `WebApp/` project folder
- [ ] No `bin/`, `obj/`, `packages/` folders in archive
- [ ] Archive size < 10MB

### Entity Structure ✅/❌
- [ ] **Trip (Primary Entity)**: Has Name + at least 3 additional attributes
- [ ] **Destination (1-N Entity)**: Has Name attribute
- [ ] **Guide (M-N Entity)**: Has Name attribute
- [ ] **User Entity**: For application users
- [ ] **TripRegistration (User M-N Entity)**: Bridge table for user actions
- [ ] All table names in singular form
- [ ] Table names match entity names exactly

---

## 🎯 **Outcome 1: RESTful Service Module (Web API)**

### Minimum Requirements (10 points) ✅/❌
#### CRUD Endpoints for Primary Entity (Trip)
- [ ] `GET /api/trip` - Get all trips
- [ ] `GET /api/trip/{id}` - Get trip by ID
- [ ] `POST /api/trip` - Create new trip
- [ ] `PUT /api/trip/{id}` - Update trip
- [ ] `DELETE /api/trip/{id}` - Delete trip

#### Search & Pagination
- [ ] `GET /api/trip/search` endpoint implemented
- [ ] Search by Name/Description attributes
- [ ] Pagination parameters (Page, Count) supported
- [ ] JSON content in request bodies where appropriate

#### Error Handling
- [ ] HTTP 400 (Bad Request) responses
- [ ] HTTP 404 (Not Found) responses  
- [ ] HTTP 500 (Internal Server Error) responses

#### Logging System
- [ ] `GET /api/logs/get/{N}` - Returns last N logs
- [ ] `GET /api/logs/count` - Returns total log count
- [ ] Log attributes: Id, Timestamp, Level, Message
- [ ] CRUD actions logged with meaningful messages
- [ ] Examples: "Trip with id=7 created", "Error updating trip id=7"

#### Documentation
- [ ] Swagger/OpenAPI interface included
- [ ] API can be demonstrated during defense

### Desired Requirements (10 points) ✅/❌
#### JWT Authentication
- [ ] `POST /api/auth/register` - User registration
- [ ] `POST /api/auth/login` - Get JWT token
- [ ] `POST /api/auth/changepassword` - Change password
- [ ] Log endpoints protected with JWT
- [ ] Swagger supports authentication demonstration

---

## 🗄️ **Outcome 2: Database Integration**

### Minimum Requirements (10 points) ✅/❌
#### Database Access
- [ ] Database used for CRUD operations on Trip (primary entity)
- [ ] CRUD endpoints for Destination (1-N entity)
- [ ] CRUD endpoints for Guide (M-N entity)  
- [ ] Proper handling of related entity deletion
- [ ] Elegant error handling for database operations

#### Database Configuration
- [ ] Connection string loaded from `appsettings.json`
- [ ] No hardcoded connection strings
- [ ] Database-first approach used (not Code-first)

### Desired Requirements (10 points) ✅/❌
#### Static HTML Pages
- [ ] Login page with JWT authentication
- [ ] Log list page showing logs securely
- [ ] User can change displayed log count (10, 25, 50)
- [ ] Dropdown for log count selection
- [ ] localStorage used for token storage
- [ ] Logout button functionality implemented

---

## 🖥️ **Outcome 3: MVC Module (Web Application)**

### Minimum Requirements - Administrator (10 points) ✅/❌
#### Admin Authentication & Navigation
- [ ] Admin login page
- [ ] Successful login redirects to Trip list page
- [ ] Consistent navigation on all pages (except login)
- [ ] Navigation links to all entity list pages
- [ ] Logout button on every page

#### Trip (Primary Entity) CRUD Pages
- [ ] **List Page**: Display all trips
  - [ ] Search textbox functionality
  - [ ] Dropdown filter by Destination (1-N entity)
  - [ ] Search button triggers filtering
  - [ ] Previous/Next buttons (10 items per page)
- [ ] **Add Page**: Create new trip
- [ ] **Edit Page**: Update existing trip
- [ ] **Delete**: Delete trip functionality

#### Other Entities CRUD Pages
- [ ] **Destination (1-N)**: List, Add, Edit, Delete pages
- [ ] **Guide (M-N)**: List, Add, Edit, Delete pages

#### Visual Design
- [ ] Visually attractive page presentation
- [ ] Consistent styling across pages

### Desired Requirements - User Interface (10 points) ✅/❌
#### Public Access
- [ ] **Landing Page**: No login required, represents theme visually
- [ ] Call-to-action leads to login page
- [ ] **Self-Registration Page**: Username, email, password, confirm password
- [ ] **Login Page**: Role-based redirection after successful login

#### User Trip Browsing
- [ ] **Trip List Page**: Shows available trips
  - [ ] Search textbox (trip name)
  - [ ] Dropdown filter by Destination  
  - [ ] Search button functionality
  - [ ] Click trip → goes to details page
  - [ ] Previous/Next pagination (10 items)
- [ ] **Trip Details Page**: Shows trip attributes
  - [ ] Back to list functionality
  - [ ] **Desired Action**: User can register for trip

#### Admin User Management
- [ ] **Admin**: View list of users and their trip registrations
- [ ] Display user actions (registered trips, etc.)

#### Additional Features
- [ ] Image upload for Trip entity (if theme requires)
- [ ] Visually attractive design

---

## ✅ **Outcome 4: Model Validation & Architecture**

### Minimum Requirements (10 points) ✅/❌
#### Model Validation
- [ ] Required field validation (Name, Description, etc.)
- [ ] Proper URL validation where applicable
- [ ] Email address validation
- [ ] Prevents empty input for required fields
- [ ] Duplicate name prevention (no two trips with same name)
- [ ] Entity IDs hidden from user interface

#### Model Annotations
- [ ] Visible labels implemented using model annotations
- [ ] Proper display names for all form fields

### Desired Requirements (10 points) ✅/❌
#### Multi-tier Architecture
- [ ] Web API and MVC layers depend on common business layer
- [ ] Separate data access layer project
- [ ] More than 2 projects in solution
- [ ] Different model sets for each tier
- [ ] No navigation properties in view models
- [ ] Database models not used in views

#### AutoMapper Integration
- [ ] AutoMapper configured and used
- [ ] Model mapping between tiers implemented
- [ ] Proper mapping configurations

---

## 🔄 **Outcome 5: AJAX Implementation**

### Minimum Requirements (10 points) ✅/❌
#### Admin Profile Page
- [ ] Profile page for administrator
- [ ] Update personal data: email, first name, last name, phone
- [ ] AJAX requests used for all updates
- [ ] No full page reloads during updates

### Desired Requirements (10 points) ✅/❌
#### User Profile Page  
- [ ] Profile page for regular users
- [ ] Update personal data: email, first name, last name, phone
- [ ] AJAX requests used for all updates

#### AJAX Pagination
- [ ] Trip list page uses AJAX for pagination
- [ ] Show multiple page numbers (e.g., 5, 6, 7, 8)
- [ ] Previous/Next buttons with AJAX
- [ ] No full page reloads during navigation

---

## 🗃️ **Database Requirements**

### SQL Script Requirements ✅/❌
- [ ] Single `Database.sql` file in Database folder
- [ ] Database-first approach used
- [ ] No `ALTER DATABASE`, `CREATE DATABASE`, or `USE` commands
- [ ] `CREATE TABLE` and `ALTER TABLE` commands used appropriately
- [ ] INSERT, UPDATE, DELETE, SELECT commands for sample data
- [ ] Sample data for 1-N and M-N entities for demonstration
- [ ] All table names in singular form
- [ ] Foreign key relationships properly defined

---

## 🎯 **Scoring Summary**

| Outcome | Minimum (10pts) | Desired (10pts) | Total |
|---------|----------------|------------------|-------|
| Outcome 1 | ⬜ | ⬜ | __/20 |
| Outcome 2 | ⬜ | ⬜ | __/20 |  
| Outcome 3 | ⬜ | ⬜ | __/20 |
| Outcome 4 | ⬜ | ⬜ | __/20 |
| Outcome 5 | ⬜ | ⬜ | __/20 |
| **TOTAL** | | | **__/100** |

---

## 🚨 **Critical Requirements for Acceptance**

- [ ] **Minimum 10 points per outcome** - Automatic disqualification if any outcome < 10 points
- [ ] **Correct archive structure** - Must follow exact specification
- [ ] **No hardcoded connection strings** - Must load from appsettings.json
- [ ] **Correct .NET version** - Must match workshop/assignment version
- [ ] **Database-first approach** - No Code-first migrations allowed
- [ ] **Working demonstration** - All features must work during defense

---

## 📋 **Next Steps**

1. **Go through each section systematically**
2. **Test every checkbox item in your application**
3. **Fix any missing or non-working features**
4. **Verify database script creates all tables correctly**
5. **Test with fresh database to ensure sample data works**
6. **Prepare for demonstration during defense**

**Remember**: All features must be working and demonstrable during the project defense! 