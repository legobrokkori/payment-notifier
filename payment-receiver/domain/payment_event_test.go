package domain_test

import (
	"testing"
	"time"

	"payment-receiver/domain"

	"github.com/stretchr/testify/assert"
)

func TestNewPaymentEvent_Success(t *testing.T) {
	event, err := domain.NewPaymentEvent(
		"evt_001",
		1200,
		"USD",
		"card",
		"paid",
		"2024-04-01T12:00:00Z",
	)

	assert.NoError(t, err)
	assert.NotNil(t, event)
	assert.Equal(t, "evt_001", event.ID)
	assert.Equal(t, 1200, event.Amount)
	assert.Equal(t, "USD", event.Currency)
	assert.Equal(t, "card", event.Method)
	assert.Equal(t, "paid", event.Status)
	assert.Equal(t, mustParse("2024-04-01T12:00:00Z"), event.OccurredAt)
}

func TestNewPaymentEvent_MissingFields(t *testing.T) {
	tests := []struct {
		name        string
		id          string
		amount      int
		currency    string
		method      string
		status      string
		occurredAt  string
		expectError bool
	}{
		{"empty ID", "", 100, "USD", "card", "paid", "2024-04-01T12:00:00Z", true},
		{"zero Amount", "evt_001", 0, "USD", "card", "paid", "2024-04-01T12:00:00Z", true},
		{"empty currency", "evt_001", 100, "", "card", "paid", "2024-04-01T12:00:00Z", true},
		{"empty method", "evt_001", 100, "USD", "", "paid", "2024-04-01T12:00:00Z", true},
		{"empty status", "evt_001", 100, "USD", "card", "", "2024-04-01T12:00:00Z", true},
		{"empty occurredAt", "evt_001", 100, "USD", "card", "paid", "", true},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			event, err := domain.NewPaymentEvent(tt.id, tt.amount, tt.currency, tt.method, tt.status, tt.occurredAt)
			assert.Nil(t, event)
			assert.Error(t, err)
		})
	}
}

func TestNewPaymentEvent_InvalidStatus(t *testing.T) {
	event, err := domain.NewPaymentEvent("evt_001", 100, "USD", "card", "unknown_status", "2024-04-01T12:00:00Z")
	assert.Nil(t, event)
	assert.EqualError(t, err, "invalid status")
}

func TestNewPaymentEvent_InvalidOccurredAtFormat(t *testing.T) {
	event, err := domain.NewPaymentEvent("evt_001", 100, "USD", "card", "paid", "not-a-date")
	assert.Nil(t, event)
	assert.EqualError(t, err, "invalid occurred_at format")
}

// mustParse is a test helper
func mustParse(s string) time.Time {
	t, _ := time.Parse(time.RFC3339, s)
	return t
}
