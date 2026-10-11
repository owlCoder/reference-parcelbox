# Oporavak stanja ParcelBox-a

Ovaj primer čuva **bezbednost zauzimanja** i kada se stanje spoljnog uređaja ne može pouzdano proveriti.
Automatski retry koji ponovo otvara vrata može biti opasan: HTTP timeout ne znači da kontroler nije otvorio pretinac.

## Stanja koja zahtevaju pregled

| Stanje | Značenje |
| --- | --- |
| `ParcelStatus.StoragePending = 4` | Smeštanje je započeto; rezultat može biti neizvestan. |
| `ParcelStatus.PickupPending = 5` | Preuzimanje je započeto; vrata su možda otvorena. |
| `CompartmentStatus.Reserved = 3` | Pretinac je atomski zauzet, ali završni status nije potvrđen. |

Prijava greške `StorageRecoveryRequired` ili `PickupRecoveryRequired` nije dozvola za novi pokušaj istog
fizičkog postupka. Postojeći redovi ostaju blokirani da bi se sprečila dvostruka dodela ili otvaranje.

## Dijagnostika (samo čitanje)

Napraviti rezervnu kopiju SQLite baze pre bilo kakve administrativne intervencije.
Primeri upita nad podrazumevanom bazom `parcelbox.db`:

```sql
SELECT Id, TrackingCode, Status, LockerCode, CompartmentNumber
FROM parcels
WHERE Status IN (4, 5);

SELECT Id, LockerCode, Number, Status, ParcelId
FROM compartments
WHERE Status = 3;
```

Zabeležiti događaje kontrolera i fizički proveriti da li su vrata otvorena,
da li je paket zaista smešten/preuzet i da li je korisnik već dobio kod.

## Ručno razrešenje

1. Ako **fizičko otvaranje nije nastupilo**, administrator može kontrolisano vratiti
   početno stanje, ali tek kada potvrdi da nijedan zahtev više nije u toku.
2. Ako je **smeštanje fizički završeno**, mora se uskladiti paket, pretinac i pickup pristup
   u jednoj DB transakciji; nije dovoljno promeniti samo jedan status.
3. Ako je **preuzimanje fizički završeno**, treba uskladiti `PickedUp`,
   oslobađanje pretinca i korišćenje pickup pristupa u istoj transakciji.
4. Ako stanje nije utvrđeno, ostaviti zapis blokiran i eskalirati slučaj.
   Ne raditi slepo `UPDATE ... SET Status = Available`.

Referentna aplikacija nema administratorski endpoint za ove korake upravo zato što
ne može dokazati fizičko stanje uređaja. U produkciji se dodaje audit log,
operaterski workflow i eventualno idempotentno očitavanje/sinhronizacija kontrolera.

## Zašto nema automatskog rollback-a?

Lokalni SQL rollback ne zatvara fizička vrata i ne povlači poslatu poruku.
Sistem zato prvo trajno rezerviše resurse, a tek onda zove spoljni uređaj.
Do kraja neuspešne operacije ostaje moguće rekonstruisati šta je rezervisano
i sprečiti drugi zahtev da koristi isti resurs.
