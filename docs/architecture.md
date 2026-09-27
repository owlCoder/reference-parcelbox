# Arhitektura

ParcelBox je mali referentni sistem organizovan po Clean Architecture i SOLID principima, uz eksplicitnu podelu **Model → Repository → Service → API**. Primer namerno koristi state-only modele: poslovne odluke nisu sakrivene u entity helper metodama, već su vidljive u application servisima.

## Pravac zavisnosti

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

Tačnije, `Infrastructure` referencira `Application` zato što implementira interfejse definisane unutra, dok `Api` referencira Application i Infrastructure samo da bi sastavio aplikaciju.

`ParcelBox.Web` nema project reference ka backend slojevima:

```text
ParcelBox.Web --HTTP--> ParcelBox.Api
```

## 1. Domain: modeli i enum-i

Domain sadrži podatke koji predstavljaju poslovno stanje:

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

Modeli nemaju:

- `CanFit`, `CanBeStored` ili slične helper metode;
- factory metode koje sadrže use-case validaciju;
- repository ili service zavisnosti;
- `Result` tip;
- HTTP, EF Core ili simulator detalje.

Primer:

```csharp
public sealed class Compartment
{
    public Guid Id { get; set; }
    public string LockerCode { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public SizeCategory Size { get; set; }
    public CompartmentStatus Status { get; set; }
    public Guid? ParcelId { get; set; }
}
```

Model ima jednu odgovornost: predstavljanje stanja.

## 2. Repository interfejsi

Repository interfejsi su u Application sloju:

```text
Interfaces/Repositories/
├── IParcelRepository.cs
├── ICompartmentRepository.cs
├── IPickupAccessRepository.cs
└── IUnitOfWork.cs
```

Repository je persistence apstrakcija. Ne sadrži poslovnu politiku.

Na primer, `ICompartmentRepository` vraća dostupne pretince. On ne odlučuje da li `Medium` paket može u `Large` pretinac. Ta odluka pripada `LockerService`-u.

Ovim se izbegava skrivena poslovna logika u LINQ upitu repository-ja.

## 3. Application servisi

Javni use-case ugovori su u:

```text
Interfaces/Services/
├── IParcelService.cs
├── ILockerService.cs
└── IPickupService.cs
```

Implementacije su u:

```text
Services/
├── ParcelService.cs
├── LockerService.cs
└── PickupService.cs
```

### ParcelService

Koristi `IParcelRepository` za podatke o paketima, `ILockerService` za smeštanje i `IPickupService` za pickup pristup. Vodi:

- validaciju registracije;
- proveru jedinstvenog tracking koda;
- registraciju paketa;
- prelaz `Registered -> Stored`;
- orkestraciju store scenarija.

### LockerService

Koristi `ICompartmentRepository` i `ILockerController`. Vodi:

- izbor najmanjeg kompatibilnog dostupnog pretinca;
- proveru stanja pretinca;
- poziv fizičkog kontrolera;
- zauzimanje i oslobađanje pretinca.

Kompatibilnost veličina je ovde, a ne u `Compartment` modelu i ne u repository-ju.

### PickupService

Koristi repository-je za paket i pickup pristup, `ILockerService`, `IMessageGateway`, `IPickupCodeService`, `IUnitOfWork` i `TimeProvider`. Vodi:

- kreiranje pickup pristupa;
- rok važenja koda;
- broj neuspešnih pokušaja i zaključavanje;
- proveru koda;
- slanje poruke;
- završetak pickup toka.

## 4. Result pattern

`Result` je deo Application sloja, jer opisuje ishod application operacije:

```text
Application/Common/Results/
├── Result.cs
└── ResultOfT.cs
```

Primer:

```csharp
Task<Result<ParcelDetails, ParcelOperationError>> RegisterAsync(...);
```

Očekivana validaciona, poslovna ili integraciona greška nije exception. Poznati ishodi su enum-i u `Application/Enums`.

Infrastructure adapter može, na primer, `HttpRequestException` iz `HttpClient`-a da prevede u `LockerControllerError.Unavailable`. Ostatak aplikacije ne zavisi od transportnog exception tipa.

## 5. Infrastructure

```text
ParcelBox.Infrastructure/
├── Persistence/
│   ├── Configurations/
│   ├── Repositories/
│   ├── ParcelBoxDbContext.cs
│   └── DatabaseInitializer.cs
├── External/
└── Security/
```

Infrastructure implementira interfejse iz Application sloja:

```text
IParcelRepository        -> ParcelRepository
ICompartmentRepository   -> CompartmentRepository
IPickupAccessRepository  -> PickupAccessRepository
ILockerController        -> LockerControllerClient
IMessageGateway          -> MessageGatewayClient
IPickupCodeService       -> PickupCodeService
IUnitOfWork              -> ParcelBoxDbContext
```

Repository implementacije sadrže samo persistence upite. Poslovne odluke ostaju u servisima.

## 6. Dependency injection

`ParcelBox.Api` je composition root.

Application servisi se registruju preko `AddApplicationServices()`:

```text
IParcelService -> ParcelService
ILockerService -> LockerService
IPickupService -> PickupService
```

Infrastructure registruje repository-je i adaptere preko `AddInfrastructure()`.

Endpoint zato dobija samo service interfejs:

```csharp
private static async Task<IResult> StoreAsync(
    Guid id,
    IParcelService service,
    CancellationToken cancellationToken)
```

Endpoint ne poznaje `DbContext`, repository implementaciju ili HTTP klijenta simulatora.

## 7. API

API sloj ima tri odgovornosti:

1. HTTP contract;
2. poziv service interfejsa;
3. prevod Result-a u HTTP odgovor / Problem Details.

API ne sadrži poslovna pravila i ne pristupa bazi direktno.

## 8. Web i simulatori

Blazor Web UI komunicira sa API-jem i nastavničkim simulatorima preko HTTP-a. Simulatori su zasebni procesi.

Locker Controller: `Normal`, `Jammed`, `Unavailable`.

Message Gateway: `Normal`, `Unavailable`.

Simulator ne zna pravila o paketima, pickup kodovima ili statusnim tranzicijama.

## SOLID u ovom primeru

- **SRP** — model čuva stanje; repository čita/piše podatke; service donosi poslovne odluke; adapter komunicira sa spoljnim sistemom; endpoint radi HTTP mapiranje.
- **OCP** — drugi repository ili external adapter može da implementira postojeći interfejs bez promene service koda.
- **LSP** — test fake implementacije mogu da zamene repository i external adapter kroz isti ugovor.
- **ISP** — interfejsi su mali i namenski (`IParcelRepository`, `ILockerController`, `IPickupCodeService`), bez jednog velikog “god service” ugovora.
- **DIP** — servisi zavise od interfejsa; konkretni EF Core i HTTP tipovi ostaju u Infrastructure sloju.

## Testovi

Pošto Domain modeli nemaju poslovne metode, testovi su fokusirani na application servise. Fake repository-ji i fake external adapteri omogućavaju proveru poslovnog pravila bez SQLite baze i bez pokretanja simulatora.

Primeri koje testovi pokrivaju:

- izbor najmanjeg kompatibilnog pretinca;
- odbijanje duplog tracking koda;
- store tok menja odgovarajuća stanja;
- jammed locker ne završava pickup;
- tri pogrešna pickup koda zaključavaju pristup.

## Namerna pojednostavljenja

- SQLite i jedan `DbContext`;
- nema generic repository-ja;
- nema MediatR-a;
- nema message bus-a ni outbox-a;
- nema dodatnog Unit of Work framework-a — `DbContext` implementira mali `IUnitOfWork` ugovor;
- nema produkcione konkurentne rezervacije pretinaca.

Cilj je da struktura bude dovoljno mala za vežbe, ali da se smer zavisnosti i odgovornosti jasno vide iz koda.
