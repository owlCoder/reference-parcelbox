# ParcelBox Reference

`ParcelBox` je mali referentni .NET 10 sistem za računarske vežbe iz predmeta Elementi razvoja softvera.
Domen je namerno odvojen od glavnog studentskog projekta. Repo služi kao primer uredne strukture, Clean Architecture granica, SOLID principa, Result pattern-a, testova i integracije sa simulatorima.

## Demo scenario

Kurir registruje paket. Sistem bira odgovarajući pretinac i otvara ga preko Locker Controller simulatora. Nakon smeštanja generiše se pickup kod i pokušava slanje preko Message Gateway simulatora. Primalac kasnije unosi kod, sistem proverava pravila i otvara isti pretinac.

Dva simulatora mogu namerno da se prebace u failure mode kako bi se na času pokazalo ponašanje sistema kada spoljna integracija nije dostupna.

## Arhitektura

```text
Web -> HTTP -> Api -> Application -> Domain
                   -> Infrastructure -> Application -> Domain
```

- `Domain` sadrži modele, enum-e, poslovna pravila i typed `Result`.
- `Application` orkestrira use-case-ove i definiše portove.
- `Infrastructure` implementira persistence, HTTP adaptere i security servis.
- `Api` je HTTP granica i composition root backend-a.
- `Web` je zaseban Blazor Web App bez project reference-a ka Domain/Application/Infrastructure slojevima.
- `simulators` sadrži dva nezavisna spoljna procesa.

Detalji: [`docs/architecture.md`](docs/architecture.md).

## Struktura repozitorijuma

```text
src/
├── ParcelBox.Domain/
├── ParcelBox.Application/
├── ParcelBox.Infrastructure/
├── ParcelBox.Api/
└── ParcelBox.Web/
    ├── Clients/
    ├── Components/
    │   ├── Dashboard/
    │   └── Pages/
    ├── Contracts/
    ├── ViewModels/
    └── wwwroot/

simulators/
├── ParcelBox.Simulators.LockerController/
└── ParcelBox.Simulators.MessageGateway/

tests/
├── ParcelBox.Domain.Tests/
└── ParcelBox.Application.Tests/

docs/
├── adr/
├── architecture.md
└── demo-ui.md
```

## Pokretanje

Potreban je .NET SDK 10. Docker je opcion.

### Lokalno

Windows PowerShell:

```powershell
./scripts/run-all.ps1
```

Linux/macOS:

```bash
./scripts/run-all.sh
```

Skripte build-uju solution, proveravaju portove i zatim pokreću sva četiri procesa.

| Servis | URL |
| --- | --- |
| ParcelBox Web | `http://localhost:5200` |
| ParcelBox API | `http://localhost:5100` |
| Locker Controller Simulator | `http://localhost:5101` |
| Message Gateway Simulator | `http://localhost:5102` |

Otvori `http://localhost:5200`.

### Docker Compose

```bash
docker compose up --build
```

Za gašenje:

```bash
docker compose down
```

## Šta pokazati na vežbama

1. `Domain` — entiteti, enum-i, invarijante i Result pattern.
2. `Application` — handler kao orkestracija use-case-a i dependency inversion preko portova.
3. `Infrastructure` — EF Core repository i `HttpClient` adapteri.
4. `Api` — HTTP contract i mapiranje typed grešaka u Problem Details.
5. `Web` — klijent koji komunicira isključivo preko HTTP-a.
6. `Simulator controls` — `Normal`, `Jammed` i `Unavailable` scenariji.
7. Testovi — domain pravila i application tokovi bez stvarnih spoljnih servisa.

UI je opisan u [`docs/demo-ui.md`](docs/demo-ui.md).

## Result pattern

Očekivani poslovni neuspeh nije exception i nije string identifikator:

```csharp
Result<Parcel, ParcelError> registerResult = Parcel.Register(...);
Result<ParcelError> storeResult = parcel.Store(...);
```

Domain i Application koriste enum-e za poznate ishode. Tek API sloj prevodi application grešku u HTTP status i tekstualni Problem Details odgovor.

## Provera koda

```bash
dotnet restore
dotnet format --verify-no-changes --no-restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
```

CI dodatno pokreće smoke testove za simulatore i Blazor static assets.

## Granice primera

Primer je namerno mali. Nema MediatR-a, message bus-a, generic repository-ja, outbox-a, posebnog Unit of Work framework-a ni dodatnih slojeva bez konkretne potrebe.

Cilj nije produkcioni paketomat, već čitljiv referentni kod u kome su odgovornosti i smer zavisnosti jasni.
