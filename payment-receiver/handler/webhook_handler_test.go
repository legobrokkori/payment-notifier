package handler_test

import (
	"bytes"
	"context"
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"testing"
	"time"

	"payment-receiver/domain"
	"payment-receiver/handler"

	"github.com/gin-gonic/gin"
	"github.com/stretchr/testify/assert"
)

type mockOutboxEnqueuer struct {
	called bool
	event  *domain.PaymentEvent
	err    error
}

func (m *mockOutboxEnqueuer) EnqueueOutboxEvent(ctx context.Context, event *domain.PaymentEvent) error {
	m.called = true
	m.event = event
	return m.err
}

func TestWebhookHandler_Success(t *testing.T) {
	gin.SetMode(gin.TestMode)

	mock := &mockOutboxEnqueuer{}
	router := gin.Default()
	router.POST("/webhook", handler.WebhookHandler(mock))

	body := map[string]interface{}{
		"id":          "evt_001",
		"amount":      1200,
		"currency":    "USD",
		"method":      "card",
		"status":      "paid",
		"occurred_at": "2024-04-01T12:00:00Z",
	}
	jsonBody, _ := json.Marshal(body)

	req := httptest.NewRequest(http.MethodPost, "/webhook", bytes.NewBuffer(jsonBody))
	req.Header.Set("Content-Type", "application/json")

	w := httptest.NewRecorder()
	router.ServeHTTP(w, req)

	assert.Equal(t, http.StatusOK, w.Code)
	assert.True(t, mock.called)
	assert.Equal(t, "evt_001", mock.event.ID)
	assert.Equal(t, 1200, mock.event.Amount)
	assert.Equal(t, "USD", mock.event.Currency)
	assert.Equal(t, "card", mock.event.Method)
	assert.Equal(t, "paid", mock.event.Status)
	assert.Equal(t, mustParse("2024-04-01T12:00:00Z"), mock.event.OccurredAt)
}

func mustParse(s string) time.Time {
	t, _ := time.Parse(time.RFC3339, s)
	return t
}
