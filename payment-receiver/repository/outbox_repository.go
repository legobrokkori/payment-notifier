// repository/outbox_repository.go
package repository

import (
	"context"

	"payment-receiver/domain"

	"github.com/google/uuid"
)

type OutboxRepository interface {
	Insert(ctx context.Context, event *domain.OutboxEvent) error
	FetchPending(ctx context.Context, limit int) ([]*domain.OutboxEvent, error)
	MarkAsSent(ctx context.Context, id uuid.UUID) error
}
