namespace core.interfaces.dataaccess;

/// <summary>
/// In-memory retry queue for operations that must eventually succeed.
/// Used for non-critical operations (e.g., Bonus rollback on ticket return).
/// </summary>
public interface IRetryQueue
{
    /// <summary>
    /// Enqueue an operation to be retried later.
    /// </summary>
    /// <param name="operationId">Unique identifier for the operation</param>
    /// <param name="operation">Async operation to retry</param>
    void Enqueue(string operationId, Func<Task> operation);
}
