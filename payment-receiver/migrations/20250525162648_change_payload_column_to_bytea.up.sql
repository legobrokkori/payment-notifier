-- change payload column to bytea for Protobuf binary storage
ALTER TABLE outbox_events
ALTER COLUMN payload TYPE bytea
USING payload::text::bytea;