using core.exceptions;

namespace core.exceptions.dataaccess.repositories;

/// <summary>
/// Exception thrown when Ticket is not found
/// </summary>
public class TicketNotFoundException : EntityNotFoundException
{
    public int TicketId { get; }

    public TicketNotFoundException(int ticketId) 
        : base("Ticket", ticketId)
    {
        TicketId = ticketId;
    }

    public TicketNotFoundException(int ticketId, Exception innerException) 
        : base("Ticket", ticketId, innerException)
    {
        TicketId = ticketId;
    }
}

/// <summary>
/// Exception thrown when Ticket already exists
/// </summary>
public class TicketAlreadyExistsException : EntityAlreadyExistsException
{
    public int TicketId { get; }

    public TicketAlreadyExistsException(int ticketId) 
        : base("Ticket", ticketId)
    {
        TicketId = ticketId;
    }

    public TicketAlreadyExistsException(int ticketId, Exception innerException) 
        : base("Ticket", ticketId, innerException)
    {
        TicketId = ticketId;
    }
}

/// <summary>
/// Exception thrown when database operation fails for Ticket
/// </summary>
public class TicketDatabaseException : DatabaseException
{
    public TicketDatabaseException() : base() { }

    public TicketDatabaseException(string message) : base(message) { }

    public TicketDatabaseException(string message, Exception innerException) 
        : base(message, innerException) { }
}
