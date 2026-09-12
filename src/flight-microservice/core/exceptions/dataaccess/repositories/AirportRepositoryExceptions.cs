using core.exceptions;

namespace core.exceptions.dataaccess.repositories;

/// <summary>
/// Exception thrown when Airport is not found
/// </summary>
public class AirportNotFoundException : EntityNotFoundException
{
    public int AirportId { get; }

    public AirportNotFoundException(int airportId) 
        : base("Airport", airportId)
    {
        AirportId = airportId;
    }

    public AirportNotFoundException(int airportId, Exception innerException) 
        : base("Airport", airportId, innerException)
    {
        AirportId = airportId;
    }
}

/// <summary>
/// Exception thrown when Airport already exists
/// </summary>
public class AirportAlreadyExistsException : EntityAlreadyExistsException
{
    public int AirportId { get; }

    public AirportAlreadyExistsException(int airportId) 
        : base("Airport", airportId)
    {
        AirportId = airportId;
    }

    public AirportAlreadyExistsException(int airportId, Exception innerException) 
        : base("Airport", airportId, innerException)
    {
        AirportId = airportId;
    }
}

/// <summary>
/// Exception thrown when database operation fails for Airport
/// </summary>
public class AirportDatabaseException : DatabaseException
{
    public AirportDatabaseException() : base() { }

    public AirportDatabaseException(string message) : base(message) { }

    public AirportDatabaseException(string message, Exception innerException) 
        : base(message, innerException) { }
}
