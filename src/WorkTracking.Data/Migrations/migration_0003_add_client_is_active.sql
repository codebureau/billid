-- migration_0003_add_client_is_active.sql
-- Adds is_active column to the client table.
-- Defaults to 1 (active) so all existing clients remain active.

ALTER TABLE client ADD COLUMN is_active INTEGER NOT NULL DEFAULT 1;
