.PHONY: build test lint format ci

# Paths
SLN=payment-processor.sln
PROJECT=src/PaymentProcessor/payment-processor.csproj
TEST_PROJECT=tests/PaymentProcessor.Tests/PaymentProcessor.Tests.csproj

# Build the project
build:
	dotnet build $(SLN)

# Run tests
test:
	dotnet test $(SLN) --verbosity normal

# Run format check (verify no changes)
format-check:
	dotnet format $(SLN) --verify-no-changes --verbosity diagnostic

# Auto-format all files
format:
	dotnet format $(SLN)

# Lint (treat all warnings as errors)
lint:
	dotnet build $(SLN) --no-restore -warnaserror

# Restore dependencies
restore:
	dotnet restore $(SLN)

# CI job (used in GitHub Actions)
ci: restore format-check lint test
