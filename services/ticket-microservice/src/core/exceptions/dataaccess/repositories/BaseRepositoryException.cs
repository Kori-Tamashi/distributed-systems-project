using core.exceptions;

namespace core.exceptions.dataaccess.repositories;

/// <summary>
/// Base exception for all repository operations
/// </summary>
public class BaseRepositoryException : BaseException
{
    public BaseRepositoryException() : base() { }

    public BaseRepositoryException(string message) : base(message) { }

    public BaseRepositoryException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Base exception for entity not found scenarios
/// </summary>
public class EntityNotFoundException : BaseRepositoryException
{
    public string EntityName { get; }
    public object? Id { get; }

    public EntityNotFoundException(string entityName, object? id) 
        : base($"{entityName} with id {id} was not found")
    {
        EntityName = entityName;
        Id = id;
    }

    public EntityNotFoundException(string entityName, object? id, Exception innerException) 
        : base($"{entityName} with id {id} was not found", innerException)
    {
        EntityName = entityName;
        Id = id;
    }
}

/// <summary>
/// Base exception for entity already exists scenarios
/// </summary>
public class EntityAlreadyExistsException : BaseRepositoryException
{
    public string EntityName { get; }
    public object? Id { get; }

    public EntityAlreadyExistsException(string entityName, object? id) 
        : base($"{entityName} with id {id} already exists")
    {
        EntityName = entityName;
        Id = id;
    }

    public EntityAlreadyExistsException(string entityName, object? id, Exception innerException) 
        : base($"{entityName} with id {id} already exists", innerException)
    {
        EntityName = entityName;
        Id = id;
    }
}

/// <summary>
/// Base exception for database operation failures
/// </summary>
public class DatabaseException : BaseRepositoryException
{
    public DatabaseException() : base() { }

    public DatabaseException(string message) : base(message) { }

    public DatabaseException(string message, Exception innerException) : base(message, innerException) { }
}
