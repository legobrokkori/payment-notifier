// Package handler provides HTTP handler functions.
package handler

import (
	"net/http"

	"payment-receiver/domain"

	"github.com/gin-gonic/gin"
)

// WebhookRequest represents the incoming webhook payload (DTO)
type WebhookRequest struct {
	ID         string `json:"id"          binding:"required"`
	Amount     int    `json:"amount"      binding:"required"`
	Currency   string `json:"currency"    binding:"required"`
	Method     string `json:"method"      binding:"required"`
	Status     string `json:"status"      binding:"required"`
	OccurredAt string `json:"occurred_at" binding:"required"`
}

// WebhookHandler handles incoming payment webhook POST requests
func WebhookHandler(c *gin.Context) {
	var req WebhookRequest

	if err := c.ShouldBindJSON(&req); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "invalid payload"})
		return
	}

	event, err := domain.NewPaymentEvent(
		req.ID,
		req.Amount,
		req.Currency,
		req.Method,
		req.Status,
		req.OccurredAt,
	)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	// TODO: usecase.EnqueuePaymentEvent(event) でキューへ送る

	c.JSON(http.StatusOK, gin.H{
		"status":  "received",
		"payload": event,
	})
}
