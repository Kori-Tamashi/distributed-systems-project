using core.circuitbreaker;
using core.domain;
using core.exceptions.businesslogic.services;
using core.filters;
using core.interfaces.dataaccess.gateways;

namespace dataaccess.gateways.circuitbreaker;

/// <summary>
/// Circuit Breaker decorator for IPrivilegeGateway.
/// Only wraps READ operations (GetAll/GetById); write operations pass through unchanged
/// (they are handled via SAGA rollback in business logic).
/// </summary>
public class CircuitBreakerPrivilegeGateway : IPrivilegeGateway
{
    private readonly IPrivilegeGateway _inner;
    private readonly CircuitBreakerState _state;

    public CircuitBreakerPrivilegeGateway(IPrivilegeGateway inner, CircuitBreakerState state)
    {
        _inner = inner;
        _state = state;
    }

    public Task<IEnumerable<Privilege>> GetAllAsync()
        => WrapReadAsync(() => _inner.GetAllAsync());

    public Task<Privilege?> GetByIdAsync(int id)
        => WrapReadAsync(() => _inner.GetByIdAsync(id));

    public Task<IEnumerable<Privilege>> GetAllAsync(PrivilegeFilter filter)
        => WrapReadAsync(() => _inner.GetAllAsync(filter));

    // Write operations — pass through, no CB wrapping
    public Task<Privilege> CreateAsync(Privilege privilege) => _inner.CreateAsync(privilege);
    public Task<Privilege> UpdateAsync(Privilege privilege) => _inner.UpdateAsync(privilege);
    public Task DeleteAsync(int id) => _inner.DeleteAsync(id);

    private async Task<T> WrapReadAsync<T>(Func<Task<T>> operation)
    {
        if (_state.ShouldShortCircuit())
            throw new ServiceUnavailableException("Bonus Service");

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
