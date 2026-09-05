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
	// Race Condition を避けるため、DB制約のみで重複チェックを行う
	// ExistsByAggregateID + Insert の間に別リクエストが入る可能性があるため削除
	if err := e.Repo.Insert(ctx, event); err != nil {
		if errors.Is(err, ErrDuplicateEvent) {
			return ErrDuplicateEvent
		}
		return fmt.Errorf("failed to insert outbox event: %w", err)
	}

	return nil
}
