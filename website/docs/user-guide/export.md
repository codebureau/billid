# Exporting Data

Billable can export your work entries — along with basic client information — to a **CSV file** that can be opened in Excel, Google Sheets, or any compatible tool.

---

## Opening the export dialog

1. Select a client and open the **Timesheet** tab
2. Apply any filters you want (date range, invoiced status, category) — the export uses whatever is currently shown
3. Click **Export…** in the bottom toolbar

---

## Choosing fields

The export dialog shows two groups of fields you can include in the CSV:

### Work Entry Fields

| Field | Default | Description |
|---|---|---|
| Date | ✅ | Date of the work entry |
| Description | ✅ | Summary of the work performed |
| Hours | ✅ | Time logged in decimal hours |
| Work Category | ✅ | Category assigned to the entry |
| Invoiced | ✅ | Whether the entry has been invoiced |
| Entry ID | — | Internal database ID |
| Invoice ID | — | ID of the associated invoice (if invoiced) |
| Notes | — | Full markdown notes for the entry |

### Client Fields

| Field | Default | Description |
|---|---|---|
| Client Name | ✅ | Client display name |
| Company Name | ✅ | Company the client belongs to |
| Hourly Rate | ✅ | Billing rate for the client |
| Client ID | — | Internal database ID |
| Email | — | Client email address |
| Phone | — | Client phone number |
| ABN | — | Client Australian Business Number |

Check or uncheck any field to include or exclude it from the export.

---

## Saving your field selection

Your field selection is **automatically saved** when you click **Export**. The next time you open the export dialog, your previous selection will be restored.

Click **Reset to Defaults** at any time to restore the default field selection.

---

## Running the export

1. Select your fields
2. Click **Export**
3. Choose a save location and filename in the file picker
4. The CSV file is written immediately — open it in Excel or your preferred tool

!!! tip
    The export always reflects your **current filters**. To export all entries for a client, click **All time** before exporting.

---

## CSV format

- UTF-8 encoded
- First row is the column header
- Fields containing commas, quotes, or line breaks are automatically quoted and escaped
- Dates are formatted as `YYYY-MM-DD`
- Hours are formatted as a decimal number (e.g. `1.5`)
- Invoiced is written as `Yes` or `No`
