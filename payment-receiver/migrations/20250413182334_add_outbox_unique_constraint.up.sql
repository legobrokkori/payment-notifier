-- Add index for (status, event_at) to support efficient dispatcher queries
CREATE INDEX idx_outbox_status_event_at ON outbox_events (status, event_at);
