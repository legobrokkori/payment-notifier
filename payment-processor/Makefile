.PHONY: build test lint format ci

# Paths
SLN=payment-processor.sln
PROJECT=src/PaymentProcessor/payment-processor.csproj
TEST_PROJECT=tests/PaymentProcessor.Tests/PaymentProcessor.Tests.csproj

# Proto
PROTO_SRC_DIR=../payment-receiver/gen/proto
PROTO_OUT_DIR=src/Protos

# Build the project
build:
	dotnet build $(SLN)

# Run tests
test:
	dotnet test $(SLN) --verbosity normal

# Run format check (verify no changes)
format-check:
	dotnet format $(SLN) --verify-no-changes --verbosity diagnostic --exclude src/PaymentProcessor/Infrastructure/Persistence/Migrations

# Auto-format all files
format:
	dotnet format $(SLN) --exclude src/PaymentProcessor/Infrastructure/Persistence/Migrations

# Lint (treat all warnings as errors)
lint:
	dotnet build $(SLN) --no-restore -warnaserror

# Restore dependencies
restore:
	dotnet restore $(SLN)

# Proto
gen-proto:
	@echo "Generating C# code from proto..."
	@if ! command -v protoc &> /dev/null; then \
		echo "Error: protoc not found. Please install protoc."; \
		exit 1; \
	fi
	@mkdir -p $(PROTO_OUT_DIR)
	@for proto in $(PROTO_SRC_DIR)/*.proto; do \
		protoc --proto_path=$(PROTO_SRC_DIR) --csharp_out=$(PROTO_OUT_DIR) $$proto; \
	done

# CI job (used in GitHub Actions)
ci: restore gen-proto format-check lint test
