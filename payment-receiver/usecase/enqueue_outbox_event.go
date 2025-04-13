// Package usecase contains application logic and orchestrators.
package usecase

import (
	"context"
	"encoding/json"
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
	EnqueueOutboxEvent(ctx context.Context, event *domain.PaymentEvent) error
}

func NewOutboxEnqueuer(repo repository.OutboxRepository) *OutboxEnqueuer {
	return &OutboxEnqueuer{Repo: repo}
}

func (e *OutboxEnqueuer) EnqueueOutboxEvent(ctx context.Context, event *domain.PaymentEvent) error {
	payload, err := json.Marshal(event)
	if err != nil {
		return fmt.Errorf("failed to marshal payment event: %w", err)
	}

	outboxEvent, err := domain.NewOutboxEvent(
		event.ID,
		"payment_event",
		payload,
	)
	if err != nil {
		return fmt.Errorf("failed to create outbox event: %w", err)
	}

	return e.Repo.Insert(ctx, outboxEvent)
}
