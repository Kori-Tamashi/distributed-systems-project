# Лабораторная работа #2 — Microservices

## Описание

Реализация системы микросервисов для [ВЫБРАТЬ ВАРИАНТ].

## Вариант

(Flight Booking / Hotels Booking / Car Rental / Library System)

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        Client / Browser                         │
└──────────────────────────┬──────────────────────────────────────┘
                           │
                           ▼
              ┌─────────────────────────┐
              │   Gateway Service       │ :8080
              │   (API Gateway)         │
              └──────────┬──────────────┘
                         │
         ┌───────────────┼───────────────┐
         │               │               │
         ▼               ▼               ▼
┌─────────────────┐ ┌─────────────┐ ┌─────────────────┐
│  Ticket         │ │   Flight    │ │   Bonus         │
│  Microservice   │ │ Microservice│ │ Microservice    │
│      :8070      │ │    :8060    │ │      :8050      │
└────────┬────────┘ └──────┬──────┘ └────────┬────────┘
         │                 │                 │
         └─────────────────┼─────────────────┘
                           ▼
              ┌─────────────────────────┐
              │   PostgreSQL            │
              │   (Multi-Database)      │
              │   - gateway             │
              │   - ticket              │
              │   - flight              │
              │   - bonus               │
              └─────────────────────────┘
```

## Microservices

### Gateway Microservice (:8080)

**API Gateways:**
- `AirportHttpGateway` — управление аэропортами
- `BookingHttpGateway` — SAGA координатор для бронирования
- `FlightHttpGateway` — управление рейсами
- `PrivilegeHttpGateway` — управление привилегиями
- `TicketHttpGateway` — управление билетами

**Features:**
- SAGA pattern для распределённых транзакций
- Компенсирующие операции при откате
- Health check: `/manage/health`

### Ticket Microservice (:8070)

**Domain:** Управление билетами

**Endpoints:**
- `GET /api/v1/tickets` — получить все билеты
- `GET /api/v1/tickets/{ticketUid}` — получить билет по ID
- `POST /api/v1/tickets` — создать билет
- `DELETE /api/v1/tickets/{ticketUid}` — отменить билет

**Database:** `ticket` schema

### Flight Microservice (:8060)

**Domain:** Управление рейсами и аэропортами

**Endpoints:**
- `GET /api/v1/flights` — получить все рейсы (пагинация)
- `GET /api/v1/airports` — получить все аэропорты

**Database:** `flight` schema

### Bonus Microservice (:8050)

**Domain:** Бонусная система и привилегии

**Endpoints:**
- `GET /api/v1/privilege` — получить статус привилегий пользователя
- `GET /api/v1/privilege-history` — получить историю бонусов

**Database:** `bonus` schema

## Features

- **SAGA Pattern**: Координация распределённых транзакций с компенсирующими операциями
- **Multi-Database**: Каждый сервис имеет свою изолированную БД
- **RESTful API**: Стандартные HTTP методы и статус-коды
- **Health Checks**: `/manage/health` на каждом сервисе
- **Unit + Integration Tests**: Полное покрытие тестами
- **Swagger UI**: API документация для всех сервисов
- **Docker Compose**: Оркестрация всех сервисов

## Quick Start

### Prerequisites

- .NET 10 SDK
- Docker & Docker Compose
- Git
- PostgreSQL 14 (для локальной разработки)

### Setup

```bash
# 1. Navigate to lab_02
cd labs/lab_02

# 2. Configure environment for each service
# Edit .env files in each service directory:
# - services/gateway-microservice/.env
# - services/ticket-microservice/.env
# - services/flight-microservice/.env
# - services/bonus-microservice/.env

# 3. Start PostgreSQL containers (one for each service)
cd services/gateway-microservice
docker-compose up -d postgres-prod

cd ../ticket-microservice
docker-compose up -d postgres-prod

cd ../flight-microservice
docker-compose up -d postgres-prod

cd ../bonus-microservice
docker-compose up -d postgres-prod

# 4. Build and run all services
cd ../../
# Run each service from its directory
cd services/gateway-microservice/src/presentation
dotnet restore && dotnet build && dotnet run

# Repeat for other services...
```

### Quick Links

- 🧪 [Postman Collections](./postman/collections/) - API tests
- 📚 [API Documentation](http://localhost:8080/swagger) - Swagger UI (Gateway)

### Configuration

Each microservice has its own `.env` file with:

```env
# Application Settings
DOTNET_ENVIRONMENT=Development
APP_NAME=Gateway Microservice
API_PORT=8080

# Microservice URLs (for Gateway)
BONUS_URL=http://localhost:8050/api/v1
FLIGHT_URL=http://localhost:8060/api/v1
TICKET_URL=http://localhost:8070/api/v1

# Database Configuration
POSTGRES_HOST=localhost
POSTGRES_PORT=5441
POSTGRES_DB=gateway
POSTGRES_USER=program
POSTGRES_PASSWORD=prod_secret_password_2024
```

## API Endpoints

### Gateway Service (:8080)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/flights` | Get all flights (paginated) |
| GET | `/api/v1/airports` | Get all airports |
| GET | `/api/v1/tickets` | Get all user tickets |
| GET | `/api/v1/tickets/{ticketUid}` | Get ticket by ID |
| POST | `/api/v1/tickets` | Buy ticket (SAGA) |
| DELETE | `/api/v1/tickets/{ticketUid}` | Cancel ticket |
| GET | `/api/v1/privilege` | Get user privilege status |
| GET | `/api/v1/privilege-history` | Get privilege history |
| GET | `/manage/health` | Health check |

### Request/Response Examples

**POST /api/v1/tickets (Buy Ticket)**

```json
{
  "flightNumber": "AFL031",
  "price": 1500,
  "paidFromBalance": true
}
```

**Response:**

```json
{
  "ticketUid": "049161bb-badd-4fa8-9d90-87c9a82b0668",
  "flightNumber": "AFL031",
  "price": 1500,
  "paidByBonuses": 500,
  "paidByMoney": 1000,
  "status": "PAID",
  "statusCode": 201,
  "timestamp": "2026-09-22T13:30:50.981803Z"
}
```

## SAGA Pattern Implementation

### Booking Creation Flow

```
1. Client → Gateway: POST /api/v1/tickets
2. Gateway → Flight: Validate flight exists
3. Gateway → Bonus: Check balance & reserve points
4. Gateway → Ticket: Create ticket record
5. Gateway → Bonus: Debit account (commit)
6. Response: 201 Created with ticket details
```

### Compensating Transactions (Rollback)

If any step fails:

```
1. Ticket creation failed → No compensation needed
2. Bonus debit failed → Rollback ticket (DELETE)
3. Flight validation failed → Rollback bonus reservation
```

## Testing

### Unit Tests

```bash
# Run tests for each service
cd services/gateway-microservice/src/tests
dotnet test

cd ../..

cd services/ticket-microservice/src/tests
dotnet test

# Repeat for flight-microservice and bonus-microservice
```

**Test Coverage:**
- Unit tests for business logic
- Integration tests for database operations
- Controller tests for API endpoints

### Postman API Tests

**Collections:**
- `gateway-airport/` — Airport API tests
- `gateway-booking/` — Booking SAGA tests
- `gateway-privilege/` — Privilege API tests
- `gateway-ticket/` — Ticket API tests
- `flight-flight/` — Flight API tests
- `ticket-ticket/` — Ticket service tests
- `bonus-privilege/` — Bonus API tests

**Environments:**
- `globals/` — Global variables
- `environments/` — Environment-specific configs

## Database Schema

### Ticket Service Database

```sql
CREATE TABLE ticket (
    id            SERIAL PRIMARY KEY,
    ticket_uid    uuid UNIQUE NOT NULL,
    username      VARCHAR(80) NOT NULL,
    flight_number VARCHAR(20) NOT NULL,
    price         INT         NOT NULL,
    status        VARCHAR(20) NOT NULL
        CHECK (status IN ('PAID', 'CANCELED'))
);
```

### Flight Service Database

```sql
CREATE TABLE flight (
    id              SERIAL PRIMARY KEY,
    flight_number   VARCHAR(20)              NOT NULL,
    datetime        TIMESTAMP WITH TIME ZONE NOT NULL,
    from_airport_id INT REFERENCES airport (id),
    to_airport_id   INT REFERENCES airport (id),
    price           INT                      NOT NULL
);

CREATE TABLE airport (
    id      SERIAL PRIMARY KEY,
    name    VARCHAR(255),
    city    VARCHAR(255),
    country VARCHAR(255)
);
```

### Bonus Service Database

```sql
CREATE TABLE privilege (
    id       SERIAL PRIMARY KEY,
    username VARCHAR(80) NOT NULL UNIQUE,
    status   VARCHAR(80) NOT NULL DEFAULT 'BRONZE'
        CHECK (status IN ('BRONZE', 'SILVER', 'GOLD')),
    balance  INT
);

CREATE TABLE privilege_history (
    id             SERIAL PRIMARY KEY,
    privilege_id   INT REFERENCES privilege (id),
    ticket_uid     uuid        NOT NULL,
    datetime       TIMESTAMP   NOT NULL,
    balance_diff   INT         NOT NULL,
    operation_type VARCHAR(20) NOT NULL
        CHECK (operation_type IN ('FILL_IN_BALANCE', 'DEBIT_THE_ACCOUNT'))
);
```

## Project Structure

```
labs/lab_02/
├── postman/
│   ├── collections/           # Postman collections
│   │   ├── gateway-airport/
│   │   ├── gateway-booking/
│   │   ├── gateway-privilege/
│   │   ├── gateway-ticket/
│   │   ├── flight-flight/
│   │   ├── ticket-ticket/
│   │   └── bonus-privilege/
│   ├── environments/          # Environment configs
│   ├── Dockerfile
│   └── globals/
├── services/
│   ├── gateway-microservice/  # API Gateway (:8080)
│   │   ├── src/
│   │   │   ├── core/          # Domain layer
│   │   │   ├── businesslogic/ # Service layer
│   │   │   ├── dataaccess/    # Repository layer
│   │   │   ├── presentation/  # API layer (Controllers)
│   │   │   └── tests/         # Unit & Integration tests
│   │   └── .env
│   ├── ticket-microservice/   # Ticket Service (:8070)
│   │   └── src/               # Same structure
│   ├── flight-microservice/   # Flight Service (:8060)
│   │   └── src/               # Same structure
│   └── bonus-microservice/    # Bonus Service (:8050)
│       └── src/               # Same structure
├── NOTES.md
├── TASK.md
└── README.md
```

## SOLID Principles

- **Single Responsibility**: Каждый микросервис отвечает за одну доменную область
- **Open/Closed**: Легко добавлять новые сервисы через HTTP gateways
- **Liskov Substitution**: Абстракции `I*Service` интерфейсов
- **Interface Segregation**: Специализированные HTTP gateways
- **Dependency Inversion**: Высокоуровневая логика не зависит от EF Core напрямую

## Key Features

### 🏗️ Architecture
- **Microservices Pattern** — независимые сервисы с изолированными БД
- **API Gateway Pattern** — единая точка входа
- **SAGA Pattern** — распределённые транзакции с компенсирующими операциями
- **Repository Pattern** — абстракция доступа к данным
- **Dependency Injection** — loose coupling

### 🧪 Testing
- **Unit Tests** — бизнес-логика, контроллеры, конвертеры
- **Integration Tests** — БД операции, API endpoints
- **Postman Collections** — 7 коллекций API тестов
- **Health Checks** — `/manage/health` на каждом сервисе

### 🔒 Security
- **Input Validation** — валидация запросов на всех endpoints
- **Error Handling** — правильные HTTP статус-коды
- **Exception Translation** — domain exceptions → HTTP exceptions

### 📊 Bonus System
- **BRONZE** — базовый уровень
- **SILVER** — после 10 бронирований (7% скидка)
- **GOLD** — после 20 бронирований (10% скидка)
- **10% cashback** — на бонусный счёт при покупке

## License

Educational project for BMSTU Distributed Systems course.

## References

- [Lab Assignment](TASK.md)
- [BMSTU RSOI Lab2 Template](https://github.com/bmstu-rsoi/lab2-template)
- [SAGA Pattern](https://microservices.io/patterns/data/saga.html)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
