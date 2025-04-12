package main

import (
	"log"
	"os"

	"payment-receiver/internal/handler"

	"github.com/gin-gonic/gin"
)

func main() {
	r := gin.Default()
	r.POST("/webhook/payment", handler.HandleWebhook)
	port := os.Getenv("PORT")
	if port == "" {
		port = "8080"
	}
	log.Printf("Starting server on :%s...", port)
	r.Run(":" + port)
}
