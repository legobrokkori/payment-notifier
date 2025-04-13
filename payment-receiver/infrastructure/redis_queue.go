// Package infrastructure implements repository interfaces using concrete tools like Redis and Postgres.
package infrastructure

import (
	"context"
	"encoding/json"
	"fmt"
	"time"

	"payment-receiver/domain"
	"payment-receiver/usecase"

	"github.com/redis/go-redis/v9"
)

// RedisQueue implements the Queue interface using Redis.
type RedisQueue struct {
	rdb     *redis.Client
	queue   string
	timeout time.Duration
}

// Ensure RedisQueue implements the OutboxQueue interface at compile time.
var _ usecase.OutboxQueue = (*RedisQueue)(nil)

// NewRedisQueue creates and initializes a new RedisQueue instance.
// It connects to Redis using the provided address, password, and queue name.
func NewRedisQueue(addr, password, queueName string) *RedisQueue {
	rdb := redis.NewClient(&redis.Options{
		Addr:     addr,
		Password: password,
		DB:       0,
	})

	return &RedisQueue{
		rdb:     rdb,
		queue:   queueName,
		timeout: 5 * time.Second,
	}
}

// Enqueue serializes the given OutboxEvent as JSON and pushes it to the Redis queue.
// This method sets a timeout to avoid hanging connections.
func (q *RedisQueue) Enqueue(ctx context.Context, event *domain.OutboxEvent) error {
	data, err := json.Marshal(event)
	if err != nil {
		return fmt.Errorf("failed to marshal outbox event: %w", err)
	}

	ctx, cancel := context.WithTimeout(ctx, q.timeout)
	defer cancel()

	return q.rdb.RPush(ctx, q.queue, data).Err()
}
