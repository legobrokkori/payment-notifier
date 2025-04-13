APP_NAME := payment-receiver
BIN_DIR := bin
CMD_DIR := ./cmd
GO_FILES := $(shell find . -name '*.go' -not -path "./vendor/*")

MIGRATE_VERSION := v4.16.2
MIGRATE := migrate

MIGRATE_DB_URL := postgres://user:password@localhost:5432/payment?sslmode=disable
MIGRATE_DIR := migrations

.PHONY: all build-webhook build-dispatcher run-webhook run-dispatcher test-unit test-integration lint fmt clean migrate-up migrate-down migrate-create

all: fmt lint test-unit test-integration

## Build binaries
build-webhook:
	go build -o $(BIN_DIR)/webhook $(CMD_DIR)/webhook

build-dispatcher:
	go build -o $(BIN_DIR)/dispatcher $(CMD_DIR)/dispatcher

## Run local binaries
run-webhook:
	@if [ ! -f $(BIN_DIR)/webhook ]; then echo "Webhook binary not found. Run 'make build-webhook' first."; exit 1; fi
	$(BIN_DIR)/webhook

run-dispatcher:
	@if [ ! -f $(BIN_DIR)/dispatcher ]; then echo "Dispatcher binary not found. Run 'make build-dispatcher' first."; exit 1; fi
	$(BIN_DIR)/dispatcher

## Format code
fmt:
	gofumpt -w .
	goimports -w .
	golines --max-len=100 -w .

## Linting
lint:
	golangci-lint run

## Test
# Run unit tests (excluding infrastructure or external services)
test-unit:
	go test -v $(shell go list ./... | grep -v /infrastructure)

# Run integration tests (requires PostgreSQL, etc.)
test-integration:
	go test -v ./infrastructure

clean:
	rm -rf $(BIN_DIR)

migrate-up:
	$(MIGRATE) -path $(MIGRATE_DIR) -database "$(MIGRATE_DB_URL)" up

migrate-down:
	$(MIGRATE) -path $(MIGRATE_DIR) -database "$(MIGRATE_DB_URL)" down

migrate-create:
	$(MIGRATE) create -ext sql -dir $(MIGRATE_DIR) -format timestamp "$(name)"