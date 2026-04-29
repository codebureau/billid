-- SQLite schema for billid / WorkTracking

PRAGMA foreign_keys = ON;

CREATE TABLE client (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
    contact_name TEXT,
    company_name TEXT,
    address TEXT,
    abn TEXT,
    email TEXT,
    phone TEXT,
    hourly_rate REAL NOT NULL DEFAULT 0,
    invoice_cap_amount REAL,
    invoice_cap_behavior TEXT,            -- 'warn', 'block', 'allow'
    invoice_frequency_days INTEGER,
    last_invoice_date TEXT,               -- ISO-8601
    next_invoice_due_date TEXT,           -- ISO-8601
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL
);

CREATE TABLE work_category (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL UNIQUE,
    description TEXT
);

CREATE TABLE client_work_category (
    client_id INTEGER NOT NULL,
    work_category_id INTEGER NOT NULL,
    PRIMARY KEY (client_id, work_category_id),
    FOREIGN KEY (client_id) REFERENCES client(id) ON DELETE CASCADE,
    FOREIGN KEY (work_category_id) REFERENCES work_category(id) ON DELETE CASCADE
);

CREATE TABLE invoice (
    id INTEGER PRIMARY KEY,
    client_id INTEGER NOT NULL,
    invoice_number TEXT NOT NULL,
    invoice_date TEXT NOT NULL,           -- ISO-8601
    total_amount REAL NOT NULL,
    pdf_path TEXT,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL,
    FOREIGN KEY (client_id) REFERENCES client(id) ON DELETE CASCADE
);

CREATE TABLE work_entry (
    id INTEGER PRIMARY KEY,
    client_id INTEGER NOT NULL,
    date TEXT NOT NULL,                   -- ISO-8601
    description TEXT NOT NULL,
    hours REAL NOT NULL,
    work_category_id INTEGER,
    notes_markdown TEXT,
    invoiced_flag INTEGER NOT NULL DEFAULT 0,
    invoice_id INTEGER,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL,
    FOREIGN KEY (client_id) REFERENCES client(id) ON DELETE CASCADE,
    FOREIGN KEY (work_category_id) REFERENCES work_category(id),
    FOREIGN KEY (invoice_id) REFERENCES invoice(id) ON DELETE SET NULL
);

CREATE TABLE invoice_line (
    id INTEGER PRIMARY KEY,
    invoice_id INTEGER NOT NULL,
    work_category_id INTEGER,
    hours REAL NOT NULL,
    rate REAL NOT NULL,
    amount REAL NOT NULL,
    description TEXT,
    FOREIGN KEY (invoice_id) REFERENCES invoice(id) ON DELETE CASCADE,
    FOREIGN KEY (work_category_id) REFERENCES work_category(id)
);

CREATE TABLE attachment (
    id INTEGER PRIMARY KEY,
    work_entry_id INTEGER NOT NULL,
    filename TEXT NOT NULL,
    mime_type TEXT,
    file_path TEXT,                       -- initial strategy: store path
    created_at TEXT NOT NULL,
    FOREIGN KEY (work_entry_id) REFERENCES work_entry(id) ON DELETE CASCADE
);

CREATE TABLE setting (
    key TEXT PRIMARY KEY,
    value TEXT
);

-- Indexes for performance

CREATE INDEX idx_work_entry_client_date
    ON work_entry (client_id, date);

CREATE INDEX idx_work_entry_invoice
    ON work_entry (invoice_id);

CREATE INDEX idx_invoice_client_date
    ON invoice (client_id, invoice_date);

CREATE INDEX idx_work_entry_invoiced_flag
    ON work_entry (invoiced_flag);
