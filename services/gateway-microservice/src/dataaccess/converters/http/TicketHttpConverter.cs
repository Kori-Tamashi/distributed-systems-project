using dataaccess.dto.http.Ticket;

namespace dataaccess.converters.http;

/// <summary>
/// Converter for Ticket between domain and HTTP DTO representations (per lab2-template v1 spec)
/// </summary>
public static class TicketHttpConverter
{
    /// <summary>
    /// Converts domain Ticket to TicketDTO
    /// </summary>
    public static TicketDTO ToDTO(core.domain.Ticket ticket)
    {
        return new TicketDTO(
            id: ticket.Id,
            ticketUid: ticket.TicketUid,
            username: ticket.Username,
            flightNumber: ticket.FlightNumber,
            price: ticket.Price,
            status: ticket.Status
        );
    }

    /// <summary>
    /// Converts domain Ticket to CreateTicketDTO
    /// </summary>
    public static CreateTicketDTO ToCreateDTO(core.domain.Ticket ticket)
    {
        return new CreateTicketDTO(
            ticketUid: ticket.TicketUid,
            username: ticket.Username,
            flightNumber: ticket.FlightNumber,
            price: ticket.Price,
            status: ticket.Status
        );
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
            Username = dto.Username,
            FlightNumber = dto.FlightNumber,
            Price = dto.Price,
            Status = dto.Status
        };
    }

    /// <summary>
    /// Converts CreateTicketDTO to domain Ticket
    /// </summary>
    public static core.domain.Ticket ToDomain(CreateTicketDTO dto)
    {
        return new core.domain.Ticket
        {
            TicketUid = dto.TicketUid,
            Username = dto.Username,
            FlightNumber = dto.FlightNumber,
            Price = dto.Price,
            Status = dto.Status
        };
    }

    /// <summary>
    /// Converts UpdateTicketDTO to domain Ticket (partial update)
    /// </summary>
    public static core.domain.Ticket ToDomain(UpdateTicketDTO dto)
    {
        var ticket = new core.domain.Ticket
        {
            TicketUid = dto.TicketUid != Guid.Empty ? dto.TicketUid : Guid.Empty,
            Username = dto.Username ?? string.Empty,
            FlightNumber = dto.FlightNumber ?? string.Empty,
            Price = dto.Price ?? 0,
            Status = dto.Status ?? 1 // Default to PAID
        };

        if (dto.Id != 0)
            ticket.Id = dto.Id;

        return ticket;
    }

    /// <summary>
    /// Converts domain Ticket to UpdateTicketDTO
    /// </summary>
    public static UpdateTicketDTO ToUpdateDTO(core.domain.Ticket ticket)
    {
        return new UpdateTicketDTO(
            ticketUid: ticket.TicketUid,
            username: ticket.Username,
            flightNumber: ticket.FlightNumber,
            price: ticket.Price,
            status: ticket.Status
        );
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
