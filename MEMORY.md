# MEMORY.md — Текущее состояние курса

## 📌 Текущая лабораторная работа
- **Лабораторная:** Lab 01 — CI/CD
- **Статус:** В процессе (Docker и Makefile настроены, осталось GitHub Actions)
- **Дата начала:** 2025-01-20
- **Текущие задачи:**
  - [x] Изучить описание задания в `TASK.md`
  - [x] Развернуть шаблон проекта
  - [x] Написать unit-тесты
  - [x] Реализовать PersonConverter с ToDTO/ToDomain методами
  - [x] Создать юнит-тесты для PersonConverter (17 тестов)
  - [x] Реализовать REST API для Person (заменить шаблонный код)
  - [x] Создать PersonHttpController с CRUD endpoints
  - [x] Написать 19 юнит-тестов для PersonHttpController (все passed!)
  - [x] Реализовать Program.cs с DI контейнером
  - [x] Настроить подключение к PostgreSQL
  - [x] Создать `Dockerfile` для приложения (multi-stage build)
  - [x] Настроить `docker-compose.yml` с prod и test БД (строгая конфигурация)
  - [x] Упростить `Makefile` с `COMPOSE_OPTS`
  - [x] Добавить health check endpoint (`/health`)
  - [x] Закоммитить и запушить изменения
  - [ ] Настроить GitHub Actions (`.github/workflows/build.yml`)
  - [ ] Настроить деплой на Heroku через GitHub Actions

## 📚 Текущая тема изучения
- **Лекция 13:** Инфраструктура — мониторинг, логирование, трейсинг, CI/CD

## ✅ Прогресс по курсу
- **Lab 01:** Person API реализован с Clean Architecture
- **Tests:** 154 теста пройдено (67 unit + 61 integration + 26 controller unit)
- **Program.cs:** Реализован по принципам SOLID с поддержкой переключения СУБД
- Лекции 1-5 (в процессе изучения)

## 📅 Задачи на ближайшую неделю
- ✅ Реализовать REST API endpoints для Person
- ✅ Написать 19 юнит-тестов для PersonHttpController (все passed!)
- ✅ Реализовать Program.cs с DI контейнером и поддержкой переключения СУБД
- ✅ Настроить Docker-контейнеризацию приложения (multi-stage build)
- ✅ Настроить `docker-compose.yml` с prod и test контейнерами БД
- ✅ Упростить `Makefile` с `COMPOSE_OPTS`
- ✅ Добавить health check endpoint (`/health`)
- ✅ Закоммитить и запушить изменения на GitHub
- [ ] Создать GitHub Actions workflow для CI/CD
- [ ] Настроить деплой на Heroku

## 📝 Заметки и проблемы
- **Стек:** C# / ASP.NET Core / PostgreSQL 14 / EF Core
- **Архитектура:** Clean Architecture (core, businesslogic, dataaccess, presentation)
- **✅ PersonHttpController реализован** с CRUD endpoints (GET, POST, PATCH, DELETE)
- **✅ PersonHttpControllerUnitTests:** 19 London-style юнит-тестов с Moq ✅ ALL PASSED!
- **✅ Все 154 теста пройдены:** 67 unit + 61 integration + 26 controller unit
- **✅ Program.cs реализован по принципам SOLID:**
  - **Single Responsibility:** Разделение на отдельные классы конфигурации
  - **Open/Closed:** Легко добавлять новые СУБД через switch-выражения
  - **Liskov Substitution:** IDatabaseContext абстракция для всех СУБД
  - **Interface Segregation:** Специализированные интерфейсы
  - **Dependency Inversion:** Высокоуровневые модули не зависят от EF Core
- **✅ Поддержка переключения СУБД через .env файл:**
  - `DATABASE_PROVIDER=POSTGRESQL` (или MYSQL, SQLITE, SQLSERVER)
  - Конфигурация PostgreSQL в `.env` файле
  - Factory Pattern для создания репозиториев
- **✅ Docker Compose настроен (строгая конфигурация, без fallbacks):**
  - **postgres-prod:** порт 5433 (с bind mount `./data/postgres_data`)
  - **postgres-test:** порт 5434 (без volume, данные стираются при остановке)
  - **app:** порт 8080 (здоров, health check `/health`)
  - Переменные конфигурации берутся из `.env` файла
- **✅ Makefile упрощён:**
  - `COMPOSE_OPTS` — единая переменная для всех docker-compose команд
  - Команды: `make up`, `make down`, `make status`, `make logs`, `make test`
- **✅ Закоммичено и запушено на GitHub** (commit `a1d2fd6`)
- **⚠️ Проблема:** `BUILD_DATE` переменная не задана (не критично, используется пустое значение)

---

## 🔧 Что нужно сделать дальше

1. **Реализовать API** — заменить `Program.cs` на Person CRUD endpoints
2. **Создать Dockerfile** — для контейнеризации .NET приложения
3. **Настроить GitHub Actions** — `.github/workflows/build.yml` с шагами:
   - Сборка проекта
   - Запуск unit-тестов
   - Сборка Docker-образа
   - Деплой на Heroku
