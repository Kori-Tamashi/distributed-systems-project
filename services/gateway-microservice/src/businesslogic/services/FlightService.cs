using core.domain;
using core.exceptions.businesslogic.services;
using core.exceptions.dataaccess.gateways;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.gateways;
using Microsoft.Extensions.Logging;

namespace businesslogic.services;

/// <summary>
/// Service implementation for Flight business logic operations
/// Provides high-level operations with validation, business rules, and error handling
/// Uses HTTP Gateway to communicate with Flight microservice
/// </summary>
public class FlightService : IFlightService
{
    private readonly IFlightGateway _flightGateway;
    private readonly ILogger<FlightService> _logger;

    /// <summary>
    /// Initializes a new instance of FlightService
    /// </summary>
    /// <param name="flightGateway">The Flight Gateway for HTTP communication</param>
    /// <param name="logger">Logger for SAGA tracking</param>
    public FlightService(IFlightGateway flightGateway, ILogger<FlightService> logger)
    {
        _flightGateway = flightGateway ?? throw new ArgumentNullException(nameof(flightGateway));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            var flight = await _flightGateway.GetByIdAsync(id);
            if (flight == null)
            {
                throw new FlightNotFoundException(id);
            }
            return flight;
        }
        catch (FlightNotFoundException)
        {
            throw;
        }
        catch (FlightGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Flight microservice for GetByIdAsync");
            throw new ValidationException($"Failed to communicate with Flight microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Flight with ID {Id}", id);
            throw new ValidationException($"Failed to get Flight with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<List<Flight>> GetAllAsync(FlightFilter? filter = null)
    {
        try
        {
            var flights = filter != null 
                ? await _flightGateway.GetAllAsync(filter)
                : await _flightGateway.GetAllAsync();
            return flights.ToList();
        }
        catch (FlightGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Flight microservice for GetAllAsync");
            throw new ValidationException($"Failed to communicate with Flight microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all Flights");
            throw new ValidationException("Failed to get all Flights", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Flight> CreateAsync(Flight flight)
    {
        ValidateFlight(flight);

        try
        {
            var createdFlight = await _flightGateway.CreateAsync(flight);
            _logger.LogInformation("Flight created with ID {Id}", createdFlight.Id);
            return createdFlight;
        }
        catch (FlightValidationException)
        {
            _logger.LogWarning("Flight validation failed");
            throw;
        }
        catch (FlightGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to create Flight");
            throw new ValidationException($"Failed to communicate with Flight microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Flight");
            throw new ValidationException("Failed to create Flight", ex);
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

        try
        {
            var updatedFlight = await _flightGateway.UpdateAsync(flight);
            _logger.LogInformation("Flight {Id} updated successfully", flight.Id);
            return updatedFlight;
        }
        catch (FlightGatewayEntityNotFoundException)
        {
            _logger.LogWarning("Flight {Id} not found", flight.Id);
            throw new FlightNotFoundException(flight.Id);
        }
        catch (FlightValidationException)
        {
            _logger.LogWarning("Flight validation failed");
            throw;
        }
        catch (FlightGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to update Flight {Id}", flight.Id);
            throw new ValidationException($"Failed to communicate with Flight microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Flight {Id}", flight.Id);
            throw new ValidationException($"Failed to update Flight with ID {flight.Id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int id)
    {
        if (id <= 0)
        {
            throw new FlightValidationException($"Invalid Flight ID: {id}. ID must be positive.");
        }

        try
        {
            await _flightGateway.DeleteAsync(id);
            _logger.LogInformation("Flight {Id} deleted successfully", id);
        }
        catch (FlightNotFoundException)
        {
            throw;
        }
        catch (FlightGatewayEntityNotFoundException)
        {
            _logger.LogWarning("Flight {Id} not found", id);
            throw new FlightNotFoundException(id);
        }
        catch (FlightGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to delete Flight {Id}", id);
            throw new ValidationException($"Failed to communicate with Flight microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete Flight {Id}", id);
            throw new ValidationException($"Failed to delete Flight with ID {id}", ex);
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
            var flight = await _flightGateway.GetByIdAsync(id);
            return flight != null;
        }
        catch (FlightGatewayEntityNotFoundException)
        {
            return false;
        }
        catch (FlightGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Flight microservice for ExistsAsync");
            throw new ValidationException($"Failed to communicate with Flight microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check existence of Flight with ID {Id}", id);
            throw new ValidationException($"Failed to check existence of Flight with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetCountAsync(FlightFilter? filter = null)
    {
        try
        {
            var flights = filter != null 
                ? await _flightGateway.GetAllAsync(filter)
                : await _flightGateway.GetAllAsync();
            return flights.Count();
        }
        catch (FlightGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Flight microservice for GetCountAsync");
            throw new ValidationException($"Failed to communicate with Flight microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Flight count");
            throw new ValidationException("Failed to get Flight count", ex);
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

        // Validate FromAirportId
        if (flight.FromAirportId <= 0)
        {
            errors["FromAirportId"] = new[] { "FromAirportId must be positive" };
        }

        // Validate ToAirportId
        if (flight.ToAirportId <= 0)
        {
            errors["ToAirportId"] = new[] { "ToAirportId must be positive" };
        }

        // Validate FromAirportId != ToAirportId
        if (flight.FromAirportId == flight.ToAirportId)
        {
            errors["ToAirportId"] = new[] { "Departure and arrival airports cannot be the same" };
        }

        // Validate DateTime (must be in the future)
        if (flight.DateTime < DateTime.UtcNow)
        {
            errors["DateTime"] = new[] { "Flight date and time must be in the future" };
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
}
