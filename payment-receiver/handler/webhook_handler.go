// Package handler provides HTTP handler functions.
package handler

import (
	"errors"
	"log"
	"net/http"

	"payment-receiver/domain"
	"payment-receiver/gen/proto"
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

		// Construct a protobuf PaymentEvent
		paymentEvent := &proto.PaymentEvent{
			Id:         req.ID,
			Amount:     int32(req.Amount),
			Currency:   req.Currency,
			Method:     req.Method,
			Status:     req.Status,
			OccurredAt: req.OccurredAt,
		}

		// Convert to OutboxEvent (with protobuf payload)
		outboxEvent, err := domain.NewOutboxEventFromProtoPayment(paymentEvent)
		if err != nil {
			c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
			return
		}

		// Enqueue to outbox
		if err := enqueuer.EnqueueOutboxEvent(c.Request.Context(), outboxEvent); err != nil {
			if errors.Is(err, usecase.ErrDuplicateEvent) {
				c.JSON(http.StatusOK, gin.H{
					"status": "duplicate",
					"note":   "event already accepted",
				})
				return
			}
			log.Printf("failed to insert to outbox: %v", err)
			c.JSON(http.StatusInternalServerError, gin.H{"error": "failed to queue event"})
			return
		}

		// Return success response with original payload
		c.JSON(http.StatusCreated, gin.H{
			"status":  "received",
			"payload": paymentEvent,
		})
	}
}
