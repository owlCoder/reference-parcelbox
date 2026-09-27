# ADR 0001: Clean Architecture sa četiri produkciona projekta

## Status

Prihvaćeno.

## Kontekst

Referentni projekat treba da pokaže odvajanje poslovnih pravila, use-case orkestracije, infrastrukture i HTTP granice, ali ne treba da uvodi po jedan projekat za svaki feature ili svaki interfejs.

## Odluka

Koriste se četiri produkciona projekta: `Domain`, `Application`, `Infrastructure` i `Api`. Tri poslovne celine su folderi unutar odgovarajućih slojeva.

## Posledice

Granice su vidljive kroz project reference zavisnosti, a solution ostaje dovoljno mala da studenti mogu da pregledaju ceo kod tokom vežbi.
