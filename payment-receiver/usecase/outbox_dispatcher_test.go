// usecase/outbox_dispatcher_test.go
package usecase_test

import (
	"context"
	"encoding/json"
	"sync"
	"testing"
	"time"

	"payment-receiver/domain"
	"payment-receiver/usecase"

	"github.com/google/uuid"
	"github.com/stretchr/testify/assert"
)

type mockOutboxRepo struct {
	mu      sync.Mutex
	Fetched bool
	Marked  []uuid.UUID
}

func (m *mockOutboxRepo) Insert(_ context.Context, _ *domain.OutboxEvent) error {
	// テストでは使わないなら空でOK
	return nil
}

func (m *mockOutboxRepo) FetchPending(_ context.Context, _ int) ([]*domain.OutboxEvent, error) {
	m.mu.Lock()
	defer m.mu.Unlock()
	m.Fetched = true
	return []*domain.OutboxEvent{
		{
			ID:          uuid.New(),
			AggregateID: "user_123",
			EventType:   "PaymentCompleted",
			Payload:     json.RawMessage(`{"id":"evt_001"}`),
			Status:      domain.StatusPending,
			CreatedAt:   time.Now(),
			EventAt:     time.Now(),
		},
	}, nil
}

func (m *mockOutboxRepo) MarkAsSent(_ context.Context, id uuid.UUID) error {
	m.mu.Lock()
	defer m.mu.Unlock()
	m.Marked = append(m.Marked, id)
	return nil
}

func (m *mockOutboxRepo) ExistsByAggregateID(_ context.Context, _ string) (bool, error) {
	return false, nil
}

func (m *mockOutboxRepo) GetMarked() []uuid.UUID {
	m.mu.Lock()
	defer m.mu.Unlock()
	return append([]uuid.UUID{}, m.Marked...)
}

type mockOutboxQueue struct {
	mu     sync.Mutex
	Called bool
	Events []*domain.OutboxEvent
}

func (m *mockOutboxQueue) Enqueue(_ context.Context, event *domain.OutboxEvent) error {
	m.mu.Lock()
	defer m.mu.Unlock()
	m.Called = true
	m.Events = append(m.Events, event)
	return nil
}

func (m *mockOutboxQueue) GetEvents() []*domain.OutboxEvent {
	m.mu.Lock()
	defer m.mu.Unlock()
	return append([]*domain.OutboxEvent{}, m.Events...)
}

func TestOutboxDispatcher_Dispatch(t *testing.T) {
	repo := &mockOutboxRepo{}
	queue := &mockOutboxQueue{}

	dispatcher := usecase.NewOutboxDispatcher(repo, queue)
	err := dispatcher.Dispatch(context.Background(), 10)

	assert.NoError(t, err)
	assert.True(t, repo.Fetched)
	assert.True(t, queue.Called)

	marked := repo.GetMarked()
	assert.Len(t, marked, 1)

	events := queue.GetEvents()
	assert.Len(t, events, 1)
}
