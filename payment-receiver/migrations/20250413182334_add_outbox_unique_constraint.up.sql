-- Add index for (status, event_at) to support efficient dispatcher queries
CREATE INDEX IF NOT EXISTS idx_outbox_status_event_at ON outbox_events (status, event_at);
