// Package infrastructure implements repository interfaces using concrete tools like Redis and Postgres.
package infrastructure

import (
	"context"
	"database/sql"
	"errors"
	"fmt"
	"log"
	"time"

	"payment-receiver/domain"

	"github.com/google/uuid"
	"github.com/lib/pq"
)

// PostgresOutbox implements the OutboxRepository interface using PostgreSQL.
type PostgresOutbox struct {
	db *sql.DB
}

// NewPostgresOutbox creates a new Postgres outbox repository.
func NewPostgresOutbox(db *sql.DB) *PostgresOutbox {
	return &PostgresOutbox{db: db}
}

// Insert inserts a new outbox event.
func (o *PostgresOutbox) Insert(ctx context.Context, event *domain.OutboxEvent) error {
	_, err := o.db.ExecContext(ctx, `
		INSERT INTO outbox_events (
			id, aggregate_id, event_type, payload, status, created_at, event_at
		) VALUES ($1, $2, $3, $4, $5, $6, $7)
	`, event.ID, event.AggregateID, event.EventType, event.Payload, event.Status, event.CreatedAt, event.EventAt)
	if err != nil {
		var pgErr *pq.Error
		if errors.As(err, &pgErr) && pgErr.Code == "23505" {
			return fmt.Errorf("%w", ErrDuplicateKey)
		}
		return err
	}
	return nil
}

// FetchPending retrieves pending events up to a limit.
func (o *PostgresOutbox) FetchPending(
	ctx context.Context,
	limit int,
) ([]*domain.OutboxEvent, error) {
	rows, err := o.db.QueryContext(ctx, `
		SELECT id, aggregate_id, event_type, payload, status, event_at, created_at, sent_at
		FROM outbox_events
		WHERE status = 'pending'
		ORDER BY event_at ASC
		LIMIT $1
	`, limit)
	if err != nil {
		return nil, err
	}
	defer func() {
		if err := rows.Close(); err != nil {
			log.Println("failed to close rows:", err)
		}
	}()

	var events []*domain.OutboxEvent
	for rows.Next() {
		var ev domain.OutboxEvent
		var sentAt sql.NullTime
		if err := rows.Scan(&ev.ID, &ev.AggregateID, &ev.EventType, &ev.Payload, &ev.Status, &ev.EventAt, &ev.CreatedAt, &sentAt); err != nil {
			return nil, err
		}
		if sentAt.Valid {
			ev.SentAt = &sentAt.Time
		}
		events = append(events, &ev)
	}
	return events, nil
}

// MarkAsSent marks an event as sent.
func (o *PostgresOutbox) MarkAsSent(ctx context.Context, id uuid.UUID) error {
	sentAt := time.Now()
	_, err := o.db.ExecContext(ctx, `
		UPDATE outbox_events
		SET status = 'sent', sent_at = $1
		WHERE id = $2
	`, sentAt, id)
	return err
}

// ExistsByAggregateID checks if an event with the given aggregate ID exists
func (o *PostgresOutbox) ExistsByAggregateID(
	ctx context.Context,
	aggregateID string,
) (bool, error) {
	var exists bool
	err := o.db.QueryRowContext(ctx, `
		SELECT EXISTS (
			SELECT 1 FROM outbox_events WHERE aggregate_id = $1
		)
	`, aggregateID).Scan(&exists)
	if err != nil {
		return false, err
	}
	return exists, nil
}
