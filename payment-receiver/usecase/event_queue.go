// Package usecase contains application logic and orchestrators.
package usecase

import (
	"context"
	"payment-receiver/domain"
)

// EventQueue defines the interface to enqueue PaymentEvent into a message queue.
type EventQueue interface {
	Enqueue(ctx context.Context, event *domain.PaymentEvent) error
}

var queue EventQueue

// InjectQueue injects a queue implementation for testing and runtime use.
func InjectQueue(q EventQueue) {
	queue = q
}
