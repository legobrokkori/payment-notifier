package domain_test

import (
	"testing"
	"time"

	"payment-receiver/domain"

	"github.com/stretchr/testify/assert"
)

func TestNewOutboxEvent_Validation(t *testing.T) {
	payload := map[string]interface{}{
		"id": "evt_001",
	}

	eventAt := time.Now()

	t.Run("valid input returns OutboxEvent", func(t *testing.T) {
		event, err := domain.NewOutboxEvent("user_123", "payment_event", eventAt, payload)
		assert.NoError(t, err)
		assert.NotNil(t, event)
		assert.Equal(t, "user_123", event.AggregateID)
	})

	t.Run("empty aggregateID returns error", func(t *testing.T) {
		event, err := domain.NewOutboxEvent("", "payment_event", eventAt, payload)
		assert.Error(t, err)
		assert.Nil(t, event)
	})

	t.Run("empty eventType returns error", func(t *testing.T) {
		event, err := domain.NewOutboxEvent("user_123", "", eventAt, payload)
		assert.Error(t, err)
		assert.Nil(t, event)
	})
}
