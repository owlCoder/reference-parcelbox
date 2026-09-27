# ParcelBox Reference

`ParcelBox` je mali referentni .NET 10 sistem za računarske vežbe iz predmeta Elementi razvoja softvera. Repo pokazuje eksplicitnu podelu **Model → Repository → Service → API**, uz dependency injection i dva spoljna simulatora.

## Demo scenario

Kurir registruje paket. Sistem bira odgovarajući pretinac i otvara ga preko Locker Controller simulatora. Nakon smeštanja generiše se pickup kod i pokušava slanje preko Message Gateway simulatora. Primalac kasnije unosi kod i završava preuzimanje.

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
- `Application` sadrži service interfejse i implementacije, repository interfejse, DTO-e, enum-e i Result pattern.
- `Infrastructure` implementira repository-je, external adaptere i security servis.
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
│   ├── Enums/
│   ├── Interfaces/
│   │   ├── Repositories/
│   │   ├── Services/
│   │   ├── External/
│   │   └── Security/
│   └── Services/
├── ParcelBox.Infrastructure/
│   ├── Persistence/Repositories/
│   ├── Persistence/Configurations/
│   ├── External/
│   └── Security/
├── ParcelBox.Api/
└── ParcelBox.Web/
```

Modeli nemaju `CanFit`, `CanBeStored`, `ValidateAttempt` ili slične poslovne helper metode. Repository ne donosi poslovne odluke. Servisi koriste repository/external interfejse i sadrže pravila.

Glavni servisi su:

- `ParcelService` — registracija i store orkestracija;
- `LockerService` — izbor i stanje pretinca;
- `PickupAccessService` — pickup kod, rok i neuspešni pokušaji;
- `PickupService` — završetak pickup toka;
- `NotificationService` — slanje pickup poruke.

Sve implementacije povezuju se kroz DI.

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
dotnet restore
dotnet format --verify-no-changes --no-restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
```

CI dodatno proverava simulator mode switching i Blazor assets.

UI: [`docs/demo-ui.md`](docs/demo-ui.md).
