# Copilot Bootstrap – barebill / WorkTracking

You are helping build a WPF (.NET 8) desktop application called **barebill**.

Internal architecture is brand-agnostic and uses the namespace prefix **WorkTracking**.

## Tech stack

- .NET 8
- WPF (MVVM)
- SQLite (file-based)
- Microsoft.Data.Sqlite
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Logging
- Markdown rendering via Markdig (or similar)

## High-level goal

Implement a local-first Windows desktop app for a solo consultant to:

- Track work entries (date, hours, description, category, notes, attachments).
- Track clients and their billing preferences.
- Track invoices (metadata only) and link work entries to invoices.
- See summaries and basic reports over time.

## Documentation

Use the following docs in `/docs` as the source of truth:

- `architecture.md`
- `schema.sql`
- `requirements.md`
- `ui-spec.md`

## What you should generate

1. Create a new solution under `/src`:
   - Solution name: `WorkTracking`
2. Create three projects:
   - `WorkTracking.Core` (class library)
   - `WorkTracking.Data` (class library)
   - `WorkTracking.UI` (WPF app)
3. Set `WorkTracking.UI` as the startup project.

### WorkTracking.Core

- Create domain models matching the schema:
  - Client
  - WorkCategory
  - ClientWorkCategory
  - WorkEntry
  - Invoice
  - InvoiceLine
  - Attachment
  - Setting
- Keep them as simple POCOs with properties only.

### WorkTracking.Data

- Implement a SQLite connection factory.
- Implement schema initialization using `schema.sql` (embedded resource or inline).
- Implement repositories:
  - IClientRepository / ClientRepository
  - IWorkEntryRepository / WorkEntryRepository
  - IWorkCategoryRepository / WorkCategoryRepository
  - IInvoiceRepository / InvoiceRepository
  - IAttachmentRepository / AttachmentRepository
  - ISettingRepository / SettingRepository
- Use async methods where appropriate.
- Handle date conversion between C# and ISO-8601 TEXT.

### WorkTracking.UI

- Implement MVVM structure:
  - ViewModels:
    - MainWindowViewModel
    - ClientListViewModel
    - ClientDetailViewModel
    - TimesheetViewModel
    - InvoicePrepViewModel
    - InvoicesViewModel
    - SummaryViewModel
    - ClientSettingsViewModel
  - Views (XAML) corresponding to these ViewModels.
- Implement:
  - Client list on the left.
  - Client detail tabs on the right (Timesheet, Invoices, Summary, Settings).
  - Timesheet grid with filters, grouping, and selection for invoice prep.
  - Notes drawer for Markdown.
  - Invoice prep dialog.
- Integrate DI:
  - Register repositories from WorkTracking.Data.
  - Register ViewModels.
  - Configure MainWindow via DI.

### Behaviour to implement

- Load clients and show them in the client list.
- For a selected client, load work entries into the timesheet.
- Implement filters (date range, invoiced/uninvoiced, category).
- Implement uninvoiced summary footer and cap highlighting.
- Implement invoice prep flow:
  - Select uninvoiced work entries.
  - Open dialog, enter invoice number/date.
  - Create invoice + invoice lines.
  - Mark work entries as invoiced.
- Implement basic per-client summary (hours per month, totals).

## Style and conventions

- Use C# 12 features where appropriate.
- Use nullable reference types.
- Use async/await for DB operations.
- Keep ViewModels testable (no direct UI dependencies).
- Use ICommand for actions.
- Keep namespaces under `WorkTracking.*`.

Start by scaffolding the solution and projects, then implement the core models, data layer, and minimal UI to navigate clients and view work entries.
