using presentation.dto.http.Ticket;
using presentation.dto.http.User;

namespace presentation.converters.http;

/// <summary>
/// Converter for User information between domain and HTTP DTO representations (per lab2-template v1 spec)
/// </summary>
public static class UserHttpConverter
{
    /// <summary>
    /// Converts domain entities to UserInfoDTO
    /// </summary>
    /// <param name="username">Username from header</param>
    /// <param name="privilege">User privilege entity (can be null)</param>
    /// <param name="tickets">List of user tickets</param>
    /// <returns>User information DTO</returns>
    public static UserInfoDTO ToDTO(string username, core.domain.Privilege? privilege, List<core.domain.Ticket> tickets)
    {
        var privilegeInfo = privilege != null
            ? new PrivilegeInfoDTO(
                privilege.Username,
                privilege.Status.ToString(),
                privilege.Balance)
            : null;

        var ticketDTOs = tickets.Select(t => new TicketDTO(
            id: t.Id,
            ticketUid: t.TicketUid,
            username: t.Username,
            flightNumber: t.FlightNumber,
            price: t.Price,
            status: t.Status
        )).ToList();

        return new UserInfoDTO(username, privilegeInfo, ticketDTOs);
    }
}
