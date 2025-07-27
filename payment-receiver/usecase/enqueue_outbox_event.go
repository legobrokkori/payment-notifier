// Package usecase contains application logic and orchestrators.
package usecase

import (
	"context"
	"errors"
	"fmt"

	"payment-receiver/domain"
	"payment-receiver/repository"
)

// EnqueueOutboxEvent inserts a PaymentEvent into the outbox table.
type OutboxEnqueuer struct {
	Repo repository.OutboxRepository
}

// OutboxEventSaver defines the interface for saving events to outbox.
type OutboxEventSaver interface {
	EnqueueOutboxEvent(ctx context.Context, event *domain.OutboxEvent) error
}

func NewOutboxEnqueuer(repo repository.OutboxRepository) *OutboxEnqueuer {
	return &OutboxEnqueuer{Repo: repo}
}

func (e *OutboxEnqueuer) EnqueueOutboxEvent(ctx context.Context, event *domain.OutboxEvent) error {
	exists, err := e.Repo.ExistsByAggregateID(ctx, event.AggregateID)
	if err != nil {
		return fmt.Errorf("failed to check idempotency: %w", err)
	}
	if exists {
		return ErrDuplicateEvent
	}

	if err := e.Repo.Insert(ctx, event); err != nil {
		if errors.Is(err, ErrDuplicateEvent) {
			return err
		}
		return fmt.Errorf("failed to insert outbox event: %w", err)
	}

	return nil
}
