# Flight Microservice Makefile

# Variables
SERVICE_NAME := flight-microservice
SRC_DIR := src
COMPOSE_FILE := docker-compose.flight-microservice.yml

# Colors for output
BLUE := \033[34m
GREEN := \033[32m
YELLOW := \033[33m
RED := \033[31m
NC := \033[0m # No Color

# Default target
.PHONY: help
help:
	@echo "$(BLUE)Flight Microservice - Available Commands$(NC)"
	@echo ""
	@echo "$(GREEN)Development:$(NC)"
	@echo "  $(YELLOW)make run$(NC)              - Run application in development mode"
	@echo "  $(YELLOW)make build$(NC)            - Build the application"
	@echo "  $(YELLOW)make clean$(NC)            - Clean build artifacts"
	@echo "  $(YELLOW)make test$(NC)             - Run all tests"
	@echo "  $(YELLOW)make test-unit$(NC)        - Run unit tests only"
	@echo "  $(YELLOW)make test-integration$(NC) - Run integration tests"
	@echo ""
	@echo "$(GREEN)Docker:$(NC)"
	@echo "  $(YELLOW)make docker-up$(NC)        - Start all containers"
	@echo "  $(YELLOW)make docker-down$(NC)      - Stop all containers"
	@echo "  $(YELLOW)make docker-restart$(NC)   - Restart all containers"
	@echo "  $(YELLOW)make docker-logs$(NC)      - View API logs"
	@echo "  $(YELLOW)make docker-build$(NC)     - Build Docker images"
	@echo "  $(YELLOW)make docker-clean$(NC)     - Remove all containers and volumes"
	@echo ""
	@echo "$(GREEN)Database:$(NC)"
	@echo "  $(YELLOW)make db-migrate$(NC)       - Run database migrations"
	@echo "  $(YELLOW)make db-seed$(NC)          - Seed database with initial data"
	@echo "  $(YELLOW)make db-reset$(NC)         - Reset database (WARNING: destroys data)"
	@echo ""
	@echo "$(GREEN)Code Quality:$(NC)"
	@echo "  $(YELLOW)make format$(NC)           - Format code"
	@echo "  $(YELLOW)make lint$(NC)             - Run linter"
	@echo ""

# Development
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

.PHONY: test-unit
test-unit:
	@echo "$(BLUE)Running unit tests...$(NC)"
	cd $(SRC_DIR) && dotnet test --filter "Category=Unit" --verbosity normal

.PHONY: test-integration
test-integration:
	@echo "$(BLUE)Running integration tests...$(NC)"
	cd $(SRC_DIR) && dotnet test --filter "Category=Integration" --verbosity normal

# Docker
.PHONY: docker-up
docker-up:
	@echo "$(BLUE)Starting containers...$(NC)"
	docker-compose -f $(COMPOSE_FILE) up -d

.PHONY: docker-down
docker-down:
	@echo "$(YELLOW)Stopping containers...$(NC)"
	docker-compose -f $(COMPOSE_FILE) down

.PHONY: docker-restart
docker-restart:
	@echo "$(YELLOW)Restarting containers...$(NC)"
	docker-compose -f $(COMPOSE_FILE) restart

.PHONY: docker-logs
docker-logs:
	docker-compose -f $(COMPOSE_FILE) logs -f flight-api

.PHONY: docker-build
docker-build:
	@echo "$(BLUE)Building Docker images...$(NC)"
	docker-compose -f $(COMPOSE_FILE) build

.PHONY: docker-clean
docker-clean:
	@echo "$(RED)Removing all containers and volumes...$(NC)"
	docker-compose -f $(COMPOSE_FILE) down -v
	docker system prune -f

# Database
.PHONY: db-migrate
db-migrate:
	@echo "$(BLUE)Running database migrations...$(NC)"
	@echo "TODO: Add EF Core migrations command"

.PHONY: db-seed
db-seed:
	@echo "$(BLUE)Seeding database...$(NC)"
	@echo "TODO: Add database seeding command"

.PHONY: db-reset
db-reset:
	@echo "$(RED)WARNING: This will destroy all data!$(NC)"
	read -p "Are you sure? (y/N) " confirm && \
	if [ "$$confirm" = "y" ]; then \
		docker-compose -f $(COMPOSE_FILE) down -v; \
		docker-compose -f $(COMPOSE_FILE) up -d postgres; \
		echo "$(GREEN)Database reset complete$(NC)"; \
	else \
		echo "$(YELLOW)Operation cancelled$(NC)"; \
	fi

# Code Quality
.PHONY: format
format:
	@echo "$(BLUE)Formatting code...$(NC)"
	cd $(SRC_DIR) && dotnet format

.PHONY: lint
lint:
	@echo "$(BLUE)Running linter...$(NC)"
	cd $(SRC_DIR) && dotnet format --verify-no-changes

# Info
.PHONY: info
info:
	@echo "$(BLUE)Flight Microservice Information$(NC)"
	@echo ""
	@echo "Service Name: $(SERVICE_NAME)"
	@echo "API URL: http://localhost:8080"
	@echo "Swagger: http://localhost:8080/swagger"
	@echo "Database: postgres://postgres:postgres@localhost:5432/flight_db"
	@echo ""
