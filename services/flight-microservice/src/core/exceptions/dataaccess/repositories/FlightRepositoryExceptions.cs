using core.exceptions;

namespace core.exceptions.dataaccess.repositories;

/// <summary>
/// Exception thrown when Flight is not found
/// </summary>
public class FlightNotFoundException : EntityNotFoundException
{
    public int FlightId { get; }

    public FlightNotFoundException(int flightId) 
        : base("Flight", flightId)
    {
        FlightId = flightId;
    }

    public FlightNotFoundException(int flightId, Exception innerException) 
        : base("Flight", flightId, innerException)
    {
        FlightId = flightId;
    }
}

/// <summary>
/// Exception thrown when Flight already exists
/// </summary>
public class FlightAlreadyExistsException : EntityAlreadyExistsException
{
    public int FlightId { get; }

    public FlightAlreadyExistsException(int flightId) 
        : base("Flight", flightId)
    {
        FlightId = flightId;
    }

    public FlightAlreadyExistsException(int flightId, Exception innerException) 
        : base("Flight", flightId, innerException)
    {
        FlightId = flightId;
    }
}

/// <summary>
/// Exception thrown when database operation fails for Flight
/// </summary>
public class FlightDatabaseException : DatabaseException
{
    public FlightDatabaseException() : base() { }

    public FlightDatabaseException(string message) : base(message) { }

    public FlightDatabaseException(string message, Exception innerException) 
        : base(message, innerException) { }
}
