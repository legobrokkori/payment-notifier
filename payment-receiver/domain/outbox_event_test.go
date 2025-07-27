package domain_test

import (
	"testing"
	"time"

	"payment-receiver/domain"
	pr "payment-receiver/gen/proto"

	"github.com/stretchr/testify/assert"
)

func TestNewOutboxEventFromProtoPayment(t *testing.T) {
	validEvent := &pr.PaymentEvent{
		Id:         "evt_001",
		Amount:     1000,
		Currency:   "USD",
		Method:     "card",
		Status:     "paid",
		OccurredAt: time.Now().Format(time.RFC3339),
	}

	t.Run("valid proto payment event", func(t *testing.T) {
		ev, err := domain.NewOutboxEventFromProtoPayment(validEvent)
		assert.NoError(t, err)
		assert.NotNil(t, ev)
		assert.Equal(t, "evt_001", ev.AggregateID)
	})

	t.Run("nil proto event returns error", func(t *testing.T) {
		ev, err := domain.NewOutboxEventFromProtoPayment(nil)
		assert.Error(t, err)
		assert.Nil(t, ev)
	})

	t.Run("missing ID returns error", func(t *testing.T) {
		event := cloneEvent(validEvent)
		event.Id = ""
		ev, err := domain.NewOutboxEventFromProtoPayment(event)
		assert.ErrorContains(t, err, "id is required")
		assert.Nil(t, ev)
	})

	t.Run("non-positive amount returns error", func(t *testing.T) {
		event := cloneEvent(validEvent)
		event.Amount = 0
		ev, err := domain.NewOutboxEventFromProtoPayment(event)
		assert.ErrorContains(t, err, "amount must be positive")
		assert.Nil(t, ev)
	})

	t.Run("empty currency returns error", func(t *testing.T) {
		event := cloneEvent(validEvent)
		event.Currency = ""
		ev, err := domain.NewOutboxEventFromProtoPayment(event)
		assert.ErrorContains(t, err, "currency is required")
		assert.Nil(t, ev)
	})

	t.Run("empty method returns error", func(t *testing.T) {
		event := cloneEvent(validEvent)
		event.Method = ""
		ev, err := domain.NewOutboxEventFromProtoPayment(event)
		assert.ErrorContains(t, err, "method is required")
		assert.Nil(t, ev)
	})

	t.Run("invalid status returns error", func(t *testing.T) {
		event := cloneEvent(validEvent)
		event.Status = "unknown"
		ev, err := domain.NewOutboxEventFromProtoPayment(event)
		assert.ErrorContains(t, err, "invalid status")
		assert.Nil(t, ev)
	})

	t.Run("invalid occurred_at format returns error", func(t *testing.T) {
		event := cloneEvent(validEvent)
		event.OccurredAt = "not-a-date"
		ev, err := domain.NewOutboxEventFromProtoPayment(event)
		assert.ErrorContains(t, err, "invalid occurred_at format")
		assert.Nil(t, ev)
	})
}

// cloneEvent creates a deep copy of a proto.PaymentEvent.
func cloneEvent(e *pr.PaymentEvent) *pr.PaymentEvent {
	return &pr.PaymentEvent{
		Id:         e.Id,
		Amount:     e.Amount,
		Currency:   e.Currency,
		Method:     e.Method,
		Status:     e.Status,
		OccurredAt: e.OccurredAt,
	}
}
