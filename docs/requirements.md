# Requirements

## Core concepts

- **Client**: A consulting client with contact and billing preferences.
- **Work entry**: A unit of work performed for a client on a date, with hours, description, category, notes, and attachments.
- **Work category**: A system-wide classification (e.g., Development, Support) that can be enabled per client.
- **Invoice**: Metadata about an invoice created in an external system, linked to work entries.
- **Invoice line**: Grouped summary of work (usually by category) for a given invoice.
- **Attachment**: Files associated with a work entry.
- **Setting**: Global app configuration.

---

## Functional requirements

### Clients

- Capture:
  - Contact name
  - Company name
  - Address
  - ABN
  - Email
  - Phone
  - Hourly rate
  - Invoice cap (optional)
  - Invoice cap behaviour (warn/block/allow)
  - Invoice frequency (in days, optional)
- Enable/disable work categories per client.
- Show a client detail page with:
  - Timesheet
  - Invoices
  - Summary / reports
  - Settings

### Work entries

- Fields:
  - Client
  - Date
  - Description
  - Hours
  - Work category (optional but recommended)
  - Notes (Markdown)
  - Attachments
  - Invoiced flag
  - Invoice reference (invoice_id)
- Behaviour:
  - Default to uninvoiced.
  - Editable until invoiced (with some constraints).
  - Support filtering by:
    - Date range
    - Invoiced/uninvoiced/all
    - Work category
  - Support grouping by:
    - Invoice number
    - Work category
    - Time period (e.g., month)

### Invoicing

- Track invoices without generating them:
  - Invoice number
  - Invoice date
  - Total amount
  - PDF path
- Link work entries to invoices via `invoice_id`.
- Allow manual selection of uninvoiced work entries to prepare an invoice:
  - Show total hours and amount.
  - Show breakdown by work category.
  - Show cap status (under/over threshold).
- After invoice creation:
  - Mark selected work entries as invoiced.
  - Create invoice and invoice_line records.
  - Store invoice metadata and PDF path.

### Thresholds and alerts

- For each client:
  - Optional invoice cap (amount).
  - Cap = hourly rate × total uninvoiced hours (conceptually).
- Visual signals:
  - On client timesheet:
    - Highlight uninvoiced total when over cap.
    - Show subtle warning icon or banner.
  - Optionally show a badge in client list.

### Invoice frequency reminders

- For each client:
  - Optional invoice frequency in days (e.g., 90 for quarterly).
  - Track last invoice date.
  - Compute next invoice due date.
- Visual signals:
  - Badge or banner when invoice is due or overdue.
  - This appears on the client page and/or client list.

### Reporting

- Per-client summary:
  - Hours per month.
  - Hours per work category.
  - Invoices over time.
  - Uninvoiced vs invoiced totals.
- Global reports:
  - Hours per month across all clients.
  - Revenue per month (from invoices).
  - Work category distribution.
  - Workload trends.

---

## Non-functional requirements

- Local-first, single-user.
- No network dependency.
- Fast and responsive UI.
- Simple backup (DB file + attachments).
- Easy to deploy and update.
- Clear, predictable behaviour for invoicing and thresholds.
