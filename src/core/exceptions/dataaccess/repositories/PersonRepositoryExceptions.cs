namespace core.exceptions.dataaccess.repositories;

/// <summary>
/// Exception thrown when Person is not found
/// </summary>
public class PersonNotFoundException : EntityNotFoundException
{
    public int PersonId { get; }

    public PersonNotFoundException(int personId) 
        : base("Person", personId)
    {
        PersonId = personId;
    }

    public PersonNotFoundException(int personId, Exception innerException) 
        : base("Person", personId, innerException)
    {
        PersonId = personId;
    }
}

/// <summary>
/// Exception thrown when Person already exists
/// </summary>
public class PersonAlreadyExistsException : EntityAlreadyExistsException
{
    public int PersonId { get; }

    public PersonAlreadyExistsException(int personId) 
        : base("Person", personId)
    {
        PersonId = personId;
    }

    public PersonAlreadyExistsException(int personId, Exception innerException) 
        : base("Person", personId, innerException)
    {
        PersonId = personId;
    }
}

/// <summary>
/// Exception thrown when database operation fails for Person
/// </summary>
public class PersonDatabaseException : DatabaseException
{
    public PersonDatabaseException() : base() { }

    public PersonDatabaseException(string message) : base(message) { }

    public PersonDatabaseException(string message, Exception innerException) : base(message, innerException) { }
}
