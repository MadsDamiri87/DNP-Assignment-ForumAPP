# DNP Assignment - ForumApp
2026-09-05 (opdateret 2026-09-26)

Dette repository indeholder en forum-applikation til kurset **DNP - .NET Programmering**. Projektet tager udgangspunkt i et Reddit-lignende forum, hvor brugere kan oprette posts, skrive kommentarer og organisere posts i subforums.

Applikationen er bygget op over flere assignments: domænemodel og repositories, en Command Line Interface, filbaseret persistence og nu en Web API, som eksterne klienter kan bruge til at læse og ændre forummets data. Der er endnu ikke en Blazor frontend eller database.

## Domænemodel
Domænemodellen viser de centrale entities i systemet og relationerne mellem dem.

![DNP-ForumAPP-DomainModel V1.svg](docs/diagrams/DNP-ForumAPP-DomainModel%20V1.svg)

## Projektets formål
Formålet med denne assignment er at give den studerende (mig) et modulært .NET-projekt, samt får en forståelse af C# og forskellighederne mellem dét og Java. Derudover er hensigten også at vi får erfaringer med andre sprog og måder hvorpå de kan interagere med hinanden.

Strukturen gør det muligt at udskifte persistence-laget (in-memory, filer, senere Entity Framework Core med SQLite) uden at resten af applikationen behøver kende detaljerne. Det samme gælder mellem lagene i Web API'en: controllere kender kun service-interfaces, og services kender kun repository-interfaces. Dette støtter op om sidste semesters læring omkring dele af SOLID principperne.

## Løsningsstruktur
```
DNP-Assignment-ForumAPP/
├── DNP-Assignment-ForumAPP.sln
├── ApiContracts/                DTO'er, som deles mellem server og klienter
│   ├── UserDTOs/
│   ├── PostDTOs/
│   ├── CommentDTOs/
│   └── SubForumDTOs/
├── Server/
│   ├── Entities/                Domæneklasser (User, Post, Comment, SubForum) og IEntity
│   ├── RepositoryContracts/     Interfaces for data-adgang
│   ├── InMemoryRepositories/    Repositories baseret på lister + DataSeeder
│   ├── FileRepository/          Repositories der gemmer data som JSON-filer
│   ├── ServiceContracts/        Interfaces for services og ConflictException
│   ├── Services/                Forretningslogik, validering og mapping til DTO'er
│   ├── CLI/                     Command Line Interface (Assignment 2)
│   └── WebAPI/                  Controllere, Program.cs og WebAPI.http
└── Tests/                       Unit tests og integration tests (xUnit + Moq)
```

## Entities
Alle entities implementerer `IEntity`, som sikrer at de har et `Id` af typen `int`.

## Relationer
Projektet modellerer relationer med foreign keys i stedet for direkte associationer som `List<Comment>` eller `User Author`.

Det betyder blandt andet:

- En `Post` har en `UserId`, som peger på den bruger, der har skrevet opslaget.
- En `Post` har en `SubForumId`, som peger på det subforum, opslaget hører til. Feltet er valgfrit (`int?`), så en post kan eksistere uden subforum.
- En `Comment` har en `PostId`, som peger på det post, kommentaren hører til.
- En `Comment` har en `UserId`, som peger på den bruger, der har skrevet kommentaren.
- En `SubForum` har en `CreatorUserId`, som peger på brugeren, der oprettede subforummet.

Denne tilgang matcher den måde relationer senere kan gemmes i en relationel database.

## Repository-lag
Repository-laget abstraherer data-adgang for hver entity. Hvert repository-interface definerer de samme grundlæggende CRUD-operationer:

- `AddAsync`
- `UpdateAsync`
- `DeleteAsync`
- `GetSingleAsync`
- `GetManyAsync`

Der findes et repository-interface for hver entity:

- `IUserRepository`
- `ISubForumRepository`
- `IPostRepository`
- `ICommentRepository`

Et id bliver aldrig genbrugt, heller ikke selv om den nyeste entity er slettet. Det er vigtigt for en Web API, fordi id'er er en del af adresserne, klienter gemmer.

### In-memory repositories
De konkrete repository-implementationer ligger i projektet `InMemoryRepositories`. De gemmer data i en `List<T>` og er derfor kun midlertidige. Den fælles klasse `RepositoryBase<T>` indeholder den generelle CRUD-logik.

Dummy data findes i `DataSeeder.cs`, så der er brugere, subforums, posts og kommentarer at arbejde med fra starten.

### File repositories
Projektet `FileRepository` gemmer hver entity-type i sin egen JSON-fil i en `Data`-mappe (`users.json`, `posts.json`, `comments.json`, `subforums.json`). Den fælles klasse `FileRepositoryBase<T>` indeholder CRUD-logikken, så de fire konkrete repositories kun er en tynd klasse hver.

Ved siden af hver datafil ligger en lille `.nextid`-fil, som husker det næste ledige id. `Data`-mappen oprettes i den mappe, programmet startes fra, og den ligger i `.gitignore`, fordi den indeholder brugerdata.

## Web API
Web API'en giver adgang til de samme funktioner som CLI'en, men over HTTP. Den følger REST-principperne: hver entity er en ressource med en adresse, og HTTP-verberne bestemmer handlingen.

### Kør den
```
dotnet run --project Server/WebAPI --launch-profile http
```

API'et lytter på `http://localhost:5028`. I Development-miljøet findes Swagger UI på `http://localhost:5028/swagger`. Filen `Server/WebAPI/WebAPI.http` indeholder eksempler på kald til alle endpoints, som kan køres direkte fra Rider eller VS Code. API'et starter uden data.

### Statuskoder
| Kode | Bruges når |
|---|---|
| 200 OK | Læsning og opdatering lykkedes |
| 201 Created | En ressource blev oprettet (med `Location`-header) |
| 204 No Content | Sletning lykkedes |
| 400 Bad Request | Manglende eller ugyldigt input, en reference til en post/bruger/subforum der ikke findes, eller et brugernavn/en e-mail/et subforum-navn der allerede er i brug |
| 404 Not Found | Ressourcen findes ikke |
| 409 Conflict | Brugeren kan ikke slettes, fordi den stadig har posts, kommentarer eller subforums |

### PUT er en fuld erstatning
`PUT` erstatter hele ressourcen, så alle felter skal med i body. Det gælder også `subForumId` på en post: send et id, eller `null` for at fjerne posten fra subforummet. Udelades feltet, svarer API'et 400, så en post ikke tavst mister sit subforum.

### Ved sletning?
| Sletter man | Sker der |
|---|---|
| en post | Dens kommentarer slettes også |
| et subforum | Postene i subforummet beholdes, men får ingen subforum |
| en bruger | Afvises med 409, hvis brugeren har posts, kommentarer eller subforums |

Der er ikke tilføjet et transaktion-pattern over skriveoperationer. Går programmet ned midt i en sletning, kan en post være slettet uden at alle dens kommentarer er det.

### DTO'er
Entities sendes ikke direkte til klienterne. `ApiContracts` indeholder dedikerede DTO'er:

- `Create*Dto` og `Update*Dto` indeholder kun de felter, klienten selv skal udfylde. Id'et tildeles af serveren.
- `UserDto` indeholder ikke password.
- `PostDto`, `CommentDto` og `SubForumDto` indeholder forfatterens brugernavn, så klienten ikke selv skal slå det op.

### Lagdeling
Controllerne indeholder kun HTTP-delen: de tager imod request, kalder en service og oversætter resultatet til en statuskode. Services indeholder validering (påkrævede felter, unikke brugernavne og e-mails, at refererede entities findes) og mapper mellem entities og DTO'er. Alle afhængigheder registreres ét sted, i `Program.cs`, med dependency injection.

Opgaven beskriver forretningslogik i controlleren og et lag mere som en mulighed. Her er det lag med, så controllerne holdes tynde og servicerne kan unit-testes uden HTTP.

### Passwords
Passwords gemmes, som de bliver sendt ind, i feltet `PasswordHash`. Selvom variablen hedder paswordhasser er dette ikke implementeret. `UserDto` indeholder ikke password, så det sendes aldrig tilbage til klienterne. Det samme gælder i CLI'en.

## Tests
Testprojektet bruger xUnit og Moq. Testene er delt i:

- **Unit tests** af repositories, services, DTO'er, DataSeeder og CLI-views (med fake repositories eller Moq).
- **Integration tests** af file repositories, DataSeeder og hele CLI'en (menuer, views og repositories sammen).

## Teknologier

- C#
- .NET
- ASP.NET Core Web API med controllere
- Swagger (Swashbuckle) til dokumentation af API'et
- Class Library projects
- Asynkrone repository-metoder med `Task`
- `IQueryable<T>` til filtrering med LINQ
- JSON-filer som persistence
- xUnit og Moq

Projektfilerne bruger `net10.0` som target framework.

## Status
Implementeret:

- Domæneklasser for `User`, `SubForum`, `Post` og `Comment`
- Repository contracts, in-memory repositories og dummydata i `DataSeeder`
- Filbaserede repositories (JSON)
- Command Line Interface
- Web API med controllere, service-lag og DTO'er
- Filtrering på liste-endpoints og nested routes for kommentarer og posts
- Unit tests og integration tests

Ikke implementeret endnu:

- Blazor frontend
- Entity Framework Core
- SQLite database
- Hashing af passwords
- Autentificering og autorisation (login)
