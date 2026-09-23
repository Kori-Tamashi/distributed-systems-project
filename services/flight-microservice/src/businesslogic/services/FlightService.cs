using core.domain;
using core.exceptions.businesslogic.services;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.repositories;

using RepositoryFlightNotFoundException = core.exceptions.dataaccess.repositories.FlightNotFoundException;
using RepositoryFlightAlreadyExistsException = core.exceptions.dataaccess.repositories.FlightAlreadyExistsException;

namespace businesslogic.services;

/// <summary>
/// Service implementation for Flight business logic operations
/// Provides high-level operations with validation and business rules
/// </summary>
public class FlightService : IFlightService
{
    private readonly IFlightRepository _flightRepository;
    private readonly IAirportRepository _airportRepository;

    /// <summary>
    /// Initializes a new instance of FlightService
    /// </summary>
    /// <param name="flightRepository">The Flight repository for data access</param>
    /// <param name="airportRepository">The Airport repository for validation</param>
    public FlightService(IFlightRepository flightRepository, IAirportRepository airportRepository)
    {
        _flightRepository = flightRepository ?? throw new ArgumentNullException(nameof(flightRepository));
        _airportRepository = airportRepository ?? throw new ArgumentNullException(nameof(airportRepository));
    }

    /// <inheritdoc/>
    public async Task<Flight> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new FlightValidationException($"Invalid Flight ID: {id}. ID must be positive.");
        }

        try
        {
            var flight = await _flightRepository.GetByIdAsync(id);
            return flight ?? throw new FlightNotFoundException(id);
        }
        catch (RepositoryFlightNotFoundException)
        {
            throw new FlightNotFoundException(id);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to get Flight with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<List<Flight>> GetAllAsync(FlightFilter? filter = null)
    {
        try
        {
            return await _flightRepository.GetAllAsync(filter);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException("Failed to get all Flights", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Flight> CreateAsync(Flight flight)
    {
        ValidateFlight(flight);

        // Validate airports exist
        await ValidateAirlinesExistAsync(flight.FromAirportId, flight.ToAirportId);

        try
        {
            var createdFlight = await _flightRepository.CreateAsync(flight);
            return createdFlight;
        }
        catch (FlightValidationException)
        {
            throw;
        }
        catch (RepositoryFlightAlreadyExistsException)
        {
            throw new FlightBusinessRuleViolationException(
                "UniqueConstraint", 
                $"Flight with UID {flight.FlightUid} already exists");
        }
        catch (Exception ex)
        {
            throw new BaseServiceException("Failed to create Flight", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Flight> UpdateAsync(Flight flight)
    {
        if (flight.Id <= 0)
        {
            throw new FlightValidationException($"Invalid Flight ID: {flight.Id}. ID must be positive.");
        }

        ValidateFlight(flight);

        // Validate airports exist
        await ValidateAirlinesExistAsync(flight.FromAirportId, flight.ToAirportId);

        // Check if flight exists before updating (outside try-catch)
        var exists = await _flightRepository.ExistsAsync(flight.Id);
        if (!exists)
        {
            throw new FlightNotFoundException(flight.Id);
        }

        try
        {
            var updatedFlight = await _flightRepository.UpdateAsync(flight);
            return updatedFlight;
        }
        catch (RepositoryFlightNotFoundException)
        {
            throw new FlightNotFoundException(flight.Id);
        }
        catch (FlightValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to update Flight with ID {flight.Id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            throw new FlightValidationException($"Invalid Flight ID: {id}. ID must be positive.");
        }

        // Check if flight exists before deleting (outside try-catch)
        var exists = await _flightRepository.ExistsAsync(id);
        if (!exists)
        {
            throw new FlightNotFoundException(id);
        }

        try
        {
            return await _flightRepository.DeleteAsync(id);
        }
        catch (RepositoryFlightNotFoundException)
        {
            throw new FlightNotFoundException(id);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to delete Flight with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(int id)
    {
        if (id <= 0)
        {
            throw new FlightValidationException($"Invalid Flight ID: {id}. ID must be positive.");
        }

        try
        {
            return await _flightRepository.ExistsAsync(id);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to check existence of Flight with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetCountAsync(FlightFilter? filter = null)
    {
        try
        {
            return await _flightRepository.GetCountAsync(filter);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException("Failed to get Flight count", ex);
        }
    }

    /// <summary>
    /// Validates Flight entity for business rules
    /// </summary>
    /// <param name="flight">The Flight to validate</param>
    /// <exception cref="FlightValidationException">Thrown when validation fails</exception>
    private void ValidateFlight(Flight flight)
    {
        if (flight == null)
        {
            throw new FlightValidationException("Flight cannot be null");
        }

        var errors = new Dictionary<string, string[]>();

        // Validate FlightNumber
        if (string.IsNullOrWhiteSpace(flight.FlightNumber))
        {
            errors["FlightNumber"] = new[] { "FlightNumber is required and cannot be empty" };
        }
        else if (flight.FlightNumber.Length > 20)
        {
            errors["FlightNumber"] = new[] { "FlightNumber cannot exceed 20 characters" };
        }

        // Validate DateTime (must be in the future)
        if (flight.DateTime < DateTime.UtcNow)
        {
            errors["DateTime"] = new[] { "Flight date and time cannot be in the past" };
        }

        // Validate FromAirportId and ToAirportId are different
        if (flight.FromAirportId == flight.ToAirportId)
        {
            errors["ToAirportId"] = new[] { "Departure and arrival airports must be different" };
        }

        // Validate Price
        if (flight.Price <= 0)
        {
            errors["Price"] = new[] { "Price must be positive" };
        }
        else if (flight.Price > int.MaxValue)
        {
            errors["Price"] = new[] { "Price exceeds maximum allowed value" };
        }

        if (errors.Count > 0)
        {
            var errorMessage = $"Flight validation failed with {errors.Count} error(s)";
            throw new FlightValidationException(errorMessage, errors);
        }
    }

    /// <summary>
    /// Validates that both departure and arrival airports exist
    /// </summary>
    /// <param name="fromAirportId">Departure airport ID</param>
    /// <param name="toAirportId">Arrival airport ID</param>
    /// <exception cref="AirportNotFoundException">Thrown when airport is not found</exception>
    private async Task ValidateAirlinesExistAsync(int fromAirportId, int toAirportId)
    {
        if (!await _airportRepository.ExistsAsync(fromAirportId))
        {
            throw new AirportNotFoundException(fromAirportId);
        }

        if (!await _airportRepository.ExistsAsync(toAirportId))
        {
            throw new AirportNotFoundException(toAirportId);
        }
    }
}
