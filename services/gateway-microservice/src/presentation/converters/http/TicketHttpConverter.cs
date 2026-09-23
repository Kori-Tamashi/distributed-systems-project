using presentation.dto.http;
using presentation.dto.http.Ticket;

namespace presentation.converters.http;

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
        if (ticket == null) return null!;
        
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
            Status = (int)ticket.Status,
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
    /// Converts list of domain Tickets to list of TicketDTOs
    /// </summary>
    public static List<TicketDTO> ToDTO(List<core.domain.Ticket> tickets)
    {
        return tickets.Select(ToDTO).ToList();
    }

    /// <summary>
    /// Converts list of TicketDTOs to list of domain Tickets
    /// </summary>
    public static List<core.domain.Ticket> ToDomain(List<TicketDTO> dtos)
    {
        return dtos.Select(ToDomain).ToList();
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
    /// Converts CreateTicketDTO to domain Ticket
    /// </summary>
    public static core.domain.Ticket ToCreateDomain(CreateTicketDTO dto)
    {
        return new core.domain.Ticket
        {
            Id = 0,
            TicketUid = Guid.NewGuid(),
            FlightId = dto.FlightId,
            PassengerName = dto.PassengerName,
            PassengerEmail = dto.PassengerEmail,
            PassengerPhone = dto.PassengerPhone,
            SeatNumber = dto.SeatNumber,
            Class = (core.enums.TicketClass)dto.Class,
            Price = dto.Price,
            BookingDate = dto.BookingDate,
            Status = (core.enums.TicketStatus)dto.Status,
        };
    }

    /// <summary>
    /// Converts domain Ticket to UpdateTicketDTO
    /// </summary>
    public static UpdateTicketDTO ToUpdateDTO(core.domain.Ticket ticket)
    {
        return new UpdateTicketDTO
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
    /// Converts UpdateTicketDTO to domain Ticket
    /// Only updates non-null properties
    /// </summary>
    public static core.domain.Ticket ToUpdateDomain(UpdateTicketDTO dto, core.domain.Ticket existingTicket)
    {
        if (dto == null || existingTicket == null) return existingTicket;
        
        existingTicket.FlightId = dto.FlightId ?? existingTicket.FlightId;
        existingTicket.PassengerName = dto.PassengerName ?? existingTicket.PassengerName;
        existingTicket.PassengerEmail = dto.PassengerEmail ?? existingTicket.PassengerEmail;
        existingTicket.PassengerPhone = dto.PassengerPhone ?? existingTicket.PassengerPhone;
        existingTicket.SeatNumber = dto.SeatNumber ?? existingTicket.SeatNumber;
        existingTicket.Class = dto.Class.HasValue ? (core.enums.TicketClass)dto.Class.Value : existingTicket.Class;
        existingTicket.Price = dto.Price ?? existingTicket.Price;
        existingTicket.BookingDate = dto.BookingDate ?? existingTicket.BookingDate;
        existingTicket.Status = dto.Status.HasValue ? (core.enums.TicketStatus)dto.Status.Value : existingTicket.Status;

        return existingTicket;
    }
}
