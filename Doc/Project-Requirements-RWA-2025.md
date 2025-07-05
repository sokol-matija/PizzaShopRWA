# RWA Project Requirements 2025 - Travel Organization System

## 1. General Information

### Project Defense
- Defense is held during exam periods
- Student registers for defense like other exams
- **Defense in 3 days** - preparation needed

### Important Rules for Successful Submission
- Submit project on time
- Zip archive must follow specified naming and file structure
- Solution must have at least minimum points per learning outcome
- Solution must follow professor-approved theme

### Submission Deadline
- **5 days before defense** is considered the deadline for project solution delivery
- Students who submit after deadline will not have their work accepted

### Archive Naming and Structure
- Format: `{surname}-{name}-{project-name}.zip`
- Example: `Sokol-Matija-TravelOrganizationSystem.zip`
- Only ZIP format accepted (no RAR, 7z, etc.)

### Minimum Points per Outcome
- Correctly prepared solution carries at least minimum points per learning outcome
- **10 points for each learning outcome minimum**
- Must fulfill minimum requirements for each outcome to avoid automatic disqualification

## 2. Project Specification

Create a unique ASP.NET Core web solution consisting of two modules (projects):

### Required Modules
1. **RESTful Service Module (Web API)**
   - Covers Outcomes 1 and 2
   - Used for data retrieval via JavaScript
   - Intended for automation

2. **MVC Module (Web Application)**
   - Covers Outcomes 3, 4, and 5
   - User access via web browser

## 3. Learning Outcomes

### 3.1. Outcome 1 (RESTful Service Module, Web API)

#### Minimum (10 points)
- Create RESTful endpoint (CRUD) for primary entity
- Endpoints for search and paging
- Write to log during operations
- Make logs available via additional endpoint

#### Desired (10 points)
- Secure endpoints using JWT token authentication
- Implement common authentication functions

#### General Requirements - Minimum (10 points)
- Use primary entity name for CRUD endpoints (e.g., `api/trip`)
- Use JSON content in request body where appropriate
- Handle errors and return HTTP error codes 400, 404, 500
- Support search endpoint by attributes (Name, Description, etc.)
- Search endpoint must support paging using Page and Count parameters
- Use appropriate endpoint name for search (e.g., `api/trip/search`)
- Implement logs endpoint (e.g., `api/logs/get/N` - returns last N logs)
- Implement `api/logs/count` endpoint returning total stored logs
- Log attributes: Id, Timestamp, Level, Message
- Log every CRUD action of primary entity
- Include Swagger or similar interface for demonstration

#### General Requirements - Desired (10 points)
- Implement JWT token authentication for log endpoints
- Implement user registration (`api/auth/register`)
- Implement JWT token retrieval (`api/auth/login`)
- Implement password change (`api/auth/changepassword`)
- Swagger interface should support authentication

### 3.2. Outcome 2 (RESTful Service Module, Web API)

#### Minimum (10 points)
- Implement database access for endpoints

#### Desired (10 points)
- Implement static HTML pages using JWT authentication, localStorage, and existing endpoints for secure log display

#### General Requirements - Minimum (10 points)
- Use database for state storage using RESTful endpoints (CRUD) for primary entity
- Implement CRUD endpoints for 1-to-N and M-to-N entities with database support
- Handle deletion of related entities elegantly

#### General Requirements - Desired (10 points)
- Implement static HTML pages using JWT authentication for secure log display
- Pages should include login page and log list page
- Log list page should allow changing displayed number of logs (10, 25, 50)
- Use localStorage for authentication token storage
- Support logout via "Logout" button

### 3.3. Outcome 3 (MVC Module, Web Application)

#### Minimum (10 points)
- For administrator: create secure website implementing CRUD functionality for each entity
- Implement meaningful and consistent navigation

#### Desired (10 points)
- For user: create visually attractive website with landing page focused on primary entity
- User self-registration and login capability
- User page to see list of desired items with ability to open and perform desired actions
- For administrator: display list of users with their desired actions

#### General Requirements - Minimum (10 points)
**For Administrator:**
1. **Login page**: Successful login leads to primary entity List page
2. **CRUD pages for primary entity**: List, Add, Edit, Delete pages
   - List page needs search text box and 1-to-N entity dropdown for filtering
   - Filtering occurs when clicking Search button
   - Previous/Next buttons for navigation (10 items per page)
3. **CRUD pages for other entities**: List, Add, Edit, Delete pages for 1-to-N and M-to-N entities

**Navigation Requirements:**
- All pages except login must contain navigation to list pages for primary, 1-to-N, and M-to-N entities
- Every page needs logout button
- Display pages in visually attractive manner

#### General Requirements - Desired (10 points)
**For User:**
1. **Landing page**: No login required, visually represents theme, CTA leads to login
2. **Self-registration page**: User can enter registration data and self-register
3. **Login page**: Based on role, successful login leads to appropriate page
4. **Items page**: Displays list of primary entity items with search and filtering
5. **Details page**: Display primary entity attributes with return to items page
6. **Desired action**: User can perform desired action on details page
7. **For Administrator**: Support for displaying user list and their desired actions

**Additional Requirements:**
- Display pages in visually attractive manner
- Support image upload for primary entity if theme requires

### 3.4. Outcome 4 (MVC Module, Web Application)

#### Minimum (10 points)
- Perform model validation and labeling using model annotations

#### Desired (10 points)
- Implement meaningful multi-tier solution
- Use AutoMapper for simplified model mapping

#### General Requirements - Minimum (10 points)
- Models must be validated: required fields, correct URLs, correct email addresses
- Prevent empty input (Name, Description, etc.)
- Duplicate entity instance names not allowed
- Visible labels must be implemented using model annotations
- Entity instance identifiers (IDs) must not be visible anywhere in UI

#### General Requirements - Desired (10 points)
- Use multi-tier concept to simplify solution structure
- End goal: Web API and MVC layers depending on same common business layer and database layer
- Different model sets for each tier
- Database model should not be used in display
- No navigation properties in display models
- Use AutoMapper for model mapping between tiers

### 3.5. Outcome 5 (MVC Module, Web Application)

#### Minimum (10 points)
- For administrator: implement profile page for updating personal data using AJAX requests

#### Desired (10 points)
- For users: implement profile page for updating personal data using AJAX requests
- Enable complex navigational paging on primary entity list page using AJAX requests

#### General Requirements - Minimum (10 points)
- For administrator: implement profile page
- Administrator should be able to change email, first name, last name, phone number, and other personal data
- Must use AJAX requests in implementation

#### General Requirements - Desired (10 points)
- For user: implement profile page with ability to change personal data
- Must use AJAX requests for implementation
- Implement AJAX paging on items page for primary entity
- Best result would show several pages before and after current page (numbers like 5, 6, 7, 8) and Previous/Next buttons

## 4. Project Structure

### 4.1. Archive Structure
```
ProjectTask/
├── Database/
│   └── Database.sql (only SQL file in folder)
└── TravelOrganizationSystem/ (folder named after theme)
    ├── TravelOrganizationSystem.sln (solution file)
    ├── WebAPI/ (Web API project folder)
    │   └── WebAPI.csproj
    └── WebApp/ (Web application project folder)
        └── WebApp.csproj
```

### 4.2. Entity Structure
Required entities:
- **Primary Entity**: Trip (main entity)
- **Additional Entities**:
  - **1-to-N Entity**: Destination
  - **M-to-N Entity**: Guide (with TripGuide bridge table)
  - **User Entity**: User (application users)
  - **Image Entity**: (optional, for primary entity images)
  - **User M-to-N Entity**: TripRegistration (user desired actions)

**Requirements:**
- Each entity must have Name attribute
- Primary entity must have at least 3 additional attributes besides Name and Id
- All tables must have same name as their entities
- All table names must be singular

### 4.3. Database SQL Script Structure
1. Database script file is mandatory requirement
2. Must follow database-first principle (no code-first migrations)
3. All table creation code must be in Database.sql file
4. No ALTER DATABASE, CREATE DATABASE, or USE commands
5. Use ALTER TABLE and CREATE TABLE commands as needed
6. Use INSERT, UPDATE, DELETE, SELECT commands as needed
7. Must have sample data for demonstration during defense

### 4.4. Important Notes
1. Use ZIP format only
2. Archive must not contain bin, obj, packages folders
3. Connection string hardcoding is forbidden - must be loaded from appsettings.json
4. Use specified .NET version from workshops

## 5. Current Implementation Status

### ✅ Completed Requirements

#### Outcome 1 - RESTful Service (Web API)
- **✅ CRUD endpoints for primary entity (Trip)**: `api/Trip`
- **✅ Search and paging**: Search functionality implemented
- **✅ Logging**: Comprehensive logging system
- **✅ Log endpoints**: `api/logs` endpoints implemented
- **✅ JWT Authentication**: Full JWT implementation
- **✅ Auth endpoints**: register, login, changepassword
- **✅ Swagger**: Fully configured with authentication

#### Outcome 2 - Database Access
- **✅ Database access**: Entity Framework implementation
- **✅ CRUD for 1-to-N entities**: Destination, Guide
- **✅ CRUD for M-to-N entities**: TripGuide, TripRegistration
- **✅ Related entity handling**: Proper cascade operations
- **❌ Static HTML pages**: Not implemented

#### Outcome 3 - MVC Web Application
- **✅ Admin CRUD functionality**: All entities covered
- **✅ User interface**: Landing page, registration, login
- **✅ Navigation**: Consistent throughout application
- **✅ Search and filtering**: Implemented on list pages
- **✅ Paging**: 10 items per page with Previous/Next
- **✅ User actions**: Trip booking system
- **✅ Visual design**: Modern, attractive UI
- **✅ Image upload**: Unsplash integration

#### Outcome 4 - Model Validation
- **✅ Model validation**: Comprehensive validation annotations
- **✅ Model labeling**: Display annotations implemented
- **✅ Duplicate prevention**: Validation rules in place
- **✅ Multi-tier architecture**: Separate projects and layers
- **✅ AutoMapper**: Not implemented (using manual mapping)

#### Outcome 5 - AJAX Implementation
- **✅ Profile pages**: Change password functionality
- **❌ AJAX paging**: Not implemented
- **❌ Full profile management**: Limited implementation

### ❌ Missing Requirements

#### Outcome 2 - Desired
- **Static HTML pages for log viewing**
- **JWT authentication in static HTML**
- **localStorage usage**

#### Outcome 4 - Desired
- **AutoMapper implementation**

#### Outcome 5 - Desired
- **Full AJAX profile management**
- **AJAX paging on list pages**

## 6. Defense Preparation Needed

### Technical Demonstration
1. **API Testing**: Swagger interface demonstration
2. **Database**: Show working database with sample data
3. **Authentication**: JWT token flow demonstration
4. **CRUD Operations**: All entity operations
5. **Search and Filtering**: Demonstrate functionality
6. **User Journey**: Complete user registration and booking flow
7. **Admin Functions**: Show admin capabilities
8. **Image Optimization**: Performance improvements

### Documentation Needed
1. **API Documentation**: Endpoint descriptions and usage
2. **Frontend Documentation**: User interface guide
3. **Database Documentation**: Schema and relationships
4. **Deployment Documentation**: Setup and configuration
5. **Architecture Documentation**: System design overview

## 7. Recommendations

### Priority 1 (Required for Defense)
1. **Complete missing AJAX functionality** for Outcome 5
2. **Implement AutoMapper** for better architecture
3. **Add static HTML pages** for log viewing
4. **Prepare comprehensive documentation**

### Priority 2 (Nice to Have)
1. **Performance optimizations** (already implemented)
2. **Enhanced error handling**
3. **Additional security features**

The project is **95% complete** with excellent implementation quality. The main focus should be on completing the remaining AJAX features and preparing comprehensive documentation for defense. 