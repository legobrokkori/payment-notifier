// usecase/outbox_dispatcher.go
package usecase

import (
	"context"
	"fmt"
	"payment-receiver/domain"
	"payment-receiver/repository"
	"time"
)

type Queue interface {
	Enqueue(ctx context.Context, event *domain.OutboxEvent) error
}

type OutboxDispatcher struct {
	repo  repository.OutboxRepository
	queue Queue
}

func NewOutboxDispatcher(repo repository.OutboxRepository, queue Queue) *OutboxDispatcher {
	return &OutboxDispatcher{repo: repo, queue: queue}
}

func (d *OutboxDispatcher) Dispatch(ctx context.Context, limit int) error {
	events, err := d.repo.FetchPending(ctx, limit)
	if err != nil {
		return fmt.Errorf("failed to fetch events: %w", err)
	}

	for _, ev := range events {
		err := d.queue.Enqueue(ctx, ev)
		if err != nil {
			// log error, but continue with others
			fmt.Printf("enqueue failed for event %s: %v\n", ev.ID, err)
			continue
		}
		if err := d.repo.MarkAsSent(ctx, ev.ID); err != nil {
			fmt.Printf("mark as sent failed for event %s: %v\n", ev.ID, err)
		}
		// optional: sleep to throttle
		time.Sleep(100 * time.Millisecond)
	}

	return nil
}
