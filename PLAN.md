# PLAN.md — ЛР3 (Fault Tolerance), ветка `lab_03`

Порядок ниже собран под текущее состояние кода в `main` (не в `lab_03` — она сейчас отстаёт, см. шаг 0). Ссылки на
файлы/классы — реальные, из `services/gateway-microservice/src` и `.github/workflows/ci.yml`.

## Шаг 0. Подтянуть `lab_03` к состоянию `main`

`lab_03` сейчас соответствует старой версии кода (до всех фиксов ЛР2). Начинать ЛР3 нужно поверх исправленного кода.

```shell
git checkout lab_03
git merge main
```

## Шаг 1. Инфраструктура — без корневого docker-compose

Никакого нового `docker-compose.yml` в корне не заводим. Используем то, что уже есть: отдельный compose-файл на
каждый сервис — для локальной разработки, и уже существующий job `autograding` в `ci.yml` — для CI.

### 1.1. Локальная разработка: связать per-service compose файлы общей сетью

Сейчас у каждого из 4 файлов (`docker-compose.<service>-microservice.yml`) секция `networks: rsoi-network: driver:
bridge` — то есть у каждого **своя** сеть `rsoi-network`, они не пересекаются, и контейнеры из разных compose-проектов
друг друга не видят. Чтобы `docker compose -f .../docker-compose.bonus-microservice.yml stop bonus-api` не ломал
видимость для Gateway, нужно:

1. В каждом из 4 файлов заменить:
   ```yaml
   networks:
     rsoi-network:
       driver: bridge
   ```
   на:
   ```yaml
   networks:
     rsoi-network:
       external: true
   ```
2. Один раз создать сеть вручную: `docker network create rsoi-network`.
3. В `docker-compose.gateway-microservice.yml` у сервиса `gateway-api` сейчас нет переменных, по которым Gateway
   реально ищет соседей (`Program.cs` читает `FLIGHT_API_URL`, `TICKET_API_URL`, `BONUS_API_URL` — в файле же
   прописаны `FLIGHT_CONNECTIONSTRINGS_DEFAULTCONNECTION` и т.п., которые код не использует). Добавить:
   ```yaml
   environment:
     - FLIGHT_API_URL=http://flight-api:8060
     - TICKET_API_URL=http://ticket-api:8070
     - BONUS_API_URL=http://bonus-api:8050
   ```

После этого локальная проверка выглядит так:
```shell
docker network create rsoi-network
docker compose -f services/flight-microservice/docker-compose.flight-microservice.yml up -d
docker compose -f services/ticket-microservice/docker-compose.ticket-microservice.yml up -d
docker compose -f services/bonus-microservice/docker-compose.bonus-microservice.yml up -d
docker compose -f services/gateway-microservice/docker-compose.gateway-microservice.yml up -d
# дождаться /manage/health на 8060/8070/8050/8080
docker compose -f services/bonus-microservice/docker-compose.bonus-microservice.yml stop bonus-api
# newman --folder=step1 ...
docker compose -f services/bonus-microservice/docker-compose.bonus-microservice.yml start bonus-api
# newman --folder=step2 ...
```

### 1.2. Postman-коллекция — заменить на версию с failover

Сейчас `postman/collection.json` и `postman/environment.json` — это коллекция ЛР2 (только папка `success`). Скачайте
и положите на их место версию из `bmstu-rsoi/lab3-template/v1/postman/` — в ней те же `success`-тесты плюс папка
`failover` с `step1`–`step4`, которые и проверяют отказоустойчивость. Заодно замените
`[inst][v1] Flight Booking System.yml` (OpenAPI) на актуальную версию оттуда же.

### 1.3. CI — дописать финальные шаги в существующий job `autograding`

В `ci.yml` job `autograding` — последний в пайплайне: поднимает `bonus-api-e2e`, `flight-api-e2e`, `ticket-api-e2e`,
`gateway-api-e2e` через `docker run` в сети `autograding-network` (без всякого compose), сидирует тестовые данные,
прогоняет `success`-коллекцию инструктора. Проверку отказоустойчивости добавляем **сюда же, последними шагами**,
сразу после `Run autograding (instructor) Postman tests` (и его upload/publish) и перед `Cleanup Docker containers` —
контейнеры уже подняты и засижены, поднимать их заново не нужно:

```yaml
      - name: Step 1 — stop Bonus Service
        run: |
          docker stop bonus-api-e2e
          docker run --rm --network autograding-network \
            -v $(pwd)/postman/test-results:/etc/newman/results \
            postman-newman-instructor:e2e \
            run /etc/newman/collection.json -e /etc/newman/environment.json \
              --env-var "baseUrl=http://gateway-api-e2e:8080" \
              --folder step1 \
              --reporters cli,junit --reporter-junit-export /etc/newman/results/step1.xml

      - name: Step 2 — start Bonus Service
        run: |
          docker start bonus-api-e2e
          for i in {1..30}; do
            curl -sf http://localhost:8050/manage/health > /dev/null 2>&1 && break
            sleep 2
          done
          docker run --rm --network autograding-network \
            -v $(pwd)/postman/test-results:/etc/newman/results \
            postman-newman-instructor:e2e \
            run /etc/newman/collection.json -e /etc/newman/environment.json \
              --env-var "baseUrl=http://gateway-api-e2e:8080" \
              --folder step2 \
              --reporters cli,junit --reporter-junit-export /etc/newman/results/step2.xml

      - name: Step 3 — stop Bonus Service again
        run: |
          docker stop bonus-api-e2e
          docker run --rm --network autograding-network \
            -v $(pwd)/postman/test-results:/etc/newman/results \
            postman-newman-instructor:e2e \
            run /etc/newman/collection.json -e /etc/newman/environment.json \
              --env-var "baseUrl=http://gateway-api-e2e:8080" \
              --folder step3 \
              --reporters cli,junit --reporter-junit-export /etc/newman/results/step3.xml

      - name: Step 4 — start Bonus Service again (final check)
        run: |
          docker start bonus-api-e2e
          for i in {1..30}; do
            curl -sf http://localhost:8050/manage/health > /dev/null 2>&1 && break
            sleep 2
          done
          docker run --rm --network autograding-network \
            -v $(pwd)/postman/test-results:/etc/newman/results \
            postman-newman-instructor:e2e \
            run /etc/newman/collection.json -e /etc/newman/environment.json \
              --env-var "baseUrl=http://gateway-api-e2e:8080" \
              --folder step4 \
              --reporters cli,junit --reporter-junit-export /etc/newman/results/step4.xml

      - name: Upload fault-tolerance test results
        uses: actions/upload-artifact@v4
        if: always()
        with:
          name: fault-tolerance-junit-results
          path: postman/test-results/step*.xml
          retention-days: 7

      - name: Publish fault-tolerance test results to GitHub
        uses: dorny/test-reporter@v1
        if: success() || failure()
        with:
          name: Fault Tolerance - Bonus Service failover (Postman)
          path: postman/test-results/step*.xml
          reporter: java-junit

      # существующий шаг "Cleanup Docker containers" остаётся последним
```

`postgres-bonus-tests` при этом не трогаем — стопается/стартует только сам API-контейнер, данные (включая
засиженный `privilege` для `Test Max`) сохраняются между шагами, что и нужно.

## Шаг 2. Порядок операций в покупке билета — фиксируем по ТЗ

Без вариантов: реализуем ровно так, как написано в ТЗ — **Flight → Ticket (создать) → Bonus (списать/начислить) →
если Bonus упал, откатить Ticket**. Сейчас в `TicketService.BuyTicketAsync` порядок `Flight → Bonus → Ticket` —
это отличается от ТЗ и требует правки:

```csharp
// Было: Flight → Bonus → Ticket
// Нужно: Flight → Ticket → Bonus, с откатом Ticket при неудаче Bonus

var flight = await _flightGateway.GetByNumberAsync(flightNumber); // критично, падает без fallback

var ticket = await _ticketGateway.CreateAsync(new Ticket { TicketUid = ticketUid, Username = username,
    FlightNumber = flightNumber, Price = price, Status = TicketStatus.Paid });

try
{
    var privilege = paidFromBalance
        ? await _privilegeGateway.DebitAsync(username, Math.Min(currentBalance, price), ticketUid)
        : await _privilegeGateway.CreditAsync(username, price / 10, ticketUid);

    return new PurchasedTicket { Ticket = ticket, Flight = flight, Privilege = privilege, ... };
}
catch (ServiceUnavailableException)
{
    await _ticketGateway.DeleteAsync(ticket.TicketUid); // откат
    throw; // presentation превратит в 503 "Bonus Service unavailable"
}
```

Обновите `PROBLEMS.md`: пункт «Order of Operations in BuyTicketAsync» переводится из «Documented (not critical for
Lab 2)» в «Fixed — reordered to match Lab 3 spec (Flight → Ticket → Bonus, with ticket rollback)».

## Шаг 3. Core — новые абстракции

```csharp
// core/interfaces/dataaccess/IRetryQueue.cs
public interface IRetryQueue
{
    void Enqueue(string operationId, Func<Task> operation);
}
```

Исключение "сервис недоступен" — наследник уже существующего `presentation.exceptions.http.BaseHttpException`, тогда
`ExceptionHandlingMiddleware` подхватит его без изменений:

```csharp
public class ServiceUnavailableException : BaseHttpException
{
    public ServiceUnavailableException(string serviceName)
        : base(503, "SERVICE_UNAVAILABLE", $"{serviceName} unavailable") { }
}
```

## Шаг 4. Data access — таймауты + Circuit Breaker декораторы

1. **Таймауты.** `AddHttpClient()` сейчас без `.Timeout` (дефолт — 100 секунд):
   ```csharp
   builder.Services.AddHttpClient("resilient", c => c.Timeout = TimeSpan.FromSeconds(2));
   ```
2. **Декоратор поверх `IFlightGateway`/`ITicketGateway`/`IPrivilegeGateway`** — тот же интерфейс, что и у
   существующих `*HttpGateway`, businesslogic не меняется:
   ```csharp
   public class CircuitBreakerPrivilegeGateway : IPrivilegeGateway
   {
       private readonly IPrivilegeGateway _inner;
       private readonly CircuitBreakerState _state; // Closed/Open/HalfOpen, счётчик ошибок, таймер probe

       public async Task<IEnumerable<Privilege>> GetAllAsync(PrivilegeFilter filter)
       {
           if (_state.IsOpen && !_state.ShouldProbe())
               throw new ServiceUnavailableException("Bonus Service");
           try
           {
               var result = await _inner.GetAllAsync(filter);
               _state.RecordSuccess();
               return result;
           }
           catch (HttpRequestException)
           {
               _state.RecordFailure();
               throw new ServiceUnavailableException("Bonus Service");
           }
       }
       // остальные методы интерфейса — так же
   }
   ```
3. Зарегистрировать по одному `CircuitBreakerState` (singleton) на каждый внешний сервис, обернуть регистрацию
   гейтвеев в `Program.cs` — конкретный `*HttpGateway` остаётся как есть, декоратор просто встаёт перед ним в DI.

Декоратор нужен только для **операций чтения** (`GetAllAsync`/`GetByIdAsync`). Методы записи (`DebitBalanceAsync`,
`CreditBalanceAsync`, `CreateAsync` у Ticket) им не оборачиваются — там нужна обработка неудачи через откат/очередь
(Шаг 2 и Шаг 5), а не fallback.

## Шаг 5. Business logic — деградация и очередь

1. **`GET /api/v1/me`, `GetTicketById`, `GetAllTickets`** — оборачиваете вызовы к Flight/Bonus в try/catch по
   `ServiceUnavailableException` и подставляете fallback (для `/me` — `privilege = null`; для тикетов —
   `fromAirport`/`toAirport`/`date` — дефолтные значения) вместо падения. `UserHttpController.GetUserInfo` сейчас
   отдаёт 404, если `privilege == null` — это два разных случая («нет privilege-записи» и «Bonus недоступен»), их
   нужно различать в коде: первое остаётся 404, второе — 200 с fallback.

2. **`ReturnTicketAsync`**:
   ```csharp
   try
   {
       await _privilegeGateway.RollbackAsync(ticketUid);
   }
   catch (ServiceUnavailableException)
   {
       _retryQueue.Enqueue($"rollback-{ticketUid}", () => _privilegeGateway.RollbackAsync(ticketUid));
       // клиенту всё равно отвечаем успехом — статус тикета уже CANCELED
   }
   ```

## Шаг 6. Retry queue — реализация

`Channel<T>` + `BackgroundService`, без внешних зависимостей:

```csharp
public class InMemoryRetryQueue : IRetryQueue, IHostedService
{
    private readonly Channel<(string Id, Func<Task> Op)> _channel = Channel.CreateUnbounded<(string, Func<Task>)>();

    public void Enqueue(string id, Func<Task> op) => _channel.Writer.TryWrite((id, op));

    public async Task StartAsync(CancellationToken ct)
    {
        _ = Task.Run(async () =>
        {
            await foreach (var (id, op) in _channel.Reader.ReadAllAsync(ct))
            {
                try { await op(); }
                catch { _channel.Writer.TryWrite((id, op)); await Task.Delay(TimeSpan.FromSeconds(10), ct); }
            }
        }, ct);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
```

Регистрация: `AddSingleton<InMemoryRetryQueue>()`, `AddSingleton<IRetryQueue>(sp =>
sp.GetRequiredService<InMemoryRetryQueue>())`, `AddHostedService(sp => sp.GetRequiredService<InMemoryRetryQueue>())`.

(В `docker-compose.integration-tests.yml` уже поднят `bonus-redis`, но код его не использует — можно оставить
in-memory реализацию, это не противоречит ТЗ: требование только исключает реляционную БД.)

## Шаг 7. Ручная проверка (локально, без CI)

```shell
docker network create rsoi-network
docker compose -f services/flight-microservice/docker-compose.flight-microservice.yml up -d --build
docker compose -f services/ticket-microservice/docker-compose.ticket-microservice.yml up -d --build
docker compose -f services/bonus-microservice/docker-compose.bonus-microservice.yml up -d --build
docker compose -f services/gateway-microservice/docker-compose.gateway-microservice.yml up -d --build
# дождаться /manage/health на 8060/8070/8050/8080
newman run postman/collection.json -e postman/environment.json --folder success

docker compose -f services/bonus-microservice/docker-compose.bonus-microservice.yml stop bonus-api
newman run postman/collection.json -e postman/environment.json --folder step1

docker compose -f services/bonus-microservice/docker-compose.bonus-microservice.yml start bonus-api
newman run postman/collection.json -e postman/environment.json --folder step2
# ...step3, step4 аналогично
```

Отдельно проверить тайминг Circuit Breaker: после `start bonus-api` в `step2` первый же запрос к
`/api/v1/privilege` должен успешно достучаться до реального сервиса — период probe (T) должен быть коротким (единицы
секунд), иначе Circuit Breaker ещё будет "закрыт для проверки" в момент прогона `step2`.

## Шаг 8. Финализация

1. Обновить `README.md` — он всё ещё упоминает `BookingHttpGateway`, которого больше нет (тянется с ЛР2).
2. Обновить `PROBLEMS.md` — зафиксировать решение по Шагу 2 (см. выше), отметить, что fallback/Circuit
   Breaker/очередь добавлены, а проверка отказоустойчивости — теперь финальный шаг job `autograding` в `ci.yml`.
3. Закоммитить, запушить `lab_03`, проверить прогон `ci.yml` в Actions — job `autograding` должен пройти все свои
   шаги, включая новые Step 1–4.
