# План реализации Лабораторной работы #3 — Fault Tolerance

## Вариант: Flight Booking System

**Сервисы:**
- Gateway Service (:8080)
- Ticket Service (:8070)
- Flight Service (:8060)
- Bonus Service (:8050)

---

## 📋 Обзор изменений

| Компонент | ЛР2 | ЛР3 |
|-----------|-----|-----|
| Circuit Breaker | ❌ Нет | ✅ Да (все GET операции) |
| Fallback ответы | ❌ Нет | ✅ Да (некритичные данные) |
| Очередь для повторов | ❌ Нет | ✅ Да (Bonus Service) |
| Health check | `/health` | `/manage/health` |

---

## 🏗️ Архитектура

```
┌─────────────────────────────────────────────────────────────────┐
│                     Gateway Service (:8080)                     │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                  Circuit Breaker Layer                     │  │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐       │  │
│  │  │   Ticket    │  │   Flight    │  │   Bonus     │       │  │
│  │  │  Breaker    │  │  Breaker    │  │  Breaker    │       │  │
│  │  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘       │  │
│  └─────────┼────────────────┼────────────────┼──────────────┘  │
│            │                │                │                  │
│  ┌─────────▼────────────────▼────────────────▼──────────────┐  │
│  │                    Fallback Handler                       │  │
│  │  - Empty objects for non-critical data                   │  │
│  │  - Default values (null, 0, empty arrays)                │  │
│  └───────────────────────────┬──────────────────────────────┘  │
│                              │                                  │
│  ┌───────────────────────────▼──────────────────────────────┐  │
│  │                    Retry Queue Worker                     │  │
│  │  - BlockingQueue<RefundCommand>                          │  │
│  │  - BackgroundService for retry logic                     │  │
│  │  - Timeout: 10 seconds                                   │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📁 Структура новых файлов

```
services/gateway-microservice/src/
├── businesslogic/
│   ├── services/
│   │   ├── CircuitBreakerService.cs          # NEW: Circuit Breaker implementation
│   │   └── ICircuitBreakerService.cs         # NEW: Interface
│   └── queues/
│       ├── IRetryQueue.cs                    # NEW: Queue interface
│       ├── RefundQueue.cs                    # NEW: Refund retry queue
│       └── RefundCommand.cs                  # NEW: Queue command DTO
├── background/
│   └──services/
│       └── RetryQueueWorker.cs               # NEW: Background worker
├── presentation/
│   ├── converters/http/
│   │   └── FallbackConverters.cs             # NEW: Fallback DTO converters
│   └── dto/http/
│       └── Fallback/
│           ├── FlightFallbackDTO.cs          # NEW: Fallback for Flight Service
│           └── PrivilegeFallbackDTO.cs       # NEW: Fallback for Bonus Service
└── Program.cs                                # MODIFY: Register services
```

---

## 🚀 Этапы реализации

### Этап 1: Circuit Breaker Service

**Файл:** `services/gateway-microservice/src/businesslogic/services/CircuitBreakerService.cs`

```csharp
namespace businesslogic.services;

public enum CircuitState
{
    Closed,    // Нормальная работа
    Open,      // Fallback режим
    HalfOpen   // Проверка доступности
}

public interface ICircuitBreakerService
{
    Task<T> ExecuteAsync<T>(
        string serviceName,
        Func<Task<T>> action,
        Func<T> fallback);
    
    CircuitState GetState(string serviceName);
    int GetFailureCount(string serviceName);
}

public class CircuitBreakerService : ICircuitBreakerService
{
    private class CircuitStateContainer
    {
        public CircuitState State { get; set; } = CircuitState.Closed;
        public int FailureCount { get; set; } = 0;
        public DateTime? LastFailureTime { get; set; }
    }
    
    private readonly Dictionary<string, CircuitStateContainer> _circuits = new();
    private readonly int _failureThreshold = 5;
    private readonly TimeSpan _resetTimeout = TimeSpan.FromSeconds(30);
    private readonly ILogger<CircuitBreakerService> _logger;
    
    public async Task<T> ExecuteAsync<T>(
        string serviceName,
        Func<Task<T>> action,
        Func<T> fallback)
    {
        var container = GetOrCreateCircuit(serviceName);
        
        // Проверка Open режима
        if (container.State == CircuitState.Open)
        {
            if (DateTime.Now - container.LastFailureTime >= _resetTimeout)
            {
                _logger.LogInformation($"Circuit {serviceName}: Open → Half-Open");
                container.State = CircuitState.HalfOpen;
            }
            else
            {
                _logger.LogWarning($"Circuit {serviceName}: Open - returning fallback");
                return fallback();
            }
        }
        
        try
        {
            var result = await action();
            OnSuccess(container);
            return result;
        }
        catch (Exception ex)
        {
            OnFailure(container);
            _logger.LogError(ex, $"Circuit {serviceName}: Failure #{container.FailureCount}");
            return fallback();
        }
    }
    
    private void OnSuccess(CircuitStateContainer container)
    {
        container.FailureCount = 0;
        if (container.State == CircuitState.HalfOpen)
        {
            container.State = CircuitState.Closed;
            _logger.LogInformation("Circuit closed: service recovered");
        }
    }
    
    private void OnFailure(CircuitStateContainer container)
    {
        container.FailureCount++;
        container.LastFailureTime = DateTime.Now;
        
        if (container.FailureCount >= _failureThreshold && container.State != CircuitState.Open)
        {
            container.State = CircuitState.Open;
            _logger.LogWarning($"Circuit OPENED after {container.FailureCount} failures");
        }
    }
    
    private CircuitStateContainer GetOrCreateCircuit(string serviceName)
    {
        return _circuits.GetOrAdd(serviceName, _ => new CircuitStateContainer());
    }
    
    public CircuitState GetState(string serviceName)
    {
        return GetOrCreateCircuit(serviceName).State;
    }
    
    public int GetFailureCount(string serviceName)
    {
        return GetOrCreateCircuit(serviceName).FailureCount;
    }
}
```

---

### Этап 2: Fallback DTOs

**Файл:** `services/gateway-microservice/src/presentation/dto/http/Fallback/FlightFallbackDTO.cs`

```csharp
namespace presentation.dto.http.Fallback;

/// <summary>
/// Fallback DTO для Flight Service (некритичные данные)
/// Возвращается при недоступности Flight Service в методах:
/// - GET /api/v1/tickets
/// - GET /api/v1/tickets/{ticketUid}
/// </summary>
public class FlightFallbackDTO
{
    public int FlightId { get; set; }  // Всегда заполняем для связи
    public string? FromAirport { get; set; } = null;
    public string? ToAirport { get; set; } = null;
    public DateTime? Date { get; set; } = null;
}
```

**Файл:** `services/gateway-microservice/src/presentation/dto/http/Fallback/PrivilegeFallbackDTO.cs`

```csharp
namespace presentation.dto.http.Fallback;

/// <summary>
/// Fallback DTO для Bonus Service (некритичные данные)
/// Возвращается при недоступности Bonus Service в методах:
/// - GET /api/v1/me
/// </summary>
public class PrivilegeFallbackDTO
{
    public string Username { get; set; } = string.Empty;
    public string Status { get; set; } = "BRONZE";  // Default value
    public int Balance { get; set; } = 0;
    public int Discount { get; set; } = 0;
}
```

---

### Этап 3: Retry Queue

**Файл:** `services/gateway-microservice/src/businesslogic/queues/RefundCommand.cs`

```csharp
namespace businesslogic.queues;

public class RefundCommand
{
    public int TicketId { get; set; }
    public int BonusAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int RetryCount { get; set; } = 0;
}
```

**Файл:** `services/gateway-microservice/src/businesslogic/queues/IRetryQueue.cs`

```csharp
namespace businesslogic.queues;

public interface IRetryQueue
{
    void Enqueue(RefundCommand command);
    Task<RefundCommand?> DequeueAsync(CancellationToken cancellationToken);
    int Count { get; }
}
```

**Файл:** `services/gateway-microservice/src/businesslogic/queues/RefundQueue.cs`

```csharp
namespace businesslogic.queues;

public class RefundQueue : IRetryQueue
{
    private readonly BlockingCollection<RefundCommand> _queue = 
        new(new ConcurrentQueue<RefundCommand>());
    private readonly ILogger<RefundQueue> _logger;
    
    public int Count => _queue.Count;
    
    public void Enqueue(RefundCommand command)
    {
        _queue.Add(command);
        _logger.LogInformation($"Refund command queued: TicketId={command.TicketId}");
    }
    
    public async Task<RefundCommand?> DequeueAsync(CancellationToken cancellationToken)
    {
        try
        {
            var task = Task.Factory.StartNew(
                () => _queue.Take(),
                cancellationToken,
                TaskCreationOptions.None,
                TaskScheduler.Default);
            
            return await task;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }
}
```

---

### Этап 4: Background Worker

**Файл:** `services/gateway-microservice/src/background/services/RetryQueueWorker.cs`

```csharp
namespace background.services;

public class RetryQueueWorker : BackgroundService
{
    private readonly IRetryQueue _queue;
    private readonly IBonusService _bonusService;
    private readonly ILogger<RetryQueueWorker> _logger;
    private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(2);
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);
    
    public RetryQueueWorker(
        IRetryQueue queue,
        IBonusService bonusService,
        ILogger<RetryQueueWorker> logger)
    {
        _queue = queue;
        _bonusService = bonusService;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RetryQueueWorker started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var command = await _queue.DequeueAsync(stoppingToken);
                
                if (command == null)
                {
                    await Task.Delay(100, stoppingToken);
                    continue;
                }
                
                await ProcessRefund(command, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("RetryQueueWorker stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RetryQueueWorker");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
    
    private async Task ProcessRefund(RefundCommand command, CancellationToken cancellationToken)
    {
        var startTime = DateTime.Now;
        
        while (DateTime.Now - startTime < _timeout)
        {
            try
            {
                await _bonusService.RefundPointsAsync(command.TicketId, command.BonusAmount);
                _logger.LogInformation($"Refund successful: TicketId={command.TicketId}");
                return;
            }
            catch (Exception ex)
            {
                command.RetryCount++;
                _logger.LogWarning(
                    ex, 
                    $"Refund retry {command.RetryCount}: TicketId={command.TicketId}");
                
                await Task.Delay(_retryDelay, cancellationToken);
            }
        }
        
        _logger.LogError(
            $"Refund timeout after {command.RetryCount} retries: TicketId={command.TicketId}");
    }
}
```

---

### Этап 5: Обновление Контроллеров

**Файл:** `services/gateway-microservice/src/presentation/controllers/http/TicketHttpController.cs`

```csharp
public class TicketHttpController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly IFlightService _flightService;
    private readonly ICircuitBreakerService _circuitBreaker;
    
    // GET /api/v1/tickets
    public async Task<ActionResult<TicketsListDTO>> GetAllTicketsAsync()
    {
        var tickets = await _ticketService.GetAllTicketsAsync();
        
        var result = new List<TicketDTO>();
        foreach (var ticket in tickets)
        {
            // Circuit Breaker для Flight Service (некритичный)
            var flight = await _circuitBreaker.ExecuteAsync(
                () => _flightService.GetFlightAsync(ticket.FlightId),
                () => new FlightFallbackDTO { FlightId = ticket.FlightId }  // Fallback
            );
            
            result.Add(new TicketDTO
            {
                TicketUid = ticket.TicketUid,
                FlightId = ticket.FlightId,
                FromAirport = flight.FromAirport,  // Может быть null
                ToAirport = flight.ToAirport,
                Date = flight.Date,
                // ... остальные поля
            });
        }
        
        return Ok(new TicketsListDTO { Items = result });
    }
}
```

**Файл:** `services/gateway-microservice/src/presentation/controllers/http/BookingHttpController.cs`

```csharp
public class BookingHttpController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IFlightService _flightService;
    private readonly ITicketService _ticketService;
    private readonly IBonusService _bonusService;
    private readonly IRetryQueue _refundQueue;
    
    // POST /api/v1/tickets (Покупка билета)
    public async Task<ActionResult<TicketDTO>> BuyTicketAsync(BuyTicketDTO dto)
    {
        // 1. Проверка рейса (критичный)
        var flight = await _flightService.GetFlightByNumberAsync(dto.FlightNumber);
        if (flight == null)
            return NotFound(new { Message = "Flight not found" });
        
        // 2. Создание билета (критичный)
        var ticket = await _ticketService.CreateTicketAsync(dto);
        
        // 3. Бонусы (некритичный с откатом)
        try
        {
            if (dto.PaidFromBalance)
            {
                await _bonusService.DebitPointsAsync(ticket.PassengerName, dto.Price);
            }
            else
            {
                await _bonusService.CreditPointsAsync(ticket.PassengerName, dto.Price * 0.1);
            }
        }
        catch (Exception ex)
        {
            // Откат: удаляем билет
            await _ticketService.DeleteTicketAsync(ticket.TicketUid);
            
            return StatusCode(502, new 
            { 
                Message = "Ticket created but bonus service unavailable. Please try again.",
                TicketUid = ticket.TicketUid 
            });
        }
        
        return Created($"/api/v1/tickets/{ticket.TicketUid}", ticket);
    }
    
    // DELETE /api/v1/tickets/{ticketUid} (Возврат билета)
    public async Task<ActionResult> CancelTicketAsync(string ticketUid)
    {
        // 1. Отмена билета (критичный)
        await _ticketService.CancelTicketAsync(ticketUid);
        
        // 2. Возврат бонусов (некритичный с очередью)
        try
        {
            await _bonusService.RefundPointsForTicketAsync(ticketUid);
        }
        catch (Exception ex)
        {
            // Ставим в очередь, возвращаем успех
            var command = new RefundCommand
            {
                TicketId = ticketUid,
                BonusAmount = await GetTicketBonusAmount(ticketUid)
            };
            _refundQueue.Enqueue(command);
            
            _logger.LogWarning($"Refund queued for ticket {ticketUid}");
        }
        
        return Ok(new { Message = "Ticket canceled successfully" });
    }
}
```

---

### Этап 6: Регистрация в Program.cs

**Файл:** `services/gateway-microservice/src/presentation/Program.cs`

```csharp
// Load .env file
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// ... existing DI setup ...

// Circuit Breaker
builder.Services.AddSingleton<ICircuitBreakerService, CircuitBreakerService>();

// Retry Queue
builder.Services.AddSingleton<IRetryQueue, RefundQueue>();

// Background Worker
builder.Services.AddHostedService<RetryQueueWorker>();

// Health check endpoint (из /health в /manage/health)
app.MapHealthChecks("/manage/health");

// ... rest of configuration ...
```

---

## ✅ Чеклист тестирования

### Тест 1: Circuit Breaker работает

```bash
# 1. Запустить все сервисы
docker-compose up -d

# 2. Сделать 5+ запросов к упавшему сервису
for i in {1..10}; do
  curl http://localhost:8080/api/v1/tickets
done

# 3. Проверить, что Circuit Breaker открылся
# (последние запросы должны возвращать fallback быстро)
```

### Тест 2: Fallback работает

```bash
# 1. Остановить Flight Service
docker-compose stop flight-service

# 2. Сделать запрос к /tickets
curl http://localhost:8080/api/v1/tickets

# 3. Проверить, что:
# - Статус: 200 OK
# - Поля fromAirport, toAirport, date: null
```

### Тест 3: Очередь работает

```bash
# 1. Остановить Bonus Service
docker-compose stop bonus-service

# 2. Сделать возврат билета
curl -X DELETE http://localhost:8080/api/v1/tickets/{ticketUid}

# 3. Проверить:
# - Статус: 200 OK (не ошибка!)
# - В логах: "Refund queued for ticket ..."

# 4. Запустить Bonus Service
docker-compose start bonus-service

# 5. Проверить, что refund выполнился
```

---

## 📊 Ожидаемые результаты

| Сценарий | ЛР2 | ЛР3 (ожидаемое) |
|----------|-----|-----------------|
| Flight Service упал, GET /tickets | 500 Error | ✅ 200 OK + null поля |
| Bonus Service упал, покупка билета | 500 Error | ✅ Откат + 502 Error |
| Bonus Service упал, возврат билета | 500 Error | ✅ 200 OK + очередь |
| Сервис восстановился | Ручной рестарт | ✅ Автоматически |

---

## 🚨 Известные проблемы и решения

| Проблема | Причина | Решение |
|----------|---------|---------|
| Circuit Breaker не сбрасывается | Неверный timeout | Проверить `_resetTimeout` |
| Очередь не обрабатывает команды | Worker не запущен | Проверить `AddHostedService` |
| Fallback возвращает null вместо объекта | Неверный тип | Проверить `Func<T> fallback` |

---

## 📚 Ресурсы

- [Circuit Breaker Pattern](https://microservices.io/patterns/reliability/circuit-breaker.html)
- [SAGA Pattern](https://microservices.io/patterns/data/saga.html)
- [BackgroundService Docs](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.backgroundservice)
- [BlockingCollection Docs](https://docs.microsoft.com/en-us/dotnet/api/system.collections.concurrent.blockingcollection-1)

---

**Дата начала:** 2024-01-XX
**Ожидаемая дата завершения:** 2024-01-XX + 7 дней
