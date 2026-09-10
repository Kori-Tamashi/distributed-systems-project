# GitHub Actions CI/CD Setup

## Overview

Этот проект использует GitHub Actions для автоматизации сборки, тестирования и деплоя:

- **CI Pipeline** (`.github/workflows/ci.yml`) — сборка и тестирование при каждом push/PR
- **CD Pipeline** (`.github/workflows/cd.yml`) — деплой на Railway после успешного CI

## Setup Instructions

### 1. Создать проект на Railway

1. Зарегистрируйтесь на [Railway.app](https://railway.app)
2. Создайте новый проект
3. Добавьте переменные окружения:
   ```env
   DATABASE_PROVIDER=POSTGRESQL
   PostgreSQL__Host=...
   PostgreSQL__Port=5432
   PostgreSQL__Database=persons
   PostgreSQL__Username=program
   PostgreSQL__Password=...
   ```

### 2. Настроить секреты в GitHub

Перейдите в **Settings → Secrets and variables → Actions** и добавьте:

| Secret Name | Description | How to get |
|-------------|-------------|------------|
| `RAILWAY_TOKEN` | API токен Railway | [Generate token](https://railway.app/settings/tokens) |
| `RAILWAY_PROJECT_ID` | ID проекта на Railway | Project Settings → General |
| `RAILWAY_ENVIRONMENT_ID` | ID окружения (production) | Project Settings → Environments |

**Как получить Railway Token:**
1. Зайдите на [Railway.app](https://railway.app)
2. Перейдите в **Settings → Tokens**
3. Нажмите **Generate New Token**
4. Скопируйте токен и добавьте в GitHub Secrets как `RAILWAY_TOKEN`

**Как получить Project ID и Environment ID:**
1. Откройте ваш проект на Railway
2. Перейдите в **Settings → General**
3. Скопируйте **Project ID**
4. Нажмите на окружение (например, **Production**)
5. Скопируйте **Environment ID**

### 3. Запустить деплой

После настройки секретов:

1. Push на ветку `main` автоматически запустит CI pipeline
2. После успешного CI, автоматически запустится CD pipeline
3. Приложение будет развёрнуто на Railway

Или запустите вручную:
- Перейдите в **Actions → CD - Deploy to Railway**
- Нажмите **Run workflow**

## Pipeline Details

### CI Pipeline (`ci.yml`)

Запускается при:
- Push на `main` или `develop`
- Pull Request на `main`

**Этапы:**
1. **CodeQL Security Analysis** — статический анализ безопасности
2. **Build** — сборка .NET проекта
3. **Unit Tests** — запуск unit-тестов с coverage
4. **Integration Tests** — запуск интеграционных тестов с PostgreSQL
5. **Docker Build Test** — проверка Docker образа

### CD Pipeline (`cd.yml`)

Запускается при:
- Успешном завершении CI на ветке `main`
- Ручном запуске (workflow_dispatch)

**Этапы:**
1. Checkout кода
2. Установка Railway CLI
3. Деплой на Railway
4. Вывод URL сервиса

## Troubleshooting

### Деплой не запускается

**Проверьте:**
1. CI pipeline завершился успешно
2. Секреты `RAILWAY_TOKEN`, `RAILWAY_PROJECT_ID` настроены
3. Ветка `main` (CD запускается только с main)

### Ошибка "RAILWAY_TOKEN не найден"

Добавьте секрет в GitHub:
- Settings → Secrets and variables → Actions → New repository secret
- Name: `RAILWAY_TOKEN`
- Value: ваш токен из Railway

### Приложение не запускается на Railway

**Проверьте:**
1. Переменные окружения в Railway Settings
2. Лог деплоя в Railway Dashboard
3. Dockerfile корректен (multi-stage build)

### Интеграционные тесты падают

**Проверьте:**
1. PostgreSQL сервис в workflow запущен
2. Правильные credentials в env переменных
3. Network connectivity между контейнерами

## Useful Links

- [GitHub Actions Docs](https://docs.github.com/en/actions)
- [Railway CLI Docs](https://docs.railway.app/guides/cli)
- [Railway API Tokens](https://docs.railway.app/guides/tokens)
- [.NET on GitHub Actions](https://github.com/actions/setup-dotnet)

## Post-Deployment

После успешного деплоя:

1. **Обновите Postman environment:**
   - Откройте `[inst][heroku] Lab1.postman_environment.json`
   - Замените `baseUrl` на URL из Railway

2. **Запустите интеграционные тесты через Newman:**
   ```bash
   npm install -g newman
   newman run "lab1.postman_collection.json" -e "[inst][heroku] Lab1.postman_environment.json"
   ```

3. **Проверьте API:**
   - `GET /api/v1/persons`
   - `POST /api/v1/persons`
   - `GET /api/v1/persons/{id}`
   - `PATCH /api/v1/persons/{id}`
   - `DELETE /api/v1/persons/{id}`
