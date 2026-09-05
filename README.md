# DNP Assignment - ForumApp
2026-09-05

Dette repository indeholder første del af en forum-applikation til kurset **DNP - .NET Programmering**. Projektet tager udgangspunkt i et Reddit-lignende forum, hvor brugere kan oprette posts, skrive kommentarer og organisere posts i subforums.

I denne assignment er fokus på domænemodellen, entities og repository-laget. Applikationen har endnu ikke en Web API, database eller Blazor frontend. De dele forventes at blive tilføjet senere i assignment-serien.

## Domænemodel
Domænemodellen viser de centrale entities i systemet og relationerne mellem dem.

![DNP-ForumAPP-DomainModel V1.svg](docs/diagrams/DNP-ForumAPP-DomainModel%20V1.svg)

## Projektets formål
Formålet med denne assignment er at give den studerende (mig) et modulært .NET-projekt, samt får en forståelse af C# og forskellighederne mellem dét og Java. Derudover er hensigten også at vi får erfaringer med andre sprog og måder hvorpå de kan interagere med hinanden.

- **Entities** indeholder domæneklasserne.
- **RepositoryContracts** indeholder interfaces for data-adgang.
- **InMemoryRepositories** indeholder midlertidige repository-implementationer baseret på lister.

Denne struktur gør det muligt senere at udskifte persistence-laget, f.eks. fra in-memory storage til filbaseret storage eller Entity Framework Core med SQLite, uden at resten af applikationen behøver kende detaljerne. Dette støtter op om sidste semesters læring omkring dele af SOLID principperne. 

## Løsningsstruktur
```te
DNP-Assignment-ForumAPP/
├── DNP-Assignment-ForumAPP.sln
└── Server/
    ├── Entities/
    │   ├── Comment.cs
    │   ├── IEntity.cs
    │   ├── Post.cs
    │   ├── SubForum.cs
    │   └── User.cs
    ├── RepositoryContracts/
    │   ├── IRepository.cs
    │   ├── ICommentRepository.cs
    │   ├── IPostRepository.cs
    │   ├── ISubForumRepository.cs
    │   └── IUserRepository.cs
    └── InMemoryRepositories/
        ├── DataSeeder.cs
        ├── CommentInMemoryRepository.cs
        ├── PostInMemoryRepository.cs
        ├── RepositoryBase.cs
        ├── SubForumInMemoryRepository.cs
        └── UserInMemoryRepository.cs
```
## Entities
Alle entities implementerer `IEntity`, som sikrer at de har et `Id` af typen `int`.

## Relationer
Projektet modellerer relationer med foreign keys i stedet for direkte associationer som `List<Comment>` eller `User Author`.

Det betyder blandt andet:

- En `Post` har en `UserId`, som peger på den bruger, der har skrevet opslaget.
- En `Post` har en `SubForumId`, som peger på det subforum, opslaget hører til.
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

## In-memory repositories
De konkrete repository-implementationer ligger i projektet `InMemoryRepositories`. De gemmer data i en `List<T>` og er derfor kun midlertidige. Data bliver ikke gemt i filer eller database.

Den fælles klasse `RepositoryBase<T>` indeholder den generelle CRUD-logik:

- Nye entities får automatisk næste ledige `Id`.
- Eksisterende entities kan opdateres ud fra deres `Id`.
- Entities kan slettes ud fra deres `Id`.
- En enkelt entity kan hentes med `GetSingleAsync`.
- Flere entities kan hentes som `IQueryable<T>` med `GetManyAsync`.

Dummy data findes i DataSeeder.cs, så der er brugere, subforums, posts og kommentarer at arbejde med fra starten.

## Teknologier

- C#
- .NET
- Class Library projects
- Asynkrone repository-metoder med `Task`
- `IQueryable<T>` til senere filtrering med LINQ

Projektfilerne bruger `net10.0` som target framework.

## Status
Implementeret:

- Domæneklasser for `User`, `SubForum`, `Post` og `Comment`
- Fælles `IEntity` interface
- Repository contracts for alle entities
- In-memory repository implementationer
- Dummydata i klassen DataSeeder
- Domænemodel inkluderet i README

Ikke implementeret endnu:

- Command Line Interface
- Web API
- Blazor frontend
- Entity Framework Core
- SQLite database
- Autentificering og autorisation