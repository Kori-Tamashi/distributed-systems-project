namespace core.circuitbreaker;

/// <summary>
/// Circuit breaker states for downstream service communication.
/// </summary>
public enum CircuitState
{
    /// <summary>
    /// Normal operation - requests pass through
    /// </summary>
    Closed,

    /// <summary>
    /// Service is failing - requests are short-circuited immediately
    /// </summary>
    Open,

    /// <summary>
    /// Probing - allowing one request through to test if service recovered
    /// </summary>
    HalfOpen
}

/// <summary>
/// In-memory Circuit Breaker state for one downstream service.
/// Thread-safe. One instance per downstream service (singleton).
/// </summary>
public class CircuitBreakerState
{
    private readonly int _failureThreshold;
    private readonly TimeSpan _probeInterval;
    private readonly object _lock = new();

    private int _consecutiveFailures;
    private DateTime _openedAt;
    private CircuitState _state = CircuitState.Closed;

    public CircuitBreakerState(int failureThreshold = 3, TimeSpan? probeInterval = null)
    {
        _failureThreshold = failureThreshold;
        _probeInterval = probeInterval ?? TimeSpan.FromSeconds(5);
    }

    public CircuitState State
    {
        get { lock (_lock) return _state; }
    }

    /// <summary>
    /// Returns true if the circuit is open and we should short-circuit.
    /// If we're due for a probe, moves to HalfOpen and returns false (allow one probe).
    /// </summary>
    public bool ShouldShortCircuit()
    {
        lock (_lock)
        {
            if (_state == CircuitState.Closed) return false;
            if (_state == CircuitState.Open)
            {
                if (DateTime.UtcNow - _openedAt >= _probeInterval)
                {
                    _state = CircuitState.HalfOpen;
                    return false; // allow one probe
                }
                return true; // still open, short-circuit
            }
            // HalfOpen: allow one request through (probe)
            return false;
        }
    }

    public void RecordSuccess()
    {
        lock (_lock)
        {
            _consecutiveFailures = 0;
            _state = CircuitState.Closed;
        }
    }

    public void RecordFailure()
    {
        lock (_lock)
        {
            _consecutiveFailures++;
            if (_state == CircuitState.HalfOpen || _consecutiveFailures >= _failureThreshold)
            {
                _state = CircuitState.Open;
                _openedAt = DateTime.UtcNow;
            }
        }
    }
}
