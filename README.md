# Billable

**Local-first time tracking and invoicing for solo consultants.**

Billable is a Windows desktop application that helps independent consultants track their time, manage clients, and prepare invoices — entirely offline, with no subscription and no cloud dependency.

[![Build](https://github.com/codebureau/billable/actions/workflows/build.yml/badge.svg)](https://github.com/codebureau/billable/actions/workflows/build.yml)
[![Docs](https://github.com/codebureau/billable/actions/workflows/docs.yml/badge.svg)](https://codebureau.github.io/billable)

---

## Features

- Client management with per-client billing configuration (rate, cap, frequency)
- Work entry logging with markdown notes and work categories
- Invoice preparation from uninvoiced timesheet entries
- Per-client summary reports
- Local SQLite database — your data never leaves your machine

## Documentation

Full documentation is at **[codebureau.github.io/billable](https://codebureau.github.io/billable)**.

- [Getting started](https://codebureau.github.io/billable/getting-started/)
- [User guide](https://codebureau.github.io/billable/user-guide/)
- [Architecture](https://codebureau.github.io/billable/developer/architecture/)
- [Contributing](https://codebureau.github.io/billable/developer/contributing/)

## Quick start (from source)

### Prerequisites

- Windows 10/11
- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

### Build and run

```powershell
git clone https://github.com/codebureau/billable
cd billable
dotnet run --project src/WorkTracking.UI
```

### Run tests

```powershell
dotnet test src/WorkTracking.slnx
```

## Tech stack

| Concern | Technology |
|---|---|
| UI | WPF (.NET 10), MVVM |
| Database | SQLite via `Microsoft.Data.Sqlite` |
| DI | `Microsoft.Extensions.DependencyInjection` |
| Testing | `xUnit`, `Moq`, `FluentAssertions` |

## Contributing

See [CONTRIBUTING.md](https://codebureau.github.io/billable/developer/contributing/) for the branch/PR workflow, coding conventions, and how to run tests.

## Licence

MIT
