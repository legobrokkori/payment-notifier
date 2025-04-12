# payment-receiver

A lightweight Webhook receiver built with **Go + Gin**, responsible for accepting incoming payment events and enqueuing them into **Redis** for asynchronous processing.

---

## 📌 Role in System

This service is the entrypoint of the architecture.
- Accepts HTTP `POST` requests from external payment providers
- Parses and validates incoming JSON payloads
- Enqueues validated events into Redis for background processing by `payment-processor`

---

## 🧪 Example Request

```bash
curl -X POST http://localhost:8080/webhook/payment \
  -H "Content-Type: application/json" \
  -d '{
        "id": "pay_1234567890",
        "amount": 1200,
        "currency": "JPY"
      }'
```

---

## 🧱 Project Structure

```bash
payment-receiver/
├── cmd/main.go                       # Entry point
├── internal/
│   ├── handler/webhook_handler.go   # Handles incoming HTTP requests
│   ├── usecase/                     # Business logic layer
│   ├── domain/                      # Entities & interfaces
│   └── infrastructure/             # Redis & persistence adapters
├── sql/                             # SQL definitions for sqlc (if used)
├── .env.sample                      # Sample environment variables
├── Makefile                         # Dev shortcuts
├── .golangci.yml                    # Linter config
└── README.md                        # (this file)
```

---

## 🚀 Running Locally

```bash
make run
```

**Or manually:**
```bash
go run ./cmd/main.go
```

> Requires `PORT` and `REDIS_URL` to be configured (see `.env.sample`)

---

## ⚙️ Makefile Commands

```bash
make run         # Run the server
make lint        # Run linters (golangci-lint)
make test        # Run unit tests
```

---

## 🔧 Environment Variables (`.env`)

| Variable     | Description                       |
|--------------|------------------------------------|
| `PORT`       | Server port (default: 8080)         |
| `REDIS_URL`  | Redis connection string (Upstash or local) |

Example:
```env
PORT=8080
REDIS_URL=redis://localhost:6379
```

---

## 📦 Dependencies

- [`gin`](https://github.com/gin-gonic/gin) - Web framework
- [`redis`](https://pkg.go.dev/github.com/redis/go-redis/v9) - Redis client
- [`zap`](https://github.com/uber-go/zap) - Structured logging
- [`sqlc`](https://sqlc.dev/) - (Optional) DB query generation

---

## 🧪 Testing

```bash
go test ./...
```

> Consider mocking Redis with interfaces for clean unit testing.

---

## 🔍 Linting

```bash
golangci-lint run
```

Configurable via `.golangci.yml`

---

## 📄 License

MIT