# ADR 0001: Clean Architecture granice

## Status

Prihvaćeno.

## Kontekst

Referentni projekat treba da pokaže odvajanje poslovnih pravila, use-case orkestracije, infrastrukture i HTTP granice, ali ne treba da uvodi po jedan projekat za svaki feature ili interfejs.

## Odluka

Backend koristi četiri projekta:

- `ParcelBox.Domain`;
- `ParcelBox.Application`;
- `ParcelBox.Infrastructure`;
- `ParcelBox.Api`.

Tri poslovne celine organizovane su kao folderi unutar odgovarajućih slojeva.

`ParcelBox.Web` je zaseban Blazor klijent. Ne referencira backend projekte, već komunicira isključivo preko HTTP ugovora. Simulatori su takođe zasebni procesi.

## Posledice

Granice su vidljive kroz project reference zavisnosti, a solution ostaje dovoljno mala da studenti mogu da pregledaju ceo kod tokom vežbi. Web dodatno demonstrira da presentation klijent ne treba da preskače API granicu.
