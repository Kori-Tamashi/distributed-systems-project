using core.exceptions;

namespace core.exceptions.dataaccess.repositories;

/// <summary>
/// Exception thrown when Privilege is not found
/// </summary>
public class PrivilegeNotFoundException : EntityNotFoundException
{
    public int PrivilegeId { get; }

    public PrivilegeNotFoundException(int privilegeId) 
        : base("Privilege", privilegeId)
    {
        PrivilegeId = privilegeId;
    }

    public PrivilegeNotFoundException(int privilegeId, Exception innerException) 
        : base("Privilege", privilegeId, innerException)
    {
        PrivilegeId = privilegeId;
    }
}

/// <summary>
/// Exception thrown when Privilege already exists
/// </summary>
public class PrivilegeAlreadyExistsException : EntityAlreadyExistsException
{
    public int PrivilegeId { get; }

    public PrivilegeAlreadyExistsException(int privilegeId) 
        : base("Privilege", privilegeId)
    {
        PrivilegeId = privilegeId;
    }

    public PrivilegeAlreadyExistsException(int privilegeId, Exception innerException) 
        : base("Privilege", privilegeId, innerException)
    {
        PrivilegeId = privilegeId;
    }
}

/// <summary>
/// Exception thrown when database operation fails for Privilege
/// </summary>
public class PrivilegeDatabaseException : DatabaseException
{
    public PrivilegeDatabaseException() : base() { }

    public PrivilegeDatabaseException(string message) : base(message) { }

    public PrivilegeDatabaseException(string message, Exception innerException) 
        : base(message, innerException) { }
}
