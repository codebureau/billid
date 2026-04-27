# Getting Started

## Requirements

- Windows 10 or Windows 11
- [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) (included in the installer)

---

## Installation

1. Go to the [Releases page](https://github.com/codebureau/billable/releases)
2. Download the latest `billable-setup.exe`
3. Run the installer — Billable will be added to your Start menu
4. Launch **Billable**

!!! tip "Portable option"
    A portable `.zip` is also available on the releases page if you prefer to run without installing.

---

## First launch

On first launch, Billable creates its database at:

```
%APPDATA%\Billable\billable.db
```

No setup is required — the schema is initialised automatically.

---

## Creating your first client

1. Click **+ Add** in the left panel
2. Enter the client's name and company
3. Fill in billing details in the **Settings** tab:
    - Hourly rate
    - Invoice cap (optional)
    - Invoice frequency (optional)
4. Click **Save changes**

→ [More about client settings](user-guide/clients.md)

---

## Logging work

1. Select a client in the left panel
2. Go to the **Timesheet** tab
3. Click **+ Add entry** (or press `Insert`)
4. Fill in the date, hours, description, and category
5. Click **Save**

→ [More about the timesheet](user-guide/timesheet.md)

---

## Preparing an invoice

1. In the **Timesheet** tab, select the uninvoiced entries you want to include
2. Click **Prepare Invoice**
3. Review the invoice lines and confirm
4. The entries are marked as invoiced and appear in the **Invoices** tab

→ [More about invoices](user-guide/invoices.md)

---

## Building from source

If you'd prefer to build from source rather than use the installer:

```powershell
git clone https://github.com/codebureau/billable
cd billable
dotnet build src/WorkTracking.slnx
dotnet run --project src/WorkTracking.UI
```

→ [Developer setup guide](developer/architecture.md)
