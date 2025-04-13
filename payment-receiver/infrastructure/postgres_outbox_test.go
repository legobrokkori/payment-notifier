package infrastructure_test

import (
	"context"
	"database/sql"
	"os"
	"testing"
	"time"

	"payment-receiver/domain"
	"payment-receiver/infrastructure"

	"github.com/google/uuid"
	"github.com/stretchr/testify/assert"
)

func setupTestDB(t *testing.T) *sql.DB {
	dsn := os.Getenv("POSTGRES_DSN")
	if dsn == "" {
		// fallback for local/integration test environments
		dsn = "postgres://user:password@localhost:5432/payment?sslmode=disable"
	}
	db, err := infrastructure.NewPostgres(dsn)
	if err != nil {
		t.Fatalf("failed to connect to DB: %v", err)
	}
	t.Cleanup(func() {
		_ = db.Close()
	})
	return db
}

func TestInsert_DuplicateAggregateID(t *testing.T) {
	db := setupTestDB(t)
	repo := infrastructure.NewPostgresOutbox(db)

	ctx := context.Background()
	event := &domain.OutboxEvent{
		ID:          uuid.New(),
		AggregateID: "evt_001",
		EventType:   "payment_event",
		Payload:     []byte(`{"id":"evt_001"}`),
		Status:      domain.StatusPending,
		CreatedAt:   time.Now(),
		EventAt:     time.Now(),
	}

	err := repo.Insert(ctx, event)
	assert.NoError(t, err)

	// 2回目：同じ AggregateID を使って Insert → duplicate error を期待
	dup := *event
	dup.ID = uuid.New()

	err = repo.Insert(ctx, &dup)
	assert.ErrorIs(t, err, infrastructure.ErrDuplicateKey)
}
