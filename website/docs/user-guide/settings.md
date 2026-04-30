# Client Settings

The **Settings** tab stores contact details and billing configuration for each client. It also controls the client's active status.

---

## Contact details

| Field | Notes |
|---|---|
| **Name** | Client contact name |
| **Company name** | Trading name |
| **Address** | Postal address |
| **ABN** | Australian Business Number (or equivalent) |
| **Email** | Contact email |
| **Phone** | Contact phone |

---

## Billing configuration

| Field | Notes |
|---|---|
| **Hourly rate** | Rate charged per hour |
| **Invoice cap** | Optional maximum amount per invoice period |
| **Cap behaviour** | `Warn` — alert when approaching cap; `Block` — prevent over-cap entries |
| **Invoice frequency** | Days between expected invoices (e.g. `30` for monthly) |

---

## Work categories

Enable or disable work categories for this client. Categories are used to classify timesheet entries and group invoice lines.

To add a new category:

1. Type the category name in the field at the bottom of the category list
2. Click **+ Add category**

---

## Saving changes

Click **Save changes** to persist any edits. Changes are not saved automatically.

!!! note
    All fields and the save button are read-only for deactivated clients.

---

## Client status actions

### Deactivate client

Ends the engagement and hides the client from the list while preserving all history. See [Deactivating a client](clients.md#deactivating-a-client) for full details.

### Reactivate client

Restores a deactivated client to full active status. Only visible when the client is currently deactivated. See [Reactivating a client](clients.md#reactivating-a-client).

### Delete client

Permanently removes the client and all associated data. Only available for deactivated clients. See [Deleting a client](clients.md#deleting-a-client).

---

## App Settings

App-wide settings are accessible via **⚙ Settings** in the sidebar.

| Setting | Notes |
|---|---|
| **Theme** | Choose Light or Dark theme |
| **Show deactivated clients** | When enabled, deactivated clients appear greyed out in the client list |
