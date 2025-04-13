// usecase/outbox_dispatcher_test.go
package usecase

import (
	"context"
	"testing"
	"time"

	"payment-receiver/domain"

	"github.com/google/uuid"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
)

type mockRepo struct {
	mock.Mock
}

func (m *mockRepo) Insert(ctx context.Context, event *domain.OutboxEvent) error {
	args := m.Called(ctx, event)
	return args.Error(0)
}

func (m *mockRepo) FetchPending(ctx context.Context, limit int) ([]*domain.OutboxEvent, error) {
	args := m.Called(ctx, limit)
	return args.Get(0).([]*domain.OutboxEvent), args.Error(1)
}

func (m *mockRepo) MarkAsSent(ctx context.Context, id uuid.UUID) error {
	args := m.Called(ctx, id)
	return args.Error(0)
}

type mockQueue struct {
	mock.Mock
}

func (m *mockQueue) Enqueue(ctx context.Context, event *domain.OutboxEvent) error {
	args := m.Called(ctx, event)
	return args.Error(0)
}

func TestOutboxDispatcher_Dispatch(t *testing.T) {
	ctx := context.Background()
	id := uuid.New()
	event := &domain.OutboxEvent{
		ID:          id,
		AggregateID: "user-123",
		EventType:   "payment.created",
		Payload:     []byte(`{"amount": 1000}`),
		Status:      domain.StatusPending,
		CreatedAt:   time.Now(),
	}

	repo := new(mockRepo)
	queue := new(mockQueue)
	dispatcher := NewOutboxDispatcher(repo, queue)

	repo.On("FetchPending", ctx, 10).Return([]*domain.OutboxEvent{event}, nil)
	queue.On("Enqueue", ctx, event).Return(nil)
	repo.On("MarkAsSent", ctx, id).Return(nil)

	err := dispatcher.Dispatch(ctx, 10)
	assert.NoError(t, err)

	repo.AssertExpectations(t)
	queue.AssertExpectations(t)
}
