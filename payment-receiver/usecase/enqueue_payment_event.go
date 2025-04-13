package usecase

import (
	"context"
	"log"

	"payment-receiver/domain"
)

func EnqueuePaymentEvent(event *domain.PaymentEvent) error {
	if queue == nil {
		log.Println("[WARN] No queue implementation injected, skipping enqueue")
		return nil
	}
	return queue.Enqueue(context.Background(), event)
}
