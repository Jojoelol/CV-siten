# CV-siten - Projektinlämning

Välkommen till vårt repository för CV-siten! Här är instruktioner för att komma igång, skapa databasen och köra applikationen.

## Gruppmedlemmar
* Oscar Hallberg
* Lino De Luca
* Adam Pettersson
* Joel Jansson
* Joel Arrebäck 

## Förutsättningar
* .NET 8 SDK (eller den version ni kör)
* Visual Studio 2022 (rekommenderas)
* SQL Server (LocalDB eller Express)


### 1. Klona och öppna
Klona repot och öppna lösningsfilen (`.sln`) i Visual Studio.

### 2. Välj CV-siten som startprojekt
Högerklicka på `CV-siten` projektet i Solution Explorer och välj "Set as StartUp Project".

### 3. Konfigurera Databasen=

Tryck på tools -> Nuget Package Manager -> Package Manager Console 

och kör = 

1. Add-Migration InitialCreate -Project Cv_siten.Data -StartupProject CV-siten

2. Update-Database -Project Cv_siten.Data -StartupProject CV-siten

Våra seedusers har användarnamn/lösenord=

test@test.se, Test123!

testsson@test.se, Test123!







Så ska det fungera =) 


