-- Drop the unique constraint if needed
ALTER TABLE outbox_events
DROP CONSTRAINT IF EXISTS unique_aggregate_id;