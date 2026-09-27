using core.circuitbreaker;
using core.domain;
using core.exceptions.businesslogic.services;
using core.filters;
using core.interfaces.dataaccess.gateways;

namespace dataaccess.gateways.circuitbreaker;

/// <summary>
/// Circuit Breaker decorator for IAirportGateway.
/// Only wraps READ operations (GetAll/GetById); write operations pass through unchanged.
/// </summary>
public class CircuitBreakerAirportGateway : IAirportGateway
{
    private readonly IAirportGateway _inner;
    private readonly CircuitBreakerState _state;

    public CircuitBreakerAirportGateway(IAirportGateway inner, CircuitBreakerState state)
    {
        _inner = inner;
        _state = state;
    }

    public Task<IEnumerable<Airport>> GetAllAsync()
        => WrapReadAsync(() => _inner.GetAllAsync());

    public Task<Airport?> GetByIdAsync(int id)
        => WrapReadAsync(() => _inner.GetByIdAsync(id));

    public Task<IEnumerable<Airport>> GetAllAsync(AirportFilter filter)
        => WrapReadAsync(() => _inner.GetAllAsync(filter));

    // Write operations — pass through, no CB wrapping
    public Task<Airport> CreateAsync(Airport airport) => _inner.CreateAsync(airport);
    public Task<Airport> UpdateAsync(Airport airport) => _inner.UpdateAsync(airport);
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
