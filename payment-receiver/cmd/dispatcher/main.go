package main

import (
	"context"
	"log"
	"os"

	"payment-receiver/infrastructure"
	"payment-receiver/usecase"
)

func main() {
	ctx := context.Background()

	// 1. Postgres 接続
	dsn := os.Getenv("POSTGRES_DSN")
	db, err := infrastructure.NewPostgres(dsn)
	if err != nil {
		log.Fatalf("failed to connect to Postgres: %v", err)
	}
	defer db.Close()

	// 2. Repository 初期化
	repo := infrastructure.NewPostgresOutbox(db)

	// 3. Redis キュー初期化
	queue := infrastructure.NewRedisQueue(
		os.Getenv("REDIS_ADDR"),
		os.Getenv("REDIS_PASSWORD"),
		"payment-events",
	)

	// 4. Dispatcher 構築
	dispatcher := usecase.NewOutboxDispatcher(repo, queue)

	// 5. 実行
	log.Println("Running Outbox Dispatcher...")
	if err := dispatcher.Dispatch(ctx, 10); err != nil {
		log.Printf("dispatch error: %v", err)
	}

	log.Println("Dispatcher finished.")
}
