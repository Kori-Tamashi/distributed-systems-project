using System;

namespace presentation.dto.http;

/// <summary>
/// DTO for creating a new Ticket
/// </summary>
public class CreateTicketDTO : BaseHttpDTO
{
    /// <summary>
    /// Unique identifier for the ticket (GUID)
    /// </summary>
    public Guid TicketUid { get; set; }

    /// <summary>
    /// Flight ID
    /// </summary>
    public int FlightId { get; set; }

    /// <summary>
    /// Passenger name
    /// </summary>
    public string PassengerName { get; set; } = string.Empty;

    /// <summary>
    /// Passenger email
    /// </summary>
    public string PassengerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Passenger phone
    /// </summary>
    public string PassengerPhone { get; set; } = string.Empty;

    /// <summary>
    /// Seat number
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
    /// Booking date and time (UTC)
    /// </summary>
    public DateTime BookingDate { get; set; }

    /// <summary>
    /// Ticket status
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public CreateTicketDTO()
        : base(0)
    {
    }

    /// <summary>
    /// Constructor with all required fields
    /// </summary>
    public CreateTicketDTO(
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
        : base(0)
    {
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
