package main

import (
	"log"
	"os"

	"payment-receiver/handler"
	"payment-receiver/infrastructure"

	"github.com/gin-gonic/gin"
)

func main() {
	// load .env if needed (optional)
	_ = os.Setenv("GIN_MODE", "release")

	// DI setup
	infrastructure.InitRedisAndInject()

	r := gin.Default()
	r.POST("/webhook", handler.WebhookHandler)

	port := os.Getenv("PORT")
	if port == "" {
		port = "8080"
	}

	if err := r.Run(":" + port); err != nil {
		log.Fatalf("server failed to start: %v", err)
	}
}
