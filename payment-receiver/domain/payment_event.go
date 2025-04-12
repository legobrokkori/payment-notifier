package domain

import (
	"errors"
	"time"
)

// PaymentStatus represents allowed payment statuses
const (
	StatusPaid     = "paid"
	StatusRefunded = "refunded"
)

var validStatuses = map[string]struct{}{
	StatusPaid:     {},
	StatusRefunded: {},
}

func IsValidStatus(status string) bool {
	_, ok := validStatuses[status]
	return ok
}

// PaymentEvent represents a domain entity for a payment webhook
type PaymentEvent struct {
	ID         string
	Amount     int
	Currency   string
	Method     string
	Status     string
	OccurredAt time.Time
}

// NewPaymentEvent creates a validated PaymentEvent entity
func NewPaymentEvent(
	id string,
	amount int,
	currency string,
	method string,
	status string,
	occurredAt string,
) (*PaymentEvent, error) {
	if id == "" || amount == 0 || currency == "" || method == "" || status == "" || occurredAt == "" {
		return nil, errors.New("all fields are required")
	}

	if !IsValidStatus(status) {
		return nil, errors.New("invalid status")
	}

	ts, err := time.Parse(time.RFC3339, occurredAt)
	if err != nil {
		return nil, errors.New("invalid occurred_at format")
	}

	return &PaymentEvent{
		ID:         id,
		Amount:     amount,
		Currency:   currency,
		Method:     method,
		Status:     status,
		OccurredAt: ts,
	}, nil
}
