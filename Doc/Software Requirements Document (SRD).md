# Software Requirements Document (SRD)

## 1. Uvod

### 1.1 Svrha
Ovaj dokument definira zahtjeve za sustav naručivanja hrane "FoodOrderingSystem", koji omogućava korisnicima naručivanje hrane online te administratorima upravljanje ponudom, kategorijama, alergenima i narudžbama.

### 1.2 Opseg
Projekt "FoodOrderingSystem" sastoji se od dva glavna modula:
1. **RESTful API modul (WebAPI)** - Omogućava CRUD operacije nad entitetima, pretraživanje, straničenje i autentifikaciju
2. **MVC modul (WebApp)** - Pruža korisničko sučelje za krajnje korisnike i administratore

### 1.3 Definicije i akronimi
- **CRUD** - Create, Read, Update, Delete (Kreiranje, Čitanje, Ažuriranje, Brisanje)
- **API** - Application Programming Interface
- **JWT** - JSON Web Token
- **MVC** - Model-View-Controller
- **SPA** - Single Page Application

## 2. Općeniti opis

### 2.1 Pregled proizvoda
FoodOrderingSystem je web aplikacija koja omogućava korisnicima pregled ponude hrane, filtriranje po kategorijama, pregled detalja proizvoda, dodavanje u košaricu i naručivanje. Administratori mogu upravljati proizvodima, kategorijama, alergenima i pratiti narudžbe.

### 2.2 Funkcije proizvoda
- Registracija i prijava korisnika
- Pregled, pretraživanje i filtriranje proizvoda
- Detalji proizvoda s opisom, cijenom, i alergenima
- Dodavanje proizvoda u košaricu
- Naručivanje hrane s dostavom
- Administratorsko sučelje za upravljanje entitetima
- Praćenje narudžbi i njihovih statusa

### 2.3 Karakteristike korisnika
Sustav ima dva tipa korisnika:
1. **Kupci** - Krajnji korisnici koji pregledavaju i naručuju hranu
2. **Administratori** - Osoblje koje upravlja sustavom

### 2.4 Ograničenja
- Sustav mora koristiti ASP.NET Core
- Mora podržavati JWT autentifikaciju
- Mora podržavati SQL Server bazu podataka
- Mora biti responsivan i podržavati različite veličine ekrana

## 3. Specifični zahtjevi

### 3.1 Vanjska sučelja

#### 3.1.1 Korisničko sučelje
- **Landing stranica** - Vizualno privlačna stranica koja predstavlja restoran
- **Stranica za prijavu/registraciju** - Omogućava korisnicima registraciju i prijavu
- **Katalog hrane** - Prikaz dostupnih jela s mogućnošću filtriranja i pretraživanja
- **Detalji proizvoda** - Prikaz detaljnih informacija o odabranom jelu
- **Košarica** - Prikaz dodanih proizvoda prije naručivanja
- **Administratorsko sučelje** - Za upravljanje proizvodima, kategorijama i narudžbama

#### 3.1.2 Softverska sučelja
- RESTful API koji koristi JSON za razmjenu podataka
- JWT autentifikacija za sigurnu komunikaciju
- SQL Server baza podataka za pohranu podataka

### 3.2 Funkcionalni zahtjevi

#### 3.2.1 WebAPI modul

##### RESTful API
1. Implementirati CRUD operacije za sve entitete:
   - Food (hrana)
   - FoodCategory (kategorija hrane)
   - Allergen (alergeni)
   - Order (narudžba)
   - User (korisnik)

2. Podržati pretraživanje i straničenje:
   - Pretraživanje hrane po nazivu i opisu
   - Filtriranje po kategorijama
   - Straničenje rezultata (10 po stranici)

3. Implementirati zapisnik (logging):
   - Zapisivanje svih CRUD operacija
   - Pristup zapisima kroz API

4. Implementirati autentifikaciju:
   - Registracija korisnika
   - Prijava korisnika i generiranje JWT tokena
   - Promjena lozinke
   - Pristup zaštićenim rutama

#### 3.2.2 MVC modul

##### Administratorsko sučelje
1. Prijava administratora
2. CRUD operacije za sve entitete:
   - Upravljanje hranom
   - Upravljanje kategorijama
   - Upravljanje alergenima
   - Pregled narudžbi i promjena statusa

##### Korisničko sučelje
1. Landing stranica
2. Registracija i prijava korisnika
3. Pregled kataloga hrane s filtracijom
4. Detalji proizvoda
5. Košarica i naručivanje
6. Pregled narudžbi korisnika
7. Profilna stranica

### 3.3 Nefunkcionalni zahtjevi

#### 3.3.1 Performanse
- Brzo učitavanje stranica (ispod 3 sekunde)
- Optimizirani API pozivi s podrškom za straničenje

#### 3.3.2 Sigurnost
- JWT autentifikacija
- Enkripcija lozinki
- Zaštita od SQL injection i XSS napada
- HTTPS protokol

#### 3.3.3 Održivost
- Višeslojna arhitektura
- Jasna separacija odgovornosti
- Dokumentirani API

#### 3.3.4 Dostupnost
- Sustav mora biti dostupan 99.9% vremena

## 4. Specifikacija podataka

### 4.1 Entiteti

#### 4.1.1 Primarni entitet - Food
- Id: int (PK)
- Name: string (obavezno)
- Description: string
- Price: decimal (obavezno)
- ImageUrl: string
- PreparationTime: int
- FoodCategoryId: int (FK)

#### 4.1.2 1-na-N entitet - FoodCategory
- Id: int (PK)
- Name: string (obavezno)
- Description: string

#### 4.1.3 M-na-N entitet - Allergen
- Id: int (PK)
- Name: string (obavezno)
- Description: string

#### 4.1.4 Bridge tablica - FoodAllergen
- FoodId: int (PK, FK)
- AllergenId: int (PK, FK)

#### 4.1.5 User
- Id: int (PK)
- Username: string (obavezno)
- Email: string (obavezno)
- PasswordHash: string (obavezno)
- FirstName: string
- LastName: string
- PhoneNumber: string
- Address: string
- IsAdmin: bool

#### 4.1.6 Order
- Id: int (PK)
- UserId: int (FK)
- OrderDate: DateTime (obavezno)
- TotalAmount: decimal (obavezno)
- DeliveryAddress: string (obavezno)
- Status: string (obavezno)

#### 4.1.7 OrderItem
- Id: int (PK)
- OrderId: int (FK)
- FoodId: int (FK)
- Quantity: int (obavezno)
- Price: decimal (obavezno)

#### 4.1.8 Log
- Id: int (PK)
- Timestamp: DateTime (obavezno)
- Level: string (obavezno)
- Message: string (obavezno)

## 5. Verifikacija

### 5.1 Testiranje
- Jedinični testovi za poslovnu logiku
- Integracijski testovi za API
- Testiranje korisničkog sučelja

### 5.2 Validacija
- Validacija unosa na klijentskoj i serverskoj strani
- Provjera ispravnosti podataka prije pohrane

## 6. Dodatak

### 6.1 Pretpostavke i ovisnosti
- Dostupnost SQL Server baze podataka
- .NET Core razvojno okruženje
- Dostupnost web poslužitelja

### 6.2 Ograničenja
- Vremenski rok za izvršenje projekta
- Ograničenja u broju entiteta

### 6.3 Budući zahtjevi
- Integracija s platformama za dostavu
- Mobilna aplikacija
- Sustav za rezervacije