# ParcelBox Reference

`ParcelBox` je mali referentni .NET 10 sistem za računarske vežbe iz predmeta Elementi razvoja softvera. Domen je namerno nepovezan sa glavnim studentskim projektom. Repo pokazuje jasnu podelu odgovornosti: modeli, repository interfejsi i implementacije, application servisi, dependency injection, API granica, Web UI i dva spoljna simulatora.

## Demo scenario

Kurir registruje paket. Sistem bira odgovarajući pretinac i otvara ga preko Locker Controller simulatora. Nakon smeštanja generiše se pickup kod i pokušava slanje preko Message Gateway simulatora. Primalac kasnije unosi kod, sistem proverava pravila i otvara isti pretinac.

Simulatori mogu namerno da se prebace u failure mode kako bi se pokazalo ponašanje sistema kada spoljna integracija nije dostupna.

## Arhitektura

Osnovni tok zavisnosti je:

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

Na nivou projekata:

```text
Api --------> Application --------> Domain
 |                 ^
 |                 |
 +----> Infrastructure

Web ----HTTP----> Api
```

- `Domain` sadrži samo modele i domenske enum-e. Modeli nemaju helper metode, repository pristup, Result logiku ili orkestraciju use-case-a.
- `Application` sadrži business servise, njihove interfejse, repository interfejse, DTO-e, application enum-e i Result pattern.
- `Infrastructure` implementira repository-je, EF Core konfiguraciju, SQLite persistence, HTTP adaptere i security servis.
- `Api` prima HTTP zahtev i poziva application service interfejs. Ne koristi repository ili `DbContext` direktno.
- `Web` je zaseban Blazor Web App i komunicira preko HTTP-a.
- `simulators` su zasebni spoljni procesi i ne sadrže poslovna pravila ParcelBox-a.

Detalji: [`docs/architecture.md`](docs/architecture.md).

## Struktura

```text
src/
├── ParcelBox.Domain/
│   ├── Common/Enums/
│   ├── Parcels/
│   │   ├── Models/
│   │   └── Enums/
│   ├── Lockers/
│   │   ├── Models/
│   │   └── Enums/
│   └── Pickup/
│       ├── Models/
│       └── Enums/
│
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
│
├── ParcelBox.Infrastructure/
│   ├── Persistence/
│   │   ├── Configurations/
│   │   └── Repositories/
│   ├── External/
│   └── Security/
│
├── ParcelBox.Api/
│   ├── Contracts/
│   ├── Endpoints/
│   └── Extensions/
│
└── ParcelBox.Web/
    ├── Clients/
    ├── Components/
    ├── Contracts/
    ├── ViewModels/
    └── wwwroot/

simulators/
├── ParcelBox.Simulators.LockerController/
└── ParcelBox.Simulators.MessageGateway/

tests/
└── ParcelBox.Application.Tests/
```

## Odgovornosti slojeva

### Model

Model predstavlja stanje. Na primer `Compartment` ima `Size`, `Status` i `ParcelId`, ali nema `CanFit`, `Occupy`, `Release` ili slične pomoćne/business metode.

### Repository

Repository radi isključivo pristup podacima. Na primer `ICompartmentRepository` može da vrati dostupne pretince, ali ne odlučuje koji pretinac odgovara veličini paketa.

### Service

Service sadrži poslovno pravilo i koristi interfejse. `LockerService` bira najmanji kompatibilni pretinac, `ParcelService` vodi registraciju i smeštanje, a `PickupService` vodi pickup kod i završetak preuzimanja.

### Dependency injection

`Api` je composition root. Application service interfejsi se mapiraju na servise, a Infrastructure mapira repository i external interfejse na konkretne implementacije.

## Result pattern

Očekivani neuspeh iz application servisa nije exception:

```csharp
Result<ParcelDetails, ParcelOperationError> result =
    await parcelService.RegisterAsync(input, cancellationToken);
```

Poznati ishodi su enum vrednosti. API ih tek na HTTP granici prevodi u status kod i Problem Details tekst.

## Pokretanje

Potreban je .NET SDK 10.

Windows PowerShell:

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

Otvori `http://localhost:5200`.

Docker varijanta:

```bash
docker compose up --build
```

## Šta pokazati na vežbama

1. `Domain` — čisti modeli i enum-i, bez poslovnih helper metoda.
2. `Interfaces/Repositories` — ugovor za persistence.
3. `Infrastructure/Persistence/Repositories` — konkretna EF Core implementacija.
4. `Services` — mesto poslovnih pravila i orkestracije.
5. Dependency injection — interfejsi se povezuju sa implementacijama u composition root-u.
6. `Api` — transport bez poslovne logike.
7. Simulatori — failure scenariji kroz zamenjive external interfejse.
8. Testovi — service pravila testirana preko fake repository-ja i fake adaptera.

UI: [`docs/demo-ui.md`](docs/demo-ui.md).

## Provera koda

```bash
dotnet restore
dotnet format --verify-no-changes --no-restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
```

CI dodatno pokreće smoke testove za simulatore i Blazor static assets.

## Granice primera

Primer je namerno mali. Nema MediatR-a, message bus-a, generic repository-ja, outbox-a ili dodatnog framework sloja. Cilj je da student iz strukture projekta odmah vidi gde pripada model, repository, servis, adapter i HTTP endpoint.
