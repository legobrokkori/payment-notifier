// Package main is the entrypoint of the payment-receiver service.
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

	// Postgres 接続
	dsn := os.Getenv("POSTGRES_DSN")
	db, err := infrastructure.NewPostgres(dsn)
	if err != nil {
		log.Fatalf("failed to connect to DB: %v", err)
	}
	defer db.Close()

	// Outbox Repository 初期化
	outboxRepo := infrastructure.NewPostgresOutbox(db)

	// Redis キュー初期化（Outbox用）
	redisQueue := infrastructure.NewRedisQueue(
		os.Getenv("REDIS_ADDR"),
		os.Getenv("REDIS_PASSWORD"),
		"outbox-events",
	)

	// Dispatcher 構築
	dispatcher := usecase.NewOutboxDispatcher(outboxRepo, redisQueue)

	// 実行
	log.Println("Running outbox dispatcher...")
	if err := dispatcher.Dispatch(ctx, 10); err != nil {
		log.Printf("dispatch error: %v", err)
	}

	// 一回だけ実行で終了（cronジョブ想定）
	log.Println("Dispatcher finished.")
}
