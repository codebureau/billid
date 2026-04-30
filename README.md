# barebill

**Local-first time tracking and invoicing for solo consultants.**

barebill is a Windows desktop application that helps independent consultants track their time, manage clients, and prepare invoices — entirely offline, with no subscription and no cloud dependency.

[![Build](https://github.com/codebureau/barebill/actions/workflows/build.yml/badge.svg)](https://github.com/codebureau/barebill/actions/workflows/build.yml)
[![Docs](https://github.com/codebureau/barebill/actions/workflows/docs.yml/badge.svg)](https://codebureau.github.io/barebill)

---

## Features

- Client management with per-client billing configuration (rate, cap, frequency)
- Work entry logging with markdown notes and work categories
- Invoice preparation from uninvoiced timesheet entries
- Per-client summary reports
- Local SQLite database — your data never leaves your machine

## Documentation

Full documentation is at **[codebureau.github.io/barebill](https://codebureau.github.io/barebill)**.

- [Getting started](https://codebureau.github.io/barebill/getting-started/)
- [User guide](https://codebureau.github.io/barebill/user-guide/)
- [Architecture](https://codebureau.github.io/barebill/developer/architecture/)
- [Contributing](https://codebureau.github.io/barebill/developer/contributing/)

## Quick start (from source)

### Prerequisites

- Windows 10/11
- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

### Build and run

```powershell
git clone https://github.com/codebureau/barebill
cd barebill
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

See [CONTRIBUTING.md](https://codebureau.github.io/barebill/developer/contributing/) for the branch/PR workflow, coding conventions, and how to run tests.

## Licence

MIT
