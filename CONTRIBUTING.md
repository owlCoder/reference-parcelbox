# Doprinos projektu ParcelBox

Ovaj repo je referentni primer za vežbe. Promene treba da ostanu male, čitljive i u skladu sa postojećim granicama sistema.

## Pre slanja promene

Pokrenite:

```bash
dotnet restore
dotnet format --verify-no-changes --no-restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
```

Za promene koje utiču na integracije ili Web UI pokrenite i kompletan demo preko `scripts/run-all.ps1` ili `scripts/run-all.sh`.

## Pravila strukture

- jedna javna klasa ili enum po fajlu;
- naziv fajla prati naziv glavnog tipa;
- poslovni modeli i poslovna pravila pripadaju `Domain` sloju;
- use-case orkestracija pripada `Application` sloju;
- EF Core, HTTP adapteri i security implementacije pripadaju `Infrastructure` sloju;
- `Api` je transportna granica i composition root backend-a;
- `Web` komunicira sa backend-om preko HTTP-a i nema reference ka unutrašnjim backend projektima;
- simulator ne sadrži poslovna pravila ParcelBox sistema;
- nova apstrakcija se uvodi samo kada predstavlja stvarnu granicu ili zamenjivu implementaciju;
- očekivani poslovni neuspeh vraća typed Result umesto exception-based kontrolnog toka.

## Organizacija Web projekta

Velike Razor stranice treba razložiti na manje prezentacione komponente. Stranica orkestrira stanje, dok ponovljive ili jasno izdvojene vizuelne celine pripadaju `Components/` folderu.

## Arhitektonske odluke

Za promenu koja menja smer zavisnosti, projektne granice ili osnovni integration pattern dodajte ili ažurirajte ADR u `docs/adr/`.
