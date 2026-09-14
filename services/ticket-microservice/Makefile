# Ticket Microservice Makefile

# Variables
SERVICE_NAME := ticket-microservice
SRC_DIR := src
COMPOSE_FILE := docker-compose.ticket-microservice.yml

# Colors for output
BLUE := \033[34m
GREEN := \033[32m
YELLOW := \033[33m
RED := \033[31m
NC := \033[0m # No Color

# Default target
.PHONY: help
help:
	@echo "$(BLUE)Ticket Microservice - Available Commands$(NC)"
	@echo ""
	@echo "$(GREEN)Docker (Production):$(NC)"
	@echo "  $(YELLOW)make docker-build$(NC)       - Build Docker image"
	@echo "  $(YELLOW)make docker-run$(NC)         - Run Docker container"
	@echo "  $(YELLOW)make docker-stop$(NC)        - Stop Docker container"
	@echo "  $(YELLOW)make docker-clean$(NC)       - Remove Docker image and container"
	@echo ""
	@echo "$(GREEN)Docker (Test Database):$(NC)"
	@echo "  $(YELLOW)make test-db-up$(NC)    - Start test database for integration tests"
	@echo "  $(YELLOW)make test-db-down$(NC)  - Stop test database"
	@echo "  $(YELLOW)make test-db-reset$(NC) - Reset test database (destroys all test data)"
	@echo ""
	@echo "$(GREEN)Development:$(NC)"
	@echo "  $(YELLOW)make run$(NC)           - Run application"
	@echo "  $(YELLOW)make build$(NC)         - Build the application"
	@echo "  $(YELLOW)make clean$(NC)         - Clean build artifacts"
	@echo "  $(YELLOW)make test$(NC)          - Run all tests"
	@echo ""
	@echo "$(GREEN)Code Quality:$(NC)"
	@echo "  $(YELLOW)make format$(NC)        - Format code"
	@echo ""
	@echo "$(GREEN)Info:$(NC)"
	@echo "  $(YELLOW)make info$(NC)          - Show service information"

# =============================================================================
# Docker - Test Database (MINIMAL SET)
# =============================================================================

.PHONY: test-db-up
test-db-up:
	@echo "$(BLUE)Starting test database for integration tests...$(NC)"
	docker-compose -f $(COMPOSE_FILE) up -d postgres-test

.PHONY: test-db-down
test-db-down:
	@echo "$(YELLOW)Stopping test database...$(NC)"
	docker-compose -f $(COMPOSE_FILE) down postgres-test

.PHONY: test-db-reset
test-db-reset:
	@echo "$(RED)WARNING: This will destroy all test data!$(NC)"
	@echo "Stopping and removing test database container..."
	docker-compose -f $(COMPOSE_FILE) down postgres-test
	@echo "Starting fresh test database..."
	docker-compose -f $(COMPOSE_FILE) up -d postgres-test
	@echo "$(GREEN)Test database reset complete!$(NC)"
	@echo "Connect to: localhost:$(shell grep POSTGRES_TEST_PORT_EXTERNAL .env | cut -d'=' -f2)"

# =============================================================================
# Docker - Production Build & Run
# =============================================================================

.PHONY: docker-build
docker-build:
	@echo "$(BLUE)Building Docker image for $(SERVICE_NAME)...$(NC)"
	@VERSION=$$(git describe --tags --always --dirty 2>/dev/null || echo "1.0.0"); \
	BUILD_DATE=$$(date -u +"%Y-%m-%dT%H:%M:%SZ"); \
	docker build \
		-t $(SERVICE_NAME):$$VERSION \
		--build-arg VERSION=$$VERSION \
		--build-arg BUILD_DATE=$$BUILD_DATE \
		-f Dockerfile \
		. 
	@echo "$(GREEN)Docker image built successfully: $(SERVICE_NAME):$$VERSION$(NC)"

.PHONY: docker-run
docker-run:
	@echo "$(BLUE)Starting Docker container...$(NC)"
	@echo "$(YELLOW)Make sure PostgreSQL is running: make docker-db-up$(NC)"
	docker-compose -f $(COMPOSE_FILE) up -d ticket-api
	@echo "$(GREEN)Container started!$(NC)"
	@echo "API URL: http://localhost:$(shell grep APP_PORT .env | cut -d'=' -f2)"
	@echo "Swagger: http://localhost:$(shell grep APP_PORT .env | cut -d'=' -f2)/swagger"

.PHONY: docker-stop
docker-stop:
	@echo "$(YELLOW)Stopping Docker container...$(NC)"
	docker-compose -f $(COMPOSE_FILE) down
	@echo "$(GREEN)Container stopped!$(NC)"

.PHONY: docker-clean
docker-clean:
	@echo "$(YELLOW)Removing Docker image and container...$(NC)"
	docker-compose -f $(COMPOSE_FILE) down
	docker rmi -f $(SERVICE_NAME):latest 2>/dev/null || true
	docker rmi -f $(SERVICE_NAME):$$(git describe --tags --always --dirty 2>/dev/null || echo "1.0.0") 2>/dev/null || true
	@echo "$(GREEN)Docker resources cleaned!$(NC)"

.PHONY: docker-db-up
docker-db-up:
	@echo "$(BLUE)Starting PostgreSQL database...$(NC)"
	docker-compose -f $(COMPOSE_FILE) up -d postgres-prod
	@echo "$(GREEN)Database started!$(NC)"
	@echo "Connect to: localhost:$(shell grep POSTGRES_PROD_PORT_EXTERNAL .env | cut -d'=' -f2)/tickets"

.PHONY: docker-db-down
docker-db-down:
	@echo "$(YELLOW)Stopping PostgreSQL database...$(NC)"
	docker-compose -f $(COMPOSE_FILE) down postgres-prod

# =============================================================================
# Development
# =============================================================================

.PHONY: run
run:
	@echo "$(BLUE)Running application...$(NC)"
	cd $(SRC_DIR) && dotnet run --project presentation/presentation.csproj

.PHONY: build
build:
	@echo "$(BLUE)Building application...$(NC)"
	cd $(SRC_DIR) && dotnet build

.PHONY: clean
clean:
	@echo "$(YELLOW)Cleaning build artifacts...$(NC)"
	cd $(SRC_DIR) && dotnet clean
	rm -rf $(SRC_DIR)/bin $(SRC_DIR)/obj

# Tests
.PHONY: test
test:
	@echo "$(BLUE)Running all tests...$(NC)"
	cd $(SRC_DIR) && dotnet test --verbosity normal

# Code Quality
.PHONY: format
format:
	@echo "$(BLUE)Formatting code...$(NC)"
	cd $(SRC_DIR) && dotnet format

# Info
.PHONY: info
info:
	@echo "$(BLUE)Ticket Microservice Information$(NC)"
	@echo ""
	@echo "Service Name: $(SERVICE_NAME)"
	@echo "API URL: http://localhost:8080"
	@echo "Swagger: http://localhost:8080/swagger"
	@echo ""
	@echo "$(GREEN)Databases:$(NC)"
	@echo "  Production: localhost:$(shell grep POSTGRES_PROD_PORT_EXTERNAL .env | cut -d'=' -f2)/tickets"
	@echo "  Test:       localhost:$(shell grep POSTGRES_TEST_PORT_EXTERNAL .env | cut -d'=' -f2)/test_tickets"
	@echo ""
	@echo "$(GREEN)Quick Start:$(NC)"
	@echo "  1. make docker-db-up    - Start PostgreSQL database"
	@echo "  2. make docker-build    - Build Docker image"
	@echo "  3. make docker-run      - Run Docker container"
	@echo "  4. make test            - Run tests"
	@echo ""
	@echo "$(GREEN)Development Mode:$(NC)"
	@echo "  1. make test-db-up      - Start test database"
	@echo "  2. make run             - Run application (dotnet run)"
	@echo "  3. make test            - Run tests"
	@echo ""
