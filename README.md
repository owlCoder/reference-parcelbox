# ParcelBox Reference

`ParcelBox` je mali .NET 10 referentni sistem za računarske vežbe iz predmeta Elementi razvoja softvera. Domen je namerno jednostavan i odvojen od studentskog projekta. Cilj repozitorijuma je da se na stvarnom kodu vidi jasan tok **Model → Repository → Service → API**, dependency injection, Result pattern i integracija sa spoljnim sistemima.

## Scenario

Kurir registruje paket, sistem bira odgovarajući pretinac i otvara ga preko Locker Controller simulatora. Nakon smeštanja kreira se pickup pristup i pokušava slanje koda preko Message Gateway simulatora. Primalac kasnije unosi kod i završava preuzimanje.

Simulatori imaju kontrolisane failure režime, pa se na vežbama može videti kako aplikacija reaguje na zaglavljen ili nedostupan locker i nedostupan message gateway.

## Projekti

```text
src/
├── ParcelBox.Domain/
├── ParcelBox.Application/
├── ParcelBox.Infrastructure/
├── ParcelBox.Api/
└── ParcelBox.Web/

simulators/
├── ParcelBox.Simulators.LockerController/
└── ParcelBox.Simulators.MessageGateway/

tests/
└── ParcelBox.Application.Tests/
```

Smer backend zavisnosti je:

```text
Api -------> Application -------> Domain
 |                 ^
 |                 |
 +-------> Infrastructure

Web -------- HTTP --------> Api
```

- `Domain` sadrži samo modele i domenske enum-e. Namespace prati folder: `*.Models` i `*.Enums`.
- `Application` sadrži service/repository ugovore, servise, DTO-e, application enum-e i Result pattern.
- `Infrastructure` implementira persistence, external i security portove.
- `Api` je HTTP granica i composition root.
- `Web` je zaseban Blazor Web App bez project reference ka backend slojevima.
- `simulators` predstavljaju spoljne sisteme i ne sadrže ParcelBox poslovna pravila.

Detalji su u [`docs/architecture.md`](docs/architecture.md), a odluka o granicama je zapisana u [`docs/adr/0001-clean-architecture.md`](docs/adr/0001-clean-architecture.md).

## Application struktura

```text
ParcelBox.Application/
├── Common/Results/
├── DTOs/
│   ├── External/
│   ├── Lockers/
│   ├── Notifications/
│   ├── Parcels/
│   └── Pickup/
├── Enums/
├── Interfaces/
│   ├── External/
│   ├── Persistence/
│   ├── Repositories/
│   ├── Security/
│   └── Services/
└── Services/
```

`Interfaces` sadrži isključivo interface tipove. Enum-i su u `Enums`, DTO/record ugovori u `DTOs`, a `IUnitOfWork` je persistence ugovor i nalazi se u `Interfaces/Persistence`.

Glavne odgovornosti servisa su razdvojene:

- `ParcelService` — registracija i čitanje paketa;
- `ParcelStorageService` — orkestracija smeštanja paketa;
- `CompartmentService` — read-only prikaz pretinaca;
- `LockerService` — izbor, otvaranje i oslobađanje pretinca;
- `PickupAccessService` — pickup kod, rok i neuspešni pokušaji;
- `PickupService` — završetak preuzimanja;
- `NotificationService` — priprema poruke i poziv Message Gateway-a.

Modeli su state-only. Repository radi data access. Poslovna pravila i orkestracija su u application servisima. Produkcione implementacije se povezuju kroz dependency injection.

## Pokretanje

Potreban je .NET SDK 10.

Windows:

```powershell
./scripts/run-all.ps1
```

Linux/macOS:

```bash
./scripts/run-all.sh
```

| Komponenta | URL |
| --- | --- |
| ParcelBox Web | `http://localhost:5200` |
| ParcelBox API | `http://localhost:5100` |
| Locker Controller Simulator | `http://localhost:5101` |
| Message Gateway Simulator | `http://localhost:5102` |

Docker:

```bash
docker compose up --build
```

## Provera pre commita

```bash
./scripts/check-architecture.sh
dotnet restore
dotnet format --verify-no-changes --no-restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
```

CI proverava arhitektonske granice, format, build, testove, simulator mode switching i Blazor static assets.

UI demonstracija je opisana u [`docs/demo-ui.md`](docs/demo-ui.md).
