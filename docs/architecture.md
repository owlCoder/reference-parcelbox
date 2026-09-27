# Arhitektura

ParcelBox je mali referentni sistem sa eksplicitnom podelom **Model → Repository → Service → API**. Podela je namerno jednostavna: modeli predstavljaju stanje, repository sloj pristupa podacima, application servisi sadrže poslovna pravila i orkestraciju, a API predstavlja transportnu granicu.

## Smer zavisnosti

```text
ParcelBox.Domain
        ^
        |
ParcelBox.Application
        ^
        |
ParcelBox.Infrastructure
        ^
        |
ParcelBox.Api
```

Tačnije, `Infrastructure` referencira `Application` zato što implementira njegove portove. `Api` referencira Application i Infrastructure da bi sastavio aplikaciju. `Domain` ne zavisi ni od jednog višeg sloja.

`ParcelBox.Web` nema backend project reference:

```text
ParcelBox.Web --HTTP--> ParcelBox.Api
```

## Domain

Domain sadrži state-only modele i domenske enum-e.

```text
ParcelBox.Domain/
├── Common/Enums/
│   └── SizeCategory.cs
├── Parcels/
│   ├── Models/Parcel.cs
│   └── Enums/ParcelStatus.cs
├── Lockers/
│   ├── Models/Compartment.cs
│   └── Enums/CompartmentStatus.cs
└── Pickup/
    ├── Models/PickupAccess.cs
    └── Enums/PickupStatus.cs
```

Folder i namespace su usklađeni (`*.Models`, `*.Enums`). Model nema repository/service zavisnost, Result tip, HTTP/EF detalje niti metode poput `CanFit`, `Validate...` ili `Mark...`.

## Repository sloj

Repository interfejsi su u `Application/Interfaces/Repositories`, a EF Core implementacije u `Infrastructure/Persistence/Repositories`.

```text
IParcelRepository       -> ParcelRepository
ICompartmentRepository  -> CompartmentRepository
IPickupAccessRepository -> PickupAccessRepository
```

Repository sadrži isključivo data-access operacije. Ne bira poslovno “najbolji” pretinac, ne radi statusnu tranziciju i ne validira pickup kod.

`IUnitOfWork` je odvojen persistence ugovor u `Application/Interfaces/Persistence`; `ParcelBoxDbContext` ga implementira.

Read-only upiti koji ne zahtevaju tracking koriste `AsNoTracking()` kada je to smisleno.

## Application servisi

Service interfejsi su u `Application/Interfaces/Services`, implementacije u `Application/Services`.

```text
IParcelService        -> ParcelService
IParcelStorageService -> ParcelStorageService
ICompartmentService   -> CompartmentService
ILockerService        -> LockerService
IPickupAccessService  -> PickupAccessService
IPickupService        -> PickupService
INotificationService  -> NotificationService
```

### ParcelService

Registruje paket i vraća podatke o paketu. Zavisi od `IParcelRepository`, `IUnitOfWork` i `TimeProvider`. Ne orkestrira storage tok.

### ParcelStorageService

Orkestrira smeštanje: učitava paket, traži pretinac preko `ILockerService`, kreira pickup pristup, menja stanje paketa, čuva promene i tek nakon uspešnog persistence koraka pokušava slanje obaveštenja.

Neuspeh Message Gateway-a ne vraća unazad već uspešno smeštanje; rezultat eksplicitno sadrži status isporuke poruke.

### CompartmentService

Read-only servis za prikaz pretinaca. Time `ILockerService` ostaje fokusiran na locker poslovne operacije umesto da kombinuje query i command odgovornosti.

### LockerService

Bira najmanji kompatibilni slobodan pretinac, poziva `ILockerController`, menja stanje izabranog pretinca i vodi open/release pravila. Repository mu daje podatke, ali ne donosi odluku o kompatibilnosti veličine.

### PickupAccessService

Kreira pickup pristup, koristi `IPickupCodeService` za generisanje/hash/proveru koda i vodi rok, neuspešne pokušaje i zaključavanje. Model `PickupAccess` ostaje state-only.

### PickupService

Orkestrira završetak preuzimanja: pronalazi paket, proverava pickup pristup, otvara pretinac, oslobađa ga, označava pristup kao iskorišćen i čuva završno stanje.

### NotificationService

Dobija minimalni `PickupNotification` DTO, a ne ceo `Parcel` model. Formira `OutboundMessage` i koristi `IMessageGateway`.

## External i Security portovi

Application definiše male portove:

```text
ILockerController
IMessageGateway
IPickupCodeService
```

`Infrastructure` sadrži konkretne HTTP i security implementacije. Transportni timeout ili `HttpRequestException` hvataju se na Infrastructure granici i prevode u typed Result. Očekivani integracioni kvar nije exception-based control flow u Application sloju.

## Result pattern

```text
Application/Common/Results/
├── Result.cs
└── ResultOfT.cs
```

Poznati poslovni i integracioni ishodi predstavljeni su enum vrednostima. API ih tek na HTTP granici prevodi u status kod i Problem Details odgovor.

## Dependency injection

`Api` je composition root. Application servisi i Infrastructure implementacije registruju se kroz DI:

```text
IParcelService        -> ParcelService
IParcelStorageService -> ParcelStorageService
ICompartmentService   -> CompartmentService
ILockerService        -> LockerService
IPickupAccessService  -> PickupAccessService
IPickupService        -> PickupService
INotificationService  -> NotificationService

IParcelRepository       -> ParcelRepository
ICompartmentRepository  -> CompartmentRepository
IPickupAccessRepository -> PickupAccessRepository
IUnitOfWork              -> ParcelBoxDbContext
ILockerController        -> LockerControllerClient
IMessageGateway          -> MessageGatewayClient
IPickupCodeService       -> PickupCodeService
```

Endpoint zavisi od service interfejsa. Application servis zavisi od repository/external/persistence/security interfejsa, ne od EF Core klase ili konkretnog HTTP adaptera.

## SOLID u ovom primeru

- **SRP** — model čuva stanje; repository radi data access; query, storage, locker, pickup i notification odgovornosti su razdvojene po servisima.
- **OCP** — persistence, external ili security implementacija može da se zameni iza postojećeg interfejsa.
- **LSP** — test doubles zamenjuju produkcione portove bez promene service koda.
- **ISP** — `IParcelService`, `IParcelStorageService`, `ICompartmentService` i external portovi imaju uske, konkretne odgovornosti.
- **DIP** — business servisi zavise od apstrakcija, a EF Core/HTTP/crypto detalji su u Infrastructure sloju.

## Arhitektonske provere

Dve provere čuvaju dogovorene granice:

- `scripts/check-architecture.sh` proverava folder/namespace taksonomiju i zabranjene zavisnosti;
- `ArchitectureTests` refleksijom proverava interface namespace, enum namespace, service constructor zavisnosti i state-only Domain modele.

CI pokreće obe provere pre finalnih smoke testova.

## Namerna ograničenja

Primer ne uvodi MediatR, generic repository, message bus, outbox ili dodatni framework sloj. Cilj je čitljiv referentni kod za vežbe, ne produkcioni paketomat.
