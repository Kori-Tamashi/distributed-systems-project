using core.exceptions;

namespace core.exceptions.dataaccess.repositories;

/// <summary>
/// Exception thrown when PrivilegeHistory is not found
/// </summary>
public class PrivilegeHistoryNotFoundException : EntityNotFoundException
{
    public int HistoryId { get; }

    public PrivilegeHistoryNotFoundException(int historyId) 
        : base("PrivilegeHistory", historyId)
    {
        HistoryId = historyId;
    }

    public PrivilegeHistoryNotFoundException(int historyId, Exception innerException) 
        : base("PrivilegeHistory", historyId, innerException)
    {
        HistoryId = historyId;
    }
}

/// <summary>
/// Exception thrown when PrivilegeHistory already exists
/// </summary>
public class PrivilegeHistoryAlreadyExistsException : EntityAlreadyExistsException
{
    public int HistoryId { get; }

    public PrivilegeHistoryAlreadyExistsException(int historyId) 
        : base("PrivilegeHistory", historyId)
    {
        HistoryId = historyId;
    }

    public PrivilegeHistoryAlreadyExistsException(int historyId, Exception innerException) 
        : base("PrivilegeHistory", historyId, innerException)
    {
        HistoryId = historyId;
    }
}

/// <summary>
/// Exception thrown when database operation fails for PrivilegeHistory
/// </summary>
public class PrivilegeHistoryDatabaseException : DatabaseException
{
    public PrivilegeHistoryDatabaseException() : base() { }

    public PrivilegeHistoryDatabaseException(string message) : base(message) { }

    public PrivilegeHistoryDatabaseException(string message, Exception innerException) 
        : base(message, innerException) { }
}
