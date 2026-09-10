namespace core.interfaces.dataaccess.contexts;

/// <summary>
/// Interface for database context abstraction
/// Follows Dependency Inversion Principle - high-level modules don't depend on low-level EF Core details
/// </summary>
public interface IDatabaseContext : IDisposable
{
    /// <summary>
    /// Saves changes to the database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures the database is created
    /// </summary>
    Task EnsureDatabaseCreatedAsync();
}
