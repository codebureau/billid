-- migration_0004_add_client_sort_order.sql
-- Applied: adds sort_order column to client table for manual drag-drop ordering

ALTER TABLE client ADD COLUMN sort_order INTEGER NOT NULL DEFAULT 0;

-- Initialise existing rows so order matches current alpha ordering
UPDATE client SET sort_order = (
    SELECT COUNT(*) FROM client c2 WHERE c2.name < client.name
);
