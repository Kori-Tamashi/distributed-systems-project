using System.Threading.Channels;
using core.interfaces.dataaccess;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace dataaccess.retry;

/// <summary>
/// In-memory retry queue for non-critical operations (e.g., Bonus rollback on ticket return).
/// Uses System.Threading.Channels. Re-enqueues failed ops with a fixed delay.
/// </summary>
public class InMemoryRetryQueue : IRetryQueue, IHostedService
{
    private readonly Channel<(string Id, Func<Task> Op)> _channel =
        Channel.CreateUnbounded<(string, Func<Task>)>();

    private readonly ILogger<InMemoryRetryQueue> _logger;
    private readonly TimeSpan _retryDelay;
    private CancellationTokenSource? _cts;
    private Task? _worker;

    public InMemoryRetryQueue(ILogger<InMemoryRetryQueue> logger, TimeSpan? retryDelay = null)
    {
        _logger = logger;
        _retryDelay = retryDelay ?? TimeSpan.FromSeconds(10);
    }

    public void Enqueue(string operationId, Func<Task> operation)
    {
        if (!_channel.Writer.TryWrite((operationId, operation)))
        {
            _logger.LogError("Failed to enqueue retry operation {OperationId}", operationId);
            return;
        }
        _logger.LogInformation("Enqueued retry operation {OperationId}", operationId);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _worker = Task.Run(() => ProcessAsync(_cts.Token), _cts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _channel.Writer.TryComplete();
        _cts?.Cancel();
        if (_worker is not null)
        {
            try { await _worker.WaitAsync(cancellationToken); }
            catch (OperationCanceledException) { /* expected */ }
        }
    }

    private async Task ProcessAsync(CancellationToken ct)
    {
        try
        {
            await foreach (var (id, op) in _channel.Reader.ReadAllAsync(ct))
            {
                try
                {
                    await op();
                    _logger.LogInformation("Retry operation {OperationId} succeeded", id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Retry operation {OperationId} failed, re-enqueue in {Delay}s",
                        id, _retryDelay.TotalSeconds);

                    await Task.Delay(_retryDelay, ct);
                    _channel.Writer.TryWrite((id, op));
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // graceful shutdown
        }
    }
}
