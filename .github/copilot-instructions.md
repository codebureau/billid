# Copilot Instructions – WorkTracking / Billable

## Project overview

**Billable** is a local-first WPF (.NET 8) desktop application for solo consultants.
Internal namespaces are brand-agnostic and use the prefix `WorkTracking.*`.

---

## Solution structure

```
src/
  WorkTracking.sln
  WorkTracking.Core/        # Domain models + business logic (no dependencies)
  WorkTracking.Data/        # SQLite repositories + schema init (refs Core)
  WorkTracking.UI/          # WPF app, MVVM, DI bootstrap (refs Core + Data)
tests/
  WorkTracking.Tests/       # xUnit tests (refs Core + Data)
docs/
  architecture.md
  schema.sql
  requirements.md
  ui-spec.md
.github/
  copilot-instructions.md   # This file
```

---

## Tech stack

| Concern      | Library                                      |
|--------------|----------------------------------------------|
| UI           | WPF (.NET 10)                                |
| Database     | SQLite via `Microsoft.Data.Sqlite`           |
| DI           | `Microsoft.Extensions.DependencyInjection`   |
| Logging      | `Microsoft.Extensions.Logging`               |
| Markdown     | `Markdig`                                    |
| Testing      | `xUnit`, `Moq`, `FluentAssertions`           |

---

## Naming conventions

- **Models**: simple noun — `Client`, `WorkEntry`, `Invoice`
- **Interfaces**: `I` prefix — `IClientRepository`, `IDialogService`
- **Repositories**: match interface — `ClientRepository`
- **ViewModels**: suffix `ViewModel` — `ClientListViewModel`
- **Views**: suffix `View` — `ClientListView` (XAML)
- **Commands**: suffix `Command` — `PrepareInvoiceCommand`
- **Test classes**: suffix `Tests` — `ClientRepositoryTests`
- **Test fixtures**: suffix `Fixture` — `SqliteTestFixture`

---

## Coding conventions

- C# 12 features (primary constructors, collection expressions, etc.)
- Nullable reference types enabled on all projects (`<Nullable>enable</Nullable>`)
- `async`/`await` for **all** database operations — no `.Result` or `.Wait()`
- `ICommand` (via a `RelayCommand` helper) for all ViewModel actions — no code-behind event handlers
- No direct UI dependencies in ViewModels (no `Window`, `MessageBox`, etc.)
- Use `IDialogService` for dialogs, `INavigationService` for navigation
- `DateOnly` in domain models; convert to/from ISO-8601 `TEXT` (`YYYY-MM-DD`) at the data layer boundary only

---

## MVVM rules

- ViewModels inherit from a common `ViewModelBase` implementing `INotifyPropertyChanged`
- Use `ObservableCollection<T>` for bound lists
- Never reference a `View` from a `ViewModel`
- Views set `DataContext` via DI — no `new ViewModel()` in code-behind
- Commands are properties of type `ICommand` on the ViewModel

---

## UI theming rules

- **All new UI must work correctly in both Light and Dark themes.**
- Never use hard-coded colours (e.g. `Foreground="Black"`, `Background="#FFF"`) in any XAML View or Dialog.
- Always use `DynamicResource` for every colour reference — e.g. `Background="{DynamicResource SurfaceBrush}"`.
- Every `Window` (dialog) must set `Background="{DynamicResource SurfaceBrush}"` and `Foreground="{DynamicResource TextPrimaryBrush}"` on the root element.
- Every root layout panel inside a dialog must also set `Background="{DynamicResource SurfaceBrush}"`.
- Use the `{StaticResource SectionHeader}` style for section heading `TextBlock` elements — never set `FontWeight` inline.
- Use `{DynamicResource BorderBrush}` on `Separator` elements (handled by the global `Separator` style automatically).
- Available theme brushes (defined in `Themes/ModernTheme.xaml`, swapped by `ThemeService`):

| Key | Purpose |
|---|---|
| `BackgroundBrush` | Page / window background |
| `SurfaceBrush` | Card / dialog / panel surface |
| `SurfaceAltBrush` | Alternate row / subtle surface |
| `BorderBrush` | Borders, dividers |
| `TextPrimaryBrush` | Primary text |
| `TextSecondaryBrush` | Secondary / hint text |
| `AccentBrush` | Primary action colour |
| `HoverBrush` | General hover state |
| `DangerBrush` | Destructive actions |

---

## Data layer rules

- All DB access goes through repository interfaces — no raw SQL outside repository classes
- Schema is applied once at startup via `SchemaInitializer` (idempotent — uses `CREATE TABLE IF NOT EXISTS`)
- Connection string resolves the DB file from the user's `%APPDATA%\Billable\` folder
- Date conversion helpers live in `WorkTracking.Data.Helpers.DateConversion`
- Repository methods follow the pattern:
  - `GetAllAsync()`, `GetByIdAsync(int id)`
  - `AddAsync(T entity)`, `UpdateAsync(T entity)`, `DeleteAsync(int id)`

---

## Database schema versioning

All DDL changes must be **additive and backwards-compatible**. The schema must never be destructively altered in place.

### Rules

- **Never** use `DROP TABLE`, `DROP COLUMN`, or `ALTER TABLE … RENAME TO` as part of a migration without a new version guard.
- **Never** modify `schema.sql` to change an existing table definition — `schema.sql` defines the baseline (v1) schema only.
- All schema changes after the initial schema are applied as **numbered migrations**, executed in order at startup by `SchemaInitializer`.
- Migrations live in `WorkTracking.Data/Migrations/` as embedded SQL files named `migration_{version:D4}_{description}.sql` (e.g. `migration_0002_add_client_notes.sql`).
- The current schema version is tracked in the `setting` table under the key `schema_version`.
- `SchemaInitializer` reads the current version, then applies all unapplied migrations in order.

### Safe DDL patterns

| Operation | Safe approach |
|---|---|
| Add a column | `ALTER TABLE t ADD COLUMN col TYPE` (SQLite supports this; always add with a default or as nullable) |
| Add a table | `CREATE TABLE IF NOT EXISTS` |
| Add an index | `CREATE INDEX IF NOT EXISTS` |
| Remove a column | Add a new table, migrate data, rename — never drop directly |
| Rename a column | Add new column, copy data, deprecate old (do not rename — SQLite support is limited) |
| Change a column type | New column + data migration |

### Migration file structure

```sql
-- migration_0002_add_client_notes.sql
-- Applied: adds notes column to client table

ALTER TABLE client ADD COLUMN notes TEXT;
```

### Testing migrations

- Each migration must have an integration test in `WorkTracking.Tests/Data/Migrations/` verifying it applies cleanly to the previous schema version.
- The idempotency test in `SchemaInitializerTests` must remain passing after any migration is added.

---

## Dependency injection

- All registrations live in `WorkTracking.UI/DependencyInjection/ServiceCollectionExtensions.cs`
- Repositories registered as **Scoped**
- ViewModels registered as **Transient**
- Services (`IDialogService`, `INavigationService`) registered as **Singleton**
- `MainWindow` resolved via DI — do not `new` it

---

## Testing conventions

- All tests in `tests/WorkTracking.Tests`
- Repository integration tests use `SqliteTestFixture` — creates a fresh temp-file DB per test class
- ViewModel unit tests mock repositories via `Moq`
- Assertions use `FluentAssertions` (`result.Should().Be(...)`)
- Test method naming: `MethodName_Scenario_ExpectedOutcome`
  - e.g., `GetAllAsync_WithNoClients_ReturnsEmptyList`
- No test should write to the real app database

---

## Phase gate — required before advancing to the next phase

Before moving to the next phase, the following must be in place:

1. **Tests written and passing** — all new logic covered by unit or integration tests; `dotnet test` passes with no failures
2. **`docs/progress.md` updated** — documents what was built in the phase, what tests were added, and any decisions made
3. **`docs/running-and-testing.md` updated** — reflects the current state of the app: how to run it, how to run the tests, and what is testable at this point

---

## Issue tracking & branching workflow

- The backlog of work is stored in **GitHub Issues** on the `codebureau/billable` repo.
- Use the **GitHub CLI** (`gh`) to read and manage issues — e.g. `gh issue view 51 --repo codebureau/billable`.
- Before starting any issue, **present a plan to the user and wait for explicit approval**.
- Every issue must be developed on its **own branch** — branch name convention: `issue/<number>-<short-description>` (e.g. `issue/51-export-work-entries`).
- **Never commit or develop on `main`**.
- Create the branch from the latest `main` before beginning work.

---

## Source of truth

Always refer to the following docs for requirements and schema decisions:

- `docs/architecture.md`
- `docs/schema.sql`
- `docs/requirements.md`
- `docs/ui-spec.md`
- `docs/progress.md` — living build log, updated each phase
- `docs/running-and-testing.md` — how to run and test the app at the current state
