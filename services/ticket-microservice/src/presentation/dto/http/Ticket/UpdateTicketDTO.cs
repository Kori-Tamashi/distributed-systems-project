using System;

namespace presentation.dto.http.Ticket;

/// <summary>
/// DTO for updating an existing Ticket
/// All properties are nullable for partial updates
/// </summary>
public class UpdateTicketDTO : BaseHttpDTO
{
    /// <summary>
    /// Ticket number (e.g., "TK100", "TK200")
    /// </summary>
    public string? TicketNumber { get; set; }

    /// <summary>
    /// Unique identifier for the ticket (GUID)
    /// </summary>
    public Guid? TicketUid { get; set; }

    /// <summary>
    /// Flight ID
    /// </summary>
    public int? FlightId { get; set; }

    /// <summary>
    /// Passenger name
    /// </summary>
    public string? PassengerName { get; set; }

    /// <summary>
    /// Passenger email
    /// </summary>
    public string? PassengerEmail { get; set; }

    /// <summary>
    /// Passenger phone
    /// </summary>
    public string? PassengerPhone { get; set; }

    /// <summary>
    /// Seat number
    /// </summary>
    public string? SeatNumber { get; set; }

    /// <summary>
    /// Ticket class (Economy, Business, First)
    /// </summary>
    public int? Class { get; set; }

    /// <summary>
    /// Ticket price in rubles
    /// </summary>
    public int? Price { get; set; }

    /// <summary>
    /// Booking date and time (UTC)
    /// </summary>
    public DateTime? BookingDate { get; set; }

    /// <summary>
    /// Ticket status
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public UpdateTicketDTO()
        : base(0)
    {
    }

    /// <summary>
    /// Constructor with ID
    /// </summary>
    /// <param name="id">Ticket identifier</param>
    public UpdateTicketDTO(int id)
        : base(id)
    {
    }
}
