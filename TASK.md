# Лабораторная работа №1
## Continuous Integration & Continuous Delivery

### Формулировка

В рамках первой лабораторной работы требуется написать простейшее веб-приложение, предоставляющее пользователю набор операций над сущностью **Person**[reference:0]. Для этого приложения необходимо автоматизировать процесс сборки, тестирования и релиза на Railway[reference:1].

Приложение должно реализовать следующее API[reference:2]:
* `GET /persons/{personId}` – информация о человеке
* `GET /persons` – информация по всем людям
* `POST /persons` – создание новой записи о человеке
* `PATCH /persons/{personId}` – обновление существующей записи о человеке
* `DELETE /persons/{personId}` – удаление записи о человеке

[Описание API](https://github.com/bmstu-rsoi/lab1-template/blob/master/person-service.yaml) в формате OpenAPI.

### Требования

1. Исходный проект хранится на Github. Для сборки использовать **только** [Github Actions](https://docs.github.com/en/actions)[reference:3].
2. Запросы и ответы должны быть в формате JSON[reference:4].
3. Если запись по id не найдена, возвращать HTTP статус **404 Not Found**[reference:5].
4. При создании новой записи (метод `POST /person`) возвращать HTTP статус **201 Created** с пустым телом и заголовком `Location: /api/v1/persons/{personId}`, где `personId` – id созданной записи[reference:6].
5. Приложение должно содержать **4–5 unit-тестов** на реализованные операции[reference:7].
6. Приложение должно быть завернуто в **Docker**[reference:8].
7. Деплой на Railway реализовать средствами GitHub Actions, используя Docker. Для деплоя **нельзя** использовать Railway CLI или webhooks[reference:9].
8. В [build.yml](https://github.com/bmstu-rsoi/lab1-template/blob/master/.github/workflows/classroom.yml) дописать шаги на сборку, прогон unit-тестов и деплой на Railway[reference:10].
9. Приложение должно использовать **БД** для хранения записей[reference:11].
10. В [Lab1.postman_environment.json](https://github.com/bmstu-rsoi/lab1-template/blob/master/postman/%5Binst%5D%5Bheroku%5D%20Lab1.postman_environment.json) заменить значение `baseUrl` на адрес развернутого сервиса на Railway[reference:12].

### Пояснения

* [Пример](https://github.com/Romanow/person-service) приложения на Kotlin / Spring.
* Для локальной разработки можно использовать Postgres в Docker: запустите `docker compose up -d` — поднимется контейнер с Postgres 13, будет создана БД `persons` и пользователь `program:test`[reference:13].
* После успешного деплоя на Railway через newman запускаются интеграционные тесты[reference:14].
* Интеграционные тесты можно проверить локально — импортируйте в Postman коллекцию [lab1.postman_collection.json](https://github.com/bmstu-rsoi/lab1-template/blob/master/postman/%5Binst%5D%20Lab1.postman_collection.json) и environment [[local] lab1.postman_environment.json](https://github.com/bmstu-rsoi/lab1-template/blob/master/postman/%5Binst%5D%5Blocal%5D%20Lab1.postman_environment.json)[reference:15].
* Для поиска нужного инструмента для сборки используйте [Github Marketplace](https://github.com/marketplace)[reference:16].