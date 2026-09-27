# ParcelBox Reference

`ParcelBox` je mali referentni .NET 10 sistem za računarske vežbe iz predmeta Elementi razvoja softvera. Repo pokazuje eksplicitnu podelu **Model → Repository → Service → API**, uz dependency injection i dva spoljna simulatora.

## Arhitektura

```text
Endpoint
   |
   v
Service interface
   |
   v
Application service
   |
   +----> Repository interface ----> Repository implementation ----> EF Core / SQLite
   |
   +----> External interface ------> HTTP adapter -----------------> Simulator
   |
   v
Domain model
```

- `Domain` sadrži state-only modele i domenske enum-e.
- `Application` sadrži servise, njihove interfejse, repository interfejse, DTO-e, enum-e i Result pattern.
- `Infrastructure` implementira persistence, external i security portove.
- `Api` je HTTP granica i composition root.
- `Web` je zaseban Blazor klijent koji koristi HTTP.

Detalji: [`docs/architecture.md`](docs/architecture.md).

## Ključna struktura

```text
src/
├── ParcelBox.Domain/
│   └── ... Models / Enums
├── ParcelBox.Application/
│   ├── Common/Results/
│   ├── DTOs/
│   │   ├── External/
│   │   ├── Lockers/
│   │   ├── Parcels/
│   │   └── Pickup/
│   ├── Enums/
│   ├── Interfaces/
│   │   ├── External/
│   │   ├── Persistence/
│   │   ├── Repositories/
│   │   ├── Security/
│   │   └── Services/
│   └── Services/
├── ParcelBox.Infrastructure/
│   ├── Persistence/Repositories/
│   ├── Persistence/Configurations/
│   ├── External/
│   └── Security/
├── ParcelBox.Api/
└── ParcelBox.Web/
```

`Application/Interfaces` sadrži **isključivo interfejse**. Integration error enum-i su u `Application/Enums`, transportni DTO-i su u `Application/DTOs`, a `IUnitOfWork` je persistence ugovor i zato je u `Interfaces/Persistence`.

Modeli nemaju `CanFit`, `CanBeStored`, `ValidateAttempt` ili slične poslovne helper metode. Repository ne donosi poslovne odluke. Servisi koriste repository/external/security interfejse i sadrže pravila.

Glavni servisi su `ParcelService`, `LockerService`, `PickupAccessService`, `PickupService` i `NotificationService`. Sve produkcione implementacije povezuju se kroz DI; endpoint ne kreira repository, adapter ili servis ručno.

## Pokretanje

Windows:

```powershell
./scripts/run-all.ps1
```

Linux/macOS:

```bash
./scripts/run-all.sh
```

| Servis | URL |
| --- | --- |
| ParcelBox Web | `http://localhost:5200` |
| ParcelBox API | `http://localhost:5100` |
| Locker Controller Simulator | `http://localhost:5101` |
| Message Gateway Simulator | `http://localhost:5102` |

Docker:

```bash
docker compose up --build
```

## Provera

```bash
./scripts/check-architecture.sh
dotnet restore
dotnet format --verify-no-changes --no-restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
```

CI prvo proverava arhitektonske granice, zatim format, build, testove, simulatore i Blazor assets.

UI: [`docs/demo-ui.md`](docs/demo-ui.md).
