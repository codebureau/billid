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

## Current phase: Phase 7 — CRUD operations

### What is runnable

The app is now fully usable without any raw SQL setup. On launch:

1. Schema initializer creates/updates `%APPDATA%\Billable\billable.db`.
2. Main window opens — click **+ Add** in the client list to create your first client.
3. Select a client to activate all four tabs:
   - **Timesheet** — use **+ Add entry** to log work; select a row to **✏ Edit** or **🗑 Delete** it; select uninvoiced entries and click **Prepare Invoice**.
   - **Invoices** — lists all invoices; selecting one shows lines and linked entries.
   - **Summary** — this-year totals, cap/frequency status, charts.
   - **Settings** — edit all client fields; manage work categories (enable/disable existing, add new via inline field + **+ Add category** button); **Save changes**.
4. **Delete client** button is in the client detail header (requires confirmation).

### How to run the application

**From Visual Studio:**
1. Set `WorkTracking.UI` as the startup project.
2. Press `F5`.

**From the terminal:**
```powershell
cd src\WorkTracking.UI
dotnet run
```

### What you will see

- Window titled **Billable**, 1100x650 px.
- On first run the client list is empty — click **+ Add** to create a client.
- All data entry flows through dialogs — no raw SQL required.

---

## How to run the tests

```powershell
cd src
dotnet test WorkTracking.slnx
```

Or from Visual Studio: open **Test Explorer** and click **Run All**.

### Current test results

| Test class | Tests | Status |
|---|---|---|
| `InvoiceCapCalculatorTests` | 7 | Passing |
| `InvoiceFrequencyCalculatorTests` | 8 | Passing |
| `SchemaInitializerTests` | 2 | Passing |
| `DateConversionTests` | 8 | Passing |
| `ClientRepositoryTests` | 7 | Passing |
| `WorkEntryRepositoryTests` | 6 | Passing |
| `WorkCategoryRepositoryTests` | 5 | Passing |
| `InvoiceRepositoryTests` | 5 | Passing |
| `SettingRepositoryTests` | 5 | Passing |
| `ClientDetailViewModelTests` | 6 | Passing |
| `ClientListViewModelTests` | 6 | Passing |
| `MainWindowViewModelTests` | 4 | Passing |
| `TimesheetViewModelTests` | 12 | Passing |
| `InvoicesViewModelTests` | 7 | Passing |
| `SummaryViewModelTests` | 8 | Passing |
| `InvoicePrepViewModelTests` | 8 | Passing |
| `ClientSettingsViewModelTests` | 14 | Passing |
| `AddClientViewModelTests` | 5 | Passing |
| `ClientListViewModelCrudTests` | 5 | Passing |
| `WorkEntryDialogViewModelTests` | 7 | Passing |
| **Total** | **139** | **All passing** |

---

## How to build

```powershell
cd src
dotnet build WorkTracking.slnx
```

Expected output: `Build succeeded` with no errors or warnings.