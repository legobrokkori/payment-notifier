// Package handler provides HTTP handler functions.
package handler

import (
	"log"
	"net/http"

	"payment-receiver/domain"
	"payment-receiver/usecase"

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

// WebhookHandler returns a gin.HandlerFunc with injected usecase.
func WebhookHandler(enqueuer usecase.OutboxEventSaver) gin.HandlerFunc {
	return func(c *gin.Context) {
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
		if err := enqueuer.EnqueueOutboxEvent(c.Request.Context(), event); err != nil {
			c.JSON(http.StatusInternalServerError, gin.H{"error": "failed to queue event"})
			return
		}
		if err := enqueuer.EnqueueOutboxEvent(c.Request.Context(), event); err != nil {
			log.Printf("failed to insert to outbox: %v", err)
			c.JSON(http.StatusInternalServerError, gin.H{"error": "failed to queue event"})
			return
		}

		c.JSON(http.StatusOK, gin.H{
			"status":  "received",
			"payload": event,
		})
	}
}
