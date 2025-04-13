// Package usecase contains application logic and orchestrators.
package usecase

import (
	"context"
	"payment-receiver/domain"
)

// PaymentQueue defines the interface to enqueue PaymentEvent into a queue.
type PaymentQueue interface {
	EnqueuePaymentEvent(ctx context.Context, event *domain.PaymentEvent) error
}

var paymentQueue PaymentQueue

// InjectPaymentQueue sets the concrete implementation for PaymentQueue.
func InjectPaymentQueue(q PaymentQueue) {
	paymentQueue = q
}

// EnqueuePaymentEvent sends a payment event to the configured queue.
func EnqueuePaymentEvent(event *domain.PaymentEvent) error {
	return paymentQueue.EnqueuePaymentEvent(context.Background(), event)
}
