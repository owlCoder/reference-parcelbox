# Doprinos projektu ParcelBox

Ovaj repo je referentni primer za vežbe. Promene treba da ostanu male, čitljive i u skladu sa postojećim granicama.

## Pre slanja promene

```bash
./scripts/check-architecture.sh
dotnet restore
dotnet format --verify-no-changes --no-restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
```

Za izmene UI-ja ili integracija pokrenite i kompletan demo preko `scripts/run-all.ps1` ili `scripts/run-all.sh`.

## Obavezna pravila strukture

- jedna javna klasa, record, interface ili enum po fajlu;
- naziv fajla prati naziv glavnog tipa;
- `Application/Interfaces` sadrži samo interface tipove — bez enum-a, record-a, DTO-a ili concrete class-a;
- application i integration enum-i pripadaju `Application/Enums`;
- DTO/record ugovori pripadaju `Application/DTOs`;
- `IUnitOfWork` je persistence ugovor i pripada `Application/Interfaces/Persistence`, ne `Repositories`;
- repository interfejsi su u `Application/Interfaces/Repositories` i nazivaju se `I*Repository`;
- repository implementacije su u `Infrastructure/Persistence/Repositories`;
- `Domain` model predstavlja stanje i ne sadrži poslovne helper metode;
- u model ne dodavati `CanFit`, `Validate...`, `Mark...`, factory/use-case logiku, repository ili service zavisnost;
- repository radi data access, ne bira poslovno “najbolji” rezultat i ne sadrži poslovne tranzicije;
- poslovna pravila pripadaju `Application/Services`;
- javni ugovor servisa pripada `Application/Interfaces/Services`;
- servis zavisi od repository/external/persistence/security interfejsa, ne od EF Core klase ili konkretnog HTTP adaptera;
- `Api` poziva service interfejs i ne koristi `DbContext` ili repository direktno;
- `Infrastructure` implementira persistence, external i security interfejse;
- `Web` nema project reference prema backend slojevima i komunicira HTTP-om;
- simulator ne sadrži poslovna pravila ParcelBox-a;
- očekivani neuspeh application operacije vraća typed `Result`, ne exception-based kontrolni tok.

## Dependency injection

Sve produkcione konkretne implementacije vezuju se kroz DI. Endpoint i application servis ne smeju da rade `new ParcelRepository(...)`, `new LockerControllerClient(...)` ili da direktno konstruišu production dependency.

## Code hygiene

- ne uvoditi static helper klasu za poslovna pravila;
- ne duplirati mapiranje ili validaciju kroz više slojeva;
- ne stavljati više nepovezanih odgovornosti u isti fajl;
- koristiti standardni Visual Studio / `dotnet format` stil;
- komentarisati samo odluku koja nije očigledna iz naziva i strukture;
- ne uvoditi novu apstrakciju bez stvarne granice ili razloga za zamenu implementacije.

CI izvršava `check-architecture.sh` i `ArchitectureTests`, tako da pogrešno smešten enum/DTO, persistence zavisnost u endpointu ili concrete Infrastructure zavisnost u Application sloju treba da padnu odmah.

## Arhitektonske odluke

Promena smera zavisnosti, odgovornosti sloja ili osnovnog integration pattern-a zahteva ažuriranje `docs/architecture.md` i odgovarajućeg ADR-a.
