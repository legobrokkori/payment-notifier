// Package usecase contains application logic and orchestrators.
package usecase

import (
	"context"
	"fmt"
	"payment-receiver/domain"
	"payment-receiver/repository"
	"time"
)

// Queue defines the interface for enqueuing outbox events.
type Queue interface {
	Enqueue(ctx context.Context, event *domain.OutboxEvent) error
}

// OutboxDispatcher processes pending outbox events and dispatches them to a queue.
type OutboxDispatcher struct {
	repo  repository.OutboxRepository
	queue Queue
}

// NewOutboxDispatcher returns a new instance of OutboxDispatcher.
func NewOutboxDispatcher(repo repository.OutboxRepository, queue Queue) *OutboxDispatcher {
	return &OutboxDispatcher{repo: repo, queue: queue}
}

// Dispatch retrieves pending events and enqueues them, marking them as sent.
func (d *OutboxDispatcher) Dispatch(ctx context.Context, limit int) error {
	events, err := d.repo.FetchPending(ctx, limit)
	if err != nil {
		return fmt.Errorf("failed to fetch events: %w", err)
	}

	for _, ev := range events {
		err := d.queue.Enqueue(ctx, ev)
		if err != nil {
			fmt.Printf("enqueue failed for event %s: %v\n", ev.ID, err)
			continue
		}
		if err := d.repo.MarkAsSent(ctx, ev.ID); err != nil {
			fmt.Printf("mark as sent failed for event %s: %v\n", ev.ID, err)
		}
		time.Sleep(100 * time.Millisecond)
	}

	return nil
}
