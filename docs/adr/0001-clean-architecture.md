# ADR 0001: Model - Repository - Service granice

## Status

Prihvaćeno.

## Kontekst

ParcelBox je nastavnički referentni projekat. Struktura mora jasno da pokaže gde pripadaju podaci, persistence, poslovna pravila i transport, bez skrivanja odgovornosti u helper metodama modela ili repository upitima.

## Odluka

Backend koristi četiri projekta:

- `ParcelBox.Domain`;
- `ParcelBox.Application`;
- `ParcelBox.Infrastructure`;
- `ParcelBox.Api`.

Primjenjuje se sledeća podela:

1. **Domain models** sadrže stanje i domenske enum-e.
2. **Repository interfaces** su u `Application/Interfaces/Repositories`.
3. **Persistence interface** `IUnitOfWork` je u `Application/Interfaces/Persistence`.
4. **Repository implementations** su u Infrastructure sloju.
5. **Application services** sadrže validaciju, poslovna pravila i orkestraciju.
6. **Service interfaces** su granica koju koristi API.
7. **External interfaces** sadrže samo portove; integration error enum-i su u `Application/Enums`, a transportni DTO-i u `Application/DTOs/External`.
8. **Infrastructure adapters** implementiraju spoljne portove.
9. **Api** je composition root i povezuje implementacije kroz dependency injection.
10. **Result pattern** pripada Application sloju; očekivani poslovni neuspeh nije exception.

`Application/Interfaces` je namerno stroga taksonomska granica: u tom stablu ne sme biti enum-a, record-a ili concrete class-a.

Modeli namerno nemaju `CanFit`, `CanBeStored`, `ValidateAttempt`, `MarkUsed` ili slične poslovne helper metode. Repository ne donosi odluke poput kompatibilnosti veličina. Takva pravila moraju biti vidljiva u odgovarajućem servisu.

## Posledice

Student može da prati use-case kroz jasan lanac:

```text
Endpoint -> Service -> Repository interface -> Repository implementation -> DbContext
```

ili integracioni tok:

```text
Service -> External interface -> HTTP adapter -> Simulator
```

CI proverava i fizičku organizaciju i smer zavisnosti, da se taksonomija ne bi vremenom ponovo pomešala.

Cena ovog izbora je anemičniji Domain model, ali je to namerna nastavna odluka: u ovom primeru želimo eksplicitnu service/repository podelu i lako uočljive SOLID odgovornosti.
