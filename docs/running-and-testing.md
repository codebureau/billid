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

## Current phase: Phase 6 — Client Settings tab

### What is runnable

All four client tabs are now fully functional. On launch:

1. Schema initializer creates/updates `%APPDATA%\Billable\billable.db`.
2. Main window opens with the client list on the left.
3. Selecting a client loads all four tabs:
   - **Timesheet** — work entry grid with filters, notes drawer, footer totals, and a working **Prepare Invoice** button.
   - **Invoices** — lists all invoices; selecting one shows lines and linked entries; Open PDF button.
   - **Summary** — this-year totals, uninvoiced summary, cap/frequency status badges, hours-per-month and by-category breakdowns.
   - **Settings** — editable form for all client fields + work category enable/disable checkboxes; **Save changes** button.

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
- Select a client to activate all four tabs.
- The grid is empty on a fresh database. Add test data (see below) to see entries.

### Adding test data

```powershell
$db = "$env:APPDATA\Billable\billable.db"
sqlite3 $db "INSERT INTO client (name, company_name, hourly_rate) VALUES ('Acme', 'Acme Corp', 150);"
sqlite3 $db "INSERT INTO work_category (name) VALUES ('Development'), ('Support');"
sqlite3 $db "INSERT INTO client_work_category (client_id, work_category_id) VALUES (1, 1), (1, 2);"
sqlite3 $db "INSERT INTO work_entry (client_id, date, description, hours, work_category_id, invoiced_flag) VALUES (1, '2025-06-01', 'Build feature X', 4.5, 1, 0);"
sqlite3 $db "INSERT INTO work_entry (client_id, date, description, hours, work_category_id, invoiced_flag) VALUES (1, '2025-06-03', 'Client support call', 1.0, 2, 0);"
```

Then restart the app and select Acme.

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
| **Total** | **122** | **All passing** |

---

## How to build

```powershell
cd src
dotnet build WorkTracking.slnx
```

Expected output: `Build succeeded` with no errors or warnings.