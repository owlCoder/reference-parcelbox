# Arhitektura

ParcelBox koristi eksplicitnu **Model → Repository → Service → API** podelu. Modeli predstavljaju stanje; repository sloj radi persistence; application servisi sadrže poslovna pravila; API je transport i composition root.

## Smer zavisnosti

```text
Api --------> Application --------> Domain
 |                 ^
 |                 |
 +----> Infrastructure

Web ----HTTP----> Api
```

Infrastructure zavisi od Application portova koje implementira. Application zavisi od Domain modela. Domain nema reference ka višim slojevima.

## Domain

Domain sadrži samo state-only modele i domenske enum-e:

```text
Domain/
├── Common/Enums/
├── Parcels/Models + Enums
├── Lockers/Models + Enums
└── Pickup/Models + Enums
```

Model ne poznaje repository, servis, Result, EF Core, HTTP ili simulator. `Compartment`, na primer, čuva `Size`, `Status` i `ParcelId`; pravilo kompatibilnosti veličina pripada `LockerService`-u.

## Application: jasna taksonomija

```text
Application/
├── Common/Results/
├── DTOs/
│   ├── External/
│   ├── Lockers/
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

Pravilo je namerno strogo:

- `Interfaces` sadrži samo `interface` tipove;
- `Enums` sadrži application i integration enum-e;
- `DTOs` sadrži podatkovne ugovore/record-e;
- `Services` sadrži konkretne application service implementacije.

Zato su `LockerControllerError` i `MessageGatewayError` u `Application/Enums`, `OutboundMessage` u `Application/DTOs/External`, a `IUnitOfWork` u `Application/Interfaces/Persistence`.

## Repository i persistence

Repository ugovori su u `Application/Interfaces/Repositories`, implementacije u `Infrastructure/Persistence/Repositories`:

```text
IParcelRepository        -> ParcelRepository
ICompartmentRepository   -> CompartmentRepository
IPickupAccessRepository  -> PickupAccessRepository
```

`IUnitOfWork` nije repository i zato je odvojen u `Interfaces/Persistence`:

```text
IUnitOfWork -> ParcelBoxDbContext
```

Repository radi data access. Ne bira poslovno “najbolji” rezultat, ne validira pickup kod i ne vodi statusne tranzicije.

## Servisi

Service ugovori su u `Application/Interfaces/Services`, implementacije u `Application/Services`:

```text
IParcelService        -> ParcelService
ILockerService        -> LockerService
IPickupAccessService  -> PickupAccessService
IPickupService        -> PickupService
INotificationService  -> NotificationService
```

- `ParcelService` vodi registraciju i store orkestraciju.
- `LockerService` bira kompatibilan pretinac i vodi promenu njegovog stanja.
- `PickupAccessService` vodi pickup kod, hash, rok važenja i neuspešne pokušaje.
- `PickupService` orkestrira završetak pickup toka.
- `NotificationService` formira i šalje pickup poruku.

Servisi zavise od interfejsa. Izuzetak je `TimeProvider`, koji je namerno injektovan kao standardna zamenjiva vremenska apstrakcija.

## External i Security portovi

Application definiše portove `ILockerController`, `IMessageGateway` i `IPickupCodeService`. Infrastructure sadrži `HttpClient` i kriptografske implementacije.

Transportni DTO `OutboundMessage` nije interface i zato nije u `Interfaces/External`. Očekivani transportni kvar adapter prevodi u `LockerControllerError` ili `MessageGatewayError`, koji su application enum-i.

## Result pattern

Result pripada Application sloju:

```text
Application/Common/Results/
├── Result.cs
└── ResultOfT.cs
```

Poznati poslovni i integracioni ishodi su typed enum vrednosti. Exception nije kontrolni tok za očekivanu validaciju ili nedostupan simulator.

## Dependency injection

API je composition root. Application servisi i Infrastructure adapteri registruju se kao zamene njihovih interfejsa:

```text
IParcelService -> ParcelService
ILockerService -> LockerService
IPickupAccessService -> PickupAccessService
IPickupService -> PickupService
INotificationService -> NotificationService

IUnitOfWork -> ParcelBoxDbContext
IParcelRepository -> ParcelRepository
ICompartmentRepository -> CompartmentRepository
IPickupAccessRepository -> PickupAccessRepository
ILockerController -> LockerControllerClient
IMessageGateway -> MessageGatewayClient
IPickupCodeService -> PickupCodeService
```

Endpoint dobija service interfejs, nikad `DbContext`, repository implementaciju ili HTTP adapter.

## SOLID

- **SRP** — model čuva stanje; repository radi podatke; servis sadrži određenu poslovnu odgovornost; adapter radi jednu integraciju; endpoint radi HTTP.
- **OCP** — repository, servis ili adapter može da se zameni implementacijom istog ugovora.
- **LSP** — fake implementacije u testovima zamenjuju produkcione implementacije istih interfejsa.
- **ISP** — repository, service, external, persistence i security interfejsi su mali i namenski.
- **DIP** — servisi zavise od interfejsa; EF Core, HTTP i crypto detalji ostaju u Infrastructure sloju.

## Arhitektonske provere

Repo ima dve zaštite od ponovnog mešanja slojeva:

1. `scripts/check-architecture.sh` proverava fizički raspored fajlova i zabranjene zavisnosti;
2. `ArchitectureTests` refleksijom proverava da `Interfaces` namespace sadrži samo interfejse, da su application enum-i u `Enums`, da servisi zavise od apstrakcija i da Domain modeli ostanu state-only.

CI pokreće obe provere pre smoke testova.

## Namerna ograničenja

Nema MediatR-a, generic repository-ja, message bus-a, outbox-a ili dodatnog Unit of Work framework-a. `DbContext` implementira mali `IUnitOfWork` ugovor. Cilj je jasan referentni kod, ne produkcioni paketomat.
