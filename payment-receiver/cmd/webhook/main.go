// Package main is the entrypoint of the payment-receiver dispatcher.
package main

import (
	"log"
	"os"

	"payment-receiver/handler"
	"payment-receiver/infrastructure"
	"payment-receiver/usecase"

	"github.com/gin-gonic/gin"
)

func main() {
	// Set Gin to release mode unless otherwise specified
	if os.Getenv("GIN_MODE") == "" {
		_ = os.Setenv("GIN_MODE", "release")
	}

	// Initialize postgres
	dsn := os.Getenv("POSTGRES_DSN")
	db, err := infrastructure.NewPostgres(dsn)
	if err != nil {
		log.Fatalf("failed to connect to DB: %v", err)
	}
	defer func() {
		if err := db.Close(); err != nil {
			log.Printf("failed to close DB: %v", err)
		}
	}()

	// Inject into usecase
	outboxRepo := infrastructure.NewPostgresOutbox(db)
	enqueuer := usecase.NewOutboxEnqueuer(outboxRepo)

	// Set up Gin router
	router := gin.Default()
	router.POST("/webhook", handler.WebhookHandler(enqueuer))

	port := os.Getenv("PORT")
	if port == "" {
		port = "8080"
	}

	log.Printf("Starting webhook server on port %s...", port)
	if err := router.Run(":" + port); err != nil {
		log.Fatalf("failed to start server: %v", err)
	}
}
