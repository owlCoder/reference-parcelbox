# Arhitektura

ParcelBox je mali modularni sistem organizovan po Clean Architecture principima. Cilj je da smer zavisnosti i odgovornosti budu vidljivi bez dodatnih framework slojeva.

## Pravac zavisnosti

```text
Web -> HTTP -> Api -> Application -> Domain
                   -> Infrastructure -> Application -> Domain
```

`Domain` nema zavisnosti prema ASP.NET Core-u, EF Core-u, `HttpClient`-u ili simulatorima. `Application` definiše portove. `Infrastructure` ih implementira. `Api` sastavlja backend, a `Web` je zaseban HTTP klijent.

## Domain

```text
Domain/
├── Common/
│   ├── Enums/
│   └── Results/
├── Parcels/
│   ├── Models/
│   └── Enums/
├── Lockers/
│   ├── Models/
│   └── Enums/
└── Pickup/
    ├── Models/
    └── Enums/
```

`Parcel`, `Compartment` i `PickupAccess` čuvaju svoje invarijante. Očekivani poslovni neuspeh vraća typed `Result`, a poznate greške su enum vrednosti (`ParcelError`, `CompartmentError`, `PickupAccessError`).

```csharp
Result<Parcel, ParcelError> registerResult = Parcel.Register(...);
Result<CompartmentError> occupyResult = compartment.Occupy(...);
```

`SizeCategory` je zajednički domenski koncept za veličinu paketa i kapacitet pretinca. Kompatibilnost veličina je eksplicitno poslovno pravilo.

`PickupAccess` vodi stanje pristupa, rok važenja i broj neuspešnih pokušaja. Kriptografski detalji ostaju van domena.

## Application

Application orkestrira use-case-ove i definiše male portove:

```text
Abstractions/
├── Persistence/
├── External/
└── Security/
```

Application greške (`ParcelOperationError`, `PickupOperationError`) predstavljaju ishod kompletnog use-case-a. Handler mapira precizan domenski ili integracioni ishod u odgovarajuću application grešku.

- `ILockerController` predstavlja spoljašnji kontroler pretinca.
- `IMessageGateway` predstavlja generički izlazni kanal za poruke.
- `IPickupCodeService` predstavlja generisanje i proveru pickup koda.
- `TimeProvider` omogućava determinističke testove vremena.

## Infrastructure

Infrastructure sadrži:

- EF Core repository implementacije;
- konfiguracije entiteta;
- SQLite `DbContext`;
- `HttpClient` adaptere prema simulatorima;
- implementaciju pickup code security servisa.

Transportni kvar koji je očekivan deo integracionog toka prevodi se u typed rezultat pre povratka u Application sloj.

## Api

API sloj:

1. prima HTTP contract;
2. poziva application handler;
3. prevodi rezultat u HTTP response ili Problem Details.

Tek na HTTP granici nastaju tekstualne poruke namenjene klijentu. Enum-i se serijalizuju kao nazivi, ne kao numeričke vrednosti.

## Web

`ParcelBox.Web` nema project reference ka backend slojevima. Koristi tri HTTP klijenta:

- `ParcelBoxApiClient`;
- `LockerControllerClient`;
- `MessageGatewayClient`.

UI nije jedna velika Razor komponenta. `Home` orkestrira stanje, dok su vizuelne celine izdvojene u `Components/Dashboard`:

```text
DashboardHeader
FlowOverview
ParcelRegistrationCard
PickupCard
LockerWall
ActivityPanel
SimulatorControls
```

Form state i activity state su mali Web view-model-i. Time prezentaciona odgovornost ostaje u Web projektu, a `Home.razor` ostaje pregledan.

## Simulatori

Simulatori su zasebni procesi i ne sadrže ParcelBox poslovna pravila.

Locker Controller podržava:

- `Normal`;
- `Jammed`;
- `Unavailable`.

Message Gateway podržava:

- `Normal`;
- `Unavailable`.

`Unavailable` utiče i na `/health`, pa se failure mode vidi i kroz UI status servisa.

## Result pattern

```text
Domain
  Result<Value, DomainErrorEnum>
              |
              v
Application
  Result<Value, OperationErrorEnum>
              |
              v
Api
  HTTP Problem Details
```

Exception se ne koristi kao kontrolni tok za očekivanu validaciju, statusnu tranziciju ili nedostupan spoljni servis.

## SOLID

- **SRP** — domain model čuva pravila; handler orkestrira; repository radi persistence; adapter komunicira sa spoljnim sistemom; UI komponenta ima jednu prezentacionu odgovornost.
- **OCP** — konkretan adapter može da se zameni implementacijom istog malog porta.
- **LSP** — test doubles zamenjuju repository-je i spoljne portove kroz isti ugovor.
- **ISP** — persistence, external i security ugovori su mali i namenski.
- **DIP** — Application zavisi od portova, a Infrastructure od njihovih konkretnih implementacija.

## Namerna pojednostavljenja

- SQLite;
- jedan `DbContext`;
- sinhrona HTTP integracija sa simulatorima;
- nema MediatR-a, message bus-a, generic repository-ja ni outbox-a;
- nema automatskog retry procesa za neuspele poruke;
- nema produkcione konkurentne rezervacije pretinaca.

Ova ograničenja su namerna: repo treba da ostane mali, čitljiv i pogodan za vežbe.
