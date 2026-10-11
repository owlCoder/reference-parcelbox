# ParcelBox

A .NET 10 reference application for software engineering exercises. ParcelBox demonstrates layered design, dependency injection, repository and service interfaces, explicit operation results, and integration with external systems.

The project models a parcel locker workflow. It is deliberately small enough to study in class, rather than a production-ready parcel delivery platform.

## What it does

1. A courier registers a parcel with a tracking code, recipient phone number, and size.
2. The application selects a compatible available compartment and asks the **Locker Controller** simulator to open it.
3. Once the parcel is stored, the application creates a pickup code and sends it through the **Message Gateway** simulator.
4. The recipient enters the tracking code and pickup code to collect the parcel.

Both simulators provide failure modes so students can observe how the system handles unavailable services, jammed compartments, and unsuccessful notifications.

## Run locally

**Requirements:** [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10). The launch scripts build and start the API, Web app, and both simulators.

**Windows (PowerShell):**

```powershell
./scripts/run-all.ps1
```

**Linux/macOS (Bash):**

```bash
./scripts/run-all.sh
```

Or use Docker:

```bash
docker compose up --build
```

| Component | Local address |
| --- | --- |
| Blazor Web app | http://localhost:5200 |
| ParcelBox API | http://localhost:5100 |
| Locker Controller simulator | http://localhost:5101 |
| Message Gateway simulator | http://localhost:5102 |

The API uses SQLite. With Docker Compose, database data is kept in the `parcelbox-data` volume.

## Try the demo

Open **http://localhost:5200**, register a parcel, and select **Store in available compartment**. Find the pickup code in the Message Gateway inbox, then complete pickup using the parcel's tracking code and that code.

Try changing the simulators' modes in the Web dashboard:

- **Locker Controller — Jammed / Unavailable:** opening a compartment fails or becomes uncertain.
- **Message Gateway — Unavailable:** notification delivery fails without undoing a successfully saved storage operation.

**Recovery matters:** ParcelBox reserves parcel and compartment state before contacting the controller. If a physical operation or final database save has an uncertain outcome, the affected resource remains in a pending/reserved state for operator review. It is not automatically released or retried. See [recovery guidance](docs/recovery.md).

## Architecture

```text
src/
├── ParcelBox.Domain/          Domain models and enums
├── ParcelBox.Application/     Interfaces, services, DTOs, results
├── ParcelBox.Infrastructure/  EF Core, SQLite, HTTP and security adapters
├── ParcelBox.Api/             HTTP endpoints and dependency injection
└── ParcelBox.Web/             Standalone Blazor demo client

simulators/
├── ParcelBox.Simulators.LockerController/
└── ParcelBox.Simulators.MessageGateway/

tests/
└── ParcelBox.Application.Tests/
```

Backend dependencies point inward:

```text
Api ──────────> Application ──────────> Domain
 │                    ^
 └──> Infrastructure ─┘

Web ──HTTP──> Api
Infrastructure ──HTTP──> Simulators
```

- **Domain** defines parcel, compartment, and pickup-access state. It does not depend on application services or database frameworks.
- **Application** contains use-case services and small interfaces for repositories, persistence, external controllers, and security. Expected failures use typed `Result` values.
- **Infrastructure** implements those interfaces using EF Core/SQLite and HTTP clients.
- **API** exposes the operations over HTTP and configures dependency injection.
- **Web** communicates through HTTP; it does not reference backend projects directly.
- **Simulators** imitate external hardware and messaging systems without implementing ParcelBox business rules.

For a more detailed walkthrough, see [architecture](docs/architecture.md) and the [architecture decision record](docs/adr/0001-clean-architecture.md).

## Code conventions

This repository is intended as readable C# teaching material: one public type per file, interfaces separate from implementations, regular classes instead of `record`, initialized DTOs, explicit results for expected failures, and conventional Visual Studio formatting. Keep method and constructor signatures on one line when practical.

See [CONTRIBUTING.md](CONTRIBUTING.md) for the detailed rules.

## Validate changes

Run these commands from the repository root (the architecture script requires Bash):

```bash
./scripts/check-architecture.sh
dotnet restore
dotnet format --verify-no-changes --no-restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
```

GitHub Actions also checks architecture boundaries, formatting, compilation, tests, simulator mode switching, and Blazor static assets.

## Further reading

- [Architecture and service responsibilities](docs/architecture.md)
- [Safe recovery after uncertain external operations](docs/recovery.md)
- [Blazor dashboard and simulator walkthrough](docs/demo-ui.md)
- [Contribution guidelines](CONTRIBUTING.md)
