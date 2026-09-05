// Package usecase contains application logic and orchestrators.
package usecase

import (
	"context"
	"fmt"
	"log"
	"sync"

	"payment-receiver/domain"
	"payment-receiver/repository"
)

// OutboxDispatcher processes pending outbox events and dispatches them to a queue.
type OutboxDispatcher struct {
	repo  repository.OutboxRepository
	queue OutboxQueue
}

// NewOutboxDispatcher returns a new instance of OutboxDispatcher.
func NewOutboxDispatcher(repo repository.OutboxRepository, queue OutboxQueue) *OutboxDispatcher {
	return &OutboxDispatcher{repo: repo, queue: queue}
}

// Dispatch retrieves pending events and enqueues them in parallel, marking them as sent.
func (d *OutboxDispatcher) Dispatch(ctx context.Context, limit int) error {
	events, err := d.repo.FetchPending(ctx, limit)
	if err != nil {
		return fmt.Errorf("failed to fetch events: %w", err)
	}

	if len(events) == 0 {
		return nil
	}

	var wg sync.WaitGroup
	errCh := make(chan error, len(events))

	for _, ev := range events {
		wg.Add(1)
		go func(event *domain.OutboxEvent) {
			defer wg.Done()

			if err := d.queue.Enqueue(ctx, event); err != nil {
				errCh <- fmt.Errorf("enqueue failed for event %s: %w", event.ID, err)
				return
			}

			if err := d.repo.MarkAsSent(ctx, event.ID); err != nil {
				errCh <- fmt.Errorf("mark as sent failed for event %s: %w", event.ID, err)
			}
		}(ev)
	}

	wg.Wait()
	close(errCh)

	// エラーがあればログ出力
	for err := range errCh {
		log.Println(err)
	}

	return nil
}
