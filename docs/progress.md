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

## Phase 1 — Core domain models (complete)

### What was built

**Models** (`WorkTracking.Core/Models/`)

| Model | Key fields |
|---|---|
| `Client` | Name, contact, billing rate, cap, frequency, dates |
| `WorkCategory` | Name, description |
| `ClientWorkCategory` | Join entity (ClientId, WorkCategoryId) |
| `WorkEntry` | Date, hours, description, category, notes, invoiced flag |
| `Invoice` | Invoice number, date, total, PDF path |
| `InvoiceLine` | Hours, rate, amount, category reference |
| `Attachment` | File path, filename, MIME type |
| `Setting` | Key/value |

**Enums** (`WorkTracking.Core/Enums/`)

| Enum | Values |
|---|---|
| `InvoiceCapStatus` | `NoCap`, `UnderCap`, `AtCap`, `OverCap` |
| `InvoiceFrequencyStatus` | `NoFrequency`, `OnTrack`, `Due`, `Overdue` |

**DTOs** (`WorkTracking.Core/DTOs/`)

| DTO | Purpose |
|---|---|
| `InvoicePrepSummary` | Computed summary for invoice prep dialog (total hours, amount, cap status, category breakdown) |
| `InvoicePrepCategoryLine` | Per-category line within an `InvoicePrepSummary` |

**Business logic** (`WorkTracking.Core/Services/`)

| Class | Responsibility |
|---|---|
| `InvoiceCapCalculator` | Calculates cap status and total billable amount from work entries |
| `InvoiceFrequencyCalculator` | Calculates next invoice due date and frequency status |

### Tests added

| Class | Tests | Coverage |
|---|---|---|
| `InvoiceCapCalculatorTests` | 7 | Null cap, zero cap, under/at/over cap, total amount calculation, empty entries |
| `InvoiceFrequencyCalculatorTests` | 8 | Null inputs, zero frequency, valid date calculation, on-track/due/overdue status |

**Total: 15 tests — all passing.**

### Decisions made

- Used `decimal` for `Hours`, `HourlyRate`, `TotalAmount`, `InvoiceCapAmount` — avoids floating-point precision issues for financial values.
- `InvoiceCapBehavior` stored as `string?` on the model to match the SQLite schema (`'warn'`, `'block'`, `'allow'`); a typed enum can be added in a later phase.
- Business logic lives in static calculator classes in `WorkTracking.Core.Services` — keeps models as pure POCOs and makes logic trivially testable without mocking.

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
