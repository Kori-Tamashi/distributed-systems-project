using core.circuitbreaker;
using core.domain;
using core.exceptions.businesslogic.services;
using core.filters;
using core.interfaces.dataaccess.gateways;

namespace dataaccess.gateways.circuitbreaker;

/// <summary>
/// Circuit Breaker decorator for IFlightGateway.
/// Only wraps READ operations (GetAll/GetById); write operations pass through unchanged.
/// </summary>
public class CircuitBreakerFlightGateway : IFlightGateway
{
    private readonly IFlightGateway _inner;
    private readonly CircuitBreakerState _state;

    public CircuitBreakerFlightGateway(IFlightGateway inner, CircuitBreakerState state)
    {
        _inner = inner;
        _state = state;
    }

    public Task<IEnumerable<Flight>> GetAllAsync()
        => WrapReadAsync(() => _inner.GetAllAsync());

    public Task<Flight?> GetByIdAsync(int id)
        => WrapReadAsync(() => _inner.GetByIdAsync(id));

    public Task<IEnumerable<Flight>> GetAllAsync(FlightFilter filter)
        => WrapReadAsync(() => _inner.GetAllAsync(filter));

    // Write operations — pass through, no CB wrapping
    public Task<Flight> CreateAsync(Flight flight) => _inner.CreateAsync(flight);
    public Task<Flight> UpdateAsync(Flight flight) => _inner.UpdateAsync(flight);
    public Task DeleteAsync(int id) => _inner.DeleteAsync(id);

    private async Task<T> WrapReadAsync<T>(Func<Task<T>> operation)
    {
        if (_state.ShouldShortCircuit())
            throw new ServiceUnavailableException("Flight Service");

        try
        {
            var result = await operation();
            _state.RecordSuccess();
            return result;
        }
        catch (Exception)
        {
            _state.RecordFailure();
            throw;
        }
    }
}
