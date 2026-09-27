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

Infrastructure zavisi od Application interfejsa koje implementira. Application zavisi od Domain modela. Domain nema reference ka višim slojevima.

## Domain

Domain sadrži samo modele i enum-e. Model nema repository, servis, Result ili use-case helper metodu.

```text
Domain/
├── Common/Enums/
├── Parcels/Models + Enums
├── Lockers/Models + Enums
└── Pickup/Models + Enums
```

`Compartment`, na primer, samo čuva `Size`, `Status` i `ParcelId`. Pravilo kompatibilnosti veličina pripada `LockerService`-u.

## Repository

Interfejsi su u `Application/Interfaces/Repositories`, a EF Core implementacije u `Infrastructure/Persistence/Repositories`.

```text
IParcelRepository        -> ParcelRepository
ICompartmentRepository   -> CompartmentRepository
IPickupAccessRepository  -> PickupAccessRepository
IUnitOfWork              -> ParcelBoxDbContext
```

Repository sadrži data-access upite. Ne radi izbor “najboljeg” pretinca, statusnu tranziciju ili validaciju pickup koda.

## Servisi

Service ugovori su u `Application/Interfaces/Services`, implementacije u `Application/Services`.

```text
IParcelService        -> ParcelService
ILockerService        -> LockerService
IPickupAccessService  -> PickupAccessService
IPickupService        -> PickupService
INotificationService  -> NotificationService
```

### ParcelService

Koristi `IParcelRepository`, `ILockerService`, `IPickupAccessService`, `INotificationService`, `IUnitOfWork` i `TimeProvider`. Odgovoran je za registraciju paketa i store orkestraciju.

### LockerService

Koristi `ICompartmentRepository` i `ILockerController`. Ovde je pravilo izbora najmanjeg kompatibilnog dostupnog pretinca, kao i promena stanja pretinca.

### PickupAccessService

Koristi `IPickupAccessRepository` i `IPickupCodeService`. Odgovoran je za kreiranje pickup pristupa, hash koda, rok važenja, proveru koda, broj neuspešnih pokušaja i zaključavanje.

### PickupService

Koristi `IParcelRepository`, `ILockerService`, `IPickupAccessService`, `IUnitOfWork` i `TimeProvider`. Orkestrira završetak pickup toka i ne sadrži kod za generisanje/hash pickup koda niti slanje poruka.

### NotificationService

Koristi samo `IMessageGateway`. Formira i šalje poruku sa pickup kodom. Time message integration nije dodatna odgovornost `PickupService`-a.

## External i Security interfejsi

Application definiše male portove:

- `ILockerController`;
- `IMessageGateway`;
- `IPickupCodeService`.

Infrastructure sadrži `HttpClient` i kriptografske implementacije. Očekivani transportni kvar adapter prevodi u typed rezultat.

## Result pattern

Result pripada Application sloju:

```text
Application/Common/Results/
├── Result.cs
└── ResultOfT.cs
```

Poznati poslovni i integracioni ishodi su enum-i iz `Application/Enums`. Exception nije kontrolni tok za očekivanu validaciju ili nedostupan simulator.

## Dependency injection

API sastavlja sistem:

```text
IParcelService -> ParcelService
ILockerService -> LockerService
IPickupAccessService -> PickupAccessService
IPickupService -> PickupService
INotificationService -> NotificationService

IParcelRepository -> ParcelRepository
ICompartmentRepository -> CompartmentRepository
IPickupAccessRepository -> PickupAccessRepository
ILockerController -> LockerControllerClient
IMessageGateway -> MessageGatewayClient
IPickupCodeService -> PickupCodeService
```

Endpoint dobija service interfejs, nikad `DbContext` ili konkretnu repository klasu.

## SOLID

- **SRP** — modeli čuvaju stanje; repository radi podatke; svaki servis ima konkretnu poslovnu odgovornost; adapter radi jednu spoljnu integraciju; endpoint radi HTTP.
- **OCP** — repository ili adapter se menja implementacijom istog interfejsa bez promene service koda.
- **LSP** — fake implementacije u testovima zamenjuju produkcione implementacije istih interfejsa.
- **ISP** — repository, service, external i security interfejsi su mali i namenski.
- **DIP** — servisi zavise od interfejsa, a konkretni EF Core/HTTP/security detalji su u Infrastructure sloju.

## Testovi

Business pravila se testiraju na service nivou uz fake repository-je i fake external adaptere. Time test ne zahteva SQLite ni pokrenute simulatore.

Pokriveni su, između ostalog:

- najmanji kompatibilni pretinac;
- dupli tracking code;
- store tok;
- jammed locker;
- zaključavanje pickup pristupa nakon tri pogrešna koda.

## Namerna ograničenja

Nema MediatR-a, generic repository-ja, message bus-a, outbox-a ili dodatnog Unit of Work framework-a. `DbContext` implementira mali `IUnitOfWork` ugovor. Cilj je jasan referentni kod, ne produkcioni paketomat.
