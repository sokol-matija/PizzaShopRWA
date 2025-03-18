# Software Design Document (SDD)

## 1. Uvod

### 1.1 Svrha dokumenta
Ovaj dokument detaljno opisuje dizajn sustava za naručivanje hrane "FoodOrderingSystem". Dokument je namijenjen razvojnim inženjerima i tehničkim dionicima projekta te služi kao vodič za implementaciju.

### 1.2 Opseg dokumenta
Dokument obuhvaća arhitekturu sustava, dizajn komponenti, modele podataka, web servise, korisničko sučelje i ostale tehničke aspekte sustava.

### 1.3 Definicije, akronimi i skraćenice
- **API** - Application Programming Interface
- **DTO** - Data Transfer Object
- **JWT** - JSON Web Token
- **MVC** - Model-View-Controller
- **ORM** - Object-Relational Mapping
- **REST** - Representational State Transfer
- **CRUD** - Create, Read, Update, Delete

### 1.4 Reference
- Software Requirements Document (SRD)
- Requirements, Design, and Analysis (RDA)
- ASP.NET Core dokumentacija
- Entity Framework Core dokumentacija

## 2. Arhitektura sustava

### 2.1 Pregled arhitekture
Sustav je implementiran kao višeslojna aplikacija koja se sastoji od dva glavna modula:
1. **WebAPI** - RESTful servis za CRUD operacije nad entitetima
2. **WebApp** - MVC aplikacija za korisničko sučelje

### 2.2 Dijagram arhitekture

```
+-------------------+        +-------------------+
|                   |        |                   |
|     WebApp        |        |     WebAPI        |
|    (MVC modul)    |<------>|  (REST servis)    |
|                   |        |                   |
+-------------------+        +-------------------+
         |                            |
         |                            |
         v                            v
+-------------------+        +-------------------+
|                   |        |                   |
|  Presentation     |        |     Business      |
|      Layer        |        |      Layer        |
|                   |        |                   |
+-------------------+        +-------------------+
         |                            |
         |                            |
         v                            v
+-------------------+        +-------------------+
|                   |        |                   |
|   Data Access     |------->|   Database        |
|      Layer        |        |                   |
|                   |        |                   |
+-------------------+        +-------------------+
```

### 2.3 Slojevi aplikacije

#### 2.3.1 Prezentacijski sloj
- **WebAPI**: Kontroleri koji izlažu RESTful servise
- **WebApp**: MVC kontroleri i Razor pogledi

#### 2.3.2 Poslovni sloj
- Servisi koji implementiraju poslovnu logiku
- DTO (Data Transfer Object) modeli
- Validacija podataka
- Autentifikacija i autorizacija

#### 2.3.3 Sloj pristupa podacima
- Entity Framework Core kontekst
- Entity modeli
- Repozitoriji ili direktni pristup putem DbContext-a

#### 2.3.4 Infrastrukturni sloj
- Konfiguracija
- Logging
- Middleware komponente

## 3. Dizajn modula

### 3.1 WebAPI modul

#### 3.1.1 Kontroleri
WebAPI modul sadrži sljedeće kontrolere:

1. **AllergenController**
   - **Ruta**: api/allergen
   - **Metode**:
     - GET /api/allergen - Dohvat svih alergena
     - GET /api/allergen/{id} - Dohvat specifičnog alergena
     - POST /api/allergen - Kreiranje novog alergena (zahtijeva Admin ulogu)
     - PUT /api/allergen/{id} - Ažuriranje alergena (zahtijeva Admin ulogu)
     - DELETE /api/allergen/{id} - Brisanje alergena (zahtijeva Admin ulogu)

2. **AuthController**
   - **Ruta**: api/auth
   - **Metode**:
     - POST /api/auth/register - Registracija novog korisnika
     - POST /api/auth/login - Prijava korisnika i izdavanje JWT tokena
     - POST /api/auth/changepassword - Promjena lozinke (zahtijeva autentifikaciju)

3. **FoodCategoryController**
   - **Ruta**: api/foodcategory
   - **Metode**:
     - GET /api/foodcategory - Dohvat svih kategorija hrane
     - GET /api/foodcategory/{id} - Dohvat specifične kategorije
     - POST /api/foodcategory - Kreiranje nove kategorije (zahtijeva Admin ulogu)
     - PUT /api/foodcategory/{id} - Ažuriranje kategorije (zahtijeva Admin ulogu)
     - DELETE /api/foodcategory/{id} - Brisanje kategorije (zahtijeva Admin ulogu)

4. **FoodController**
   - **Ruta**: api/food
   - **Metode**:
     - GET /api/food - Dohvat svih proizvoda s paginacijom
     - GET /api/food/{id} - Dohvat specifičnog proizvoda
     - POST /api/food - Kreiranje novog proizvoda (zahtijeva Admin ulogu)
     - PUT /api/food/{id} - Ažuriranje proizvoda (zahtijeva Admin ulogu)
     - DELETE /api/food/{id} - Brisanje proizvoda (zahtijeva Admin ulogu)
     - GET /api/food/search - Pretraživanje proizvoda s paginacijom

5. **LogsController**
   - **Ruta**: api/logs
   - **Metode**:
     - GET /api/logs/get/{count} - Dohvat zadnjih N zapisa (zahtijeva Admin ulogu)
     - GET /api/logs/count - Dohvat ukupnog broja zapisa (zahtijeva Admin ulogu)

6. **OrderController**
   - **Ruta**: api/order
   - **Metode**:
     - GET /api/order - Dohvat narudžbi trenutnog korisnika
     - GET /api/order/{id} - Dohvat specifične narudžbe
     - POST /api/order - Kreiranje nove narudžbe
     - PUT /api/order/{id}/status - Ažuriranje statusa narudžbe (zahtijeva Admin ulogu)
     - DELETE /api/order/{id} - Otkazivanje narudžbe
     - GET /api/order/all - Dohvat svih narudžbi (zahtijeva Admin ulogu)

#### 3.1.2 Servisi
WebAPI modul koristi sljedeće servise:

1. **IAllergenService**
   - Implementacija: AllergenService
   - Metode:
     - Task<List<AllergenDTO>> GetAllAsync()
     - Task<AllergenDTO> GetByIdAsync(int id)
     - Task<AllergenDTO> CreateAsync(AllergenCreateDTO allergenDto)
     - Task<AllergenDTO> UpdateAsync(int id, AllergenUpdateDTO allergenDto)
     - Task<bool> DeleteAsync(int id)

2. **IFoodCategoryService**
   - Implementacija: FoodCategoryService
   - Metode:
     - Task<List<FoodCategoryDTO>> GetAllAsync()
     - Task<FoodCategoryDTO> GetByIdAsync(int id)
     - Task<FoodCategoryDTO> CreateAsync(FoodCategoryCreateDTO categoryDto)
     - Task<FoodCategoryDTO> UpdateAsync(int id, FoodCategoryUpdateDTO categoryDto)
     - Task<bool> DeleteAsync(int id)

3. **IFoodService**
   - Implementacija: FoodService
   - Metode:
     - Task<PagedResultDTO<FoodDTO>> GetAllAsync(int page, int count)
     - Task<FoodDTO> GetByIdAsync(int id)
     - Task<PagedResultDTO<FoodDTO>> SearchAsync(FoodSearchDTO searchParams)
     - Task<FoodDTO> CreateAsync(FoodCreateDTO foodDto)
     - Task<FoodDTO> UpdateAsync(int id, FoodUpdateDTO foodDto)
     - Task<bool> DeleteAsync(int id)

4. **IUserService**
   - Implementacija: UserService
   - Metode:
     - Task<User> AuthenticateAsync(string username, string password)
     - Task<User> RegisterAsync(RegisterDTO model)
     - Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
     - Task<User> GetByIdAsync(int userId)

5. **IJwtService**
   - Implementacija: JwtService
   - Metode:
     - string GenerateToken(User user)

6. **ILogService**
   - Implementacija: LogService
   - Metode:
     - Task LogInformationAsync(string message)
     - Task LogWarningAsync(string message)
     - Task LogErrorAsync(string message)
     - Task<List<LogDTO>> GetLogsAsync(int count)
     - Task<int> GetLogsCountAsync()

7. **IOrderService**
   - Implementacija: OrderService
   - Metode:
     - Task<List<OrderDTO>> GetUserOrdersAsync(int userId)
     - Task<OrderDTO> GetOrderByIdAsync(int orderId, int userId)
     - Task<OrderDTO> CreateOrderAsync(OrderCreateDTO orderDto, int userId)
     - Task<OrderDTO> UpdateOrderStatusAsync(int orderId, string status)
     - Task<bool> CancelOrderAsync(int orderId, int userId)
     - Task<PagedResultDTO<OrderDTO>> GetAllOrdersAsync(OrderFilterDTO filterDto)

#### 3.1.3 DTO modeli
WebAPI modul koristi sljedeće DTO (Data Transfer Object) modele:

1. **AllergenDTO, AllergenCreateDTO, AllergenUpdateDTO**
2. **FoodCategoryDTO, FoodCategoryCreateDTO, FoodCategoryUpdateDTO**
3. **FoodDTO, FoodCreateDTO, FoodUpdateDTO, FoodSearchDTO**
4. **OrderDTO, OrderItemDTO, OrderCreateDTO, OrderItemCreateDTO, OrderStatusUpdateDTO, OrderFilterDTO**
5. **RegisterDTO, LoginDTO, ChangePasswordDTO, TokenResponseDTO**
6. **LogDTO**
7. **PagedResultDTO<T>**

### 3.2 WebApp modul (MVC)

#### 3.2.1 Kontroleri
WebApp modul će sadržavati sljedeće kontrolere:

1. **AccountController**
   - Akcije:
     - Login - Prikaz i obrada forme za prijavu
     - Register - Prikaz i obrada forme za registraciju
     - Logout - Odjava korisnika
     - ChangePassword - Promjena lozinke
     - Profile - Upravljanje profilom korisnika

2. **HomeController**
   - Akcije:
     - Index - Početna (landing) stranica
     - About - O nama
     - Contact - Kontakt
     - Error - Prikaz greške

3. **FoodController**
   - Akcije:
     - Index - Prikaz kataloga hrane s filtriranjem i paginacijom
     - Details - Detalji pojedinačnog proizvoda
     - Add - Dodavanje u košaricu (AJAX)

4. **CartController**
   - Akcije:
     - Index - Pregled košarice
     - Add - Dodavanje proizvoda u košaricu
     - Remove - Uklanjanje proizvoda iz košarice
     - Update - Ažuriranje količine
     - Checkout - Naručivanje

5. **OrderController**
   - Akcije:
     - Index - Pregled narudžbi korisnika
     - Details - Detalji pojedinačne narudžbe
     - Cancel - Otkazivanje narudžbe

6. **AdminController**
   - Akcije:
     - Index - Admin dashboard
     - Users - Upravljanje korisnicima
     - Orders - Upravljanje narudžbama
     - Foods - Upravljanje proizvodima
     - Categories - Upravljanje kategorijama
     - Allergens - Upravljanje alergenima
     - Logs - Pregled zapisa sustava

#### 3.2.2 View modeli
WebApp modul će koristiti sljedeće View modele:

1. **AccountViewModels**
   - LoginViewModel
   - RegisterViewModel
   - ChangePasswordViewModel
   - ProfileViewModel

2. **FoodViewModels**
   - FoodListViewModel
   - FoodDetailsViewModel
   - FoodFilterViewModel

3. **CartViewModels**
   - CartViewModel
   - CartItemViewModel
   - CheckoutViewModel

4. **OrderViewModels**
   - OrderListViewModel
   - OrderDetailsViewModel

5. **AdminViewModels**
   - UserListViewModel
   - OrderManagementViewModel
   - FoodManagementViewModel
   - CategoryManagementViewModel
   - AllergenManagementViewModel
   - LogsViewModel

#### 3.2.3 Pogledi (Views)
WebApp modul će sadržavati sljedeće poglede:

1. **Shared**
   - _Layout.cshtml - Glavna layout stranica
   - _LoginPartial.cshtml - Parcijalni pogled za prijavu/odjavu
   - _ValidationScriptsPartial.cshtml - Parcijalni pogled za validaciju

2. **Account**
   - Login.cshtml
   - Register.cshtml
   - ChangePassword.cshtml
   - Profile.cshtml

3. **Home**
   - Index.cshtml - Landing stranica
   - About.cshtml
   - Contact.cshtml
   - Error.cshtml

4. **Food**
   - Index.cshtml - Katalog hrane
   - Details.cshtml - Detalji proizvoda
   - _FoodCard.cshtml - Parcijalni pogled za prikaz proizvoda

5. **Cart**
   - Index.cshtml - Pregled košarice
   - Checkout.cshtml - Završetak narudžbe

6. **Order**
   - Index.cshtml - Pregled narudžbi
   - Details.cshtml - Detalji narudžbe

7. **Admin**
   - Index.cshtml
   - Users.cshtml
   - Orders.cshtml
   - Foods.cshtml
   - Categories.cshtml
   - Allergens.cshtml
   - Logs.cshtml

## 4. Dizajn baze podataka

### 4.1 Shema baze podataka

#### 4.1.1 User
```sql
CREATE TABLE [User] (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    FirstName NVARCHAR(100) NULL,
    LastName NVARCHAR(100) NULL,
    PhoneNumber NVARCHAR(20) NULL,
    Address NVARCHAR(200) NULL,
    IsAdmin BIT NOT NULL DEFAULT 0,
    CONSTRAINT UQ_User_Username UNIQUE(Username),
    CONSTRAINT UQ_User_Email UNIQUE(Email)
);
```

#### 4.1.2 FoodCategory
```sql
CREATE TABLE [FoodCategory] (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    CONSTRAINT UQ_FoodCategory_Name UNIQUE(Name)
);
```

#### 4.1.3 Allergen
```sql
CREATE TABLE [Allergen] (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    CONSTRAINT UQ_Allergen_Name UNIQUE(Name)
);
```

#### 4.1.4 Food
```sql
CREATE TABLE [Food] (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    Price DECIMAL(18,2) NOT NULL,
    ImageUrl NVARCHAR(500) NULL,
    PreparationTime INT NULL,
    FoodCategoryId INT NOT NULL,
    CONSTRAINT UQ_Food_Name UNIQUE(Name),
    CONSTRAINT FK_Food_FoodCategory FOREIGN KEY (FoodCategoryId) 
        REFERENCES [FoodCategory](Id) ON DELETE RESTRICT
);
```

#### 4.1.5 FoodAllergen
```sql
CREATE TABLE [FoodAllergen] (
    FoodId INT NOT NULL,
    AllergenId INT NOT NULL,
    CONSTRAINT PK_FoodAllergen PRIMARY KEY (FoodId, AllergenId),
    CONSTRAINT FK_FoodAllergen_Food FOREIGN KEY (FoodId)
        REFERENCES [Food](Id) ON DELETE CASCADE,
    CONSTRAINT FK_FoodAllergen_Allergen FOREIGN KEY (AllergenId)
        REFERENCES [Allergen](Id) ON DELETE CASCADE
);
```

#### 4.1.6 Order
```sql
CREATE TABLE [Order] (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) NOT NULL,
    DeliveryAddress NVARCHAR(200) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    CONSTRAINT FK_Order_User FOREIGN KEY (UserId)
        REFERENCES [User](Id) ON DELETE RESTRICT
);
```

#### 4.1.7 OrderItem
```sql
CREATE TABLE [OrderItem] (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    FoodId INT NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_OrderItem_Order FOREIGN KEY (OrderId)
        REFERENCES [Order](Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItem_Food FOREIGN KEY (FoodId)
        REFERENCES [Food](Id) ON DELETE RESTRICT
);
```

#### 4.1.8 Log
```sql
CREATE TABLE [Log] (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Timestamp DATETIME NOT NULL DEFAULT GETDATE(),
    Level NVARCHAR(50) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL
);
```

### 4.2 Indeksi
```sql
-- Optimizacija za pretraživanje hrane po nazivu
CREATE INDEX IX_Food_Name ON [Food] (Name);

-- Optimizacija za filtriranje hrane po kategoriji
CREATE INDEX IX_Food_FoodCategoryId ON [Food] (FoodCategoryId);

-- Optimizacija za dohvat narudžbi korisnika
CREATE INDEX IX_Order_UserId ON [Order] (UserId);

-- Optimizacija za filtriranje narudžbi po statusu
CREATE INDEX IX_Order_Status ON [Order] (Status);

-- Optimizacija za sortiranje narudžbi po datumu
CREATE INDEX IX_Order_OrderDate ON [Order] (OrderDate DESC);

-- Optimizacija za sortiranje zapisa po vremenu
CREATE INDEX IX_Log_Timestamp ON [Log] (Timestamp DESC);
```

### 4.3 Inicijalni podaci
```sql
-- Inicijalni administrator
INSERT INTO [User] (Username, Email, PasswordHash, FirstName, LastName, IsAdmin)
VALUES ('admin', 'admin@example.com', 'AQAAAAEAACcQAAAAEF5Sxre42...', 'Admin', 'User', 1);

-- Kategorije hrane
INSERT INTO [FoodCategory] (Name, Description)
VALUES 
('Pizza', 'Various pizza options'),
('Pasta', 'Italian pasta dishes'),
('Salad', 'Fresh salads'),
('Dessert', 'Sweet treats'),
('Beverage', 'Drinks and refreshments');

-- Alergeni
INSERT INTO [Allergen] (Name, Description)
VALUES 
('Gluten', 'Wheat and other gluten-containing grains'),
('Lactose', 'Milk and dairy products'),
('Nuts', 'Tree nuts and peanuts'),
('Eggs', 'Chicken eggs'),
('Soy', 'Soybeans and soy products');

-- Inicijalni proizvodi
INSERT INTO [Food] (Name, Description, Price, ImageUrl, PreparationTime, FoodCategoryId)
VALUES 
('Margherita Pizza', 'Classic pizza with tomato sauce, mozzarella and basil', 9.99, '/images/margherita.jpg', 15, 1),
('Spaghetti Carbonara', 'Spaghetti with creamy sauce, eggs, cheese and bacon', 11.99, '/images/carbonara.jpg', 20, 2),
('Caesar Salad', 'Fresh romaine lettuce with Caesar dressing, croutons and parmesan', 8.99, '/images/caesar.jpg', 10, 3),
('Tiramisu', 'Classic Italian dessert with coffee, mascarpone and cocoa', 6.99, '/images/tiramisu.jpg', 0, 4),
('Coca-Cola', 'Classic refreshing cola drink', 2.49, '/images/coke.jpg', 0, 5);
```

## 5. Dizajn API-ja

### 5.1 Autentifikacija i autorizacija

#### 5.1.1 JWT autentifikacija
Sustav koristi JWT (JSON Web Token) za autentifikaciju. Proces je sljedeći:

1. Korisnik šalje zahtjev za prijavu s korisničkim imenom i lozinkom na `/api/auth/login`
2. Sustav provjerava korisničke podatke i ako su ispravni, generira JWT token
3. Token se vraća klijentu i trebao bi biti pohranjen (npr. u localStorage)
4. Za pristup zaštićenim rutama, klijent mora uključiti token u HTTP zaglavlje:
   ```
   Authorization: Bearer [token]
   ```

JWT token sadrži sljedeće informacije (claims):
- Id korisnika (nameidentifier)
- Korisničko ime (name)
- E-mail adresa (email)
- Uloga (role): "Admin" ili "User"

#### 5.1.2 Autorizacija
Za zaštitu krajnjih točaka koristi se atribut `[Authorize]`:

- `[Authorize]` - Zahtijeva autentifikaciju (bilo koji prijavljeni korisnik)
- `[Authorize(Roles = "Admin")]` - Zahtijeva korisnika s admin ulogom
- `[AllowAnonymous]` - Dopušta pristup bez autentifikacije

### 5.2 API endpointi

#### 5.2.1 Autentifikacija

**Registracija korisnika**
```
POST /api/auth/register
Content-Type: application/json

{
  "username": "string",
  "email": "string",
  "password": "string",
  "confirmPassword": "string",
  "firstName": "string",
  "lastName": "string",
  "phoneNumber": "string",
  "address": "string"
}
```

**Prijava korisnika**
```
POST /api/auth/login
Content-Type: application/json

{
  "username": "string",
  "password": "string"
}
```
Odgovor:
```json
{
  "token": "string",
  "username": "string",
  "isAdmin": boolean,
  "expiresAt": "string"
}
```

**Promjena lozinke**
```
POST /api/auth/changepassword
Authorization: Bearer [token]
Content-Type: application/json

{
  "currentPassword": "string",
  "newPassword": "string",
  "confirmNewPassword": "string"
}
```

#### 5.2.2 Proizvodi

**Dohvat svih proizvoda**
```
GET /api/food?page=1&count=10
```
Odgovor:
```json
{
  "items": [
    {
      "id": 0,
      "name": "string",
      "description": "string",
      "price": 0,
      "imageUrl": "string",
      "preparationTime": 0,
      "foodCategoryId": 0,
      "foodCategoryName": "string",
      "allergens": [
        {
          "id": 0,
          "name": "string",
          "description": "string"
        }
      ]
    }
  ],
  "totalCount": 0,
  "pageCount": 0,
  "currentPage": 0
}
```

**Dohvat proizvoda po ID-u**
```
GET /api/food/{id}
```

**Pretraživanje proizvoda**
```
GET /api/food/search?name=string&description=string&categoryId=0&page=1&count=10
```

**Kreiranje proizvoda (admin)**
```
POST /api/food
Authorization: Bearer [token]
Content-Type: application/json

{
  "name": "string",
  "description": "string",
  "price": 0,
  "imageUrl": "string",
  "preparationTime": 0,
  "foodCategoryId": 0,
  "allergenIds": [0]
}
```

**Ažuriranje proizvoda (admin)**
```
PUT /api/food/{id}
Authorization: Bearer [token]
Content-Type: application/json

{
  "name": "string",
  "description": "string",
  "price": 0,
  "imageUrl": "string",
  "preparationTime": 0,
  "foodCategoryId": 0,
  "allergenIds": [0]
}
```

**Brisanje proizvoda (admin)**
```
DELETE /api/food/{id}
Authorization: Bearer [token]
```

#### 5.2.3 Narudžbe

**Kreiranje narudžbe**
```
POST /api/order
Authorization: Bearer [token]
Content-Type: application/json

{
  "deliveryAddress": "string",
  "items": [
    {
      "foodId": 0,
      "quantity": 0
    }
  ]
}
```

**Dohvat narudžbi korisnika**
```
GET /api/order
Authorization: Bearer [token]
```

**Ažuriranje statusa narudžbe (admin)**
```
PUT /api/order/{id}/status
Authorization: Bearer [token]
Content-Type: application/json

{
  "status": "string" // "Pending", "Accepted", "In Progress", "Delivered", "Cancelled"
}
```

### 5.3 API odgovori i kodovi pogrešaka

#### 5.3.1 Uspješni odgovori
- **200 OK**: Zahtjev je uspješno obrađen (GET, PUT)
- **201 Created**: Resurs je uspješno kreiran (POST)
- **204 No Content**: Zahtjev je uspješno obrađen, nema sadržaja za povratak (DELETE)

#### 5.3.2 Klijentske pogreške
- **400 Bad Request**: Zahtjev nije validan (npr. neispravni podaci)
- **401 Unauthorized**: Autentifikacija je potrebna
- **403 Forbidden**: Korisnik nema dovoljne ovlasti
- **404 Not Found**: Traženi resurs nije pronađen

#### 5.3.3 Serverske pogreške
- **500 Internal Server Error**: Pogreška na serveru

#### 5.3.4 Format poruka o pogreškama
```json
{
  "error": "Kratki opis pogreške",
  "details": "Detaljnije objašnjenje ili niz validacijskih poruka"
}
```

## 6. Dizajn korisničkog sučelja

### 6.1 Wireframes

#### 6.1.1 Početna stranica (Home/Landing)
Početna stranica predstavlja vizualno privlačan ulaz u aplikaciju s jasnim pozivom na akciju.

- Elementi:
  - Navigacijska traka s linkovima za prijavu/registraciju
  - Veliki hero banner s atraktivnom slikom hrane
  - Poziv na akciju (CTA) "Naruči odmah"
  - Kratki prikaz popularnih kategorija
  - Sekcija s popularnim proizvodima
  - Podnožje s kontakt informacijama

#### 6.1.2 Stranica za prijavu/registraciju
Jednostavan obrazac za unos korisničkih podataka.

- Elementi za prijavu:
  - Polje za korisničko ime
  - Polje za lozinku
  - Gumb za prijavu
  - Link na registraciju

- Elementi za registraciju:
  - Polje za korisničko ime
  - Polje za e-mail
  - Polje za lozinku
  - Polje za potvrdu lozinke
  - Dodatna polja (ime, prezime, adresa, telefon)
  - Gumb za registraciju

#### 6.1.3 Katalog proizvoda
Stranica koja prikazuje dostupne proizvode s mogućnostima filtriranja i pretraživanja.

- Elementi:
  - Navigacijska traka s kategorijama
  - Pretraživačko polje
  - Filteri za sortiranje (cijena, popularnost)
  - Mreža proizvoda s prikazom slike, naziva, kratkog opisa i cijene
  - Paginacija (Prvi, Prethodni, 1, 2, 3, ..., Sljedeći, Zadnji)

#### 6.1.4 Detalji proizvoda
Stranica koja prikazuje detaljne informacije o odabranom proizvodu.

- Elementi:
  - Slika proizvoda
  - Naziv proizvoda
  - Cijena
  - Detaljan opis
  - Informacije o kategoriji
  - Lista alergena
  - Vrijeme pripreme
  - Kontrola količine
  - Gumb "Dodaj u košaricu"

#### 6.1.5 Košarica
Pregled proizvoda dodanih u košaricu i priprema za naručivanje.

- Elementi:
  - Lista odabranih proizvoda (slika, naziv, cijena, količina)
  - Kontrola za ažuriranje količine ili uklanjanje proizvoda
  - Prikaz podzbrojeva i ukupne cijene
  - Polje za unos adrese dostave
  - Gumb "Naruči"

#### 6.1.6 Pregled narudžbi
Pregled trenutnih i prošlih narudžbi korisnika.

- Elementi:
  - Lista narudžbi s datumom, statusom i ukupnom cijenom
  - Mogućnost otvaranja detalja narudžbe
  - Filteri za status narudžbe
  - Opcija otkazivanja narudžbe (ako je moguće)

#### 6.1.7 Admin panel
Sučelje za upravljanje svim aspektima sustava.

- Elementi:
  - Bočna navigacija s kategorijama (Korisnici, Narudžbe, Proizvodi, Kategorije, Alergeni, Zapisnici)
  - Tablični prikaz podataka
  - Gumbi za dodavanje, uređivanje i brisanje
  - Filteri i pretraživanje
  - Paginacija

### 6.2 Navigacija

#### 6.2.1 Glavna navigacija (Svi korisnici)
- Početna
- Kategorije hrane
- O nama
- Kontakt

#### 6.2.2 Korisnička navigacija (Prijavljeni korisnici)
- Profil
- Košarica
- Moje narudžbe
- Odjava

#### 6.2.3 Admin navigacija (Administratori)
- Dashboard
- Korisnici
- Narudžbe
- Proizvodi
- Kategorije
- Alergeni
- Zapisnici
- Odjava

### 6.3 Responzivni dizajn
Aplikacija će koristiti responzivni dizajn kako bi se prilagodila različitim veličinama ekrana:

- **Desktop**: Optimiziran za široke ekrane s prikazom više proizvoda u retku
- **Tablet**: Prilagođen za srednje ekrane, manje proizvoda u retku
- **Mobilni**: Optimiziran za male ekrane, vertikalni prikaz proizvoda

Implementacija će koristiti:
- Bootstrap framework
- CSS media queries
- Fleksibilne slike
- Prilagodljive navigacijske komponente

## 7. Implementacijski detalji

### 7.1 Konfiguracija projekta

#### 7.1.1 WebAPI projekt
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
  </ItemGroup>
</Project>
```

#### 7.1.2 WebApp projekt
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  </ItemGroup>
</Project>
```

### 7.2 Ključne implementacijske komponente

#### 7.2.1 JWT autentifikacija
JWT autentifikacija je implementirana pomoću JwtService klase koja generira token na temelju korisničkih podataka:

```csharp
public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        // Dohvat postavki iz konfiguracije
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]);
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expiryInMinutes = Convert.ToDouble(jwtSettings["ExpiryInMinutes"]);

        // Kreiranje claims-a za token
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
        };

        // Kreiranje JWT tokena
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryInMinutes),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            ),
            Issuer = issuer,
            Audience = audience
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
```

#### 7.2.2 HTTP klijent u WebApp projektu
Za komunikaciju s WebAPI-jem, WebApp će koristiti HttpClient s podrškom za JWT autentifikaciju:

```csharp
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    private async Task<HttpRequestMessage> CreateRequestMessage(HttpMethod method, string url, object content = null)
    {
        var request = new HttpRequestMessage(method, url);
        
        // Dodavanje JWT tokena iz session-a
        var token = _httpContextAccessor.HttpContext.Session.GetString("Token");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        
        // Dodavanje content-a ako postoji
        if (content != null)
        {
            request.Content = new StringContent(
                JsonConvert.SerializeObject(content),
                Encoding.UTF8,
                "application/json");
        }
        
        return request;
    }

    public async Task<T> GetAsync<T>(string url)
    {
        var request = await CreateRequestMessage(HttpMethod.Get, url);
        var response = await _httpClient.SendAsync(request);
        
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(json);
    }

    public async Task<T> PostAsync<T>(string url, object data)
    {
        var request = await CreateRequestMessage(HttpMethod.Post, url, data);
        var response = await _httpClient.SendAsync(request);
        
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(json);
    }

    // Slične metode za PUT i DELETE
}
```

#### 7.2.3 Implementacija košarice
Košarica će biti implementirana pomoću session storage-a u WebApp projektu:

```csharp
public class CartService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _cartKey = "Cart";

    public CartService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private List<CartItemViewModel> GetCart()
    {
        var session = _httpContextAccessor.HttpContext.Session;
        var cartJson = session.GetString(_cartKey);
        
        if (string.IsNullOrEmpty(cartJson))
            return new List<CartItemViewModel>();
        
        return JsonConvert.DeserializeObject<List<CartItemViewModel>>(cartJson);
    }

    private void SaveCart(List<CartItemViewModel> cart)
    {
        var session = _httpContextAccessor.HttpContext.Session;
        var cartJson = JsonConvert.SerializeObject(cart);
        session.SetString(_cartKey, cartJson);
    }

    public void AddItem(FoodDTO food, int quantity)
    {
        var cart = GetCart();
        var existingItem = cart.FirstOrDefault(x => x.FoodId == food.Id);
        
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            cart.Add(new CartItemViewModel
            {
                FoodId = food.Id,
                Name = food.Name,
                Price = food.Price,
                ImageUrl = food.ImageUrl,
                Quantity = quantity
            });
        }
        
        SaveCart(cart);
    }

    // Slične metode za UpdateItem, RemoveItem, GetItems, ClearCart
}
```

### 7.3 AJAX implementacija

#### 7.3.1 Dodavanje u košaricu (AJAX)
```javascript
function addToCart(foodId, quantity) {
    $.ajax({
        url: '/Cart/Add',
        type: 'POST',
        data: { foodId: foodId, quantity: quantity },
        success: function(result) {
            // Ažuriranje broja stavki u košarici u navigaciji
            $('#cart-count').text(result.totalItems);
            
            // Prikazivanje poruke o uspjehu
            showNotification('Proizvod je dodan u košaricu!', 'success');
        },
        error: function(xhr) {
            showNotification('Greška pri dodavanju u košaricu.', 'error');
        }
    });
}
```

#### 7.3.2 Pretraga proizvoda (AJAX)
```javascript
function searchProducts() {
    var searchTerm = $('#search-term').val();
    var categoryId = $('#category-filter').val();
    var page = 1;
    
    $.ajax({
        url: '/Food/Search',
        type: 'GET',
        data: { 
            searchTerm: searchTerm,
            categoryId: categoryId,
            page: page
        },
        success: function(result) {
            // Ažuriranje prikaza proizvoda
            $('#products-container').html(result);
        },
        error: function(xhr) {
            showNotification('Greška pri pretraživanju.', 'error');
        }
    });
}
```

#### 7.3.3 Ažuriranje profila (AJAX)
```javascript
$('#profile-form').submit(function(e) {
    e.preventDefault();
    
    var formData = $(this).serialize();
    
    $.ajax({
        url: '/Account/UpdateProfile',
        type: 'POST',
        data: formData,
        success: function(result) {
            showNotification('Profil je uspješno ažuriran!', 'success');
        },
        error: function(xhr) {
            // Prikaz validacijskih poruka
            if (xhr.status === 400) {
                var errors = JSON.parse(xhr.responseText);
                displayValidationErrors(errors);
            } else {
                showNotification('Greška pri ažuriranju profila.', 'error');
            }
        }
    });
});
```

## 8. Testiranje

### 8.1 Jedinični testovi
Jedinični testovi će se koristiti za testiranje poslovne logike i servisa:

```csharp
[TestClass]
public class FoodServiceTests
{
    private Mock<ApplicationDbContext> _mockContext;
    private Mock<ILogService> _mockLogService;
    private FoodService _foodService;
    
    [TestInitialize]
    public void Initialize()
    {
        _mockContext = new Mock<ApplicationDbContext>();
        _mockLogService = new Mock<ILogService>();
        _foodService = new FoodService(_mockContext.Object, _mockLogService.Object);
    }
    
    [TestMethod]
    public async Task GetByIdAsync_ExistingFood_ReturnsFoodDTO()
    {
        // Arrange
        var foodId = 1;
        var food = new Food { Id = foodId, Name = "Test Food", Price = 9.99m };
        _mockContext.Setup(c => c.Foods.FindAsync(foodId)).ReturnsAsync(food);
        
        // Act
        var result = await _foodService.GetByIdAsync(foodId);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(foodId, result.Id);
        Assert.AreEqual("Test Food", result.Name);
        Assert.AreEqual(9.99m, result.Price);
    }
    
    [TestMethod]
    public async Task GetByIdAsync_NonExistingFood_ReturnsNull()
    {
        // Arrange
        var foodId = 99;
        _mockContext.Setup(c => c.Foods.FindAsync(foodId)).ReturnsAsync((Food)null);
        
        // Act
        var result = await _foodService.GetByIdAsync(foodId);
        
        // Assert
        Assert.IsNull(result);
    }
    
    // Više testnih metoda...
}
```

### 8.2 Integracijski testovi
Integracijski testovi će testirati suradnju između komponenti:

```csharp
[TestClass]
public class FoodControllerIntegrationTests
{
    private TestServer _server;
    private HttpClient _client;
    
    [TestInitialize]
    public void Initialize()
    {
        // Postaviti test server s kompletnom konfiguracijom
        var webHostBuilder = new WebHostBuilder()
            .UseStartup<TestStartup>();
        
        _server = new TestServer(webHostBuilder);
        _client = _server.CreateClient();
    }
    
    [TestMethod]
    public async Task GetAll_ReturnsPagedResults()
    {
        // Act
        var response = await _client.GetAsync("/api/food?page=1&count=10");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<PagedResultDTO<FoodDTO>>(content);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Items.Count > 0);
        Assert.AreEqual(1, result.CurrentPage);
    }
    
    // Više testnih metoda...
}
```

### 8.3 UI testovi
Za testiranje korisničkog sučelja koristit će se Selenium WebDriver:

```csharp
[TestClass]
public class LoginUITests
{
    private IWebDriver _driver;
    
    [TestInitialize]
    public void Initialize()
    {
        _driver = new ChromeDriver();
        _driver.Manage().Window.Maximize();
    }
    
    [TestMethod]
    public void Login_ValidCredentials_RedirectsToHome()
    {
        // Arrange
        _driver.Navigate().GoToUrl("https://localhost:5001/Account/Login");
        
        var usernameInput = _driver.FindElement(By.Id("Username"));
        var passwordInput = _driver.FindElement(By.Id("Password"));
        var loginButton = _driver.FindElement(By.Id("login-button"));
        
        // Act
        usernameInput.SendKeys("testuser");
        passwordInput.SendKeys("Password123!");
        loginButton.Click();
        
        // Wait for redirect
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.Url.Contains("/Home"));
        
        // Assert
        Assert.IsTrue(_driver.Url.Contains("/Home"));
        
        // Check if user menu is visible
        var userMenu = _driver.FindElement(By.Id("user-menu"));
        Assert.IsTrue(userMenu.Displayed);
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        _driver.Quit();
    }
}
```

## 9. Deployment

### 9.1 Preduvjeti
- Windows Server 2019 ili noviji
- IIS 10 ili noviji
- SQL Server 2019 ili noviji
- .NET 8.0 Runtime
- SSL certifikat

### 9.2 Koraci za deployment

#### 9.2.1 Priprema baze podataka
1. Stvoriti bazu podataka na SQL Server instanci
2. Izvršiti SQL skriptu Database.sql za kreiranje strukture i inicijalnih podataka

#### 9.2.2 Priprema aplikacije
1. Postaviti ispravne connection string-ove u appsettings.json za oba projekta
2. Izgraditi oba projekta u Release modu
3. Objaviti WebAPI i WebApp projekte

#### 9.2.3 Konfiguracija IIS-a
1. Stvoriti dva aplikacijska pool-a (WebAPI i WebApp)
2. Kreirati dvije web aplikacije koje koriste te pool-ove
3. Podesiti SSL bindings
4. Konfigurirati autentifikaciju i autorizaciju

### 9.3 Postdeployment provjere
1. Provjera pristupa API-ju putem Swagger-a
2. Provjera prijavljivanja i registracije
3. Provjera CRUD operacija
4. Provjera naručivanja

## 10. Zaključak

### 10.1 Ograničenja sustava
- Sustav ne podržava plaćanje putem platnih kartica
- Nema integracije s vanjskim sustavima za dostavu
- Nema naprednog upravljanja zalihama
- Nema loyalty programa

### 10.2 Budući razvoj
- Implementacija plaćanja putem kartice
- Mobilna aplikacija za korisnike
- Integracija s dostavnim službama
- Upravljanje zalihama i obavijesti
- Loyalty program i popusti
- Napredna analitika prodaje

### 10.3 Rizici i njihovo umanjivanje
- **Sigurnost**: Redovito ažuriranje, praćenje OWASP smjernica, penetration testing
- **Performanse**: Optimizacija indeksa, caching, CDN
- **Dostupnost**: Load balancing, redundancija, cloud hosting
- **Kompleksnost**: Dokumentacija, code reviews, automatski testovi