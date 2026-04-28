-- migration_0002_seed_export_definition.sql
-- Inserts the default export definition JSON into the setting table.
-- Uses INSERT OR IGNORE so existing user customisations are never overwritten.

INSERT OR IGNORE INTO setting (key, value) VALUES (
    'export_definition',
    '{"IncludeWorkEntryId":false,"IncludeDate":true,"IncludeDescription":true,"IncludeHours":true,"IncludeWorkCategory":true,"IncludeInvoicedFlag":true,"IncludeInvoiceId":false,"IncludeNotesMarkdown":false,"IncludeClientId":false,"IncludeClientName":true,"IncludeClientCompanyName":true,"IncludeClientEmail":false,"IncludeClientPhone":false,"IncludeClientHourlyRate":true,"IncludeClientAbn":false}'
);
