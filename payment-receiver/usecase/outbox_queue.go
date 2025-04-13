package usecase

import (
	"context"

	"payment-receiver/domain"
)

// OutboxQueue defines interface for outbox events queue
type OutboxQueue interface {
	Enqueue(ctx context.Context, event *domain.OutboxEvent) error
}
