namespace core.interfaces.dataaccess.contexts;

/// <summary>
/// Interface for database context operations
/// Provides abstraction over database connection and transaction management
/// </summary>
public interface IDatabaseContext : IDisposable
{
    /// <summary>
    /// Gets the database connection string
    /// </summary>
    string ConnectionString { get; }

    /// <summary>
    /// Opens the database connection
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    Task OpenAsync();

    /// <summary>
    /// Closes the database connection
    /// </summary>
    void Close();

    /// <summary>
    /// Begins a database transaction
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    Task BeginTransactionAsync();

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    Task CommitTransactionAsync();

    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    Task RollbackTransactionAsync();
}
