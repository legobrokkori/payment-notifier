package handler

import (
	"net/http"

	"github.com/gin-gonic/gin"
)

type WebhookRequest struct {
	ID       string `json:"id" binding:"required"`
	Amount   int    `json:"amount" binding:"required"`
	Currency string `json:"currency" binding:"required"`
}

func HandleWebhook(c *gin.Context) {
	var req WebhookRequest
	if err := c.ShouldBindJSON(&req); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	// TODO: call usecase to enqueue to Redis
	c.JSON(http.StatusOK, gin.H{"message": "received"})
}
