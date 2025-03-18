# Requirements, Design, and Analysis (RDA)

## 1. Uvod

### 1.1 Svrha dokumenta
Ovaj dokument pruža detaljnu analizu zahtjeva, dizajn i analizu sustava za naručivanje hrane "FoodOrderingSystem". Namijenjen je razvojnim inženjerima, projektnim menadžerima i ostalim dionicima projekta.

### 1.2 Opseg projekta
Projekt uključuje razvoj cjelovitog rješenja za naručivanje hrane koje se sastoji od RESTful API modula i MVC modula. RESTful API modul pruža back-end funkcionalnosti, a MVC modul pruža korisničko sučelje.

### 1.3 Reference
- Software Requirements Document (SRD)
- ASP.NET Core dokumentacija
- Entity Framework Core dokumentacija
- JWT autentifikacija specifikacija

## 2. Detaljni zahtjevi

### 2.1 Korisnički slučajevi (Use Cases)

#### 2.1.1 UC1: Registracija korisnika
- **Opis**: Korisnik se registrira u sustav
- **Akteri**: Neregistrirani korisnik
- **Preduvjeti**: Korisnik nije prijavljen
- **Glavni scenarij**:
  1. Korisnik otvara stranicu za registraciju
  2. Unosi korisničko ime, e-mail, lozinku i osobne podatke
  3. Sustav provjerava validnost podataka
  4. Sustav stvara novi korisnički račun
  5. Sustav obavještava korisnika o uspješnoj registraciji
- **Alternativni scenariji**:
  - Korisničko ime već postoji
  - E-mail adresa već postoji
  - Podaci nisu validni

#### 2.1.2 UC2: Prijava korisnika
- **Opis**: Korisnik se prijavljuje u sustav
- **Akteri**: Registrirani korisnik
- **Preduvjeti**: Korisnik je registriran
- **Glavni scenarij**:
  1. Korisnik otvara stranicu za prijavu
  2. Unosi korisničko ime i lozinku
  3. Sustav provjerava validnost podataka
  4. Sustav generira JWT token
  5. Korisnik je preusmjeren na početnu stranicu
- **Alternativni scenariji**:
  - Pogrešno korisničko ime ili lozinka

#### 2.1.3 UC3: Pregled kataloga hrane
- **Opis**: Korisnik pregledava dostupnu hranu
- **Akteri**: Korisnik (registrirani ili neregistrirani)
- **Preduvjeti**: Nema
- **Glavni scenarij**:
  1. Korisnik otvara stranicu s katalogom
  2. Sustav prikazuje dostupne proizvode
  3. Korisnik može filtrirati po kategorijama
  4. Korisnik može pretraživati po nazivu ili opisu
- **Alternativni scenariji**:
  - Nema dostupnih proizvoda

#### 2.1.4 UC4: Naručivanje hrane
- **Opis**: Korisnik naručuje hranu
- **Akteri**: Registrirani korisnik
- **Preduvjeti**: Korisnik je prijavljen
- **Glavni scenarij**:
  1. Korisnik dodaje hranu u košaricu
  2. Korisnik otvara košaricu
  3. Korisnik unosi adresu dostave
  4. Korisnik potvrđuje narudžbu
  5. Sustav kreira narudžbu i obavještava korisnika
- **Alternativni scenariji**:
  - Košarica je prazna
  - Adresa dostave nije validna

#### 2.1.5 UC5: Upravljanje proizvodima (admin)
- **Opis**: Administrator upravlja proizvodima
- **Akteri**: Administrator
- **Preduvjeti**: Administrator je prijavljen
- **Glavni scenarij**:
  1. Administrator otvara admin panel
  2. Pregledava, dodaje, mijenja ili briše proizvode
  3. Sustav ažurira bazu podataka
- **Alternativni scenariji**:
  - Proizvod je povezan s postojećim narudžbama (brisanje nije moguće)

### 2.2 Dijagram toka podataka (Data Flow Diagram)

#### 2.2.1 Kontekstni dijagram (Razina 0)
```
+-------------+          +------------------+          +-------------+
|             |  Zahtjev  |                  |  Zahtjev  |             |
|  Korisnik   | --------> | FoodOrderSystem | <-------- | Administrator|
|             |          |                  |          |             |
|             | <-------- |                  | -------> |             |
|             |  Odgovor  |                  |  Odgovor  |             |
+-------------+          +------------------+          +-------------+
                                  ^
                                  |
                                  v
                          +---------------+
                          |               |
                          |    Baza       |
                          |  podataka     |
                          |               |
                          +---------------+
```

#### 2.2.2 Dijagram razine 1
```
                          +-------------------+
                          |                   |
                          |  Autentifikacija  |
                          |                   |
                          +-------------------+
                            ^             ^
                            |             |
                            v             v
+-------------+          +------------------+          +-------------+
|             | <------> |                  | <------> |             |
|  Korisnik   |          |   Upravljanje    |          | Administrator|
|             | <------> |   katalogom      | <------> |             |
+-------------+          +------------------+          +-------------+
                            ^             ^
                            |             |
                            v             v
                          +-------------------+
                          |                   |
                          |   Upravljanje     |
                          |   narudžbama      |
                          |                   |
                          +-------------------+
                                  ^
                                  |
                                  v
                          +---------------+
                          |               |
                          |    Baza       |
                          |  podataka     |
                          |               |
                          +---------------+
```

### 2.3 ERD (Entity Relationship Diagram)

```
+------------------+       +------------------+       +------------------+
|       User       |       |      Order       |       |    OrderItem     |
+------------------+       +------------------+       +------------------+
| PK: Id           |       | PK: Id           |       | PK: Id           |
| Username         |       | FK: UserId       |       | FK: OrderId      |
| Email            |       | OrderDate        |       | FK: FoodId       |
| PasswordHash     |       | TotalAmount      |       | Quantity         |
| FirstName        |       | DeliveryAddress  |       | Price            |
| LastName         |       | Status           |       +------------------+
| PhoneNumber      |       +------------------+
| Address          |                |
| IsAdmin          |                |
+------------------+                |
        |                          |
        |                          |
        v                          v
+------------------+       +------------------+       +------------------+
|       Food       |       |   FoodCategory   |       |     Allergen     |
+------------------+       +------------------+       +------------------+
| PK: Id           |       | PK: Id           |       | PK: Id           |
| Name             |       | Name             |       | Name             |
| Description      |       | Description      |       | Description      |
| Price            |       +------------------+       +------------------+
| ImageUrl         |                |                         |
| PreparationTime  |                |                         |
| FK: CategoryId   |                |                         |
+------------------+                |                         |
        ^                          |                         |
        |                          |                         |
        +--------------------------|-------------------------+
                                   |
                                   v
                         +------------------+
                         |   FoodAllergen   |
                         +------------------+
                         | PK: FoodId       |
                         | PK: AllergenId   |
                         +------------------+

                         +------------------+
                         |       Log        |
                         +------------------+
                         | PK: Id           |
                         | Timestamp        |
                         | Level            |
                         | Message          |
                         +------------------+
```

## 3. Analiza sustava

### 3.1 Tehnološki stack
- **Back-end**: ASP.NET Core 8.0
- **ORM**: Entity Framework Core
- **Baza podataka**: Microsoft SQL Server
- **Autentifikacija**: JWT (JSON Web Tokens)
- **Front-end**: Razor Pages, JavaScript, Bootstrap
- **Dodatni alati**: AutoMapper, Swagger

### 3.2 Analiza performansi
- Implementacija straničenja za optimizaciju učitavanja velikih skupova podataka
- Indeksiranje ključnih polja u bazi podataka
- Asinkrone metode za bolje iskorištavanje resursa
- Cachiranje često korištenih podataka

### 3.3 Analiza sigurnosti
- JWT autentifikacija za sigurnu komunikaciju
- Hashiranje lozinki s ASP.NET Core Identity
- Validacija unosa na klijentskoj i serverskoj strani
- Zaštita od CSRF napada
- Autorizacija na razini kontrolera i akcija

### 3.4 Analiza skalabilnosti
- Višeslojna arhitektura omogućava lakše skaliranje
- Odvajanje servisa omogućava neovisno skaliranje komponenti
- Asinkrono izvršavanje omogućava veći broj istovremenih korisnika

## 4. Dizajn sustava

### 4.1 Arhitektura sustava

#### 4.1.1 Višeslojna arhitektura
Sustav je organiziran u sljedeće slojeve:

1. **Prezentacijski sloj**
   - WebAPI kontroleri
   - MVC kontroleri i pogledi

2. **Servisni sloj**
   - Implementacija poslovne logike
   - Autentifikacija i autorizacija
   - Mapiranje modela

3. **Podatkovni sloj**
   - Entity Framework Core kontekst
   - Repozitoriji
   - Modeli

4. **Infrastrukturni sloj**
   - Logging
   - Konfiguracija
   - Middleware

### 4.2 Komponente sustava

#### 4.2.1 WebAPI modul
- **Kontroleri**: AllergenController, AuthController, FoodCategoryController, FoodController, LogsController, OrderController
- **Servisi**: AllergenService, FoodCategoryService, FoodService, JwtService, LogService, OrderService, UserService
- **DTOs**: AllergenDTO, FoodCategoryDTO, FoodDTO, OrderDTO, UserDTO

#### 4.2.2 MVC modul (WebApp)
- **Kontroleri**: AccountController, AdminController, FoodController, HomeController, OrderController
- **Pogledi**: Razor stranice za svaki kontroler
- **ViewModels**: AccountViewModel, FoodViewModel, OrderViewModel

### 4.3 Dizajn baze podataka

#### 4.3.1 Tablice
- **User**: Podaci o korisnicima
- **Food**: Informacije o hrani (proizvodi)
- **FoodCategory**: Kategorije hrane
- **Allergen**: Alergeni
- **FoodAllergen**: Veza između hrane i alergena (many-to-many)
- **Order**: Narudžbe
- **OrderItem**: Stavke narudžbe
- **Log**: Zapisnik sustava

#### 4.3.2 Indeksi
- Primarni ključevi za sve tablice
- Strani ključevi za relacije
- Indeks na Name za pretraživanje
- Indeks na OrderDate za filtriranje

#### 4.3.3 Ograničenja
- Jedinstveno ograničenje na Username i Email u tablici User
- Jedinstveno ograničenje na Name u tablicama FoodCategory, Allergen i Food
- Ograničenja stranog ključa s odgovarajućim Delete Behavior (Cascade ili Restrict)

### 4.4 API dizajn

#### 4.4.1 Autentifikacija
- **POST /api/auth/register**: Registracija korisnika
- **POST /api/auth/login**: Prijava korisnika i generiranje JWT tokena
- **POST /api/auth/changepassword**: Promjena lozinke (zahtijeva autentifikaciju)

#### 4.4.2 Hrana
- **GET /api/food**: Dohvat svih proizvoda s podrškom za straničenje
- **GET /api/food/{id}**: Dohvat specifičnog proizvoda
- **POST /api/food**: Kreiranje novog proizvoda (admin)
- **PUT /api/food/{id}**: Ažuriranje proizvoda (admin)
- **DELETE /api/food/{id}**: Brisanje proizvoda (admin)
- **GET /api/food/search**: Pretraživanje proizvoda

#### 4.4.3 Kategorije
- **GET /api/foodcategory**: Dohvat svih kategorija
- **GET /api/foodcategory/{id}**: Dohvat specifične kategorije
- **POST /api/foodcategory**: Kreiranje nove kategorije (admin)
- **PUT /api/foodcategory/{id}**: Ažuriranje kategorije (admin)
- **DELETE /api/foodcategory/{id}**: Brisanje kategorije (admin)

#### 4.4.4 Alergeni
- **GET /api/allergen**: Dohvat svih alergena
- **GET /api/allergen/{id}**: Dohvat specifičnog alergena
- **POST /api/allergen**: Kreiranje novog alergena (admin)
- **PUT /api/allergen/{id}**: Ažuriranje alergena (admin)
- **DELETE /api/allergen/{id}**: Brisanje alergena (admin)

#### 4.4.5 Narudžbe
- **GET /api/order**: Dohvat narudžbi trenutnog korisnika
- **GET /api/order/{id}**: Dohvat specifične narudžbe
- **POST /api/order**: Kreiranje nove narudžbe
- **PUT /api/order/{id}/status**: Ažuriranje statusa narudžbe (admin)
- **DELETE /api/order/{id}**: Otkazivanje narudžbe
- **GET /api/order/all**: Dohvat svih narudžbi (admin)

#### 4.4.6 Zapisnici
- **GET /api/logs/get/{count}**: Dohvat zadnjih N zapisa (admin)
- **GET /api/logs/count**: Dohvat ukupnog broja zapisa (admin)

## 5. Implementacijski detalji

### 5.1 Autentifikacija
- **JWT**: Implementacija JWT tokena za autentifikaciju
- **Password Hashing**: Korištenje ASP.NET Core Identity za sigurno hashiranje lozinki
- **Role-based Authorization**: Implementacija autorizacije temeljene na ulogama

### 5.2 Logging
- Implementacija prilagođenog logging servisa
- Zapisivanje važnih operacija u bazu podataka
- Razine logiranja: Information, Warning, Error

### 5.3 Validacija
- Anotacije modela za automatsku validaciju
- Prilagođena validacija u servisnom sloju
- Klijentska validacija pomoću JavaScript/jQuery

### 5.4 Upravljanje narudžbama
- Kreiranje narudžbe s više stavki
- Praćenje statusa narudžbe
- Otkazivanje narudžbe

### 5.5 Pretraživanje i filtriranje
- Implementacija dinamičkog pretraživanja
- Filtriranje po kategorijama
- Straničenje rezultata

## 6. Testiranje

### 6.1 Strategija testiranja
- **Jedinični testovi**: Testiranje pojedinih komponenti
- **Integracijski testovi**: Testiranje interakcije između komponenti
- **UI testovi**: Testiranje korisničkog sučelja

### 6.2 Plan testiranja
- Testiranje API endpointa
- Testiranje autentifikacije i autorizacije
- Testiranje korisničkog sučelja
- Testiranje validacije

## 7. Implementacijski plan

### 7.1 Faze razvoja
1. **Postavljanje okruženja**: Kreiranje projekta, konfiguracija, postavljanje baze podataka
2. **WebAPI modul**: Implementacija API endpointa, servisa i autentifikacije
3. **MVC modul**: Implementacija korisničkog sučelja
4. **Testiranje**: Testiranje funkcionalnosti i ispravljanje grešaka
5. **Finalizacija**: Dokumentacija, optimizacija, deploy

### 7.2 Vremenski plan
- **Faza 1**: 1 tjedan
- **Faza 2**: 2 tjedna
- **Faza 3**: 2 tjedna
- **Faza 4**: 1 tjedan
- **Faza 5**: 1 tjedan

## 8. Zaključak

### 8.1 Rizici
- Vremenski rokovi
- Kompleksnost implementacije
- Integracija između modula

### 8.2 Preporuke
- Redovni code review
- Kontinuirana integracija
- Rana implementacija testova

### 8.3 Budući razvoj
- Mobilna aplikacija
- Proširenje funkcionalnosti
- Integracija s drugim sustavima