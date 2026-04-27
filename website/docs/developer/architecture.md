# Architecture

Billable is a local-first WPF desktop application built on .NET 10, using MVVM and SQLite.

---

## Tech stack

| Concern | Technology |
|---|---|
| UI | WPF (.NET 10), MVVM |
| Database | SQLite via `Microsoft.Data.Sqlite` |
| DI | `Microsoft.Extensions.DependencyInjection` |
| Logging | `Microsoft.Extensions.Logging` |
| Markdown | `Markdig` |
| Testing | `xUnit`, `Moq`, `FluentAssertions` |

---

## Solution structure

```
src/
  WorkTracking.sln
  WorkTracking.Core/      # Domain models + business logic (no dependencies)
  WorkTracking.Data/      # SQLite repositories + schema initialisation
  WorkTracking.UI/        # WPF app, MVVM, DI bootstrap
tests/
  WorkTracking.Tests/     # xUnit integration + unit tests
docs/                     # Internal dev docs
website/                  # MkDocs source for this site
design/                   # Affinity Designer source files for branding assets
```

---

## MVVM pattern

ViewModels inherit from `ViewModelBase` (implements `INotifyPropertyChanged`). Views bind via `DataContext` set through DI — no `new ViewModel()` in code-behind.

Commands are `ICommand` properties on ViewModels, implemented via `RelayCommand`.

---

## Data layer

All database access goes through repository interfaces. No raw SQL exists outside repository classes.

Schema is applied at startup via `SchemaInitializer` (idempotent). Migrations live in `WorkTracking.Data/Migrations/` as numbered SQL files.

Data is stored at `%APPDATA%\Billable\billable.db`.

---

## Dependency injection

Registrations are in `WorkTracking.UI/DependencyInjection/ServiceCollectionExtensions.cs`:

- **Repositories** — Scoped
- **ViewModels** — Transient
- **Services** (`IDialogService`, `INavigationService`) — Singleton

---

## Running locally

### Prerequisites

- Windows 10/11
- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- Visual Studio 2022 17.10+ or VS 2026

### Build and run

```powershell
git clone https://github.com/codebureau/billable
cd billable
dotnet build src/WorkTracking.slnx
dotnet run --project src/WorkTracking.UI
```

### Run tests

```powershell
dotnet test src/WorkTracking.slnx
```
