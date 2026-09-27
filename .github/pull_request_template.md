## Šta je promenjeno

<!-- Kratak opis promene. -->

## Kako je provereno

- [ ] `./scripts/check-architecture.sh`
- [ ] `dotnet format --verify-no-changes --no-restore`
- [ ] `dotnet build`
- [ ] `dotnet test`
- [ ] relevantan ručni scenario je proveren

## Arhitektura

- [ ] folder i namespace su usklađeni
- [ ] modeli su state-only
- [ ] repository sadrži samo data access
- [ ] business pravila su u odgovarajućem application servisu
- [ ] endpoint zavisi od service interfejsa, ne od persistence sloja
- [ ] novi tip je smešten u odgovarajući `Models`, `Enums`, `DTOs` ili `Interfaces` folder
- [ ] dokumentacija/ADR je ažuriran ako je odluka značajna
