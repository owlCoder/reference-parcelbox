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
2. **Repository interfaces** su u Application sloju.
3. **Repository implementations** su u Infrastructure sloju.
4. **Application services** sadrže validaciju, poslovna pravila i orkestraciju.
5. **Service interfaces** su granica koju koristi API.
6. **Infrastructure adapters** implementiraju spoljne portove.
7. **Api** je composition root i povezuje implementacije kroz dependency injection.
8. **Result pattern** pripada Application sloju; očekivani poslovni neuspeh nije exception.

Modeli namerno nemaju `CanFit`, `CanBeStored`, `ValidateAttempt`, `MarkUsed` ili slične poslovne helper metode. Repository ne donosi odluke poput kompatibilnosti veličina. Takva pravila moraju biti vidljiva u odgovarajućem servisu.

`ParcelBox.Web` i simulatori ostaju zasebni procesi i komuniciraju preko HTTP-a.

## Posledice

Student može da prati jedan use-case kroz jasan lanac:

```text
Endpoint -> Service -> Repository interface -> Repository implementation -> DbContext
```

ili integracioni tok:

```text
Service -> External interface -> HTTP adapter -> Simulator
```

Cena ovog izbora je anemičniji Domain model, ali je to namerna nastavna odluka: u ovom primeru želimo eksplicitnu service/repository podelu i lako uočljive SOLID odgovornosti.
