package usecase

import (
	"context"
	"payment-receiver/domain"
)

type EventQueue interface {
	Enqueue(ctx context.Context, event *domain.PaymentEvent) error
}

var queue EventQueue

func InjectQueue(q EventQueue) {
	queue = q
}
