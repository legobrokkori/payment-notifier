// Package domain handles core business entities and logic.
package domain

import (
	"encoding/json"
	"errors"
	"time"

	"github.com/google/uuid"
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
}

// NewOutboxEvent constructs a new OutboxEvent with validation.
func NewOutboxEvent(aggregateID, eventType string, payload interface{}) (*OutboxEvent, error) {
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
		CreatedAt:   time.Now(),
	}, nil
}
