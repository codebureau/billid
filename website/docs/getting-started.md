# Getting Started

## Installation

1. Go to the [Releases page](https://github.com/codebureau/billid/releases)
2. Download `billid-win-Setup.exe` from the latest release
3. Run the installer — it will install billid and create a Start Menu shortcut
4. Launch billid from the Start Menu

No .NET prerequisite — everything is bundled inside the installer.

!!! warning "Windows SmartScreen / Defender warning"
    Because the installer is not yet code-signed, Windows may show a SmartScreen
    prompt or flag the file as suspicious. This is a false positive caused by the
    installer having no code-signing reputation.

    Before running, verify the download using the `SHA256SUMS.txt` file attached to
    the release (see [Verifying your download](#verifying-your-download) below).
    Then click **More info → Run anyway** in the SmartScreen dialog.

---

## Verifying your download

Each release includes a `SHA256SUMS.txt` file on the [Releases page](https://github.com/codebureau/billid/releases). Use it to confirm your download hasn't been tampered with.

Open PowerShell in the folder where you saved the installer and run:

```powershell
(Get-FileHash .\billid-win-Setup.exe -Algorithm SHA256).Hash.ToLower()
```

Compare the output against the hash listed for `billid-win-Setup.exe` in `SHA256SUMS.txt`. They must match exactly.

---

## First launch

On first launch, billid creates its database at:

```
%APPDATA%\billid\billid.db
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
git clone https://github.com/codebureau/billid
cd billid
dotnet build src/WorkTracking.slnx
dotnet run --project src/WorkTracking.UI
```

-> [Developer setup guide](developer/architecture.md)