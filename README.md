# Лабораторная работа #2 — Распределённые системы: Микросервисы

## Описание

Реализация системы бронирования авиабилетов на микросервисной архитектуре.

**Курс**: Распределённые системы обработки информации (РСОИ)  
**Магистратура**: МАИ-22-2  
**Студент**: Кори Тамаси

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
- SAGA pattern для распределённых транзакций
- Компенсирующие операции при откате
- Health check: `/manage/health`
- Swagger UI: `http://localhost:8080/swagger`

### Ticket Microservice (:8070)

**Domain**: Управление билетами

**Endpoints**:
- `GET /api/v1/tickets` — получить все билеты
- `GET /api/v1/tickets/{ticketUid}` — получить билет по ID
- `POST /api/v1/tickets` — создать билет
- `DELETE /api/v1/tickets/{ticketUid}` — отменить билет

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
cd labs/lab_02

# Запустить все сервисы (PostgreSQL + 4 микросервиса)
docker-compose up -d

# Проверить статус сервисов
docker-compose ps

# Просмотр логов
docker-compose logs -f

# Остановить сервисы
docker-compose down
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

**Response**:

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

### CI/CD Pipeline

Автоматическое тестирование при каждом push в `main`:
- ✅ Unit Tests (367 тестов)
- ✅ Integration Tests
- ✅ Autograding (Postman тесты преподавателя)

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
docker build -t newman-runner .
docker run --network lab_02_autograding-network newman-runner
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
labs/lab_02/
├── postman/
│   ├── collections/           # Postman collections
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
- **367 Unit Tests** — бизнес-логика, контроллеры, конвертеры
- **Integration Tests** — БД операции, API endpoints
- **Autograding** — Postman тесты преподавателя
- **Health Checks** — `/manage/health` на каждом сервисе

### 🔒 Security
- **Input Validation** — валидация запросов на всех endpoints
- **Error Handling** — правильные HTTP статус-коды
- **Exception Translation** — domain exceptions → HTTP exceptions

### 🎫 Ticket Booking (SAGA)
1. Валидация рейса (Flight Service)
2. Проверка и резервирование бонусов (Bonus Service)
3. Создание билета (Ticket Service)
4. Списание бонусов (Bonus Service)
5. **Rollback** при ошибке на любом этапе

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
- [BMSTU RSOI Lab2 Template](https://github.com/bmstu-rsoi/lab2-template)
- [SAGA Pattern](https://microservices.io/patterns/data/saga.html)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
