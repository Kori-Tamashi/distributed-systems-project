using core.domain;
using core.filters;
using core.exceptions.dataaccess.gateways;

namespace core.interfaces.dataaccess.gateways;

/// <summary>
/// Gateway interface for Ticket microservice communication.
/// Provides methods to interact with Ticket microservice via HTTP API.
/// This gateway acts as a client for the Ticket microservice, abstracting
/// the HTTP communication details from the business logic layer.
/// </summary>
public interface ITicketGateway
{
    /// <summary>
    /// Retrieves all tickets from the Ticket microservice.
    /// </summary>
    /// <returns>A collection of all tickets.</returns>
    /// <exception cref="TicketGatewayCommunicationException">Thrown when communication with Ticket microservice fails.</exception>
    /// <exception cref="TicketGatewayTimeoutException">Thrown when the request to Ticket microservice times out.</exception>
    /// <exception cref="TicketGatewayInternalServerException">Thrown when Ticket microservice returns an internal server error.</exception>
    Task<IEnumerable<Ticket>> GetAllAsync();

    /// <summary>
    /// Retrieves a ticket by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the ticket.</param>
    /// <returns>The ticket entity, or <c>null</c> if not found.</returns>
    /// <exception cref="TicketGatewayCommunicationException">Thrown when communication with Ticket microservice fails.</exception>
    /// <exception cref="TicketGatewayTimeoutException">Thrown when the request to Ticket microservice times out.</exception>
    /// <exception cref="TicketGatewayInternalServerException">Thrown when Ticket microservice returns an internal server error.</exception>
    Task<Ticket?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves tickets filtered by the specified criteria.
    /// </summary>
    /// <param name="filter">The filter criteria for querying tickets.</param>
    /// <returns>A collection of tickets matching the filter criteria.</returns>
    /// <exception cref="TicketGatewayCommunicationException">Thrown when communication with Ticket microservice fails.</exception>
    /// <exception cref="TicketGatewayTimeoutException">Thrown when the request to Ticket microservice times out.</exception>
    /// <exception cref="TicketGatewayInternalServerException">Thrown when Ticket microservice returns an internal server error.</exception>
    Task<IEnumerable<Ticket>> GetAllAsync(TicketFilter filter);

    /// <summary>
    /// Creates a new ticket in the Ticket microservice.
    /// </summary>
    /// <param name="ticket">The ticket entity to create.</param>
    /// <returns>The created ticket with the generated identifier.</returns>
    /// <exception cref="TicketGatewayCommunicationException">Thrown when communication with Ticket microservice fails.</exception>
    /// <exception cref="TicketGatewayTimeoutException">Thrown when the request to Ticket microservice times out.</exception>
    /// <exception cref="TicketGatewayInternalServerException">Thrown when Ticket microservice returns an internal server error.</exception>
    Task<Ticket> CreateAsync(Ticket ticket);

    /// <summary>
    /// Updates an existing ticket in the Ticket microservice.
    /// </summary>
    /// <param name="ticket">The ticket entity to update.</param>
    /// <returns>The updated ticket.</returns>
    /// <exception cref="TicketGatewayEntityNotFoundException">Thrown when the ticket with specified ID is not found.</exception>
    /// <exception cref="TicketGatewayCommunicationException">Thrown when communication with Ticket microservice fails.</exception>
    /// <exception cref="TicketGatewayTimeoutException">Thrown when the request to Ticket microservice times out.</exception>
    /// <exception cref="TicketGatewayInternalServerException">Thrown when Ticket microservice returns an internal server error.</exception>
    Task<Ticket> UpdateAsync(Ticket ticket);

    /// <summary>
    /// Deletes a ticket by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the ticket to delete.</param>
    /// <returns><c>true</c> if the ticket was successfully deleted; <c>false</c> if the ticket was not found.</returns>
    /// <exception cref="TicketGatewayCommunicationException">Thrown when communication with Ticket microservice fails.</exception>
    /// <exception cref="TicketGatewayTimeoutException">Thrown when the request to Ticket microservice times out.</exception>
    /// <exception cref="TicketGatewayInternalServerException">Thrown when Ticket microservice returns an internal server error.</exception>
    Task DeleteAsync(int id);
}
