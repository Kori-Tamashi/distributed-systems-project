# Gateway Microservice Makefile

# Build the application
build:
	dotnet build src/gateway-microservice.slnx -c Release

# Run tests
test:
	dotnet test src/gateway-microservice.slnx -c Release

# Run tests with coverage
test-coverage:
	dotnet test src/gateway-microservice.slnx -c Release --collect:"XPlat Code Coverage"

# Start test database
test-db-up:
	docker-compose -f docker-compose.gateway-microservice.yml up -d postgres-test

# Stop test database
test-db-down:
	docker-compose -f docker-compose.gateway-microservice.yml down

# Build Docker image
docker-build:
	docker build -t gateway-microservice:latest .

# Run Docker container
docker-run:
	docker-compose -f docker-compose.gateway-microservice.yml up -d

# Stop all Docker containers
docker-stop:
	docker-compose -f docker-compose.gateway-microservice.yml down

# Clean build artifacts
clean:
	dotnet clean src/gateway-microservice.slnx
	rm -rf src/**/bin src/**/obj

# Full clean including Docker
full-clean: clean docker-stop
	docker system prune -f

# Run the application locally
run:
	dotnet run src/presentation/presentation.csproj

# Help
help:
	@echo "Gateway Microservice Makefile"
	@echo ""
	@echo "Available targets:"
	@echo "  build          - Build the application"
	@echo "  test           - Run all tests"
	@echo "  test-coverage  - Run tests with coverage"
	@echo "  test-db-up     - Start test database"
	@echo "  test-db-down   - Stop test database"
	@echo "  docker-build   - Build Docker image"
	@echo "  docker-run     - Run Docker container"
	@echo "  docker-stop    - Stop all Docker containers"
	@echo "  clean          - Clean build artifacts"
	@echo "  full-clean     - Clean everything including Docker"
	@echo "  run            - Run the application locally"
