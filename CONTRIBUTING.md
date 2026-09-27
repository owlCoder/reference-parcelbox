# Doprinos projektu ParcelBox

Ovaj repo je referentni primer za vežbe. Promene treba da ostanu male, čitljive i u skladu sa postojećim granicama.

## Pre slanja promene

```bash
dotnet restore
dotnet format --verify-no-changes --no-restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
```

Za izmene UI-ja ili integracija pokrenite i kompletan demo preko `scripts/run-all.ps1` ili `scripts/run-all.sh`.

## Obavezna pravila strukture

- jedna javna klasa, record, interface ili enum po fajlu;
- naziv fajla prati naziv glavnog tipa;
- `Domain` model predstavlja stanje i ne sadrži poslovne helper metode;
- u model ne dodavati `CanFit`, `Validate...`, `Mark...`, factory/use-case logiku, repository ili service zavisnost;
- repository interfejsi su u `Application/Interfaces/Repositories`;
- repository implementacije su u `Infrastructure/Persistence/Repositories`;
- repository radi data access, ne bira poslovno “najbolji” rezultat i ne sadrži poslovne tranzicije;
- poslovna pravila pripadaju `Application/Services`;
- javni ugovor servisa pripada `Application/Interfaces/Services`;
- servis zavisi od repository/external/security interfejsa, ne od EF Core klase ili konkretnog HTTP adaptera;
- `Api` poziva service interfejs i ne koristi `DbContext` ili repository direktno;
- `Infrastructure` implementira persistence, external i security interfejse;
- `Web` nema project reference prema backend slojevima i komunicira HTTP-om;
- simulator ne sadrži poslovna pravila ParcelBox-a;
- očekivani neuspeh application operacije vraća typed `Result`, ne exception-based kontrolni tok.

## Dependency injection

Sve konkretne implementacije vezuju se kroz DI:

```text
IParcelService -> ParcelService
IParcelRepository -> ParcelRepository
ILockerController -> LockerControllerClient
```

Ne praviti `new ParcelRepository(...)`, `new LockerControllerClient(...)` ili `new ParcelService(...)` u endpointima i produkcionom application kodu.

## Code hygiene

- ne uvoditi static helper klasu za poslovna pravila;
- ne duplirati mapiranje ili validaciju kroz više slojeva;
- ne stavljati više nepovezanih odgovornosti u isti fajl;
- koristiti standardni Visual Studio / `dotnet format` stil;
- komentarisati samo odluku koja nije očigledna iz naziva i strukture;
- ne uvoditi novu apstrakciju bez stvarne granice ili razloga za zamenu implementacije.

## Arhitektonske odluke

Promena smera zavisnosti, odgovornosti sloja ili osnovnog integration pattern-a zahteva ažuriranje `docs/architecture.md` i odgovarajućeg ADR-a.
