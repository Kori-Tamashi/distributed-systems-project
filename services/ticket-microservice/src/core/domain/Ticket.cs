using core.enums;

using System;

namespace core.domain;

/// <summary>
/// Domain entity representing a Ticket
/// </summary>
public class Ticket
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public int Id { get; set; }

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
    public TicketClass Class { get; set; }

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
    public TicketStatus Status { get; set; }

    /// <summary>
    /// Timestamp when the ticket was created (UTC)
    /// </summary>

    /// <summary>
    /// Timestamp when the ticket was last updated (UTC)
    /// </summary>
}
