package main

import (
	"context"
	"log"
	"os"
	"os/signal"
	"syscall"
	"time"

	"payment-receiver/infrastructure"
	"payment-receiver/usecase"
)

func main() {
	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()

	// 1. Postgres 接続
	dsn := os.Getenv("POSTGRES_DSN")
	if dsn == "" {
		log.Fatal("POSTGRES_DSN is required")
	}
	db, err := infrastructure.NewPostgres(dsn)
	if err != nil {
		log.Fatalf("failed to connect to Postgres: %v", err)
	}
	defer db.Close()

	// 2. Repository 初期化
	repo := infrastructure.NewPostgresOutbox(db)

	// 3. Redis キュー初期化
	redisAddr := os.Getenv("REDIS_ADDR")
	if redisAddr == "" {
		log.Fatal("REDIS_ADDR is required")
	}
	queue := infrastructure.NewRedisQueue(
		redisAddr,
		os.Getenv("REDIS_PASSWORD"),
		"payment-events",
	)
	defer func() {
		if err := queue.Close(); err != nil {
			log.Printf("failed to close Redis: %v", err)
		}
	}()

	// 4. Dispatcher 構築
	dispatcher := usecase.NewOutboxDispatcher(repo, queue)

	// 5. Graceful shutdown 設定
	sigChan := make(chan os.Signal, 1)
	signal.Notify(sigChan, syscall.SIGINT, syscall.SIGTERM)

	// 6. ポーリング間隔設定
	ticker := time.NewTicker(5 * time.Second)
	defer ticker.Stop()

	log.Println("Starting Outbox Dispatcher (polling every 5 seconds)...")

	for {
		select {
		case <-ticker.C:
			if err := dispatcher.Dispatch(ctx, 10); err != nil {
				log.Printf("dispatch error: %v", err)
			}
		case sig := <-sigChan:
			log.Printf("Received signal %v, shutting down gracefully...", sig)
			cancel()
			log.Println("Dispatcher stopped.")
			return
		case <-ctx.Done():
			log.Println("Context canceled, exiting...")
			return
		}
	}
}
