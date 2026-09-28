# Лабораторная работа #3 — Распределённые системы: Отказоустойчивость

## Описание

Реализация механизмов отказоустойчивости в системе бронирования авиабилетов:
- **Circuit Breaker** — защита от каскадных сбоев
- **Graceful Degradation** — возврат частичных данных при недоступности вторичных сервисов
- **Retry Queue** — асинхронная повторная обработка неудачных операций

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
              │                         │
              │  ┌──────────────────┐   │
              │  │ Circuit Breaker  │   │
              │  └──────────────────┘   │
              │  ┌──────────────────┐   │
              │  │ Retry Queue      │   │
              │  └──────────────────┘   │
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
              │   (4 isolated databases)│
              │   - gateway             │
              │   - ticket              │
              │   - flight              │
              │   - bonus               │
              └─────────────────────────┘
```

## Microservices

### Gateway Microservice (:8080)

**API Gateways**:
- `AirportHttpGateway` — управление аэропортами
- `FlightHttpGateway` — управление рейсами
- `PrivilegeHttpGateway` — управление привилегиями
- `TicketHttpGateway` — управление билетами (SAGA координатор)
- `UserHttpGateway` — управление пользователями

**Features**:
- **Circuit Breaker Pattern** — защита от каскадных сбоев
- **Graceful Degradation** — возврат частичных данных при сбоях
- **Retry Queue** — асинхронная повторная обработка операций
- **SAGA Pattern** — распределённые транзакции с компенсирующими операциями
- Health check: `/manage/health`
- Swagger UI: `http://localhost:8080/swagger`

### Ticket Microservice (:8070)

**Domain**: Управление билетами

**Endpoints**:
- `GET /api/v1/tickets` — получить все билеты
- `GET /api/v1/tickets/{ticketUid}` — получить билет по ID
- `POST /api/v1/tickets` — создать билет
- `DELETE /api/v1/tickets/{ticketUid}` — вернуть билет

**Database**: `ticket` schema

### Flight Microservice (:8060)

**Domain**: Управление рейсами и аэропортами

**Endpoints**:
- `GET /api/v1/flights` — получить все рейсы (пагинация)
- `GET /api/v1/airports` — получить все аэропорты

**Database**: `flight` schema

### Bonus Microservice (:8050)

**Domain**: Бонусная система и привилегии

**Endpoints**:
- `GET /api/v1/privilege` — получить статус привилегий пользователя
- `GET /api/v1/privilege-history` — получить историю бонусов

**Database**: `bonus` schema

## Features

- **Circuit Breaker Pattern**: Автоматическое отключение неработающих сервисов после 3 неудачных попыток
- **Graceful Degradation**: Возврат частичных данных вместо ошибок 500
- **Retry Queue**: Асинхронная повторная обработка неудачных операций (10s delay)
- **SAGA Pattern**: Координация распределённых транзакций с компенсирующими операциями
- **Multi-Database**: Каждый сервис имеет свою изолированную БД
- **RESTful API**: Стандартные HTTP методы и статус-коды
- **Health Checks**: `/manage/health` на каждом сервисе
- **Unit + Integration Tests**: Полное покрытие тестами
- **Swagger UI**: API документация для всех сервисов
- **Docker Compose**: Оркестрация всех сервисов

## Quick Start

### Prerequisites

- Docker & Docker Compose
- .NET 10 SDK (для локальной разработки)
- PostgreSQL 14+ (для локальной разработки)

### Запуск через Docker Compose (рекомендуется)

```bash
# Перейти в папку лабораторной работы
cd labs/lab_03

# Запустить все сервисы (PostgreSQL + 4 микросервиса)
docker compose up -d

# Проверить статус сервисов
docker compose ps

# Просмотр логов
docker compose logs -f

# Остановить сервисы
docker compose down
```

### Проверка работы

```bash
# Health check
curl http://localhost:8080/manage/health

# Swagger UI
open http://localhost:8080/swagger

# Получить все аэропорты
curl http://localhost:8080/api/v1/airports

# Получить все рейсы
curl http://localhost:8080/api/v1/flights
```

## API Endpoints

### Gateway Service (:8080)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/flights` | Get all flights (paginated) |
| GET | `/api/v1/airports` | Get all airports |
| GET | `/api/v1/tickets` | Get all user tickets |
| GET | `/api/v1/tickets/{ticketUid}` | Get ticket by ID |
| POST | `/api/v1/tickets` | Buy ticket (SAGA + Circuit Breaker) |
| DELETE | `/api/v1/tickets/{ticketUid}` | Return ticket (with Retry Queue) |
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

**Response**:

```json
{
  "ticketUid": "049161bb-badd-4fa8-9d90-87c9a82b0668",
  "flightNumber": "AFL031",
  "price": 1500,
  "paidByMoney": 1500,
  "paidByBonuses": 0,
  "status": "PAID",
  "statusCode": 201,
  "timestamp": "2026-09-22T13:30:50.981803Z"
}
```

## Circuit Breaker Pattern Implementation

### How It Works

```
1. Circuit State: Closed (normal operation)
2. Service call fails → Failure count++
3. After 3 failures → Circuit Opens (3s probe interval)
4. Next request → Short-circuit (immediate 503)
5. After 3s → Half-Open (probe request)
6. Success → Circuit Closes, Failure count = 0
7. Failure → Circuit Opens again
```

### Circuit Breaker States

| State | Behavior |
|-------|----------|
| **Closed** | Requests pass through normally |
| **Open** | Requests immediately return 503 Service Unavailable |
| **Half-Open** | One probe request allowed to check if service recovered |

### Affected Gateways

- `CircuitBreakerFlightGateway` — Flight Service calls
- `CircuitBreakerTicketGateway` — Ticket Service calls
- `CircuitBreakerPrivilegeGateway` — Bonus Service calls
- `CircuitBreakerAirportGateway` — Airport Service calls

## Graceful Degradation

### Read Operations (Partial Data)

When secondary services are unavailable:

| Endpoint | Primary Service | Secondary Service | Behavior |
|----------|----------------|-------------------|----------|
| `GET /tickets` | Ticket | Flight | Return tickets without flight details (200) |
| `GET /tickets/{uid}` | Ticket | Flight | Return ticket without flight details (200) |
| `GET /me` | Ticket | Flight + Bonus | Return user data without flight/bonus info (200) |

### Write Operations (SAGA + Retry Queue)

| Operation | Critical | Non-Critical | Behavior |
|-----------|----------|--------------|----------|
| `POST /tickets` | Ticket creation | Bonus debit | SAGA rollback on failure (503) |
| `DELETE /tickets/{uid}` | Ticket update | Bonus rollback | Retry Queue for bonus (204 + async retry) |

## Retry Queue Pattern

### Implementation

```csharp
// InMemoryRetryQueue uses Channel<T> for thread-safe queue
// BackgroundService processes queue with 10s delay

public interface IRetryQueue
{
    void Enqueue(string operationId, Func<Task> operation);
}
```

### Use Case: Return Ticket

```
1. Update ticket status to CANCELED (critical) → Always succeeds
2. Bonus rollback (non-critical) → Try-catch
   - Success → Complete
   - Failure (503) → Enqueue to Retry Queue
   - Retry Queue → Repeat bonus rollback after 10s
   - Still failing → Re-enqueue (infinite retry)
```

## SAGA Pattern Implementation

### Booking Creation Flow

```
1. Client → Gateway: POST /api/v1/tickets
2. Gateway → Flight: Validate flight exists
3. Gateway → Ticket: Create ticket record
4. Gateway → Bonus: Check balance & reserve points
5. Gateway → Bonus: Debit account (commit)
6. Response: 201 Created with ticket details
```

### Compensating Transactions (Rollback)

If any step fails:

```
1. Flight validation failed → No compensation needed
2. Ticket creation failed → No compensation needed
3. Bonus debit failed → Rollback ticket (DELETE)
   - TryRollbackTicketAsync (swallow errors, don't fail user request)
```

## Testing

### CI/CD Pipeline

Автоматическое тестирование при каждом push в `main`:
- ✅ Unit Tests (367 тестов)
- ✅ Integration Tests
- ✅ Autograding (Postman тесты преподавателя)
- ✅ Fault Tolerance Tests (Circuit Breaker + Degradation + Retry Queue)

Статус CI: [![CI](https://github.com/Kori-Tamashi/distributed-systems-project/actions/workflows/ci.yml/badge.svg)](https://github.com/Kori-Tamashi/distributed-systems-project/actions)

### Локальное тестирование

**Unit Tests**:
```bash
cd services/gateway-microservice/src/tests
dotnet test
```

**Postman API Tests**:
```bash
# Запустить Postman тесты в Docker
cd postman
docker build -t lab-03-postman .
docker run --network lab_03_autograding-network lab-03-postman
```

### Fault Tolerance Tests

**Circuit Breaker**:
```bash
# 1. Stop bonus-api
docker compose stop bonus-api

# 2. Make 3+ requests → Circuit opens
# First request: slow (timeout), subsequent: fast (503)

# 3. Wait 3s (probe interval), start bonus-api
# Circuit closes automatically
```

**Graceful Degradation**:
```bash
# Stop secondary service, verify 200 with partial data
docker compose stop flight-api
curl http://localhost:8080/api/v1/tickets -H "X-User-Name: Test User"
# Returns tickets without flight details
```

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
labs/lab_03/
├── postman/
│   ├── collection.json              # Instructor autograding tests
│   ├── fault-tolerance-collection.json  # Lab 03 tests (success + failover)
│   ├── environment.json
│   └── Dockerfile
├── services/
│   ├── gateway-microservice/        # API Gateway (:8080)
│   │   ├── src/
│   │   │   ├── core/
│   │   │   │   ├── circuitbreaker/  # Circuit Breaker implementation
│   │   │   │   └── exceptions/businesslogic/services/
│   │   │   │       └── ServiceUnavailableException.cs
│   │   │   ├── businesslogic/
│   │   │   ├── dataaccess/
│   │   │   │   ├── gateways/circuitbreaker/  # CB decorators
│   │   │   │   └── retry/  # Retry Queue implementation
│   │   │   │       └── InMemoryRetryQueue.cs
│   │   │   ├── presentation/
│   │   │   │   ├── controllers/http/
│   │   │   │   ├── middleware/
│   │   │   │   │   └── ExceptionHandlingMiddleware.cs
│   │   │   │   └── Program.cs  # CB + Retry Queue registration
│   │   │   └── tests/
│   │   └── docker-compose.gateway-microservice.yml
│   ├── ticket-microservice/         # Ticket Service (:8070)
│   ├── flight-microservice/         # Flight Service (:8060)
│   └── bonus-microservice/          # Bonus Service (:8050)
├── docker-compose.yml  # Root compose with include: statements
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
- **Circuit Breaker Pattern** — защита от каскадных сбоев (3 failures → open, 3s probe)
- **Retry Queue Pattern** — асинхронная повторная обработка (Channel + BackgroundService)
- **Graceful Degradation** — частичные данные вместо ошибок 500
- **SAGA Pattern** — распределённые транзакции с компенсирующими операциями
- **Repository Pattern** — абстракция доступа к данным
- **Dependency Injection** — loose coupling

### 🧪 Testing
- **367 Unit Tests** — бизнес-логика, контроллеры, конвертеры
- **Integration Tests** — БД операции, API endpoints
- **Autograding** — Postman тесты преподавателя
- **Fault Tolerance Tests** — Circuit Breaker + Degradation + Retry Queue
- **Health Checks** — `/manage/health` на каждом сервисе

### 🛡️ Fault Tolerance
- **Circuit Breaker**:
  - 3 consecutive failures → Circuit Opens
  - All requests immediately return 503 (short-circuit)
  - After 3s → Half-Open (probe request)
  - Success → Circuit Closes
  - Failure → Circuit Opens again

- **Graceful Degradation**:
  - GET /tickets → Return tickets without flight details (200)
  - GET /tickets/{uid} → Return ticket without flight details (200)
  - GET /me → Return user data without flight/bonus info (200)

- **Retry Queue**:
  - Critical operations (ticket update) → Synchronous, must succeed
  - Non-critical operations (bonus rollback) → Try-catch + enqueue
  - Background worker → Retry after 10s, re-enqueue on failure

### 🎫 Ticket Booking (SAGA + Circuit Breaker)
1. Валидация рейса (Flight Service) — Circuit Breaker
2. Проверка и резервирование бонусов (Bonus Service) — Circuit Breaker
3. Создание билета (Ticket Service)
4. Списание бонусов (Bonus Service) — Circuit Breaker
5. **Rollback** при ошибке на любом этапе (compensating transaction)
6. **503 Service Unavailable** — если сервис недоступен (Circuit Breaker Open)

### 🔄 Ticket Return (Retry Queue)
1. Обновление статуса билета (критично) → Всегда синхронно
2. Откат бонусов (некритично) → Try-catch
   - Успех → Завершение
   - Ошибка → Enqueue в Retry Queue
   - Background worker → Повтор через 10с

### 👤 User Management
- Регистрация пользователей с автоматическим созданием привилегии
- Система уровней: BRONZE → SILVER (7%) → GOLD (10%)
- Кэшбэк 10% на бонусный счёт при покупке билета
- История операций с бонусами

### 🏆 Bonus System
- **BRONZE** — базовый уровень (0%)
- **SILVER** — после 10 бронирований (7% скидка)
- **GOLD** — после 20 бронирований (10% скидка)
- **10% cashback** — на бонусный счёт при покупке

## License

Educational project for BMSTU Distributed Systems course.

## References

- [Lab Assignment](TASK.md)
- [BMSTU RSOI Lab3 Template](https://github.com/bmstu-rsoi/lab3-template)
- [Circuit Breaker Pattern](https://microservices.io/patterns/reliability/circuit-breaker.html)
- [Retry Pattern](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-implement-resilient-applications/use-retry-pattern/)
- [SAGA Pattern](https://microservices.io/patterns/data/saga.html)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
