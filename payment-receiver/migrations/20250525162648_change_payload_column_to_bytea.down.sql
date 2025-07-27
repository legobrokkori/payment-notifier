-- revert payload column back to jsonb
ALTER TABLE outbox_events
ALTER COLUMN payload TYPE jsonb
USING convert_from(payload, 'UTF8')::jsonb;