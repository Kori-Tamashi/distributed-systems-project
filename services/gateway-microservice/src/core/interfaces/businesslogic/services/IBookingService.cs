using core.domain;
using core.exceptions.businesslogic.services;
using core.filters;

namespace core.interfaces.businesslogic.services;

/// <summary>
/// Service interface for Booking business logic operations
/// Provides high-level operations for managing Booking entities
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// Gets a Booking by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Booking</param>
    /// <returns>The Booking entity if found</returns>
    /// <exception cref="BookingNotFoundException">Thrown when Booking with specified id is not found</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<Booking> GetByIdAsync(int id);

    /// <summary>
    /// Gets all Booking entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for querying bookings (null returns all)</param>
    /// <returns>List of all Booking entities matching the filter or all bookings if filter is null</returns>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<List<Booking>> GetAllAsync(BookingFilter? filter = null);

    /// <summary>
    /// Creates a new Booking
    /// </summary>
    /// <param name="booking">The Booking entity to create</param>
    /// <returns>The created Booking entity with generated Id</returns>
    /// <exception cref="BookingValidationException">Thrown when Booking data is invalid</exception>
    /// <exception cref="BookingBusinessRuleViolationException">Thrown when business rules are violated</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<Booking> CreateAsync(Booking booking);

    /// <summary>
    /// Updates an existing Booking
    /// </summary>
    /// <param name="booking">The Booking entity to update</param>
    /// <returns>The updated Booking entity</returns>
    /// <exception cref="BookingNotFoundException">Thrown when Booking with specified id is not found</exception>
    /// <exception cref="BookingValidationException">Thrown when Booking data is invalid</exception>
    /// <exception cref="BookingBusinessRuleViolationException">Thrown when business rules are violated</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<Booking> UpdateAsync(Booking booking);

    /// <summary>
    /// Deletes a Booking by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Booking to delete</param>
    /// <returns>True if Booking was deleted successfully</returns>
    /// <exception cref="BookingNotFoundException">Thrown when Booking with specified id is not found</exception>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task DeleteAsync(int id);

    /// <summary>
    /// Checks if a Booking exists by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier to check</param>
    /// <returns>True if Booking exists, false otherwise</returns>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Gets the total count of Booking entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for counting bookings (null counts all)</param>
    /// <returns>Total number of Booking entities matching the filter</returns>
    /// <exception cref="BaseServiceException">Thrown when service operation fails</exception>
    Task<int> GetCountAsync(BookingFilter? filter = null);
}
