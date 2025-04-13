APP_NAME=payment-receiver
GO_FILES=$(shell find . -name '*.go' -not -path "./vendor/*")

.PHONY: all build run test lint clean

all: lint test

build:
	go build -o bin/$(APP_NAME) ./...

run:
	go run main.go

test:
	go test ./... -v

lint:
	golangci-lint run --timeout=5m

clean:
	rm -rf bin/