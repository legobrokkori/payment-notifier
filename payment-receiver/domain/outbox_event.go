// Package domain handles core business entities and logic.
package domain

import (
	"encoding/json"
	"errors"
	"fmt"
	"time"

	pr "payment-receiver/gen/proto"

	"github.com/google/uuid"
	"google.golang.org/protobuf/proto"
)

// OutboxStatus represents the status of an outbox event.
type OutboxStatus string

// StatusPending indicates the event is not yet sent.
const (
	StatusPending OutboxStatus = "pending"
	StatusSent    OutboxStatus = "sent"
	StatusFailed  OutboxStatus = "failed"
)

// OutboxEvent represents a stored domain event for async dispatch.
type OutboxEvent struct {
	ID          uuid.UUID
	AggregateID string
	EventType   string
	Payload     json.RawMessage
	Status      OutboxStatus
	CreatedAt   time.Time
	SentAt      *time.Time
	EventAt     time.Time
}

// NewOutboxEvent constructs a new OutboxEvent with validation.
func NewOutboxEvent(
	aggregateID, eventType string,
	eventAt time.Time,
	payload interface{},
) (*OutboxEvent, error) {
	if aggregateID == "" || eventType == "" {
		return nil, errors.New("aggregateID and eventType are required")
	}

	data, err := json.Marshal(payload)
	if err != nil {
		return nil, err
	}

	return &OutboxEvent{
		ID:          uuid.New(),
		AggregateID: aggregateID,
		EventType:   eventType,
		Payload:     data,
		Status:      StatusPending,
		EventAt:     eventAt,
		CreatedAt:   time.Now(),
	}, nil
}

// NewOutboxEventFromProtoPayment creates an OutboxEvent from a proto.PaymentEvent.
func NewOutboxEventFromProtoPayment(event *pr.PaymentEvent) (*OutboxEvent, error) {
	if event == nil {
		return nil, errors.New("event is nil")
	}

	if err := validateProtoPaymentEvent(event); err != nil {
		return nil, err
	}

	t, _ := parseISO8601Strict(event.OccurredAt)

	payload, err := proto.Marshal(event)
	if err != nil {
		return nil, fmt.Errorf("failed to marshal protobuf: %w", err)
	}

	return &OutboxEvent{
		ID:          uuid.New(),
		AggregateID: event.Id,
		EventType:   "payment_event",
		Payload:     payload,
		Status:      StatusPending,
		EventAt:     t,
		CreatedAt:   time.Now(),
	}, nil
}

func validateProtoPaymentEvent(event *pr.PaymentEvent) error {
	if event.Id == "" {
		return errors.New("id is required")
	}
	if event.Amount <= 0 {
		return errors.New("amount must be positive")
	}
	if event.Currency == "" {
		return errors.New("currency is required")
	}
	if event.Method == "" {
		return errors.New("method is required")
	}
	switch event.Status {
	case "paid", "failed", "refunded":
		// ok
	default:
		return errors.New("invalid status")
	}
	if _, err := parseISO8601Strict(event.OccurredAt); err != nil {
		return fmt.Errorf("invalid occurred_at format: %w", err)
	}
	return nil
}

func parseISO8601Strict(input string) (time.Time, error) {
	return time.Parse(time.RFC3339, input)
}
