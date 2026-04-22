# Running and Testing – Billable / WorkTracking

This file is updated at the end of every phase and always reflects the **current state** of the app: what you can run, what you can test, and how.

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- Windows (WPF requires Windows)
- Visual Studio 2022 17.10+ or VS 2026 (for `.slnx` solution support)
- Git

---

## Opening the solution

```powershell
# Clone (if not already)
git clone https://github.com/codebureau/billable
cd billable

# Open in Visual Studio
start src\WorkTracking.slnx
```

Or open `src\WorkTracking.slnx` directly from Visual Studio.

---

## Current phase: Phase 1 — Core domain models

### What is runnable

The WPF application shell (`WorkTracking.UI`) can be launched. It shows the default WPF template window — no application logic is wired up yet. All domain models and core business logic are in place but not yet visible in the UI.

### How to run the application

**From Visual Studio:**
1. Set `WorkTracking.UI` as the startup project (right-click → Set as Startup Project).
2. Press `F5` or click **Start**.

**From the terminal:**
```powershell
cd src\WorkTracking.UI
dotnet run
```

### What you will see

A blank WPF window titled "MainWindow". No data, no navigation — this is the scaffold only.

---

## How to run the tests

```powershell
# From the repo root
cd src
dotnet test WorkTracking.slnx
```

Or from Visual Studio: open **Test Explorer** and click **Run All**.

### Current test results

| Test class | Tests | Status |
|---|---|---|
| `InvoiceCapCalculatorTests` | 7 | ✅ Passing |
| `InvoiceFrequencyCalculatorTests` | 8 | ✅ Passing |
| **Total** | **15** | **✅ All passing** |

---

## How to build

```powershell
cd src
dotnet build WorkTracking.slnx
```

Expected output: `Build succeeded` with no errors or warnings.

---

## Upcoming (Phase 2)

After Phase 2 completes this file will be updated to include:
- How to run repository integration tests against a real (temp) SQLite database
- Schema initializer verification
- Date conversion round-trip tests
