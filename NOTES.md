# Заметки по лабораторной работе №1
## Continuous Integration & Continuous Delivery

---

## Важная информация

### Технические решения

> Записывайте здесь принятые архитектурные и технические решения

- **Выбранный стек технологий:**
  ```
  Язык:          C#
  Фреймворк:     ASP.NET Core Web API (.NET 10.0)
  Архитектура:   Clean Architecture
  БД:            PostgreSQL 13
  ORM:           Entity Framework Core
  Тестирование:  xUnit / NUnit
  Контейнериз.:  Docker
  CI/CD:         GitHub Actions
  Деплой:        Heroku
  ```

- **Структура проекта:**
  ```
  PersonService.sln
  └── src/
      ├── core/           # Ядро (сущности, доменные события)
      ├── businesslogic/  # Бизнес-логика (use cases, сервисы)
      ├── dataaccess/     # Доступ к данным (EF Core, репозитории)
      ├── presentation/   # Представление (API controllers, DI)
  ├── postgres/         # SQL-скрипты инициализации БД
  ├── postman/          # Коллекции для тестирования
  ├── .github/workflows/ # GitHub Actions workflows
  ├── docker-compose.yml # Локальная БД
  ├── Dockerfile         # Контейнеризация приложения
  └── person-service.yaml # OpenAPI спецификация
  ```

---

## Проблемы и решения

| Проблема | Решение | Дата |
|----------|---------|------|
| [Опишите проблему] | [Опишите решение] | [ДД.ММ.ГГГГ] |
| | | |
| | | |

---

## Полезные команды

### Docker
```bash
# Запуск локальной БД
docker compose up -d

# Просмотр логов БД
docker compose logs -f postgres

# Сборка образа
docker build -t person-service .

# Запуск контейнера
docker run -p 8080:8080 person-service

# Просмотр запущенных контейнеров
docker ps

# Остановка всех контейнеров
docker compose down
```

### GitHub Actions
```bash
# Проверка workflow локально (если используется act)
act -j build
```

### Postman/Newman
```bash
# Запуск коллекции через Newman
newman run lab1.postman_collection.json -e lab1.postman_environment.json

# Экспорт результатов в HTML
newman run lab1.postman_collection.json -e lab1.postman_environment.json --reporters cli,html
```

### Git
```bash
# Создание новой ветки для работы
git checkout -b feature/lab1-implementation

# Просмотр истории коммитов
git log --oneline -10
```

---

## Чек-листы для отладки

### Перед запуском пайплайна
- [ ] Все секреты настроены в GitHub Repository Settings → Secrets
- [ ] `HEROKU_API_KEY` добавлен
- [ ] `HEROKU_EMAIL` добавлен
- [ ] `HEROKU_APP_NAME` добавлен
- [ ] Локально все тесты проходят успешно

### Проверка деплоя
- [ ] Приложение доступно по URL Heroku
- [ ] GET /persons возвращает пустой массив или список
- [ ] POST /persons создаёт запись и возвращает 201
- [ ] GET /persons/{id} возвращает 404 для несуществующего ID
- [ ] DELETE /persons/{id} удаляет запись

### Проверка БД
- [ ] Подключение к БД работает
- [ ] Таблица `persons` создана
- [ ] CRUD-операции работают с БД
- [ ] Данные сохраняются после перезапуска контейнера

---

## Логи ошибок

### Ошибки GitHub Actions
```
[Копируйте сюда логи ошибок из GitHub Actions с указанием шага и времени]
```

### Ошибки Heroku
```
[Копируйте сюда логи из Heroku: heroku logs --tail]
```

### Ошибки приложения
```
[Копируйте сюда stack trace и контекст ошибки]
```

---

## Полезные ссылки

### Документация
- [GitHub Actions Docs](https://docs.github.com/en/actions)
- [Heroku Deployment Guide](https://devcenter.heroku.com/categories/deployment)
- [Docker Documentation](https://docs.docker.com/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)

### Action'ы из GitHub Marketplace
- [Docker Build & Push](https://github.com/marketplace/actions/build-and-push-docker-images)
- [Heroku Deploy](https://github.com/marketplace/actions/deploy-to-heroku)
- [Node.js Actions](https://github.com/marketplace?type=actions&query=node)

---

## Лог изменений

| Дата | Изменение | Автор |
|------|-----------|-------|
| | [Опишите изменение] | |
| | | |
| | | |

---

## Примечания преподавателя

> Записывайте здесь комментарии и замечания от преподавателя/ревьюера

---

## Ссылки на ресурсы проекта

- **Репозиторий:** [Вставьте ссылку на ваш GitHub репозиторий]
- **Приложение на Heroku:** [Вставьте URL вашего приложения]
- **Workflow файл:** `.github/workflows/build.yml`
- **Dockerfile:** `Dockerfile`
- **Postman коллекция:** `postman/lab1.postman_collection.json`

---

## Резюме работы

**Дата начала:** _______________

**Дата завершения:** _______________

**Статус:** [ ] В процессе / [ ] Завершено / [ ] На доработке

**Замечания:**
```
[Итоговые замечания по работе]
```
