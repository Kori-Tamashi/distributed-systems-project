## **Лабораторная работа №2. Microservices. Вариант 1: Flight Booking System**

**Формулировка**

В рамках второй лабораторной работы по вариантам требуется реализовать систему, состоящую из нескольких взаимодействующих друг с другом сервисов.

**Требования**

1. Каждый сервис имеет своё собственное хранилище, если оно ему нужно. Для учебных целей можно использовать один instance базы данных, но каждый сервис работает **только** со своей логической базой. Запросы между базами **запрещены**.
2. Для межсервисного взаимодействия использовать HTTP (придерживаться RESTful). Допускается использование других протоколов (например, gRPC), но это требуется согласовать с преподавателем.
3. Выделить **Gateway Service** как единую точку входа и межсервисной коммуникации. Горизонтальные запросы между сервисами делать **нельзя**.
4. На каждом сервисе сделать специальный endpoint `GET /manage/health`, отдающий 200 OK — он будет использоваться для проверки доступности сервиса.
5. Код хранить на Github, для сборки использовать Github Actions.
6. Gateway Service должен запускаться на порту **8080**, остальные сервисы — на портах **8050, 8060, 8070**.
7. Каждый сервис должен быть завернут в **Docker**.
8. В `docker-compose.yml` прописать сборку и запуск Docker-контейнеров.
9. В `classroom.yml` дописать шаги на сборку и прогон unit-тестов.
10. Для автоматических прогонов тестов в файлах `autograding.json` и `classroom.yml` заменить `<variant>` на ваш вариант.

**Ваш вариант: Flight Booking System**

Система предоставляет пользователю возможность поиска и покупки билетов. При покупке билетов пользователю начисляются баллы, которые он может использовать для оплаты.

**Структура базы данных**

**Ticket Service** (порт 8070)
```sql
CREATE TABLE ticket (
    id SERIAL PRIMARY KEY,
    ticket_uid uuid UNIQUE NOT NULL,
    username VARCHAR(80) NOT NULL,
    flight_number VARCHAR(20) NOT NULL,
    price INT NOT NULL,
    status VARCHAR(20) NOT NULL CHECK (status IN ('PAID', 'CANCELED'))
);
```

**Flight Service** (порт 8060)
```sql
CREATE TABLE flight (
    id SERIAL PRIMARY KEY,
    flight_number VARCHAR(20) NOT NULL,
    datetime TIMESTAMP WITH TIME ZONE NOT NULL,
    from_airport_id INT REFERENCES airport (id),
    to_airport_id INT REFERENCES airport (id),
    price INT NOT NULL
);

CREATE TABLE airport (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255),
    city VARCHAR(255),
    country VARCHAR(255)
);
```

**Bonus Service** (порт 8050)
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

**Описание API**

- Получить список всех перелетов: `GET {{baseUrl}}/api/v1/flights&page={{page}}&size={{size}}`
- Получить полную информацию о пользователе (билеты и статус в системе привилегий): `GET {{baseUrl}}/api/v1/me` с заголовком `X-User-Name: {{username}}`
- Получить информацию о всех билетах пользователя: `GET {{baseUrl}}/api/v1/tickets` с заголовком `X-User-Name: {{username}}`
- Получить информацию по конкретному билету пользователя (с проверкой принадлежности): `GET {{baseUrl}}/api/v1/tickets/{{ticketUid}}` с заголовком `X-User-Name: {{username}}`
- Покупка билета: пользователь вызывает `GET {{baseUrl}}/api/v1/flights`, выбирает нужный рейс и передает в запросе на покупку: `flightNumber`, `price`, `paidFromBalance`. Система проверяет существование рейса. Если `paidFromBalance: true`, с бонусного счёта списывается максимальное количество баллов из расчёта 1 балл = 1 рубль.

---

## **Файлы для обновления**

### **1. docker-compose.yml**

```yaml
version: "3"
services:
  postgres:
    image: library/postgres:13
    container_name: postgres
    restart: on-failure
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: "postgres"
      POSTGRES_DB: postgres
    volumes:
      - db-data:/var/lib/postgresql/data
      - ./postgres/:/docker-entrypoint-initdb.d/
    ports:
      - "5432:5432"

  flight-service:
    build: ./flight-service
    container_name: flight-service
    ports:
      - "8060:8060"
    depends_on:
      - postgres
    environment:
      - DB_HOST=postgres
      - DB_PORT=5432
      - DB_NAME=flights
      - DB_USER=program
      - DB_PASSWORD=test

  ticket-service:
    build: ./ticket-service
    container_name: ticket-service
    ports:
      - "8070:8070"
    depends_on:
      - postgres
    environment:
      - DB_HOST=postgres
      - DB_PORT=5432
      - DB_NAME=tickets
      - DB_USER=program
      - DB_PASSWORD=test

  bonus-service:
    build: ./bonus-service
    container_name: bonus-service
    ports:
      - "8050:8050"
    depends_on:
      - postgres
    environment:
      - DB_HOST=postgres
      - DB_PORT=5432
      - DB_NAME=privileges
      - DB_USER=program
      - DB_PASSWORD=test

  gateway-service:
    build: ./gateway-service
    container_name: gateway-service
    ports:
      - "8080:8080"
    depends_on:
      - flight-service
      - ticket-service
      - bonus-service

volumes:
  db-data:
```

### **2. .github/workflows/classroom.yml**

```yaml
name: GitHub Classroom Workflow

on:
  push:
    branches:
      - master
  pull_request:
    branches:
      - master

jobs:
  build:
    name: Autograding
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
        with:
          fetch-depth: 0

      - uses: docker/setup-buildx-action@v2

      - name: Build images
        timeout-minutes: 10
        run: docker compose build

      - name: Run containers
        timeout-minutes: 5
        run: |
          docker compose up -d
          ./scripts/wait-script.sh
        env:
          WAIT_PORTS: 8080,8070,8060,8050

      - name: Run Unit Tests
        run: |
          docker compose exec -T flight-service ./gradlew test
          docker compose exec -T ticket-service ./gradlew test
          docker compose exec -T bonus-service ./gradlew test

      - name: Run API Tests
        uses: matt-ball/newman-action@master
        with:
          collection: /postman/v1/collection.json
          environment: /postman/v1/environment.json
          delayRequest: 100
          reporters: '[ "cli" ]'

      - uses: education/autograding@v1
        id: autograder
        continue-on-error: true

      - name: Github auto grader mark
        uses: Romanow/google-sheet-autograder-marker@v1.0
        with:
          google_token: ${{secrets.GOOGLE_API_KEY}}
          sheet_id: "1xkgjUX6Qmk7rdJG-QPOToav-HWWtthJjnShIKnw3oIY"
          homework_number: 2
          user_column: 'D'
          column_offset: 'F'
          mark: "'+"

      - name: Stop containers
        if: always()
        continue-on-error: true
        run: docker compose down -v
```

### **3. .github/classroom/autograding.json**

```json
{
  "tests": [
    {
      "name": "Run Postman",
      "setup": "",
      "run": "newman run -e /postman/v1/environment.json /postman/v1/collection.json",
      "input": "",
      "output": "",
      "comparison": "included",
      "timeout": 10,
      "points": 10
    }
  ]
}
```

### **4. postgres/20-create-databases.sh**

```bash
#!/usr/bin/env bash
set -e

# TODO для создания баз прописать свой вариант
export VARIANT="v1"
export SCRIPT_PATH=/docker-entrypoint-initdb.d/
export PGPASSWORD=postgres
psql -f "$SCRIPT_PATH/scripts/db-$VARIANT.sql"
```

---

## **Пояснения**

1. **Flight Booking System** — вариант 1. Система состоит из трёх микросервисов: Flight Service (8060), Ticket Service (8070), Bonus Service (8050) и Gateway Service (8080).
2. Каждый сервис использует свою логическую базу данных (`flights`, `tickets`, `privileges`), создаваемую скриптом `db-v1.sql`. Запросы между базами запрещены.
3. Межсервисное взаимодействие осуществляется только через Gateway Service по HTTP/REST. Горизонтальные запросы между сервисами не допускаются.
4. Для каждого сервиса реализован endpoint `GET /manage/health`, возвращающий 200 OK.
5. Все сервисы упакованы в Docker; сборка и запуск описаны в `docker-compose.yml`.
6. Github Actions настроен на сборку образов, прогон unit-тестов и выполнение Postman-коллекции из папки `/postman/v1/`.
7. В `autograding.json` и `classroom.yml` заменён `<variant>` на `v1` в путях к Postman-коллекции и environment-файлу.

При необходимости замените пути к исходным каталогам сервисов (`./flight-service`, `./ticket-service`, `./bonus-service`, `./gateway-service`) на фактические названия папок в вашем репозитории.