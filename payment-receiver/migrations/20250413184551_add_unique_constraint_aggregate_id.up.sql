-- Add unique constraint on aggregate_id to ensure idempotency
ALTER TABLE outbox_events
ADD CONSTRAINT unique_aggregate_id UNIQUE (aggregate_id);