# Doprinos projektu ParcelBox

Ovaj repo služi kao mali referentni primer, pa promene treba da ostanu fokusirane i čitljive.

## Tok rada

1. Ažurirajte lokalni `main`.
2. Napravite granu `feature/<kratak-opis>` ili `fix/<kratak-opis>`.
3. Radite u manjim, smislenim commit-ovima.
4. Pokrenite `dotnet build` i `dotnet test`.
5. Otvorite Pull Request prema `main`.
6. Odgovorite na review komentare i ne merge-ujte promenu sa neuspešnim CI proverama.

## Arhitektonska pravila

- poslovna pravila pripadaju `Domain` sloju;
- use-case orkestracija pripada `Application` sloju;
- EF Core, HTTP klijenti i implementacije portova pripadaju `Infrastructure` sloju;
- `Api` je composition root i ne sadrži poslovnu logiku;
- simulator nije mesto za poslovna pravila glavnog sistema;
- nova apstrakcija se uvodi kada postoji stvarna granica ili razlog za zamenu implementacije, ne unapred.

Za odluku koja menja strukturu ili granice sistema dodajte kratak ADR u `docs/adr/`.
