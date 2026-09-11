# Lab 01: Person API with CI/CD

## Overview
REST API для управления сущностью Person с поддержкой CRUD операций, мульти-базы данных и автоматического CI/CD.

## Tech Stack
- **.NET 10** / ASP.NET Core
- **Entity Framework Core 10**
- **PostgreSQL 14** (primary), MySQL, SQLite, SQLServer
- **Docker** / Docker Compose
- **GitHub Actions** for CI/CD
- **Swagger** для API документации

## Architecture
```
src/
├── core/              # Domain models, interfaces, exceptions
├── businesslogic/     # Business logic services
├── dataaccess/        # Repository pattern, EF Core contexts
├── presentation/      # Controllers, DTOs, Program.cs
└── tests/             # Unit and integration tests
```

## Features
- **CRUD endpoints**: GET, POST, PATCH, DELETE `/api/v1/persons`
- **Multi-database**: переключение через `DATABASE_PROVIDER` в `.env`
- **Factory Pattern**: создание репозиториев по типу БД
- **154 теста**: 67 unit + 61 integration + 26 controller tests
- **Swagger UI**: `http://localhost:8080/swagger`

## Quick Start

### Prerequisites
- .NET 10 SDK
- Docker & Docker Compose
- Git

### Setup
```bash
# 1. Clone repository
git clone https://github.com/Kori-Tamashi/distributed-systems-project.git
cd labs/lab_01

# 2. Start PostgreSQL container
docker-compose up -d postgres-prod

# 3. Configure environment
cp .env.example .env  # or edit .env directly

# 4. Build and run
cd src/presentation
dotnet restore
dotnet build
dotnet run
```

### Quick Links
- 📋 [Requirements Check](./LAB01_REQUIREMENTS_CHECK.md) - Lab requirements verification
- 🧪 [API Tests](./postman/collections/Person.postman_collection.json) - 18 Postman tests
- 🚀 [CI/CD Workflows](./.github/workflows/) - GitHub Actions pipelines

### Configuration (.env)
```env
DATABASE_PROVIDER=POSTGRESQL
PostgreSQL__Host=postgres-prod
PostgreSQL__Port=5432
PostgreSQL__Database=persons
PostgreSQL__Username=program
PostgreSQL__Password=prod_secret_password_2024
Application__Host=localhost
Application__Port=8080
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/persons` | Get all persons (paginated) |
| GET | `/api/v1/persons/{id}` | Get person by ID |
| POST | `/api/v1/persons` | Create new person |
| PATCH | `/api/v1/persons/{id}` | Update person (partial) |
| DELETE | `/api/v1/persons/{id}` | Delete person |

### Request/Response Examples

**POST /api/v1/persons**
```json
{
  "name": "John Doe",
  "age": 34,
  "address": "123 Main St",
  "work": "Software Engineer"
}
```

**Response:**
```json
{
  "id": 1,
  "name": "John Doe",
  "age": 34,
  "address": "123 Main St",
  "work": "Software Engineer",
  "statusCode": 201,
  "timestamp": "2026-09-10T13:30:50.981803Z"
}
```

## Testing
```bash
cd src/tests
dotnet test
```

**Test Coverage:**
- `PersonConverterUnitTests`: 17 tests
- `PersonServiceUnitTests`: service layer tests
- `PersonHttpControllerUnitTests`: 19 tests
- `PersonHttpControllerIntegrationTests`: 26 tests
- `PersonPostgresqlRepositoryIntegrationTests`: repository tests

## Docker
```bash
# Start all services
docker-compose up -d

# PostgreSQL on port 5433
docker-compose ps

# View logs
docker-compose logs -f postgres-prod
```

## CI/CD
GitHub Actions workflow автоматически:
- Build проекта
- Запуск тестов
- Сборка Docker-образа
- Деплой на Railway (настраивается)

Workflow файл: `.github/workflows/build.yml`

## Database Support
Переключение БД через `.env`:
```env
DATABASE_PROVIDER=POSTGRESQL  # or MYSQL, SQLITE, SQLSERVER
```

Каждая БД имеет:
- Собственный `DatabaseContext`
- Собственный `Repository` implementation
- Собственный `Converter` для маппинга

## Postman API Tests

### Collections
- **Person.postman_collection.json** - 18 API tests in JSON format (for Newman)
- **Person/** - Native Git format (YAML) for code review

### Environments
- **ci.postman_environment.json** - CI/CD environment
- **railway.environment.yaml** - Production (Railway deployment)
- **local.environment.yaml** - Local development

### Test Coverage
1. Health Check
2. GetAllPersons (empty)
3. CreatePerson (valid, empty name, excessive age, negative age)
4. GetPersonById (valid, invalid, not found)
5. UpdatePerson (valid, not found)
6. GetAllPersons (with data)
7. DeletePerson (valid, invalid, not found)
8. GetPersonAfterDelete (not found)
9. Cleanup (create, delete)

## SOLID Principles
- **Single Responsibility**: разделение на слои (core, businesslogic, dataaccess, presentation)
- **Open/Closed**: легко добавлять новые БД через switch-выражения
- **Liskov Substitution**: `IDatabaseContext` абстракция
- **Interface Segregation**: специализированные интерфейсы (`IPersonRepository`, `IPersonService`)
- **Dependency Inversion**: высокоуровневые модули не зависят от EF Core напрямую

## Project Structure
```
labs/lab_01/
├── .env                    # Environment configuration
├── docker-compose.yml      # PostgreSQL container
├── src/
│   ├── core/               # Domain layer
│   │   ├── domain/
│   │   ├── interfaces/
│   │   └── exceptions/
│   ├── businesslogic/      # Service layer
│   │   └── services/
│   ├── dataaccess/         # Repository layer
│   │   ├── contexts/
│   │   ├── repositories/
│   │   └── converters/
│   ├── presentation/       # API layer
│   │   ├── controllers/
│   │   ├── dto/
│   │   ├── converters/
│   │   └── Program.cs
│   └── tests/              # Test projects
│       ├── businesslogic/
│       ├── dataaccess/
│       └── presentation/
└── data/
    └── postgres_data/      # PostgreSQL data (gitignored)
```

## License
Educational project for BMSTU Distributed Systems course.

## Key Features

### 🏗️ Architecture
- **Clean Architecture** - separation of concerns (core, businesslogic, dataaccess, presentation)
- **Repository Pattern** - data access abstraction
- **Dependency Injection** - loose coupling
- **Factory Pattern** - multi-database support

### 🧪 Testing
- **Unit Tests** - 67 tests (business logic, controllers, converters)
- **Integration Tests** - 61 tests (database, API endpoints)
- **API Tests** - 18 Postman/Newman tests (E2E validation)
- **Test Coverage** - comprehensive coverage of all endpoints

### 🚀 CI/CD
- **Automated Build** - every push triggers build
- **Automated Testing** - unit + integration + API tests
- **Docker Build & Push** - automatic containerization
- **Railway Deployment** - auto-update on new image

### 🔒 Security
- **CodeQL Analysis** - security scanning
- **Input Validation** - request validation on all endpoints
- **Error Handling** - proper HTTP status codes
