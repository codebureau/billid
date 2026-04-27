# Getting Started

## Installation

1. Go to the [Releases page](https://github.com/codebureau/billable/releases)
2. Download `billable.exe` from the latest release
3. Place it anywhere convenient -- your Desktop, `C:\Tools\`, wherever you prefer
4. Double-click to launch

No installer, no .NET prerequisite -- everything is bundled inside the single file.

!!! warning "Windows SmartScreen warning"
    Because the executable is not yet code-signed, Windows may show a SmartScreen
    prompt on first run. This is expected.
    Click **More info**, then **Run anyway** to proceed. This prompt only appears once.

---

## First launch

On first launch, Billable creates its database at:

```
%APPDATA%\Billable\billable.db
```

No setup is required -- the schema is initialised automatically.

---

## Creating your first client

1. Click **+ Add** in the left panel
2. Enter the client's name and company
3. Fill in billing details in the **Settings** tab:
    - Hourly rate
    - Invoice cap (optional)
    - Invoice frequency (optional)
4. Click **Save changes**

-> [More about client settings](user-guide/clients.md)

---

## Logging work

1. Select a client in the left panel
2. Go to the **Timesheet** tab
3. Click **+ Add entry** (or press `Insert`)
4. Fill in the date, hours, description, and category
5. Click **Save**

-> [More about the timesheet](user-guide/timesheet.md)

---

## Preparing an invoice

1. In the **Timesheet** tab, select the uninvoiced entries you want to include
2. Click **Prepare Invoice**
3. Review the invoice lines and confirm
4. The entries are marked as invoiced and appear in the **Invoices** tab

-> [More about invoices](user-guide/invoices.md)

---

## Building from source

If you prefer to build from source:

```powershell
git clone https://github.com/codebureau/billable
cd billable
dotnet build src/WorkTracking.slnx
dotnet run --project src/WorkTracking.UI
```

-> [Developer setup guide](developer/architecture.md)