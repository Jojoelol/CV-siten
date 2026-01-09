-- 1. Skapa 10 nya Identity-användare (om de inte redan finns)
-- Lösenord: Password123!
DECLARE @Pass NVARCHAR(MAX) = 'AQAAAAIAAYagAAAAEIsX3O0G7G5W6R8P9Q0K1L2M3N4O5P6Q7R8S9T0U1V2W3X4Y5Z6A7B8C9D0E1F2G3H==';

INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
SELECT 
    CAST(NEWID() AS NVARCHAR(450)), 
    'user'+CAST(i AS NVARCHAR)+'@test.se', 
    'USER'+CAST(i AS NVARCHAR)+'@TEST.SE', 
    'user'+CAST(i AS NVARCHAR)+'@test.se', 
    'USER'+CAST(i AS NVARCHAR)+'@TEST.SE', 
    1, @Pass, NEWID(), NEWID(), 0, 0, 1, 0
FROM (VALUES (3),(4),(5),(6),(7),(8),(9),(10),(11),(12)) AS T(i)
WHERE NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'user'+CAST(i AS NVARCHAR)+'@test.se');

-- 2. Skapa Person-profiler för dessa nya användare
INSERT INTO Persons (FirstName, LastName, JobTitle, Description, Skills, City, IsActive, IsPrivate, ViewCount, IdentityUserId, ImageUrl)
SELECT 
    CHOOSE(i-2, 'Erik', 'Sara', 'Mikael', 'Linda', 'Johan', 'Anna', 'David', 'Sofia', 'Marcus', 'Elena'),
    CHOOSE(i-2, 'Andersson', 'Lundin', 'Nilsson', 'Bergqvist', 'Holm', 'Sjöberg', 'Ek', 'Viktorsson', 'Björk', 'Popova'),
    CHOOSE(i-2, 'Systemutvecklare', 'Frontendare', 'Fullstack', 'Agil Coach', 'Data Scientist', 'UX Designer', 'DevOps', 'Apputvecklare', 'Security', 'Webbutvecklare'),
    'En passionerad utvecklare med fokus på kvalitet.',
    CHOOSE(i-2, 'C#, SQL', 'React, JS', 'C#, React', 'Scrum, Jira', 'Python, ML', 'Figma', 'Azure, Docker', 'Swift, Kotlin', 'Linux, Security', 'Node, Vue'),
    'Stockholm', 1, 0, 0, U.Id, 'Bild1.png'
FROM (VALUES (3),(4),(5),(6),(7),(8),(9),(10),(11),(12)) AS T(i)
JOIN AspNetUsers U ON U.Email = 'user'+CAST(i AS NVARCHAR)+'@test.se'
WHERE NOT EXISTS (SELECT 1 FROM Persons WHERE IdentityUserId = U.Id);

-- 3. SKAPA PROJEKT OCH TILLDELA GILTIGA ÄGARE (OwnerId)
-- Vi mappar så att varje projekt får en ägare som faktiskt finns i Persons-tabellen
INSERT INTO Projects (ProjectName, Description, Status, StartDate, Type, OwnerId)
SELECT 
    'Projekt ' + CHOOSE(i, 'Alpha', 'Beta', 'Gamma', 'Delta', 'Epsilon', 'Zeta', 'Eta', 'Theta', 'Iota', 'Kappa'),
    'Ett spännande projekt fokuserat på modern teknik.',
    CHOOSE((i % 3) + 1, 'Pågående', 'Avslutat', 'Planerat'),
    DATEADD(DAY, -i*10, GETDATE()),
    CHOOSE((i % 3) + 1, 'Webbutveckling', 'Systemutveckling', 'App-projekt'),
    P.Id -- Här hämtar vi ett riktigt Id från Persons-tabellen
FROM (VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10)) AS T(i)
CROSS APPLY (SELECT TOP 1 Id FROM Persons ORDER BY NEWID()) AS P -- Slumpar en ägare från Persons
WHERE NOT EXISTS (SELECT 1 FROM Projects WHERE ProjectName = 'Projekt ' + CHOOSE(i, 'Alpha', 'Beta', 'Gamma', 'Delta', 'Epsilon', 'Zeta', 'Eta', 'Theta', 'Iota', 'Kappa'));

-- 4. KOPPLA PERSONER TILL PROJEKT (Deltagare i PersonProjects)
INSERT INTO PersonProjects (PersonId, ProjectId, Role)
SELECT TOP 15
    P.Id,
    PR.Id,
    'Utvecklare'
FROM Persons P
CROSS JOIN Projects PR
WHERE NOT EXISTS (SELECT 1 FROM PersonProjects WHERE PersonId = P.Id AND ProjectId = PR.Id)
ORDER BY NEWID();

SELECT 'Exempeldata skapad! Alla projekt har nu en giltig OwnerId som pekar på en person.' AS Status;