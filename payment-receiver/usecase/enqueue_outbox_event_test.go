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
	ShouldExist bool
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
	return m.ShouldExist, nil
}

// --- Test Case ---

func TestOutboxEnqueuer_EnqueueOutboxEvent(t *testing.T) {
	mockRepo := &mockOutboxEnqueuerRepo{ShouldExist: false}
	enqueuer := usecase.NewOutboxEnqueuer(mockRepo)

	paymentEvent := &domain.PaymentEvent{
		ID:         "test-id",
		Amount:     1000,
		Currency:   "USD",
		Method:     "card",
		Status:     "paid",
		OccurredAt: time.Now(),
	}

	mockRepo.On("Insert", mock.Anything, mock.MatchedBy(func(ev *domain.OutboxEvent) bool {
		// match only key fields
		return ev.AggregateID == "test-id" &&
			ev.EventType == "payment_event" &&
			len(ev.Payload) > 0 // ensure it’s marshaled
	})).Return(nil).Once()

	outboxEvent := &domain.OutboxEvent{
		AggregateID: paymentEvent.ID,
		EventType:   "payment_event",
		Payload:     []byte(`{"test": "data"}`),
	}

	err := enqueuer.EnqueueOutboxEvent(context.Background(), outboxEvent)
	assert.NoError(t, err)
	mockRepo.AssertExpectations(t)
}
