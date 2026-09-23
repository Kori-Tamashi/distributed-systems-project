namespace presentation.dto.http.Ticket;

/// <summary>
/// DTO for updating Ticket
/// </summary>
public class UpdateTicketDTO
{
    /// <summary>
    /// Flight identifier (foreign key to Flights table) (optional)
    /// </summary>
    public int? FlightId { get; set; }

    /// <summary>
    /// Passenger full name (optional)
    /// </summary>
    public string? PassengerName { get; set; }

    /// <summary>
    /// Passenger email address (optional)
    /// </summary>
    public string? PassengerEmail { get; set; }

    /// <summary>
    /// Passenger phone number (optional)
    /// </summary>
    public string? PassengerPhone { get; set; }

    /// <summary>
    /// Seat number (e.g., "12A", "23B") (optional)
    /// </summary>
    public string? SeatNumber { get; set; }

    /// <summary>
    /// Ticket class (Economy, Business, First) (optional)
    /// </summary>
    public int? Class { get; set; }

    /// <summary>
    /// Ticket price in rubles (optional)
    /// </summary>
    public int? Price { get; set; }

    /// <summary>
    /// Booking date and time (optional)
    /// </summary>
    public DateTime? BookingDate { get; set; }

    /// <summary>
    /// Ticket status (Confirmed, Cancelled, Refunded) (optional)
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public UpdateTicketDTO()
    {
    }

    /// <summary>
    /// Constructor with all fields
    /// </summary>
    public UpdateTicketDTO(
        int? flightId = null,
        string? passengerName = null,
        string? passengerEmail = null,
        string? passengerPhone = null,
        string? seatNumber = null,
        int? @class = null,
        int? price = null,
        DateTime? bookingDate = null,
        int? status = null)
    {
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
