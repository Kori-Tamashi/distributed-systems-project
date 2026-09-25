using presentation.dto.http;
using presentation.dto.http.Ticket;

namespace presentation.converters.http;

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
        if (ticket == null) return null!;
        
        return new TicketDTO
        {
            Id = ticket.Id,
            TicketUid = ticket.TicketUid,
            Username = ticket.Username,
            FlightNumber = ticket.FlightNumber,
            Price = ticket.Price,
            Status = ticket.Status,
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
            Username = dto.Username,
            FlightNumber = dto.FlightNumber,
            Price = dto.Price,
            Status = dto.Status
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
            Username = ticket.Username,
            FlightNumber = ticket.FlightNumber,
            Price = ticket.Price,
            Status = ticket.Status
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
            TicketUid = dto.TicketUid != Guid.Empty ? dto.TicketUid : Guid.NewGuid(),
            Username = dto.Username,
            FlightNumber = dto.FlightNumber,
            Price = dto.Price,
            Status = dto.Status,
        };
    }

    /// <summary>
    /// Converts domain Ticket to UpdateTicketDTO
    /// </summary>
    public static UpdateTicketDTO ToUpdateDTO(core.domain.Ticket ticket)
    {
        return new UpdateTicketDTO
        {
            TicketUid = ticket.TicketUid,
            Username = ticket.Username,
            FlightNumber = ticket.FlightNumber,
            Price = ticket.Price,
            Status = ticket.Status
        };
    }

    /// <summary>
    /// Converts UpdateTicketDTO to domain Ticket
    /// Only updates non-null properties
    /// </summary>
    public static core.domain.Ticket ToUpdateDomain(UpdateTicketDTO dto, core.domain.Ticket existingTicket)
    {
        if (dto == null || existingTicket == null) return existingTicket;
        
        existingTicket.Username = !string.IsNullOrEmpty(dto.Username) ? dto.Username : existingTicket.Username;
        existingTicket.FlightNumber = !string.IsNullOrEmpty(dto.FlightNumber) ? dto.FlightNumber : existingTicket.FlightNumber;
        existingTicket.Price = dto.Price.HasValue ? dto.Price.Value : existingTicket.Price;
        existingTicket.Status = dto.Status.HasValue ? dto.Status.Value : existingTicket.Status;

        return existingTicket;
    }
}
