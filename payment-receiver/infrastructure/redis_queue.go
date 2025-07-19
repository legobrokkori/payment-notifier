// Package infrastructure implements repository interfaces using concrete tools like Redis and Postgres.
package infrastructure

import (
	"context"
	"fmt"
	"time"

	"payment-receiver/domain"
	pb "payment-receiver/gen/proto" // Protobuf-generated Go code
	"payment-receiver/usecase"

	redis "github.com/redis/go-redis/v9"
	"google.golang.org/protobuf/proto"
)

// RedisQueue implements the Queue interface using Redis.
type RedisQueue struct {
	rdb     *redis.Client
	queue   string
	timeout time.Duration
}

var _ usecase.OutboxQueue = (*RedisQueue)(nil)

// NewRedisQueue creates and initializes a new RedisQueue instance.
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

// Enqueue serializes the event payload using Protobuf and sends it to Redis Stream.
func (q *RedisQueue) Enqueue(ctx context.Context, event *domain.OutboxEvent) error {
	ctx, cancel := context.WithTimeout(ctx, q.timeout)
	defer cancel()

	// Unmarshal OutboxEvent.Payload into Protobuf model
	var paymentEvent pb.PaymentEvent
	if err := proto.Unmarshal(event.Payload, &paymentEvent); err != nil {
		return fmt.Errorf("failed to unmarshal protobuf payload: %w", err)
	}

	// Marshal back to binary protobuf for transport (optional, could use Payload as-is)
	data, err := proto.Marshal(&paymentEvent)
	if err != nil {
		return fmt.Errorf("failed to marshal protobuf: %w", err)
	}

	return q.rdb.XAdd(ctx, &redis.XAddArgs{
		Stream: q.queue,
		Values: map[string]interface{}{
			"data": data,
		},
	}).Err()
}
