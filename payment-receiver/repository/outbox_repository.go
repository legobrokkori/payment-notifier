// Package repository defines interfaces for data access.
package repository

import (
	"context"

	"payment-receiver/domain"

	"github.com/google/uuid"
)

// OutboxRepository defines DB operations for the outbox pattern.
type OutboxRepository interface {
	Insert(ctx context.Context, event *domain.OutboxEvent) error
	FetchPending(ctx context.Context, limit int) ([]*domain.OutboxEvent, error)
	MarkAsSent(ctx context.Context, id uuid.UUID) error
	ExistsByAggregateID(ctx context.Context, aggregateID string) (bool, error)
}
