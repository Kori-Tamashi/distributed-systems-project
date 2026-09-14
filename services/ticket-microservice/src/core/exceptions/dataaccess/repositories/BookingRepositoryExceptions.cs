using core.exceptions;

namespace core.exceptions.dataaccess.repositories;

/// <summary>
/// Exception thrown when Booking is not found
/// </summary>
public class BookingNotFoundException : EntityNotFoundException
{
    public int BookingId { get; }

    public BookingNotFoundException(int bookingId) 
        : base("Booking", bookingId)
    {
        BookingId = bookingId;
    }

    public BookingNotFoundException(int bookingId, Exception innerException) 
        : base("Booking", bookingId, innerException)
    {
        BookingId = bookingId;
    }
}

/// <summary>
/// Exception thrown when Booking already exists
/// </summary>
public class BookingAlreadyExistsException : EntityAlreadyExistsException
{
    public int BookingId { get; }

    public BookingAlreadyExistsException(int bookingId) 
        : base("Booking", bookingId)
    {
        BookingId = bookingId;
    }

    public BookingAlreadyExistsException(int bookingId, Exception innerException) 
        : base("Booking", bookingId, innerException)
    {
        BookingId = bookingId;
    }
}

/// <summary>
/// Exception thrown when database operation fails for Booking
/// </summary>
public class BookingDatabaseException : DatabaseException
{
    public BookingDatabaseException() : base() { }

    public BookingDatabaseException(string message) : base(message) { }

    public BookingDatabaseException(string message, Exception innerException) 
        : base(message, innerException) { }
}
