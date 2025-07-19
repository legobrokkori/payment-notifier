package handler_test

import (
	"bytes"
	"context"
	"encoding/json"
	"errors"
	"net/http"
	"net/http/httptest"
	"strings"
	"testing"

	"payment-receiver/domain"
	"payment-receiver/handler"
	"payment-receiver/usecase"

	"github.com/gin-gonic/gin"
	"github.com/stretchr/testify/assert"
)

type mockOutboxEnqueuer struct {
	called bool
	event  *domain.OutboxEvent
	err    error
}

func (m *mockOutboxEnqueuer) EnqueueOutboxEvent(
	ctx context.Context,
	event *domain.OutboxEvent,
) error {
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

	assert.Equal(t, http.StatusCreated, w.Code)
	assert.True(t, mock.called)
	assert.Equal(t, "evt_001", mock.event.AggregateID)
	assert.Equal(t, "payment_event", mock.event.EventType)
}

func TestWebhookHandler_InvalidJSON(t *testing.T) {
	router := gin.Default()
	router.POST("/webhook", handler.WebhookHandler(&mockOutboxEnqueuer{}))

	req := httptest.NewRequest(http.MethodPost, "/webhook", strings.NewReader("{invalid json"))
	req.Header.Set("Content-Type", "application/json")

	w := httptest.NewRecorder()
	router.ServeHTTP(w, req)

	assert.Equal(t, http.StatusBadRequest, w.Code)
	assert.Contains(t, w.Body.String(), "invalid payload")
}

func TestWebhookHandler_MissingField(t *testing.T) {
	router := gin.Default()
	router.POST("/webhook", handler.WebhookHandler(&mockOutboxEnqueuer{}))

	body := map[string]interface{}{
		"id": "evt_001",
	}
	jsonBody, _ := json.Marshal(body)

	req := httptest.NewRequest(http.MethodPost, "/webhook", bytes.NewBuffer(jsonBody))
	req.Header.Set("Content-Type", "application/json")

	w := httptest.NewRecorder()
	router.ServeHTTP(w, req)

	assert.Equal(t, http.StatusBadRequest, w.Code)
	assert.Contains(t, w.Body.String(), "invalid payload")
}

func TestWebhookHandler_InvalidOccurredAtFormat(t *testing.T) {
	router := gin.Default()
	router.POST("/webhook", handler.WebhookHandler(&mockOutboxEnqueuer{}))

	body := map[string]interface{}{
		"id":          "evt_001",
		"amount":      1200,
		"currency":    "USD",
		"method":      "card",
		"status":      "paid",
		"occurred_at": "not-a-date",
	}
	jsonBody, _ := json.Marshal(body)

	req := httptest.NewRequest(http.MethodPost, "/webhook", bytes.NewBuffer(jsonBody))
	req.Header.Set("Content-Type", "application/json")

	w := httptest.NewRecorder()
	router.ServeHTTP(w, req)

	assert.Equal(t, http.StatusBadRequest, w.Code)
	assert.Contains(t, w.Body.String(), "invalid occurred_at format")
}

func TestWebhookHandler_InvalidStatus(t *testing.T) {
	router := gin.Default()
	router.POST("/webhook", handler.WebhookHandler(&mockOutboxEnqueuer{}))

	body := map[string]interface{}{
		"id":          "evt_001",
		"amount":      1200,
		"currency":    "USD",
		"method":      "card",
		"status":      "unknown",
		"occurred_at": "2024-04-01T12:00:00Z",
	}
	jsonBody, _ := json.Marshal(body)

	req := httptest.NewRequest(http.MethodPost, "/webhook", bytes.NewBuffer(jsonBody))
	req.Header.Set("Content-Type", "application/json")

	w := httptest.NewRecorder()
	router.ServeHTTP(w, req)

	assert.Equal(t, http.StatusBadRequest, w.Code)
	assert.Contains(t, w.Body.String(), "invalid status")
}

func TestWebhookHandler_InternalError(t *testing.T) {
	router := gin.Default()
	mock := &mockOutboxEnqueuer{
		err: errors.New("unexpected error"),
	}
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

	assert.Equal(t, http.StatusInternalServerError, w.Code)
	assert.Contains(t, w.Body.String(), "failed to queue event")
}

func TestWebhookHandler_Duplicate(t *testing.T) {
	gin.SetMode(gin.TestMode)

	mock := &mockOutboxEnqueuer{
		err: usecase.ErrDuplicateEvent,
	}
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
	assert.Contains(t, w.Body.String(), `"duplicate"`)
}
