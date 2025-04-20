# payment-processor

A background worker service built with **C# and .NET**, responsible for consuming payment events from **Redis**, validating them, and persisting them to **PostgreSQL**.

---

## 📌 Role in System

This service listens to a Redis queue (or stream) and processes payment events received by the `payment-receiver` service.

- Connects to Redis
- Deserializes messages
- Performs validation and transformation
- Stores processed data into the database

---

## 🧱 Project Structure

```bash
payment-processor/
├── Program.cs                        # Entry point
├── Application/                      # Use case logic
├── Domain/                           # Entities and interfaces
│   ├── Entities/Payment.cs
│   └── Interfaces/IPaymentRepository.cs
├── Infrastructure/                  # Adapters for Redis and database
│   ├── Redis/RedisPaymentEventSource.cs
│   └── Persistence/PaymentRepository.cs
├── Tests/                            # Unit/integration tests

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

Or manually:
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

- `gin` - Web framework
- `redis` - Redis client
- `zap` - Structured logging
- `sqlc` - (Optional) DB query generation

> Dependency versions are automatically managed by Renovate and Dependabot.

---

## 🧪 Testing

```bash
go test ./...
```

---

## 🔍 Linting

```bash
golangci-lint run
```

---

## 📄 License

MIT