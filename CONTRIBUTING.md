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

Za UI ili integracione izmene pokrenite i kompletan demo preko `scripts/run-all.ps1` ili `scripts/run-all.sh`.

## Pravila strukture

- jedna javna klasa, record, interface ili enum po fajlu;
- naziv fajla prati naziv glavnog tipa;
- folder i namespace moraju da budu usklađeni;
- `Domain/*/Models` sadrži state-only modele, a `Domain/*/Enums` domenske enum-e;
- model nema repository/service zavisnost, Result tip ili business helper metode (`CanFit`, `Validate...`, `Mark...` i slično);
- `Application/Interfaces` sadrži samo interface tipove;
- application/integration enum-i su u `Application/Enums`;
- DTO/record ugovori su u `Application/DTOs`;
- repository interfejsi su u `Application/Interfaces/Repositories` i nazivaju se `I*Repository`;
- `IUnitOfWork` pripada `Application/Interfaces/Persistence`;
- repository implementacije su u `Infrastructure/Persistence/Repositories`;
- repository radi data access, ne bira poslovno “najbolji” rezultat i ne radi poslovne tranzicije;
- poslovna pravila i orkestracija pripadaju `Application/Services`;
- service interfejs treba da bude uzak; ne spajati nepovezan query i command API samo zato što rade nad sličnim podacima;
- servis drugom servisu ne prosleđuje ceo domain model ako je dovoljan mali DTO/ugovor;
- application servis zavisi od repository/external/persistence/security interfejsa, ne od EF Core klase ili konkretnog HTTP adaptera;
- `Api` poziva service interfejs i ne koristi `DbContext` ili repository direktno;
- `Infrastructure` implementira persistence, external i security portove;
- `Web` nema project reference prema backend slojevima i komunicira HTTP-om;
- simulator ne sadrži poslovna pravila ParcelBox-a;
- očekivani neuspeh vraća typed `Result`, ne exception-based kontrolni tok.

## Dependency injection

Produkcione konkretne implementacije vezuju se kroz DI. Endpoint i application servis ne konstruišu production repository, adapter ili servis ručno.

## Code hygiene

- ne uvoditi static helper klasu za poslovna pravila;
- ne duplirati mapiranje ili validaciju kroz više slojeva;
- ne držati više nepovezanih odgovornosti u istom servisu ili fajlu;
- koristiti standardni Visual Studio / `dotnet format` stil;
- koristiti opisna imena promenljivih umesto generičkih `x`, `item`, `data` kada kontekst nije očigledan;
- komentarisati samo odluku koja nije očigledna iz naziva i strukture;
- ne uvoditi novu apstrakciju bez stvarne granice ili razloga za zamenu implementacije.

CI izvršava `check-architecture.sh` i `ArchitectureTests`, pa pogrešno smešten tip ili zabranjena zavisnost treba da padnu pre merge-a.

## Arhitektonske odluke

Promena smera zavisnosti, odgovornosti sloja ili osnovnog integration pattern-a zahteva ažuriranje `docs/architecture.md` i odgovarajućeg ADR-a.
