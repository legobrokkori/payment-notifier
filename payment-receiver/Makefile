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
.PHONY: lint fmt

lint:
	golangci-lint run

fmt:
	gofumpt -w .
	goimports -w .
	golines --max-len=100 -w .

clean:
	rm -rf bin/