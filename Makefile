# ===========================================
# Person Service - Makefile
# ===========================================
# Commands for managing Docker containers and database
#

# Variables
COMPOSE_FILE := docker-compose.yml
ENV_FILE := .env
PROJECT_NAME := person-service
COMPOSE_OPTS := --env-file $(ENV_FILE) -f $(COMPOSE_FILE)

# Colors for output
BLUE := \033[34m
GREEN := \033[32m
YELLOW := \033[33m
RED := \033[31m
NC := \033[0m # No Color

# ===========================================
# Database Commands
# ===========================================

# Start all containers (production + test databases)
.PHONY: up
up:
	@echo "$(BLUE)Starting all containers...$(NC)"
	docker-compose $(COMPOSE_OPTS) up -d
	@echo "$(GREEN)✓ All containers started successfully!$(NC)"
	@echo ""
	@echo "Production DB: localhost:$(shell grep POSTGRES_PROD_PORT_EXTERNAL $(ENV_FILE) | cut -d'=' -f2)"
	@echo "Test DB:       localhost:$(shell grep POSTGRES_TEST_PORT_EXTERNAL $(ENV_FILE) | cut -d'=' -f2)"
	@echo "Data stored:   $(shell grep POSTGRES_PROD_DATA_PATH $(ENV_FILE) | cut -d'=' -f2)"

# Start only production database
.PHONY: up-prod
up-prod:
	@echo "$(BLUE)Starting production database...$(NC)"
	docker-compose $(COMPOSE_OPTS) up -d postgres-prod
	@echo "$(GREEN)✓ Production database started!$(NC)"

# Start only test database
.PHONY: up-test
up-test:
	@echo "$(BLUE)Starting test database...$(NC)"
	docker-compose $(COMPOSE_OPTS) up -d postgres-test
	@echo "$(GREEN)✓ Test database started!$(NC)"

# Start all containers with logs
.PHONY: up-logs
up-logs:
	docker-compose $(COMPOSE_OPTS) up

# Stop all containers
.PHONY: down
down:
	@echo "$(YELLOW)Stopping all containers...$(NC)"
	docker-compose $(COMPOSE_OPTS) down
	@echo "$(GREEN)✓ All containers stopped!$(NC)"

# Stop and remove volumes (WARNING: this will delete all data!)
.PHONY: down-volumes
down-volumes:
	@echo "$(RED)WARNING: This will delete all database data!$(NC)"
	@read -p "Are you sure? (y/N): " confirm && \
	if [ "$$confirm" = "y" ]; then \
		docker-compose $(COMPOSE_OPTS) down -v; \
		echo "$(GREEN)✓ Containers stopped and volumes removed!$(NC)"; \
	else \
		echo "$(YELLOW)Operation cancelled.$(NC)"; \
	fi

# Restart containers
.PHONY: restart
restart: down up

# ===========================================
# Database Management Commands
# ===========================================

# Check container status
.PHONY: status
status:
	@echo "$(BLUE)Container Status:$(NC)"
	docker-compose $(COMPOSE_OPTS) ps

# View logs for all containers
.PHONY: logs
logs:
	docker-compose $(COMPOSE_OPTS) logs -f

# View logs for production database
.PHONY: logs-prod
logs-prod:
	docker-compose $(COMPOSE_OPTS) logs -f postgres-prod

# View logs for test database
.PHONY: logs-test
logs-test:
	docker-compose $(COMPOSE_OPTS) logs -f postgres-test

# View logs for application
.PHONY: logs-app
logs-app:
	docker-compose $(COMPOSE_OPTS) logs -f app

# ===========================================
# Database Access Commands
# ===========================================

# Connect to production database via psql
.PHONY: db-prod
db-prod:
	@echo "$(BLUE)Connecting to production database...$(NC)"
	docker-compose $(COMPOSE_OPTS) exec postgres-prod psql -U $(shell grep POSTGRES_PROD_USER $(ENV_FILE) | cut -d'=' -f2) -d $(shell grep POSTGRES_PROD_DATABASE $(ENV_FILE) | cut -d'=' -f2)

# Connect to test database via psql
.PHONY: db-test
db-test:
	@echo "$(BLUE)Connecting to test database...$(NC)"
	docker-compose $(COMPOSE_OPTS) exec postgres-test psql -U $(shell grep POSTGRES_TEST_USER $(ENV_FILE) | cut -d'=' -f2) -d $(shell grep POSTGRES_TEST_DATABASE $(ENV_FILE) | cut -d'=' -f2)

# Execute SQL script on production database
.PHONY: exec-sql-prod
exec-sql-prod:
	@echo "$(BLUE)Executing SQL on production database...$(NC)"
	@read -p "Enter SQL file path: " sqlfile && \
	docker-compose $(COMPOSE_OPTS) exec -T postgres-prod psql -U $(shell grep POSTGRES_PROD_USER $(ENV_FILE) | cut -d'=' -f2) -d $(shell grep POSTGRES_PROD_DATABASE $(ENV_FILE) | cut -d'=' -f2) < $$sqlfile

# Execute SQL script on test database
.PHONY: exec-sql-test
exec-sql-test:
	@echo "$(BLUE)Executing SQL on test database...$(NC)"
	@read -p "Enter SQL file path: " sqlfile && \
	docker-compose $(COMPOSE_OPTS) exec -T postgres-test psql -U $(shell grep POSTGRES_TEST_USER $(ENV_FILE) | cut -d'=' -f2) -d $(shell grep POSTGRES_TEST_DATABASE $(ENV_FILE) | cut -d'=' -f2) < $$sqlfile

# Backup production database
.PHONY: backup-prod
backup-prod:
	@echo "$(BLUE)Backing up production database...$(NC)"
	@mkdir -p backups
	@filename=backups/prod_backup_$$(date +%Y%m%d_%H%M%S).sql; \
	docker-compose $(COMPOSE_OPTS) exec -T postgres-prod pg_dump -U $(shell grep POSTGRES_PROD_USER $(ENV_FILE) | cut -d'=' -f2) -d $(shell grep POSTGRES_PROD_DATABASE $(ENV_FILE) | cut -d'=' -f2) > $$filename && \
	echo "$(GREEN)✓ Backup saved to $$filename$(NC)"

# Backup test database
.PHONY: backup-test
backup-test:
	@echo "$(BLUE)Backing up test database...$(NC)"
	@mkdir -p backups
	@filename=backups/test_backup_$$(date +%Y%m%d_%H%M%S).sql; \
	docker-compose $(COMPOSE_OPTS) exec -T postgres-test pg_dump -U $(shell grep POSTGRES_TEST_USER $(ENV_FILE) | cut -d'=' -f2) -d $(shell grep POSTGRES_TEST_DATABASE $(ENV_FILE) | cut -d'=' -f2) > $$filename && \
	echo "$(GREEN)✓ Backup saved to $$filename$(NC)"

# Restore from backup to production database
.PHONY: restore-prod
restore-prod:
	@echo "$(RED)WARNING: This will overwrite production database!$(NC)"
	@read -p "Enter backup file path: " backupfile && \
	read -p "Are you sure? (y/N): " confirm && \
	if [ "$$confirm" = "y" ]; then \
		docker-compose $(COMPOSE_OPTS) exec -T postgres-prod psql -U $(shell grep POSTGRES_PROD_USER $(ENV_FILE) | cut -d'=' -f2) -d $(shell grep POSTGRES_PROD_DATABASE $(ENV_FILE) | cut -d'=' -f2) < $$backupfile; \
		echo "$(GREEN)✓ Database restored!$(NC)"; \
	else \
		echo "$(YELLOW)Operation cancelled.$(NC)"; \
	fi

# ===========================================
# Testing Commands
# ===========================================

# Run unit tests with test database
.PHONY: test
test: up-test
	@echo "$(BLUE)Running all tests...$(NC)"
	dotnet test src/tests/tests.csproj --verbosity normal
	@echo "$(GREEN)✓ Tests completed!$(NC)"

# Run only integration tests
.PHONY: test-integration
test-integration: up-test
	@echo "$(BLUE)Running integration tests only...$(NC)"
	dotnet test src/tests/tests.csproj --filter "FullyQualifiedName~integration" --verbosity normal
	@echo "$(GREEN)✓ Integration tests completed!$(NC)"

# Run only unit tests (no database)
.PHONY: test-unit
test-unit:
	@echo "$(BLUE)Running unit tests only...$(NC)"
	dotnet test src/tests/tests.csproj --filter "FullyQualifiedName~unit" --verbosity normal
	@echo "$(GREEN)✓ Unit tests completed!$(NC)"

# Run tests and clean up
.PHONY: test-cleanup
test-cleanup: test down-test

# ===========================================
# Build Commands
# ===========================================

# Build Docker image
.PHONY: build
build:
	@echo "$(BLUE)Building Docker image...$(NC)"
	docker build -t person-service:latest .
	@echo "$(GREEN)✓ Image built successfully!$(NC)"

# Build and run application
.PHONY: run
run: up build
	@echo "$(BLUE)Starting application...$(NC)"
	docker-compose $(COMPOSE_OPTS) up -d app
	@echo "$(GREEN)✓ Application started at http://localhost:$(shell grep APP_PORT $(ENV_FILE) | cut -d'=' -f2)$(NC)"

# ===========================================
# Development Commands
# ===========================================

# Clean build artifacts
.PHONY: clean
clean:
	@echo "$(YELLOW)Cleaning build artifacts...$(NC)"
	find . -type d -name "bin" -exec rm -rf {} + 2>/dev/null || true
	find . -type d -name "obj" -exec rm -rf {} + 2>/dev/null || true
	@echo "$(GREEN)✓ Cleaned!$(NC)"

# Show environment variables
.PHONY: env
env:
	@echo "$(BLUE)Current Environment Variables:$(NC)"
	@echo ""
	@echo "=== Production Database ==="
	@grep "POSTGRES_PROD" $(ENV_FILE) | grep -v "^#"
	@echo ""
	@echo "=== Test Database ==="
	@grep "POSTGRES_TEST" $(ENV_FILE) | grep -v "^#"
	@echo ""
	@echo "=== Application ==="
	@grep "APP_\|ASPNETCORE" $(ENV_FILE) | grep -v "^#"

# Help message
.PHONY: help
help:
	@echo "$(BLUE)============================================$(NC)"
	@echo "$(BLUE)  Person Service - Makefile Commands$(NC)"
	@echo "$(BLUE)============================================$(NC)"
	@echo ""
	@echo "$(YELLOW)Database Commands:$(NC)"
	@echo "  make up              - Start all containers (prod + test DB)"
	@echo "  make up-prod         - Start only production database"
	@echo "  make up-test         - Start only test database"
	@echo "  make up-logs         - Start all containers with logs"
	@echo "  make down            - Stop all containers"
	@echo "  make down-volumes    - Stop and remove volumes (WARNING: deletes data!)"
	@echo "  make restart         - Restart all containers"
	@echo ""
	@echo "$(YELLOW)Status & Logs:$(NC)"
	@echo "  make status          - Show container status"
	@echo "  make logs            - View all logs"
	@echo "  make logs-prod       - View production DB logs"
	@echo "  make logs-test       - View test DB logs"
	@echo "  make logs-app        - View application logs"
	@echo ""
	@echo "$(YELLOW)Database Access:$(NC)"
	@echo "  make db-prod         - Connect to production database (psql)"
	@echo "  make db-test         - Connect to test database (psql)"
	@echo "  make backup-prod     - Backup production database"
	@echo "  make backup-test     - Backup test database"
	@echo ""
	@echo "$(YELLOW)Testing:$(NC)"
	@echo "  make test            - Run all tests with test database"
	@echo "  make test-integration- Run only integration tests (PostgreSQL)"
	@echo "  make test-unit       - Run only unit tests (no database)"
	@echo "  make test-cleanup    - Run all tests and stop test database"
	@echo ""
	@echo "$(YELLOW)Build & Run:$(NC)"
	@echo "  make build           - Build Docker image"
	@echo "  make run             - Build and run application"
	@echo ""
	@echo "$(YELLOW)Development:$(NC)"
	@echo "  make clean           - Clean build artifacts"
	@echo "  make env             - Show environment variables"
	@echo "  make help            - Show this help message"
	@echo ""

# Default target
.DEFAULT_GOAL := help
