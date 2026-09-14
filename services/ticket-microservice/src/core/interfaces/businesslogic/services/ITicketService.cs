using core.domain;
using core.exceptions.businesslogic.services;
using core.filters;

namespace core.interfaces.businesslogic.services;

/// <summary>
/// Service interface for Ticket business logic operations
/// Provides high-level operations for managing Ticket entities
/// </summary>
public interface ITicketService
{
    /// <summary>
    /// Gets a Ticket by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Ticket</param>
    /// <returns>The Ticket entity if found</returns>
    /// <exception cref="TicketNotFoundException">Thrown when Ticket with specified id is not found</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<Ticket> GetByIdAsync(int id);

    /// <summary>
    /// Gets all Ticket entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for querying tickets (null returns all)</param>
    /// <returns>List of Ticket entities matching the filter or all tickets if filter is null</returns>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<List<Ticket>> GetAllAsync(TicketFilter? filter = null);

    /// <summary>
    /// Creates a new Ticket
    /// </summary>
    /// <param name="ticket">The Ticket entity to create</param>
    /// <returns>The created Ticket entity with generated Id</returns>
    /// <exception cref="TicketValidationException">Thrown when Ticket data is invalid</exception>
    /// <exception cref="TicketBusinessRuleViolationException">Thrown when business rules are violated</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<Ticket> CreateAsync(Ticket ticket);

    /// <summary>
    /// Updates an existing Ticket
    /// </summary>
    /// <param name="ticket">The Ticket entity to update</param>
    /// <returns>The updated Ticket entity</returns>
    /// <exception cref="TicketNotFoundException">Thrown when Ticket with specified id is not found</exception>
    /// <exception cref="TicketValidationException">Thrown when Ticket data is invalid</exception>
    /// <exception cref="TicketBusinessRuleViolationException">Thrown when business rules are violated</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<Ticket> UpdateAsync(Ticket ticket);

    /// <summary>
    /// Deletes a Ticket by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Ticket to delete</param>
    /// <returns>True if Ticket was deleted successfully</returns>
    /// <exception cref="TicketNotFoundException">Thrown when Ticket with specified id is not found</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Checks if a Ticket exists by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier to check</param>
    /// <returns>True if Ticket exists, false otherwise</returns>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Gets the total count of Ticket entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for counting tickets (null counts all)</param>
    /// <returns>Total number of Ticket entities matching the filter</returns>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<int> GetCountAsync(TicketFilter? filter = null);
}
