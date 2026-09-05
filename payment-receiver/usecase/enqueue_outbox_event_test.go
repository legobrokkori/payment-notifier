// Package usecase_test contains tests for application logic.
package usecase_test

import (
	"context"
	"testing"
	"time"

	"payment-receiver/domain"
	"payment-receiver/usecase"

	"github.com/google/uuid"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
)

// --- Mock Repository ---

type mockOutboxEnqueuerRepo struct {
	mock.Mock
}

func (m *mockOutboxEnqueuerRepo) Insert(ctx context.Context, event *domain.OutboxEvent) error {
	args := m.Called(ctx, event)
	return args.Error(0)
}

func (m *mockOutboxEnqueuerRepo) FetchPending(
	ctx context.Context,
	limit int,
) ([]*domain.OutboxEvent, error) {
	panic("not implemented")
}

func (m *mockOutboxEnqueuerRepo) MarkAsSent(ctx context.Context, id uuid.UUID) error {
	panic("not implemented")
}

func (m *mockOutboxEnqueuerRepo) ExistsByAggregateID(ctx context.Context, id string) (bool, error) {
	panic("not implemented - no longer used")
}

// --- Test Cases ---

func TestOutboxEnqueuer_EnqueueOutboxEvent_Success(t *testing.T) {
	mockRepo := new(mockOutboxEnqueuerRepo)
	enqueuer := usecase.NewOutboxEnqueuer(mockRepo)

	outboxEvent := &domain.OutboxEvent{
		ID:          uuid.New(),
		AggregateID: "test-id",
		EventType:   "payment_event",
		Payload:     []byte(`{"test": "data"}`),
		Status:      domain.StatusPending,
		CreatedAt:   time.Now(),
		EventAt:     time.Now(),
	}

	mockRepo.On("Insert", mock.Anything, mock.MatchedBy(func(ev *domain.OutboxEvent) bool {
		return ev.AggregateID == "test-id" &&
			ev.EventType == "payment_event" &&
			len(ev.Payload) > 0
	})).Return(nil).Once()

	err := enqueuer.EnqueueOutboxEvent(context.Background(), outboxEvent)
	assert.NoError(t, err)
	mockRepo.AssertExpectations(t)
}

func TestOutboxEnqueuer_EnqueueOutboxEvent_Duplicate(t *testing.T) {
	mockRepo := new(mockOutboxEnqueuerRepo)
	enqueuer := usecase.NewOutboxEnqueuer(mockRepo)

	outboxEvent := &domain.OutboxEvent{
		ID:          uuid.New(),
		AggregateID: "duplicate-id",
		EventType:   "payment_event",
		Payload:     []byte(`{"test": "data"}`),
		Status:      domain.StatusPending,
		CreatedAt:   time.Now(),
		EventAt:     time.Now(),
	}

	// DB制約による重複エラーをシミュレート
	mockRepo.On("Insert", mock.Anything, mock.Anything).
		Return(usecase.ErrDuplicateEvent).Once()

	err := enqueuer.EnqueueOutboxEvent(context.Background(), outboxEvent)
	assert.ErrorIs(t, err, usecase.ErrDuplicateEvent)
	mockRepo.AssertExpectations(t)
}
