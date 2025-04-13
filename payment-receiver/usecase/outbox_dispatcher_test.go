// usecase/outbox_dispatcher_test.go
package usecase_test

import (
	"context"
	"encoding/json"
	"testing"
	"time"

	"payment-receiver/domain"
	"payment-receiver/usecase"

	"github.com/google/uuid"
	"github.com/stretchr/testify/assert"
)

type mockOutboxRepo struct {
	Fetched bool
	Marked  []uuid.UUID
}

func (m *mockOutboxRepo) Insert(ctx context.Context, event *domain.OutboxEvent) error {
	// テストでは使わないなら空でOK
	return nil
}

func (m *mockOutboxRepo) FetchPending(_ context.Context, limit int) ([]*domain.OutboxEvent, error) {
	m.Fetched = true
	return []*domain.OutboxEvent{
		{
			ID:          uuid.New(),
			AggregateID: "user_123",
			EventType:   "PaymentCompleted",
			Payload:     json.RawMessage(`{"id":"evt_001"}`),
			Status:      "pending",
			CreatedAt:   time.Now(),
		},
	}, nil
}

func (m *mockOutboxRepo) MarkAsSent(_ context.Context, id uuid.UUID) error {
	m.Marked = append(m.Marked, id)
	return nil
}

type mockOutboxQueue struct {
	Called bool
	Event  *domain.OutboxEvent
}

func (m *mockOutboxQueue) Enqueue(_ context.Context, event *domain.OutboxEvent) error {
	m.Called = true
	m.Event = event
	return nil
}

func TestOutboxDispatcher_Dispatch(t *testing.T) {
	repo := &mockOutboxRepo{}
	queue := &mockOutboxQueue{}

	dispatcher := usecase.NewOutboxDispatcher(repo, queue)
	err := dispatcher.Dispatch(context.Background(), 10)

	assert.NoError(t, err)
	assert.True(t, repo.Fetched)
	assert.True(t, queue.Called)
	assert.Len(t, repo.Marked, 1)
}
