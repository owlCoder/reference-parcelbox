# UI predlog

Predlog za demo interfejs je mali **Blazor Web App (.NET 10, Interactive Server)** projekat `ParcelBox.Web`.

Cilj UI-ja nije da sakrije arhitekturu, već da omogući da se na vežbama jasno vidi ceo tok: registracija paketa, izbor pretinca, komunikacija sa simulatorom, slanje pickup koda i preuzimanje.

## Glavni ekran

Jedna stranica je dovoljna.

```text
┌─────────────────────────────────────────────────────────────────────┐
│ ParcelBox Demo                                  API ●  Locker ● SMS ●│
├─────────────────────┬───────────────────────┬───────────────────────┤
│ 1. Register parcel  │ 2. Locker wall        │ 3. Pickup             │
│                     │                       │                       │
│ Tracking code       │ A1  SMALL   available│ Tracking code         │
│ Recipient phone     │ A2  SMALL   occupied │ Pickup code           │
│ Size                │ B1  MEDIUM  available│                       │
│ [Register]          │ B2  MEDIUM  available│ [Open & pickup]       │
│                     │ C1  LARGE   available│                       │
├─────────────────────┴───────────────────────┴───────────────────────┤
│ Activity timeline / poslednji rezultat operacije                   │
└─────────────────────────────────────────────────────────────────────┘
```

## Demo controls

Poseban, vizuelno odvojen panel služi samo za vežbe i simulaciju kvarova:

- Locker Controller: `Normal`, `Jammed`, `Unavailable`;
- Message Gateway: `Normal`, `Unavailable`;
- prikaz poslednjih poruka Message Gateway simulatora;
- dugme za čišćenje simulator inbox-a.

Ovaj panel treba jasno označiti kao **Demo controls**, kako studenti ne bi pomešali simulator sa poslovnim UI-jem sistema.

## Predloženi tok na vežbama

1. Registruj paket.
2. Prikaži parcelu i dostupne pretince.
3. Pokreni `Store` i vizuelno označi izabrani pretinac.
4. U Message Gateway panelu prikaži generisani pickup kod.
5. Prebaci Locker Controller na `Jammed` i probaj pickup.
6. Pokaži da poslovno stanje nije završeno kada simulator vrati neuspeh.
7. Vrati simulator na `Normal` i ponovi pickup.

## Arhitektonsko pravilo

`ParcelBox.Web` ne treba da referencira `ParcelBox.Domain`, `ParcelBox.Application` ili `ParcelBox.Infrastructure`. UI komunicira HTTP-om sa ParcelBox API-jem. Demo panel može imati posebne HTTP klijente prema simulatorima, jer je to nastavnički/test harness, a ne poslovni deo aplikacije.

Predložena struktura:

```text
src/ParcelBox.Web/
├── Components/
│   ├── Pages/
│   │   └── Home.razor
│   └── Shared/
│       ├── ServiceStatus.razor
│       ├── LockerGrid.razor
│       └── DemoControls.razor
├── Clients/
│   ├── ParcelBoxApiClient.cs
│   ├── LockerSimulatorClient.cs
│   └── MessageGatewayClient.cs
├── Contracts/
└── Program.cs
```

Za ovaj demo nema potrebe za Redux-om, posebnim state framework-om, JavaScript SPA slojem niti komponentnom bibliotekom. Standardni Blazor i mali CSS su dovoljni.
