# Bonus Service

Микросервис для управления бонусной программой в системе бронирования авиабилетов (Lab 02 - Flight Booking System).

## Описание

Сервис управляет привилегиями пользователей и начислением/списанием бонусных баллов при покупке билетов. Баллы можно использовать для оплаты билетов из расчета 1 балл = 1 рубль.

## Структура базы данных

```sql
CREATE TABLE privilege (
    id SERIAL PRIMARY KEY,
    username VARCHAR(80) NOT NULL UNIQUE,
    status VARCHAR(80) NOT NULL DEFAULT 'BRONZE' CHECK (status IN ('BRONZE', 'SILVER', 'GOLD')),
    balance INT
);

CREATE TABLE privilege_history (
    id SERIAL PRIMARY KEY,
    privilege_id INT REFERENCES privilege (id),
    ticket_uid uuid NOT NULL,
    datetime TIMESTAMP NOT NULL,
    balance_diff INT NOT NULL,
    operation_type VARCHAR(20) NOT NULL CHECK (operation_type IN ('FILL_IN_BALANCE', 'DEBIT_THE_ACCOUNT'))
);
```

---

## Структура проекта

Проект следует архитектуре Clean Architecture с четким разделением на слои:

```
bonus-service/
├── src/
│   ├── core/                          # Core Layer (Domain Logic)
│   │   ├── domain/                    # Domain entities (Privilege, PrivilegeHistory)
│   │   ├── enum/                      # Enumerations (PrivilegeStatus, OperationType)
│   │   ├── filters/                   # Filter classes
│   │   ├── exceptions/                # Exception hierarchy
│   │   │   ├── dataaccess/
│   │   │   │   └── repositories/      # Repository exceptions
│   │   │   └── businesslogic/
│   │   │       └── services/          # Service exceptions
│   │   └── interfaces/
│   │       ├── dataaccess/
│   │       │   ├── contexts/          # Database context interfaces
│   │       │   └── repositories/      # Repository interfaces
│   │       └── businesslogic/
│   │           └── services/          # Service interfaces
│   │
│   ├── dataaccess/                    # Data Access Layer
│   │   ├── models/
│   │   │   └── postgres/              # PostgreSQL models
│   │   ├── contexts/
│   │   │   └── postgres/              # Database contexts
│   │   ├── converters/
│   │   │   └── postgres/              # Entity converters
│   │   └── repositories/
│   │       └── postgres/              # Repository implementations
│   │
│   ├── businesslogic/                 # Business Logic Layer
│   │   └── services/                  # Service implementations
│   │
│   ├── presentation/                  # Presentation Layer
│   │   ├── dto/
│   │   │   └── http/
│   │   │       └── Privilege/         # HTTP DTOs
│   │   ├── controllers/
│   │   │   └── http/                  # HTTP API controllers
│   │   ├── converters/
│   │   │   └── http/                  # HTTP converters
│   │   └── exceptions/
│   │       └── http/
│   │           └── Privilege/         # HTTP exceptions
│   │
│   └── tests/                         # Test Infrastructure
│       ├── config/
│       │   └── attributes/            # Test attributes
│       ├── fixtures/
│       │   ├── builders/              # Test builders
│       │   ├── mothers/               # Test mothers
│       │   └── contexts/
│       │       ├── postgres/          # PostgreSQL test context
│       │       └── sqlite/            # SQLite test context
│       ├── dataaccess/
│       │   ├── converters/
│       │   │   └── unit/
│       │   │       └── postgres/      # Converter unit tests
│       │   └── repositories/
│       │       ├── unit/
│       │       │   └── postgres/      # Repository unit tests
│       │       └── integration/
│       │           └── postgres/      # Repository integration tests
│       ├── businesslogic/
│       │   └── services/
│       │       ├── unit/              # Service unit tests
│       │       └── integration/
│       │           └── postgres/      # Service integration tests
│       └── presentation/
│           ├── converters/
│           │   └── unit/              # Converter unit tests
│           └── controllers/
│               └── unit/              # Controller unit tests
│
├── .env                               # Environment variables
├── .dockerignore                      # Docker ignore file
├── Dockerfile                         # Docker build configuration
├── docker-compose.bonus-microservice.yml  # Docker Compose config
├── Makefile                           # Build and run commands
└── README.md                          # This file
```

## Слои архитектуры

### Core Layer (`core/`)
- **Domain entities**: Бизнес-сущности (Privilege, PrivilegeHistory)
- **Enums**: Перечисления (PrivilegeStatus: BRONZE/SILVER/GOLD, OperationType: FILL_IN_BALANCE/DEBIT_THE_ACCOUNT)
- **Filters**: Классы для фильтрации данных
- **Exceptions**: Иерархия исключений
- **Interfaces**: Контракты для репозиториев и сервисов

### Data Access Layer (`dataaccess/`)
- **Models**: Модели для работы с PostgreSQL
- **Contexts**: EF Core DatabaseContext
- **Converters**: Конвертеры между Domain и PostgreSQL models
- **Repositories**: Реализации репозиториев с EF Core

### Business Logic Layer (`businesslogic/`)
- **Services**: Бизнес-логика с валидацией и правилами
  - CreditBalanceAsync: начисление баллов
  - DebitBalanceAsync: списание баллов
  - GetMaxDebitAmountAsync: получение максимального доступного баланса

### Presentation Layer (`presentation/`)
- **DTOs**: Data Transfer Objects для HTTP API
- **Controllers**: REST API контроллеры
- **Converters**: Конвертеры между Domain и DTO
- **Exceptions**: HTTP исключения с кодами состояния

### Test Infrastructure (`tests/`)
- **Builders**: Паттерн Builder для создания тестовых данных
- **Mothers**: Static factories для создания тестовых данных
- **Test Contexts**: Test DatabaseContext (SQLite, PostgreSQL)
- **Unit Tests**: Юнит-тесты для всех слоев
- **Integration Tests**: Интеграционные тесты с PostgreSQL

## Технологии

- **Language**: C# / .NET 10
- **ORM**: Entity Framework Core 10
- **Database**: PostgreSQL 14
- **API**: ASP.NET Core Web API
- **Testing**: xUnit, Moq
- **Containerization**: Docker, Docker Compose

## Порты

- **API**: 8050
- **Production Database**: 5451
- **Test Database**: 5452

## Команды

```bash
# Сборка проекта
dotnet build src/bonus-service.slnx

# Запуск тестов
dotnet test src/bonus-service.slnx

# Запуск тестовой базы данных
make test-db-up

# Остановка тестовой базы данных
make test-db-down

# Сборка Docker образа
make docker-build

# Запуск Docker контейнеров
make docker-run

# Остановка Docker контейнеров
make docker-stop
```

## API Endpoints

Будут реализованы после создания сущностей:

- `GET /api/v1/privileges` - Получить все привилегии
- `GET /api/v1/privileges/{id}` - Получить привилегию по ID
- `POST /api/v1/privileges` - Создать привилегию
- `PATCH /api/v1/privileges/{id}` - Обновить привилегию
- `DELETE /api/v1/privileges/{id}` - Удалить привилегию
- `GET /manage/health` - Health check endpoint (требуется по TASK.md)

## Интеграция с другими сервисами

Сервис взаимодействует с другими микросервисами через Gateway Service:

- **Ticket Service (8070)**: получение информации о купленных билетах для начисления баллов
- **Flight Service (8060)**: получение информации о рейсах
- **Gateway Service (8080)**: единая точка входа для внешних запросов

## Следующие шаги

1. ✅ Определить доменные сущности (Privilege, PrivilegeHistory)
2. ✅ Создать Core Layer (entities, enums, interfaces, exceptions)
3. Реализовать Data Access Layer (models, context, converters, repositories)
4. Реализовать Business Logic Layer (services with validation)
5. Реализовать Presentation Layer (DTOs, controllers, converters)
6. Написать тесты (unit + integration)
7. Настроить Docker конфигурацию
8. Настроить CI/CD pipeline
