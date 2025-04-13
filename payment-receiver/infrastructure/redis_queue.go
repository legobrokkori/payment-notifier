package infrastructure

import (
	"context"
	"encoding/json"
	"fmt"
	"os"
	"time"

	"payment-receiver/domain"
	"payment-receiver/usecase"

	"github.com/redis/go-redis/v9"
)

type RedisQueue struct {
	rdb     *redis.Client
	queue   string
	timeout time.Duration
}

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

func (q *RedisQueue) Enqueue(ctx context.Context, event *domain.PaymentEvent) error {
	jsonData, err := json.Marshal(event)
	if err != nil {
		return fmt.Errorf("failed to marshal event: %w", err)
	}

	ctx, cancel := context.WithTimeout(ctx, q.timeout)
	defer cancel()

	return q.rdb.RPush(ctx, q.queue, jsonData).Err()
}

// InitRedisAndInject sets up Redis and injects it to usecase layer
func InitRedisAndInject() {
	addr := os.Getenv("REDIS_ADDR")
	if addr == "" {
		addr = "localhost:6379"
	}
	password := os.Getenv("REDIS_PASSWORD")
	queueName := os.Getenv("REDIS_QUEUE")
	if queueName == "" {
		queueName = "payment-events"
	}

	queue := NewRedisQueue(addr, password, queueName)
	usecase.InjectQueue(queue)
}
