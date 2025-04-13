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

// Ensure RedisQueue implements both interfaces
var (
	_ usecase.OutboxQueue = (*RedisQueue)(nil)
)

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

// enqueueToRedis serializes the given event and pushes it to Redis.
func (q *RedisQueue) enqueueToRedis(ctx context.Context, event interface{}) error {
	data, err := json.Marshal(event)
	if err != nil {
		return fmt.Errorf("failed to marshal event: %w", err)
	}

	ctx, cancel := context.WithTimeout(ctx, q.timeout)
	defer cancel()

	return q.rdb.RPush(ctx, q.queue, data).Err()
}

// Enqueue implements OutboxQueue interface.
func (q *RedisQueue) Enqueue(ctx context.Context, event *domain.OutboxEvent) error {
	return q.enqueueToRedis(ctx, event)
}
