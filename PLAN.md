# План выполнения лабораторной работы №1
## Continuous Integration & Continuous Delivery

---

## Цель работы
Разработать REST API для управления сущностью **Person** и настроить автоматизированный процесс сборки, тестирования и деплоя с помощью GitHub Actions и Heroku.

---

## Этапы выполнения

### Этап 1: Подготовка окружения
- [ ] Клонировать шаблон репозитория: `https://github.com/bmstu-rsoi/lab1-template`
- [ ] Установить необходимые инструменты:
  - [ ] Docker и Docker Compose
  - [ ] Node.js / Java / Kotlin (в зависимости от выбранного стека)
  - [ ] Git
- [ ] Запустить локальную базу данных: `docker compose up -d`
- [ ] Проверить работу БД: `persons`, пользователь `program:test`

### Этап 2: Разработка веб-приложения
- [x] Реализовать сущность `Person` (модель данных)
- [x] Реализовать REST API endpoints:
  - [x] `GET /api/v1/persons/{personId}` — получение информации о человеке
  - [x] `GET /api/v1/persons` — получение всех людей с пагинацией
  - [x] `POST /api/v1/persons` — создание новой записи
  - [x] `PATCH /api/v1/persons/{personId}` — обновление записи
  - [x] `DELETE /api/v1/persons/{personId}` — удаление записи
- [x] Обеспечить формат запросов/ответов **JSON**
- [x] Реализовать обработку ошибок:
  - [x] Возвращать **404 Not Found** при отсутствии записи
  - [x] Возвращать **201 Created** при успешном создании с заголовком `Location`
- [x] Реализовать Clean Architecture (core, businesslogic, dataaccess, presentation)
- [x] Реализовать PersonService с валидацией и бизнес-логикой
- [x] Реализовать PersonPostgresqlRepository с EF Core

### Этап 3: Написание тестов
- [x] Написать **67 unit-тестов** на реализованные операции
  - [x] PersonServiceUnitTests: 25 тестов (London style с Moq)
  - [x] PersonConverterUnitTests: 17 тестов (DTO ↔ Domain)
  - [x] PersonHttpControllerUnitTests: 19 тестов (REST API endpoints)
  - [x] PersonPostgresqlConverterUnitTests: 15 тестов
  - [x] PersonPostgresqlRepositoryUnitTests: 10 тестов
- [x] Проверить работу unit-тестов локально: `dotnet test --filter "FullyQualifiedName~Unit"` ✅ (67/67 passed)
- [x] Написать **61 интеграционный тест**:
  - [x] PersonServiceIntegrationTests: 34 теста с PostgreSQL
  - [x] PersonPostgresqlRepositoryIntegrationTests: 27 тестов
- [x] Проверить работу интеграционных тестов: `dotnet test --filter "FullyQualifiedName~Integration"` ✅ (61/61 passed)
- [x] **Итого: 154 теста пройдено**

### Этап 4: Docker-контейнеризация
- [ ] Создать `Dockerfile` для приложения
- [ ] Проверить сборку образа локально: `docker build -t person-service .`
- [ ] Проверить запуск контейнера локально
- [ ] Убедиться, что приложение подключается к БД из контейнера

### Этап 5: Настройка GitHub Actions
- [ ] Создать/отредактировать файл `.github/workflows/build.yml`
- [ ] Добавить шаги:
  - [ ] Сборка приложения
  - [ ] Запуск unit-тестов
  - [ ] Сборка Docker-образа
  - [ ] Деплой на Heroku через GitHub Actions (без Heroku CLI и webhooks)
- [ ] Настроить секреты в репозитории:
  - [ ] `HEROKU_API_KEY`
  - [ ] `HEROKU_EMAIL`
  - [ ] `HEROKU_APP_NAME`

### Этап 6: Деплой на Heroku
- [ ] Создать приложение на Heroku
- [ ] Настроить переменные окружения в Heroku (подключение к БД)
- [ ] Запустить пайплайн через GitHub Actions
- [ ] Убедиться, что приложение доступно по публичному URL

### Этап 7: Пост-деплой настройки
- [ ] Обновить `baseUrl` в `Lab1.postman_environment.json` на адрес Heroku-приложения
- [ ] Протестировать API через Postman
- [ ] Запустить интеграционные тесты через Newman (если применимо)

### Этап 8: Финальная проверка
- [ ] Все endpoints работают корректно
- [ ] Unit-тесты проходят в GitHub Actions
- [ ] Интеграционные тесты проходят успешно
- [ ] Документация обновлена (README.md)

---

## Критерии приемки

### Обязательные требования
| № | Требование | Статус |
|---|------------|--------|
| 1 | Проект хранится на GitHub | [ ] |
| 2 | Сборка только через GitHub Actions | [ ] |
| 3 | Запросы/ответы в формате JSON | ✅ |
| 4 | Возврат **404** при отсутствии записи | ✅ |
| 5 | Возврат **201** с заголовком `Location` при создании | ✅ |
| 6 | Наличие **4–5 unit-тестов** | ✅ (67 unit + 19 controller = 86 unit-тестов) |
| 7 | Приложение завернуто в Docker | [ ] |
| 8 | Деплой на Heroku через GitHub Actions (без CLI/webhooks) | [ ] |
| 9 | Приложение использует БД для хранения данных | ✅ |
| 10 | Обновлён `baseUrl` в Postman-окружении | [ ] |

### Дополнительные критерии
| № | Критерий | Статус |
|---|----------|--------|
| D1 | Код покрыт тестами (минимум 70% coverage) | ✅ (154 теста) |
| D2 | README.md содержит инструкцию по запуску | [ ] |
| D3 | Используется `.gitignore` для артефактов сборки | ✅ |
| D4 | Пайплайн проходит успешно с первого раза | [ ] |
| D5 | Интеграционные тесты запускаются автоматически после деплоя | [ ] |

---

## Ссылки на ресурсы

- [Шаблон лабораторной работы](https://github.com/bmstu-rsoi/lab1-template)
- [Описание API (OpenAPI)](https://github.com/bmstu-rsoi/lab1-template/blob/master/person-service.yaml)
- [Документация GitHub Actions](https://docs.github.com/en/actions)
- [Пример приложения на Kotlin/Spring](https://github.com/Romanow/person-service)
- [GitHub Marketplace (поиск действий)](https://github.com/marketplace)

---

## Примечания

- При возникновении проблем с деплоем проверьте логи в GitHub Actions
- Для локального тестирования интеграционных тестов импортируйте коллекцию и окружение в Postman
- Убедитесь, что секреты GitHub Actions настроены корректно
