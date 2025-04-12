package handler_test

import (
	"bytes"
	"net/http"
	"net/http/httptest"
	"testing"

	"payment-receiver/handler"

	"github.com/gin-gonic/gin"
	"github.com/stretchr/testify/assert"
)

func setupRouter() *gin.Engine {
	r := gin.Default()
	r.POST("/webhook", handler.WebhookHandler)
	return r
}

func TestWebhookHandler_ValidPayload(t *testing.T) {
	r := setupRouter()

	json := `{
		"id": "evt_123",
		"amount": 1500,
		"currency": "JPY",
		"method": "credit_card",
		"status": "paid",
		"occurred_at": "2024-04-13T12:00:00Z"
	}`

	req, _ := http.NewRequest("POST", "/webhook", bytes.NewBufferString(json))
	req.Header.Set("Content-Type", "application/json")
	resp := httptest.NewRecorder()

	r.ServeHTTP(resp, req)

	assert.Equal(t, http.StatusOK, resp.Code)
	assert.Contains(t, resp.Body.String(), "received")
}

func TestWebhookHandler_InvalidStatus(t *testing.T) {
	r := setupRouter()

	json := `{
		"id": "evt_456",
		"amount": 1200,
		"currency": "USD",
		"method": "paypal",
		"status": "unknown",
		"occurred_at": "2024-04-13T12:00:00Z"
	}`

	req, _ := http.NewRequest("POST", "/webhook", bytes.NewBufferString(json))
	req.Header.Set("Content-Type", "application/json")
	resp := httptest.NewRecorder()

	r.ServeHTTP(resp, req)

	assert.Equal(t, http.StatusBadRequest, resp.Code)
	assert.Contains(t, resp.Body.String(), "invalid status")
}
