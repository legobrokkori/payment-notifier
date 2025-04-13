// domain/outbox_event.go
package domain

import (
	"encoding/json"
	"errors"
	"time"

	"github.com/google/uuid"
)

type OutboxStatus string

const (
	StatusPending OutboxStatus = "pending"
	StatusSent    OutboxStatus = "sent"
	StatusFailed  OutboxStatus = "failed"
)

type OutboxEvent struct {
	ID          uuid.UUID
	AggregateID string
	EventType   string
	Payload     json.RawMessage
	Status      OutboxStatus
	CreatedAt   time.Time
	SentAt      *time.Time
}

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
