# Demo UI

`ParcelBox.Web` je nastavnički Blazor demo klijent. Nema project reference ka `Domain`, `Application` ili `Infrastructure` projektima; sve podatke dobija preko HTTP-a.

## Raspored

Stranica ima četiri jasno odvojena dela:

1. **Flow overview** — Register → Store → Pickup.
2. **Business workflow** — registracija paketa, stanje trenutnog paketa i pickup forma.
3. **Locker wall / Activity** — trenutno stanje pretinaca i istorija akcija u sesiji.
4. **External system controls** — nastavničke kontrole za oba simulatora i Message Gateway inbox.

Simulator kontrole nisu deo ParcelBox poslovnog domena. Vizuelno su označene kao teaching/test harness, ali koriste isti dizajn sistem kao ostatak stranice.

## Komponente

```text
Components/
├── Dashboard/
│   ├── DashboardHeader.razor
│   ├── FlowOverview.razor
│   ├── ParcelRegistrationCard.razor
│   ├── PickupCard.razor
│   ├── LockerWall.razor
│   ├── ActivityPanel.razor
│   └── SimulatorControls.razor
└── Pages/
    ├── Home.razor
    └── Home.razor.cs
```

`Home` je orchestration komponenta. Manje komponente dobijaju stanje preko parametara i vraćaju korisničke akcije preko `EventCallback`-a. Mutable stanje formi je u malim Web view-model klasama (`RegisterParcelForm`, `PickupForm`), dok activity timeline koristi `ActivityEntry`.

Web poziva isključivo HTTP granice. Locker wall dobija read model preko API endpointa, dok business akcije idu preko Parcel/Storage/Pickup application servisa iza API-ja. UI ne pristupa repository-ju ili Domain modelu direktno.

## Demonstracija failure scenarija

### Locker Controller

- `Normal` — otvaranje pretinca uspeva.
- `Jammed` — simulator je dostupan, ali fizička operacija ne uspeva.
- `Unavailable` — simulator vraća 503 i health indikator prelazi u offline stanje.

### Message Gateway

- `Normal` — pickup poruka ulazi u simulator inbox.
- `Unavailable` — slanje ne uspeva, ali prethodno uspešno smeštanje paketa se ne vraća unazad.

Pickup kod se prikazuje u inbox-u samo da bi se scenario mogao demonstrirati bez stvarnog SMS provajdera.
