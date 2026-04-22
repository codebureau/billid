# UI Specification

## Main layout

- Main window:
  - Left: Client list (searchable).
  - Right: Client detail area (tabs).

Tabs on client detail:

1. Timesheet
2. Invoices
3. Summary
4. Settings

---

## Timesheet tab

### Layout

- Top bar:
  - Date range filter (quick presets + custom).
  - Invoiced filter: [Uninvoiced] [Invoiced] [All].
  - Work category filter (optional).
  - Grouping dropdown: [None, Invoice, Work category, Month].

- Main area:
  - Data grid of work entries:
    - Date
    - Description
    - Hours
    - Work category
    - Invoiced status
    - Invoice number (if any)
    - Checkbox for selection (for invoice prep).

- Right or bottom:
  - Notes drawer:
    - Collapsible panel.
    - Shows Markdown editor/viewer for selected work entry.
    - Attachments list with open/add/remove.

- Footer:
  - Uninvoiced summary for current filters:
    - Total hours
    - Total amount
    - Breakdown by work category
  - If over cap:
    - Highlight total and show tooltip explaining cap.

### Behaviour

- Selecting a row updates the notes drawer.
- Filters and grouping update the grid and summary.
- “Prepare Invoice” button:
  - Enabled when one or more uninvoiced entries are selected.
  - Opens Invoice Prep dialog.

---

## Invoice Prep dialog

- Shows:
  - Selected work entries.
  - Total hours and amount.
  - Breakdown by work category.
  - Cap status (under/over).
- Fields:
  - Invoice number (required).
  - Invoice date (default: today).
  - Optional notes.
  - PDF file picker (optional at creation time).
- Actions:
  - Confirm:
    - Create invoice + invoice_line records.
    - Mark work entries as invoiced and link to invoice.
  - Cancel:
    - No changes.

---

## Invoices tab

- List of invoices for the client:
  - Invoice number
  - Date
  - Total amount
  - Count of work entries
- Selecting an invoice:
  - Shows:
    - Invoice lines (by work category).
    - Linked work entries.
    - Link/button to open PDF (if path stored).

---

## Summary tab

- Charts / summaries:
  - Hours per month (line or bar chart).
  - Hours per work category (pie or bar).
  - Invoices over time (bar).
  - Uninvoiced vs invoiced totals.
- Textual summaries:
  - Total hours this year.
  - Total invoiced amount this year.
  - Next invoice due date (if frequency set).
  - Invoice cap status (if cap set).

---

## Settings tab (per client)

- Editable fields:
  - Contact name
  - Company name
  - Address
  - ABN
  - Email
  - Phone
  - Hourly rate
  - Invoice cap amount (optional)
  - Invoice cap behaviour (warn/block/allow)
  - Invoice frequency in days (optional)
- Work categories:
  - List of all work categories with checkboxes.
  - Only checked categories appear in timesheet dropdown.

---

## Global views (future)

- Global reports window:
  - Hours per month (all clients).
  - Revenue per month.
  - Work category distribution.
  - Top clients by hours/revenue.

These can be implemented after core client-centric flows are stable.
