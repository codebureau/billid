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

## Phase 3 — UI shell & navigation (complete)

### What was built

**Main window** (`WorkTracking.UI/MainWindow.xaml`)

| Component | Detail |
|---|---|
| Two-column layout | Client list (left, 220 px, resizable) + client detail area (right) |
| Client list panel | Header, live search box, `ListBox` showing client name + company |
| Client detail area | "Select a client" placeholder when nothing selected; `TabControl` with 4 tabs when a client is selected |
| Tab placeholders | Timesheet, Invoices, Summary, Settings — each with a "coming in Phase N" label |
| `BoolToVisibilityConverter` / `InverseBoolToVisibilityConverter` | Used to toggle placeholder vs. detail panel |

**ViewModels** (`WorkTracking.UI/ViewModels/`)

| Class | Responsibility |
|---|---|
| `ViewModelBase` | `INotifyPropertyChanged` + `SetField` helper |
| `MainWindowViewModel` | Owns `ClientListViewModel` + `ClientDetailViewModel`; wires selection change to detail load |
| `ClientListViewModel` | Loads clients from `IClientRepository`; live search filter |
| `ClientDetailViewModel` | Holds selected `Client`; `HasClient` flag; `SelectedTabIndex`; `LoadClient` / `Clear` |

**Commands and services**

| Item | Detail |
|---|---|
| `RelayCommand` | `ICommand` implementation for ViewModel actions |
| `IDialogService` / `DialogService` | Stub for future dialog support |
| `INavigationService` / `NavigationService` | Tracks current navigation destination |

**DI bootstrap** (`WorkTracking.UI/DependencyInjection/ServiceCollectionExtensions.cs`)
Repositories (Scoped), ViewModels (Transient), Services (Singleton), `MainWindow` (Singleton).

**App startup** (`App.xaml.cs`)
Builds DI container, runs `SchemaInitializer`, resolves `MainWindow` + `MainWindowViewModel`, calls `InitializeAsync`.

### Tests added

| Class | Tests | Coverage |
|---|---|---|
| `ClientDetailViewModelTests` | 6 | `HasClient` default, `LoadClient`, tab reset, `Clear`, `PropertyChanged` for both mutations |
| `ClientListViewModelTests` | 6 | Load, empty load, search filter (case-insensitive), clear search, no match, `SelectedClient` property change |
| `MainWindowViewModelTests` | 4 | `InitializeAsync` loads list, selecting client populates detail, deselecting clears detail, empty list leaves detail empty |

**Total new: 16 tests — all passing. Cumulative total: 70.**

### Decisions made

- Test project TFM updated to `net10.0-windows` and a project reference to `WorkTracking.UI` added — required to test WPF-dependent ViewModels.
- `NavigationService` is a minimal stub — full navigation between top-level views deferred to a later phase.
- `DialogService` is a minimal stub — concrete dialog support implemented per-feature as needed.

---

## Phase 4 — Timesheet feature (complete)

### What was built

**ViewModels** (`WorkTracking.UI/ViewModels/`)

| Class | Responsibility |
|---|---|
| `WorkEntryRowViewModel` | Wraps `WorkEntry`; exposes `IsSelected` (notifying) for invoice prep checkboxes; `CategoryName`, `InvoiceRef`, `NotesMarkdown` |
| `CategorySummaryLine` | Immutable record for the footer breakdown: category name, hours, amount |
| `TimesheetViewModel` | Full timesheet VM — loads entries, filters (invoiced/date/category), summary totals, cap check, category breakdown, notes drawer toggle, Prepare Invoice command stub |

Key `TimesheetViewModel` features:
- `LoadAsync(clientId, hourlyRate, invoiceCapAmount)` — loads categories then entries
- `ApplyFiltersAsync()` — reloads from DB using current filter state; public for testability
- `InvoicedFilterOptions` / `InvoicedFilterText` — "Uninvoiced" (default), "Invoiced", "All"
- `FilterStartDate` / `FilterEndDate` — `DateTime?` wrappers (WPF `DatePicker` compatible); backed by `DateOnly?`
- Date range preset commands: This Month, Last Month, This Year, All Time
- `CategoriesWithAll` — category list prepended with "All categories" sentinel for filter ComboBox
- `TotalUninvoicedHours`, `TotalUninvoicedAmount`, `IsOverCap`, `CategorySummaryLines` — all computed from current `Entries`
- `HasAnySelectedUninvoiced` — drives `PrepareInvoiceCommand.CanExecute`
- `PrepareInvoiceCommand` — stub, wired up in Phase 5

**View** (`WorkTracking.UI/Views/TimesheetView.xaml`)

| Section | Detail |
|---|---|
| Toolbar | Period preset buttons, custom date pickers (`DatePicker`), invoiced filter combo, category filter combo, Notes drawer toggle |
| DataGrid | Checkbox, Date, Description, Hours, Category, Invoiced, Inv. Ref columns; `IsSelected` bound two-way |
| Notes drawer | Collapsible 280 px panel; shows `NotesMarkdown` of selected entry as read-only text; attachments placeholder |
| Footer | Uninvoiced total (hours + amount); over-cap warning in red; per-category breakdown; Prepare Invoice button |

**`ClientDetailViewModel`** updated: injects `TimesheetViewModel`; calls `Timesheet.LoadAsync` fire-and-forget in `LoadClient`.

### Tests added

| Class | Tests | Coverage |
|---|---|---|
| `TimesheetViewModelTests` | 12 | Load with/without entries, uninvoiced hours, amount calculation, over/under/no cap, category grouping, selection flag, filter text Invoiced/All |
| `ClientDetailViewModelTests` (updated) | 6 | Updated to inject `TimesheetViewModel` via helper |
| `MainWindowViewModelTests` (updated) | 4 | Updated to inject `TimesheetViewModel` via helper |

**Total new: 12 tests — all passing. Cumulative total: 83.**

### Decisions made

- `FilterStartDate`/`FilterEndDate` exposed as `DateTime?` on the ViewModel (WPF `DatePicker.SelectedDate` uses `DateTime?`); internally stored as `DateOnly?` and converted at the boundary.
- `ApplyFiltersAsync()` is `public` to enable direct test control without relying on fire-and-forget timing.
- Date preset commands bypass property setters (direct field assignment + `OnPropertyChanged`) to avoid double DB loads.
- Category filter uses an "All categories" sentinel (`WorkCategory` with `Id = 0`) to avoid null items in the ComboBox.
- `PrepareInvoiceCommand` is a stub — implementation deferred to Phase 5.
- Notes drawer shows `NotesMarkdown` as plain text; Markdig rendering deferred to a later phase.

---

## Phase 5 — Invoices & Summary tabs (complete)

### What was built

**ViewModels** (`WorkTracking.UI/ViewModels/`)

| Class | Responsibility |
|---|---|
| `InvoiceRowViewModel` | Wraps `Invoice`; exposes `Lines`, `LinkedEntries`, `EntryCount`, `HasPdf` |
| `InvoicesViewModel` | Loads and lists invoices per client; lazy-loads lines + linked entries on selection; `HasInvoices`, `IsLoading`, `OpenPdfCommand` |
| `SummaryViewModel` | Computes this-year totals (hours, invoiced amount), uninvoiced hours/amount, cap status, frequency status, `HoursPerMonth`, `HoursByCategory`; `FrequencyStatusLabel` |
| `InvoicePrepViewModel` | Holds invoice number, date, PDF path; groups selected entries into `LinesByCategory`; `ConfirmCommand` / `CancelCommand`; raises `CloseRequested` |
| `InvoicePrepLine` | Immutable record: `WorkCategoryId`, `CategoryName`, `Hours`, `Amount` |

**Views** (`WorkTracking.UI/Views/`)

| View | Detail |
|---|---|
| `InvoicesView.xaml` | Two-column layout: invoice list (left) + detail panel (right); "No invoices yet" placeholder; linked entries + lines display; Open PDF button |
| `SummaryView.xaml` | Scrollable stats panel: this-year totals, uninvoiced summary, cap status badge (green/red), frequency status badge, hours-per-month bar chart (ItemsControl), hours-by-category breakdown |
| `InvoicePrepDialog.xaml` | Modal dialog: invoice number, date picker, optional PDF path, per-category line table, over-cap warning, total, Confirm/Cancel |

**Services** (`WorkTracking.UI/Services/`)

| Item | Detail |
|---|---|
| `IDialogService` | Added `ShowInvoicePrepDialog(InvoicePrepViewModel)` |
| `DialogService` | Implemented `ShowInvoicePrepDialog` — opens `InvoicePrepDialog` as a modal `Window` |

**`TimesheetViewModel`** — `PrepareInvoiceCommand` fully implemented: builds `InvoicePrepViewModel`, shows dialog, then persists `Invoice` + `InvoiceLines` via repository, calls `MarkInvoicedAsync` on selected entries, fires `InvoiceCreated` event.

**`ClientDetailViewModel`** — updated to inject `InvoicesViewModel` + `SummaryViewModel`; `LoadClient` calls `Invoices.LoadAsync` + `Summary.LoadAsync` fire-and-forget; subscribes to `Timesheet.InvoiceCreated` to refresh both tabs.

**DI** — `InvoicesViewModel` and `SummaryViewModel` registered as Transient in `ServiceCollectionExtensions`.

### Tests added

| Class | Tests | Coverage |
|---|---|---|
| `InvoicesViewModelTests` | 7 | Load with/without invoices, `HasInvoices`, `IsLoading`, `SelectedInvoice` clears on reload, detail (lines + entries) loaded on selection |
| `SummaryViewModelTests` | 8 | All-zero baseline, uninvoiced hours/amount, this-year hours filter, this-year invoiced filter, over-cap, `HasCap`, `HoursPerMonth`, `HoursByCategory` grouping, `IsLoading` |
| `InvoicePrepViewModelTests` | 8 | Total hours/amount, no-cap, over-cap, category grouping, `ConfirmCommand` can/cannot execute, confirm sets `Confirmed` + raises event, cancel raises event with false, default date |

**Total new: 23 tests — all passing. Cumulative total: 108.**

### Decisions made

- `InvoicePrepViewModel` only counts non-invoiced entries in totals/lines — invoiced entries passed in `SelectedEntries` are ignored for financial calculations.
- `SummaryViewModel.HoursPerMonth` covers Jan through the current month of the current year only — no future months shown.
- `HoursByCategory` groups all-time entries (not just this year) to give a full picture of category distribution.
- `ClientDetailViewModel` subscribes to `Timesheet.InvoiceCreated` inside `LoadClient` — this means repeated calls to `LoadClient` accumulate handlers; a future clean-up pass should unsubscribe on clear.
- `DialogService.ShowInvoicePrepDialog` creates a new `Window` wrapping the XAML dialog and uses `ShowDialog()` — dialog result driven by `CloseRequested` event from the ViewModel.

---

## Phase 6 — Client Settings tab

_To be completed._

---

## Phase 7 — Polish & logging

_To be completed._
