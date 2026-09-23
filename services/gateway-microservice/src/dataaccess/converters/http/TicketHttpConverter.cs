using dataaccess.dto.http;
using dataaccess.dto.http.Ticket;

namespace dataaccess.converters.http;

/// <summary>
/// Converter for Ticket between domain and HTTP DTO representations
/// </summary>
public static class TicketHttpConverter
{
    /// <summary>
    /// Converts domain Ticket to TicketDTO
    /// </summary>
    public static TicketDTO ToDTO(core.domain.Ticket ticket)
    {
        return new TicketDTO
        {
            Id = ticket.Id,
            TicketUid = ticket.TicketUid,
            FlightId = ticket.FlightId,
            PassengerName = ticket.PassengerName,
            PassengerEmail = ticket.PassengerEmail,
            PassengerPhone = ticket.PassengerPhone,
            SeatNumber = ticket.SeatNumber,
            Class = (int)ticket.Class,
            Price = ticket.Price,
            BookingDate = ticket.BookingDate,
            Status = (int)ticket.Status
        };
    }

    /// <summary>
    /// Converts domain Ticket to CreateTicketDTO
    /// </summary>
    public static CreateTicketDTO ToCreateDTO(core.domain.Ticket ticket)
    {
        return new CreateTicketDTO
        {
            FlightId = ticket.FlightId,
            PassengerName = ticket.PassengerName,
            PassengerEmail = ticket.PassengerEmail,
            PassengerPhone = ticket.PassengerPhone,
            SeatNumber = ticket.SeatNumber,
            Class = (int)ticket.Class,
            Price = ticket.Price,
            BookingDate = ticket.BookingDate,
            Status = (int)ticket.Status
        };
    }

    /// <summary>
    /// Converts TicketDTO to domain Ticket
    /// </summary>
    public static core.domain.Ticket ToDomain(TicketDTO dto)
    {
        return new core.domain.Ticket
        {
            Id = dto.Id,
            TicketUid = dto.TicketUid,
            FlightId = dto.FlightId,
            PassengerName = dto.PassengerName,
            PassengerEmail = dto.PassengerEmail,
            PassengerPhone = dto.PassengerPhone,
            SeatNumber = dto.SeatNumber,
            Class = (core.enums.TicketClass)dto.Class,
            Price = dto.Price,
            BookingDate = dto.BookingDate,
            Status = (core.enums.TicketStatus)dto.Status
        };
    }

    /// <summary>
    /// Converts CreateTicketDTO to domain Ticket
    /// </summary>
    public static core.domain.Ticket ToDomain(CreateTicketDTO dto)
    {
        return new core.domain.Ticket
        {
            Id = dto.Id,
            TicketUid = dto.TicketUid,
            FlightId = dto.FlightId,
            PassengerName = dto.PassengerName,
            PassengerEmail = dto.PassengerEmail,
            PassengerPhone = dto.PassengerPhone,
            SeatNumber = dto.SeatNumber,
            Class = (core.enums.TicketClass)dto.Class,
            Price = dto.Price,
            BookingDate = dto.BookingDate,
            Status = (core.enums.TicketStatus)dto.Status
        };
    }

    /// <summary>
    /// Converts UpdateTicketDTO to domain Ticket
    /// </summary>
    public static core.domain.Ticket ToDomain(UpdateTicketDTO dto)
    {
        return new core.domain.Ticket
        {
            Id = dto.Id,
            TicketUid = dto.TicketUid ?? Guid.Empty,
            FlightId = dto.FlightId ?? 0,
            PassengerName = dto.PassengerName ?? string.Empty,
            PassengerEmail = dto.PassengerEmail ?? string.Empty,
            SeatNumber = dto.SeatNumber ?? string.Empty,
            Class = dto.Class.HasValue ? (core.enums.TicketClass)dto.Class.Value : core.enums.TicketClass.Economy,
            Price = dto.Price ?? 0,
            Status = dto.Status.HasValue ? (core.enums.TicketStatus)dto.Status.Value : core.enums.TicketStatus.Confirmed
        };
    }

    /// <summary>
    /// Converts domain Ticket to UpdateTicketDTO
    /// </summary>
    public static UpdateTicketDTO ToUpdateDTO(core.domain.Ticket ticket)
    {
        return new UpdateTicketDTO
        {
            Id = ticket.Id,
            TicketUid = ticket.TicketUid,
            FlightId = ticket.FlightId,
            PassengerName = ticket.PassengerName,
            PassengerEmail = ticket.PassengerEmail,
            PassengerPhone = ticket.PassengerPhone,
            SeatNumber = ticket.SeatNumber,
            Class = (int?)ticket.Class,
            Price = ticket.Price,
            BookingDate = ticket.BookingDate,
            Status = (int?)ticket.Status
        };
    }

    /// <summary>
    /// Converts list of domain Tickets to list of TicketDTOs
    /// </summary>
    public static List<TicketDTO> ToDTO(List<core.domain.Ticket> tickets)
    {
        return tickets.Select(ToDTO).ToList();
    }

    /// <summary>
    /// Converts IEnumerable of domain Tickets to IEnumerable of TicketDTOs
    /// </summary>
    public static IEnumerable<TicketDTO> ToDTO(IEnumerable<core.domain.Ticket> tickets)
    {
        return tickets.Select(ToDTO);
    }
}
