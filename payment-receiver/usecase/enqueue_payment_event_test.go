package usecase_test

import (
	"context"
	"log"
	"testing"
	"time"

	"payment-receiver/domain"
	"payment-receiver/usecase"

	"github.com/stretchr/testify/assert"
)

type mockQueue struct {
	Called bool
	Event  *domain.PaymentEvent
}

func (m *mockQueue) Enqueue(ctx context.Context, event *domain.PaymentEvent) error {
	m.Called = true
	m.Event = event
	return nil
}

func mustParseTime(value string) time.Time {
	t, err := time.Parse(time.RFC3339, value)
	if err != nil {
		log.Fatalf("invalid time format: %v", err)
	}
	return t
}

func TestEnqueuePaymentEvent_WithInjectedQueue(t *testing.T) {
	mq := &mockQueue{}
	usecase.InjectQueue(mq)

	evt := &domain.PaymentEvent{
		ID:         "evt_abc",
		Amount:     1000,
		Currency:   "JPY",
		Method:     "credit_card",
		Status:     "paid",
		OccurredAt: mustParseTime("2025-04-13T12:00:00Z"),
	}

	err := usecase.EnqueuePaymentEvent(evt)
	assert.NoError(t, err)
	assert.True(t, mq.Called)
	assert.Equal(t, "evt_abc", mq.Event.ID)
}

func TestEnqueuePaymentEvent_WithoutQueue(t *testing.T) {
	usecase.InjectQueue(nil)

	evt := &domain.PaymentEvent{
		ID:         "evt_xyz",
		Amount:     2000,
		Currency:   "USD",
		Method:     "paypal",
		Status:     "paid",
		OccurredAt: mustParseTime("2025-04-13T12:00:00Z"),
	}

	err := usecase.EnqueuePaymentEvent(evt)
	assert.NoError(t, err)
}
