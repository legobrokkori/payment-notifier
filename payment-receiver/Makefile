APP_NAME := payment-receiver
BIN_DIR := bin
CMD_DIR := ./cmd
GO_FILES := $(shell find . -name '*.go' -not -path "./vendor/*")

.PHONY: all build-webhook build-dispatcher run-webhook run-dispatcher test lint fmt clean

all: fmt lint test

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
test:
	go test ./... -v

clean:
	rm -rf $(BIN_DIR)
