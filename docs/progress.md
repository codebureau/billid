# Build Progress Log

This file is updated at the end of every phase and serves as the living record of what has been built, what tests exist, and any decisions made along the way.

---

## Phase 0 — Scaffold (complete)

### What was built

| Item | Detail |
|---|---|
| `.github/copilot-instructions.md` | Workspace-level Copilot conventions (naming, patterns, test rules, phase gate) |
| `.gitignore` | Standard .NET gitignore |
| `src/WorkTracking.slnx` | Solution file (.NET 10 `.slnx` format) |
| `WorkTracking.Core` | Class library, `net10.0`, nullable + implicit usings enabled |
| `WorkTracking.Data` | Class library → refs Core; `Microsoft.Data.Sqlite` package |
| `WorkTracking.UI` | WPF app → refs Core + Data; `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Logging`, `Microsoft.Extensions.Logging.Console`, `Markdig` packages |
| `WorkTracking.Tests` | xUnit test project → refs Core + Data; `Moq`, `FluentAssertions` packages |

### Tests added

None — Phase 0 is scaffolding only. No business logic exists yet to test.

### Decisions made

- Targeting **net10.0** — .NET 10 SDK is the installed version; .NET 8 SDK is not present.
- Solution uses `.slnx` format (new in .NET 10). Visual Studio 2022 17.10+ and VS 2026 support this natively.
- Markdig version 1.1.3 (latest stable at time of scaffold).
- FluentAssertions version 8.9.0 (latest stable).
- Moq version 4.20.72 (latest stable).

---

## Phase 1 — Core domain models

_To be completed._

---

## Phase 2 — Data layer

_To be completed._

---

## Phase 3 — UI shell & navigation

_To be completed._

---

## Phase 4 — Timesheet feature

_To be completed._

---

## Phase 5 — Invoices & Summary tabs

_To be completed._

---

## Phase 6 — Client Settings tab

_To be completed._

---

## Phase 7 — Polish & logging

_To be completed._
