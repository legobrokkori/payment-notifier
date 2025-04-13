-- schema.sql

CREATE TABLE outbox_events (
  id UUID PRIMARY KEY,
  aggregate_id TEXT NOT NULL,
  event_type TEXT NOT NULL,
  payload JSONB NOT NULL,
  status TEXT NOT NULL DEFAULT 'pending', -- 'pending', 'sent', 'failed'
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  sent_at TIMESTAMP
);

-- インデックス追加（任意）
CREATE INDEX idx_outbox_status ON outbox_events (status);
CREATE INDEX idx_outbox_created_at ON outbox_events (created_at);
