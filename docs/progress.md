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

## Phase 2 — Data layer (complete)

### What was built

**Infrastructure** (`WorkTracking.Data/Database/`)

| Component | Detail |
|---|---|
| `IDatabaseConnectionFactory` | Interface for creating `SqliteConnection` instances |
| `DatabaseConnectionFactory` | Resolves DB path from `%APPDATA%\Billable\billable.db` |
| `SchemaInitializer` | Applies baseline schema + numbered migrations at startup; idempotent; migration version tracked in `setting` table |

**Helpers** (`WorkTracking.Data/Helpers/`)

| Component | Detail |
|---|---|
| `DateConversion` | `DateOnly` ↔ ISO-8601 `TEXT`, `DateTime` ↔ ISO-8601 `TEXT`, nullable variants |

**Repositories** (interface + implementation pairs)

| Repository | Key extras beyond basic CRUD |
|---|---|
| `ClientRepository` | `GetAllAsync` ordered by name |
| `WorkEntryRepository` | `GetFilteredAsync` (date range, invoiced flag, category); `MarkInvoicedAsync` (bulk) |
| `WorkCategoryRepository` | `GetByClientAsync`; `EnableForClientAsync`; `DisableForClientAsync` |
| `InvoiceRepository` | `AddWithLinesAsync` (transactional); `GetLinesAsync` |
| `AttachmentRepository` | `GetByWorkEntryAsync` |
| `SettingRepository` | `GetAsync` / `SetAsync` (upsert) / `GetAllAsync` |

**Schema SQL** (`WorkTracking.Data/schema.sql`) — embedded resource, all tables use `CREATE TABLE IF NOT EXISTS`.

**Migration infrastructure** (`WorkTracking.Data/Migrations/`) — `SchemaInitializer` auto-discovers and applies `migration_NNNN_*.sql` files in version order.

### Tests added

| Class | Tests | Coverage |
|---|---|---|
| `SchemaInitializerTests` | 2 | All tables created; idempotent run |
| `DateConversionTests` | 8 | Round-trips, nullables, DateTime format |
| `ClientRepositoryTests` | 7 | CRUD, nullable optional fields |
| `WorkEntryRepositoryTests` | 6 | CRUD, date filter, invoiced filter, `MarkInvoiced` |
| `WorkCategoryRepositoryTests` | 5 | CRUD, enable/disable per-client, no duplicates |
| `InvoiceRepositoryTests` | 5 | CRUD, `AddWithLines` transactional, line FK |
| `SettingRepositoryTests` | 5 | Get/set upsert, null value, `GetAll` |

**Total new: 38 tests — all passing. Cumulative total: 54.**

### Decisions made

- `SqliteConnection.ClearAllPools()` called in `SqliteTestFixture.Dispose()` — required to release the file lock before temp DB deletion in tests.
- `decimal` used for all financial values in C#; stored as `REAL` (double) in SQLite with explicit cast at the boundary.
- `SchemaInitializer` extended to support numbered migration files (`migration_NNNN_*.sql`) embedded in `WorkTracking.Data/Migrations/`. Current schema version tracked in the `setting` table under key `schema_version`.
- Migration files use additive-only DDL — no `DROP TABLE`, `DROP COLUMN`, or destructive renames (see copilot-instructions.md for full rules).
- Each migration is wrapped in a transaction; failure rolls back cleanly.

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
