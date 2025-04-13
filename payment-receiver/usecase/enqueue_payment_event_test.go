package usecase_test

import (
	"context"
	"testing"
	"time"

	"payment-receiver/domain"
	"payment-receiver/usecase"

	"github.com/stretchr/testify/assert"
)

// mockQueue implements usecase.PaymentQueue for testing
type mockQueue struct {
	called bool
	event  *domain.PaymentEvent
}

func (m *mockQueue) EnqueuePaymentEvent(ctx context.Context, event *domain.PaymentEvent) error {
	m.called = true
	m.event = event
	return nil
}

func TestEnqueuePaymentEvent(t *testing.T) {
	mock := &mockQueue{}
	usecase.InjectPaymentQueue(mock)

	occurredAt := time.Now()
	event := &domain.PaymentEvent{
		ID:         "test-123",
		Amount:     1000,
		Currency:   "USD",
		Method:     "card",
		Status:     "paid",
		OccurredAt: occurredAt,
	}

	err := usecase.EnqueuePaymentEvent(event)

	assert.NoError(t, err)
	assert.True(t, mock.called)
	assert.Equal(t, event, mock.event)
}
