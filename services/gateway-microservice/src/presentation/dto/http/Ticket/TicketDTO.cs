using presentation.dto.http;

namespace presentation.dto.http.Ticket;

/// <summary>
/// DTO for reading Ticket data (full representation)
/// </summary>
public class TicketDTO : BaseHttpDTO
{
    /// <summary>
    /// Unique ticket UID (UUID)
    /// </summary>
    public Guid TicketUid { get; set; }

    /// <summary>
    /// Flight identifier (foreign key to Flights table)
    /// </summary>
    public int FlightId { get; set; }

    /// <summary>
    /// Passenger full name
    /// </summary>
    public string PassengerName { get; set; } = string.Empty;

    /// <summary>
    /// Passenger email address
    /// </summary>
    public string PassengerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Passenger phone number
    /// </summary>
    public string PassengerPhone { get; set; } = string.Empty;

    /// <summary>
    /// Seat number (e.g., "12A", "23B")
    /// </summary>
    public string SeatNumber { get; set; } = string.Empty;

    /// <summary>
    /// Ticket class (Economy, Business, First)
    /// </summary>
    public int Class { get; set; }

    /// <summary>
    /// Ticket price in rubles
    /// </summary>
    public int Price { get; set; }

    /// <summary>
    /// Booking date and time
    /// </summary>
    public DateTime BookingDate { get; set; }

    /// <summary>
    /// Ticket status (Confirmed, Cancelled, Refunded)
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public TicketDTO()
        : base(0)
    {
    }

    /// <summary>
    /// Constructor with all fields
    /// </summary>
    public TicketDTO(
        int id,
        Guid ticketUid,
        int flightId,
        string passengerName,
        string passengerEmail,
        string passengerPhone,
        string seatNumber,
        int @class,
        int price,
        DateTime bookingDate,
        int status)
        : base(id)
    {
        Id = id;
        TicketUid = ticketUid;
        FlightId = flightId;
        PassengerName = passengerName;
        PassengerEmail = passengerEmail;
        PassengerPhone = passengerPhone;
        SeatNumber = seatNumber;
        Class = @class;
        Price = price;
        BookingDate = bookingDate;
        Status = status;
    }
}
