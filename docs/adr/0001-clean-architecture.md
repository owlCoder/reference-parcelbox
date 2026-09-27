# ADR 0001: Model - Repository - Service granice

## Status

Prihvaćeno.

## Kontekst

ParcelBox je nastavnički referentni projekat. Struktura mora jasno da pokaže gde pripadaju stanje, data access, poslovna pravila, integracije i transport, bez skrivanja odgovornosti u helper metodama modela ili repository upitima.

## Odluka

Backend koristi projekte `ParcelBox.Domain`, `ParcelBox.Application`, `ParcelBox.Infrastructure` i `ParcelBox.Api`.

Usvojena su sledeća pravila:

1. `Domain` sadrži state-only modele i domenske enum-e; folder i namespace moraju da budu usklađeni (`Models`, `Enums`).
2. Repository interfejsi su u `Application/Interfaces/Repositories`; implementacije su u `Infrastructure/Persistence/Repositories`.
3. `IUnitOfWork` je persistence port i nalazi se u `Application/Interfaces/Persistence`.
4. `Application/Interfaces` sadrži samo interface tipove. DTO-i i enum-i imaju zasebne foldere.
5. Poslovna pravila i use-case orkestracija pripadaju application servisima.
6. Service interfejsi treba da budu uski. Query prikaz pretinaca je odvojen od locker command pravila, a storage orkestracija od registracije/čitanja paketa.
7. Servis prema drugom servisu prosleđuje minimalan potreban ugovor kada ceo domen model nije potreban; `NotificationService`, na primer, dobija `PickupNotification` DTO.
8. Infrastructure adapteri implementiraju external/security/persistence portove.
9. `Api` je composition root i endpoint zavisi od service interfejsa, ne od repository-ja ili `DbContext`-a.
10. Očekivani poslovni i integracioni neuspeh vraća typed Result; exception nije kontrolni tok za očekivano ponašanje.

Modeli namerno nemaju metode poput `CanFit`, `CanBeStored`, `ValidateAttempt` ili `MarkUsed`. Repository ne odlučuje koji je pretinac poslovno odgovarajući. Takva pravila moraju biti vidljiva u odgovarajućem application servisu.

`ParcelBox.Web` i simulatori ostaju zasebni procesi i komuniciraju preko HTTP-a.

## Posledice

Student može da prati use-case kroz jasan lanac:

```text
Endpoint -> Service -> Repository interface -> Repository implementation -> DbContext
```

ili integracioni tok:

```text
Service -> External interface -> HTTP adapter -> Simulator
```

Cena pristupa je anemičniji Domain model i nekoliko eksplicitnih application servisa. To je namerna nastavna odluka: u ovom primeru prioritet su jasna podela odgovornosti, dependency inversion i lako uočljiva struktura.
