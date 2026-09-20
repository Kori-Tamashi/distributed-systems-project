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
    /// Converts CreateTicketDTO to domain Ticket
    /// </summary>
    public static core.domain.Ticket ToCreateDomain(CreateTicketDTO dto)
    {
        return new core.domain.Ticket
        {
            Id = 0,
            TicketUid = dto.TicketUid,
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
        return new UpdateTicketDTO(ticket.Id)
        {
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
    /// Converts UpdateTicketDTO to domain Ticket
    /// Only updates non-null properties
    /// </summary>
    public static core.domain.Ticket ToUpdateDomain(UpdateTicketDTO dto, core.domain.Ticket existingTicket)
    {
        existingTicket.TicketUid = dto.TicketUid ?? existingTicket.TicketUid;
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

    /// <summary>
    /// Converts domain Ticket to UpdateTicketDTO (without existing ticket)
    /// </summary>
    public static UpdateTicketDTO ToUpdateDTOWithoutMerge(core.domain.Ticket ticket)
    {
        return new UpdateTicketDTO(ticket.Id)
        {
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
    /// Converts domain Ticket to UpdateTicketDTO with only changed properties
    /// </summary>
    public static UpdateTicketDTO ToUpdateDTOPartial(core.domain.Ticket ticket, params string[] changedProperties)
    {
        var dto = new UpdateTicketDTO(ticket.Id);

        if (Array.Exists(changedProperties, p => p.Equals("TicketUid", StringComparison.OrdinalIgnoreCase)))
            dto.TicketUid = ticket.TicketUid;

        if (Array.Exists(changedProperties, p => p.Equals("FlightId", StringComparison.OrdinalIgnoreCase)))
            dto.FlightId = ticket.FlightId;

        if (Array.Exists(changedProperties, p => p.Equals("PassengerName", StringComparison.OrdinalIgnoreCase)))
            dto.PassengerName = ticket.PassengerName;

        if (Array.Exists(changedProperties, p => p.Equals("PassengerEmail", StringComparison.OrdinalIgnoreCase)))
            dto.PassengerEmail = ticket.PassengerEmail;

        if (Array.Exists(changedProperties, p => p.Equals("PassengerPhone", StringComparison.OrdinalIgnoreCase)))
            dto.PassengerPhone = ticket.PassengerPhone;

        if (Array.Exists(changedProperties, p => p.Equals("SeatNumber", StringComparison.OrdinalIgnoreCase)))
            dto.SeatNumber = ticket.SeatNumber;

        if (Array.Exists(changedProperties, p => p.Equals("Class", StringComparison.OrdinalIgnoreCase)))
            dto.Class = (int)ticket.Class;

        if (Array.Exists(changedProperties, p => p.Equals("Price", StringComparison.OrdinalIgnoreCase)))
            dto.Price = ticket.Price;

        if (Array.Exists(changedProperties, p => p.Equals("BookingDate", StringComparison.OrdinalIgnoreCase)))
            dto.BookingDate = ticket.BookingDate;

        if (Array.Exists(changedProperties, p => p.Equals("Status", StringComparison.OrdinalIgnoreCase)))
            dto.Status = (int)ticket.Status;

        return dto;
    }

    /// <summary>
    /// Converts domain Ticket to UpdateTicketDTO with only changed properties (nullable check)
    /// </summary>
    public static UpdateTicketDTO ToUpdateDTOPartialNullable(core.domain.Ticket ticket, params bool[] propertyChanged)
    {
        var dto = new UpdateTicketDTO(ticket.Id);

        if (propertyChanged.Length >= 1 && propertyChanged[0])
            dto.TicketUid = ticket.TicketUid;

        if (propertyChanged.Length >= 2 && propertyChanged[1])
            dto.FlightId = ticket.FlightId;

        if (propertyChanged.Length >= 3 && propertyChanged[2])
            dto.PassengerName = ticket.PassengerName;

        if (propertyChanged.Length >= 4 && propertyChanged[3])
            dto.PassengerEmail = ticket.PassengerEmail;

        if (propertyChanged.Length >= 5 && propertyChanged[4])
            dto.PassengerPhone = ticket.PassengerPhone;

        if (propertyChanged.Length >= 6 && propertyChanged[5])
            dto.SeatNumber = ticket.SeatNumber;

        if (propertyChanged.Length >= 7 && propertyChanged[6])
            dto.Class = (int)ticket.Class;

        if (propertyChanged.Length >= 8 && propertyChanged[7])
            dto.Price = ticket.Price;

        if (propertyChanged.Length >= 9 && propertyChanged[8])
            dto.BookingDate = ticket.BookingDate;

        if (propertyChanged.Length >= 10 && propertyChanged[9])
            dto.Status = (int)ticket.Status;

        return dto;
    }
}
