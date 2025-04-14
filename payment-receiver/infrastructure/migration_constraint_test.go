package infrastructure_test

import (
	"database/sql"
	"os"
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestOutboxAggregateIDHasUniqueConstraint(t *testing.T) {
	dsn := os.Getenv("POSTGRES_DSN")
	db, err := sql.Open("postgres", dsn)
	assert.NoError(t, err)
	defer db.Close()

	var exists bool
	query := `
		SELECT EXISTS (
			SELECT 1
			FROM pg_constraint
			WHERE conrelid = 'outbox_events'::regclass
			AND contype = 'u'
			AND conname = 'unique_aggregate_id'
		);
	`
	err = db.QueryRow(query).Scan(&exists)
	assert.NoError(t, err)
	assert.True(t, exists, "expected unique constraint 'unique_aggregate_id' to exist")
}
