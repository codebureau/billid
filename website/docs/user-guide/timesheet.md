# Timesheet

The **Timesheet** tab is where you log all work performed for a client. Each row is a work entry.

---

## Adding a work entry

Click **+ Add entry** or press `Insert`. Fill in:

| Field | Notes |
|---|---|
| **Date** | Defaults to today |
| **Hours** | Decimal hours (e.g. `1.5` = 1h 30m) |
| **Description** | Brief summary of the work |
| **Category** | Optional — classify by type (Development, Support, etc.) |
| **Notes** | Full markdown notes for the entry |

Click **Save** or press `Enter` to confirm.

---

## Editing an entry

Select a row and click **Edit** or press `F2`. Make your changes and click **Save**.

---

## Deleting an entry

Select a row and click **Delete** or press `Delete`. Confirm the prompt.

!!! note
    Invoiced entries can still be edited or deleted. Doing so will not automatically update any external invoice.

---

## Preparing an invoice

1. Select the uninvoiced entries to include (use `Ctrl+Click` or `Shift+Click` for multiple)
2. Click **Prepare Invoice**
3. Review the grouped invoice lines
4. Confirm — entries are marked as invoiced and move to the **Invoices** tab

---

## Invoice cap warning

If your client has an **invoice cap** configured, the timesheet shows a progress indicator. Depending on the cap behaviour setting, barebill will warn or block when you approach or exceed the cap.

---

## Keyboard shortcuts

| Key | Action |
|---|---|
| `Insert` | Add new entry |
| `F2` | Edit selected entry |
| `Delete` | Delete selected entry |
| `F5` | Refresh list |

---

## Exporting work entries

Click **Export…** in the Timesheet footer to export the currently filtered work entries to a CSV file.

See [Exporting data](export.md) for full details.
