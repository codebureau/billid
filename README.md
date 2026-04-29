# billid

**Local-first time tracking and invoicing for solo consultants.**

billid is a Windows desktop application that helps independent consultants track their time, manage clients, and prepare invoices — entirely offline, with no subscription and no cloud dependency.

[![Build](https://github.com/codebureau/billid/actions/workflows/build.yml/badge.svg)](https://github.com/codebureau/billid/actions/workflows/build.yml)
[![Docs](https://github.com/codebureau/billid/actions/workflows/docs.yml/badge.svg)](https://codebureau.github.io/billid)

---

## Features

- Client management with per-client billing configuration (rate, cap, frequency)
- Work entry logging with markdown notes and work categories
- Invoice preparation from uninvoiced timesheet entries
- Per-client summary reports
- Local SQLite database — your data never leaves your machine

## Documentation

Full documentation is at **[codebureau.github.io/billid](https://codebureau.github.io/billid)**.

- [Getting started](https://codebureau.github.io/billid/getting-started/)
- [User guide](https://codebureau.github.io/billid/user-guide/)
- [Architecture](https://codebureau.github.io/billid/developer/architecture/)
- [Contributing](https://codebureau.github.io/billid/developer/contributing/)

## Quick start (from source)

### Prerequisites

- Windows 10/11
- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

### Build and run

```powershell
git clone https://github.com/codebureau/billid
cd billid
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

See [CONTRIBUTING.md](https://codebureau.github.io/billid/developer/contributing/) for the branch/PR workflow, coding conventions, and how to run tests.

## Licence

MIT
