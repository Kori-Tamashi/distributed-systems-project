using core.circuitbreaker;
using core.domain;
using core.exceptions.businesslogic.services;
using core.filters;
using core.interfaces.dataaccess.gateways;

namespace dataaccess.gateways.circuitbreaker;

/// <summary>
/// Circuit Breaker decorator for ITicketGateway.
/// Only wraps READ operations (GetAll/GetById); write operations pass through unchanged.
/// </summary>
public class CircuitBreakerTicketGateway : ITicketGateway
{
    private readonly ITicketGateway _inner;
    private readonly CircuitBreakerState _state;

    public CircuitBreakerTicketGateway(ITicketGateway inner, CircuitBreakerState state)
    {
        _inner = inner;
        _state = state;
    }

    public Task<IEnumerable<Ticket>> GetAllAsync()
        => WrapReadAsync(() => _inner.GetAllAsync());

    public Task<Ticket?> GetByIdAsync(int id)
        => WrapReadAsync(() => _inner.GetByIdAsync(id));

    public Task<IEnumerable<Ticket>> GetAllAsync(TicketFilter filter)
        => WrapReadAsync(() => _inner.GetAllAsync(filter));

    // Write operations — pass through, no CB wrapping
    public Task<Ticket> CreateAsync(Ticket ticket) => _inner.CreateAsync(ticket);
    public Task<Ticket> UpdateAsync(Ticket ticket) => _inner.UpdateAsync(ticket);
    public Task DeleteAsync(int id) => _inner.DeleteAsync(id);

    private async Task<T> WrapReadAsync<T>(Func<Task<T>> operation)
    {
        if (_state.ShouldShortCircuit())
            throw new ServiceUnavailableException("Ticket Service");

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
