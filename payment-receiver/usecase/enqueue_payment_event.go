// Package usecase contains application logic and orchestrators.
package usecase

import (
	"context"
	"log"

	"payment-receiver/domain"
)

// EnqueuePaymentEvent sends a validated payment event to the queue.
func EnqueuePaymentEvent(event *domain.PaymentEvent) error {
	if queue == nil {
		log.Println("[WARN] No queue implementation injected, skipping enqueue")
		return nil
	}
	return queue.Enqueue(context.Background(), event)
}
