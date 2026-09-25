Копнул глубже — сравнил ваш код и с официальным ТЗ (`bmstu-rsoi/lab2-template`, вариант v1 — Flight Booking System), и с логикой самого приложения, независимо от ТЗ. Список получился длинный, но каждый пункт — с конкретной ссылкой на файл, чтобы можно было проверить самостоятельно.

## A. Баги в коде (не про ТЗ, просто не работает как задумано)

**1. Нет глобальной обработки исключений — большинство `throw new *Exception` "теряются".**
Ни в одном из 4 сервисов не зарегистрирован `app.UseExceptionHandler(...)` / `IExceptionFilter` / ProblemDetails-middleware (проверил `Program.cs` во всех). При этом по всей кодовой базе **66 мест** делают `throw new TicketInternalServerException(...)`, `throw new SagaException(...)` и т.п. вместо `return StatusCode(500, ...)`. Без middleware это необработанное исключение ASP.NET Core — клиент получит либо голый 500 без тела, либо диагностическую HTML-страницу, а точно не `{"message": "..."}`, который ждут тесты.

Маскируется это юнит-тестами: они дёргают метод контроллера напрямую и проверяют `Assert.ThrowsAsync<TicketInternalServerException>(...)` (см. `TicketHttpControllerUnitTests.cs`) — тест подтверждает, что исключение вылетело, а не то, что реальный HTTP-ответ выглядит правильно. 100% зелёных юнит-тестов тут ничего не говорит о работоспособности API через Postman.

**2. Даже "правильные" ветки под вопросом.** Там, где код возвращает `return NotFound(new HttpTicketNotFoundException(ticketId))`, в тело ответа кладётся сам объект исключения (наследник `System.Exception`). System.Text.Json по умолчанию известен тем, что падает на сериализации `Exception`-производных типов (свойство `TargetSite: MethodBase` не поддерживается) без явного конвертера — это частый .NET-готча. Не проверял руками (нет dotnet в песочнице), но стоит явно протестировать curl'ом, не улетает ли внезапный 500 там, где должен быть 404/400/409. Правильный паттерн — отдельные *Response-DTO для ошибок, а не сами исключения в теле.

**3. `BookingSagaCoordinator` — мёртвый код.** Создан, заинжектирован в `TicketHttpController`, но ни разу не вызван внутри `CreateTicket`. Реально работающая "SAGA" есть только в `BookingHttpController.CreateBookingWithTicket` — эндпоинте, которого нет в задании.

**4. `new HttpClient()` без `IHttpClientFactory` и без таймаута** (`gateway/Program.cs`) — в проде утечка сокетов, а для ЛР3 — нет fail-fast.

**5. Пагинация на Gateway считается уже после того, как все записи вытащены из сервиса** (`GetAllAsync()` без фильтра → `.Skip().Take()` в памяти). Не критично для лабы, но не то, что ожидается от микросервисной пагинации.

**6. `BaseHttpGateway.HandleResponseAsync`: `if (content == "[]") return default!;`** — для пустого массива метод возвращает `null`, а не пустую коллекцию. Часть кода подстраховывается (`dtos?.Select(...) ?? Enumerable.Empty<T>()`), но это защита от собственной ошибки, а не решение — легко забыть `?.` и словить NRE.

**7. Дублирующаяся сущность `Booking`** на Gateway и в Ticket-microservice — не используется реальным сценарием покупки, не описана в задании, добавляет только поверхность для багов.

## B. Несоответствия ТЗ (сверено построчно с `lab2-template/v1/README.md` и OpenAPI)

| # | Требует ТЗ | По факту в коде |
|---|---|---|
| 1 | `ticket(ticket_uid, username, flight_number, price, status)` | Совсем другая модель: `bookings` (customer_name/email/phone, payment_method...) + `tickets` (flight_id **int**, passenger_name, seat_number, class...). Поля `username` нет вообще, `flight_number` заменён числовым `flight_id`. |
| 2 | `POST /api/v1/tickets`: проверить рейс, `paidByBonuses = min(balance, price)` при `paidFromBalance=true`, иначе +10% кешбэк, запись в `privilege_history` | Flight не вызывается, Bonus не вызывается. Тело запроса — не `{flightNumber, price, paidFromBalance}`, а форма generic-бронирования (`FlightId`, `PassengerName`, `SeatNumber`, `Class`...). |
| 3 | `DELETE /api/v1/tickets/{uid}`: статус `CANCELED` + откат/списание бонусов в Bonus Service | Bonus вообще не трогается. |
| 4 | `GET /api/v1/tickets/{ticketUid}`: "проверить, что билет принадлежит пользователю" | `X-User-Name` в `TicketHttpController` не читается вообще — ни для проверки владения, ни для фильтрации. |
| 5 | `GET /api/v1/flights` → `PaginationResponse {page, pageSize, totalElements, items}` | Голый `List<FlightDTO>`, полей `items`/`page`/`totalElements` нет. |
| 6 | `fromAirport`/`toAirport` — строка `"Санкт-Петербург Пулково"` | Отдаются только числовые `FromAirportId`/`ToAirportId`, резолвинга в название нет. |
| 7 | `ticketUid` — UUID в пути | Роуты Gateway используют числовой `{ticketId}`. |
| 8 | Таблицы `ticket`, `flight`, `airport` (ед. число, как в SQL из ТЗ) | В Flight-service — `flights`, `airports` (мн. число); для тестов не критично, но прямое расхождение со схемой из задания. |
| 9 | Статусы BRONZE/SILVER/GOLD — в ТЗ описаны только как enum, **правил перехода нет вовсе** | В вашем README ЛР2 заявлено "SILVER после 10 броней — 7% скидка", "GOLD после 20 — 10%" — этого нет ни в официальном ТЗ, ни в `PrivilegeService.cs` (там нет логики повышения статуса/расчёта скидки). Похоже на выдуманную фичу, которая к тому же не реализована — только задокументирована. |
| 10 | Ошибка = `ErrorResponse {message}` / `ValidationErrorResponse {message, errors[]}` | Тело ошибки — сериализованное исключение (см. баг №2 из части A), гарантий соответствия контракту нет. |

## Итог

Слои Clean Architecture — это скорее качественно сделанный скелет, а доменная логика конкретно вашего варианта (покупка/возврат билета с бонусами) в Gateway почти не реализована: там сейчас generic CRUD над моделью бронирований, не связанной ни с Flight, ни с Bonus сервисом. Это стоит переписать заново под ТЗ до того, как наворачивать Circuit Breaker и retry-queue из ЛР3 — иначе вы обернёте отказоустойчивостью то, что и в штатном режиме не проходит тесты ЛР2.
