package usecase_test

import (
	"context"
	"testing"
	"time"

	"payment-receiver/domain"

	"github.com/stretchr/testify/assert"
)

// mockPaymentQueue implements usecase.PaymentQueue for testing
type mockPaymentQueue struct {
	called bool
	event  *domain.PaymentEvent
}

func (m *mockPaymentQueue) Enqueue(ctx context.Context, event *domain.PaymentEvent) error {
	m.called = true
	m.event = event
	return nil
}

func TestPaymentQueue_Enqueue(t *testing.T) {
	mock := &mockPaymentQueue{}

	occurredAt, _ := time.Parse(time.RFC3339, "2024-04-01T12:00:00Z")
	event := &domain.PaymentEvent{
		ID:         "test-id",
		Amount:     1000,
		Currency:   "USD",
		Method:     "card",
		Status:     "paid",
		OccurredAt: occurredAt,
	}

	// 呼び出すテスト関数
	err := mock.Enqueue(context.Background(), event)

	assert.NoError(t, err)
	assert.True(t, mock.called)
	assert.Equal(t, event, mock.event)
}
