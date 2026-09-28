using core.domain;
using core.exceptions.businesslogic.services;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.gateways;
using Microsoft.Extensions.Logging;
using core.exceptions.dataaccess.gateways;

namespace businesslogic.services;

/// <summary>
/// Service implementation for Airport business logic operations
/// Provides high-level operations with validation and business rules
/// Uses HTTP Gateway to communicate with Flight microservice
/// </summary>
public class AirportService : IAirportService
{
    private readonly IAirportGateway _airportGateway;
    private readonly ILogger<AirportService> _logger;

    /// <summary>
    /// Initializes a new instance of AirportService
    /// </summary>
    /// <param name="airportGateway">The Airport Gateway for HTTP communication</param>
    /// <param name="logger">Logger for tracking operations</param>
    public AirportService(IAirportGateway airportGateway, ILogger<AirportService> logger)
    {
        _airportGateway = airportGateway ?? throw new ArgumentNullException(nameof(airportGateway));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Airport> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new AirportValidationException($"Invalid Airport ID: {id}. ID must be positive.");
        }

        try
        {
            var airport = await _airportGateway.GetByIdAsync(id);
            if (airport == null)
            {
                throw new AirportNotFoundException(id);
            }
            return airport;
        }
        catch (AirportNotFoundException)
        {
            throw;
        }
        catch (AirportGatewayEntityNotFoundException)
        {
            throw new AirportNotFoundException(id);
        }
        catch (AirportGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Flight microservice for GetByIdAsync");
            throw new ValidationException($"Failed to communicate with Flight microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Airport with ID {Id}", id);
            throw new ValidationException($"Failed to get Airport with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<List<Airport>> GetAllAsync(AirportFilter? filter = null)
    {
        try
        {
            var airports = filter != null 
                ? await _airportGateway.GetAllAsync(filter)
                : await _airportGateway.GetAllAsync();
            return airports.ToList();
        }
        catch (AirportGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Flight microservice for GetAllAsync");
            throw new ValidationException($"Failed to communicate with Flight microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all Airports");
            throw new ValidationException("Failed to get all Airports", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Airport> CreateAsync(Airport airport)
    {
        ValidateAirport(airport);

        try
        {
            var createdAirport = await _airportGateway.CreateAsync(airport);
            _logger.LogInformation("Airport created with ID {Id}", createdAirport.Id);
            return createdAirport;
        }
        catch (AirportValidationException)
        {
            _logger.LogWarning("Airport validation failed");
            throw;
        }
        catch (AirportGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to create Airport");
            throw new ValidationException($"Failed to communicate with Flight microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Airport");
            throw new ValidationException("Failed to create Airport", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Airport> UpdateAsync(Airport airport)
    {
        if (airport.Id <= 0)
        {
            throw new AirportValidationException($"Invalid Airport ID: {airport.Id}. ID must be positive.");
        }

        ValidateAirport(airport);

        // Check if airport exists before updating
        try
        {
            var exists = await _airportGateway.GetByIdAsync(airport.Id);
            if (exists == null)
            {
                throw new AirportNotFoundException(airport.Id);
            }
        }
        catch (AirportGatewayEntityNotFoundException)
        {
            throw new AirportNotFoundException(airport.Id);
        }

        try
        {
            var updatedAirport = await _airportGateway.UpdateAsync(airport);
            _logger.LogInformation("Airport {Id} updated successfully", airport.Id);
            return updatedAirport;
        }
        catch (AirportGatewayEntityNotFoundException)
        {
            _logger.LogWarning("Airport {Id} not found", airport.Id);
            throw new AirportNotFoundException(airport.Id);
        }
        catch (AirportValidationException)
        {
            _logger.LogWarning("Airport validation failed");
            throw;
        }
        catch (AirportGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to update Airport {Id}", airport.Id);
            throw new ValidationException($"Failed to communicate with Flight microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Airport {Id}", airport.Id);
            throw new ValidationException($"Failed to update Airport with ID {airport.Id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int id)
    {
        if (id <= 0)
        {
            throw new AirportValidationException($"Invalid Airport ID: {id}. ID must be positive.");
        }

        // Check if airport exists before deleting
        try
        {
            var exists = await _airportGateway.GetByIdAsync(id);
            if (exists == null)
            {
                throw new AirportNotFoundException(id);
            }
        }
        catch (AirportGatewayEntityNotFoundException)
        {
            throw new AirportNotFoundException(id);
        }

        try
        {
            await _airportGateway.DeleteAsync(id);
            _logger.LogInformation("Airport {Id} deleted successfully", id);
        }
        catch (AirportNotFoundException)
        {
            throw;
        }
        catch (AirportGatewayEntityNotFoundException)
        {
            _logger.LogWarning("Airport {Id} not found", id);
            throw new AirportNotFoundException(id);
        }
        catch (AirportGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to delete Airport {Id}", id);
            throw new ValidationException($"Failed to communicate with Flight microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete Airport {Id}", id);
            throw new ValidationException($"Failed to delete Airport with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(int id)
    {
        if (id <= 0)
        {
            throw new AirportValidationException($"Invalid Airport ID: {id}. ID must be positive.");
        }

        try
        {
            var airport = await _airportGateway.GetByIdAsync(id);
            return airport != null;
        }
        catch (AirportGatewayEntityNotFoundException)
        {
            return false;
        }
        catch (AirportGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Flight microservice for ExistsAsync");
            throw new ValidationException($"Failed to communicate with Flight microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check existence of Airport with ID {Id}", id);
            throw new ValidationException($"Failed to check existence of Airport with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetCountAsync(AirportFilter? filter = null)
    {
        try
        {
            var airports = filter != null 
                ? await _airportGateway.GetAllAsync(filter)
                : await _airportGateway.GetAllAsync();
            return airports.Count();
        }
        catch (AirportGatewayCommunicationException ex)
        {
            _logger.LogError(ex, "Failed to communicate with Flight microservice for GetCountAsync");
            throw new ValidationException($"Failed to communicate with Flight microservice: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Airport count");
            throw new ValidationException("Failed to get Airport count", ex);
        }
    }

    /// <summary>
    /// Validates Airport entity for business rules
    /// </summary>
    /// <param name="airport">The Airport to validate</param>
    /// <exception cref="AirportValidationException">Thrown when validation fails</exception>
    private void ValidateAirport(Airport airport)
    {
        if (airport == null)
        {
            throw new AirportValidationException("Airport cannot be null");
        }

        var errors = new Dictionary<string, string[]>();

        // Validate City
        if (string.IsNullOrWhiteSpace(airport.City))
        {
            errors["City"] = new[] { "City is required and cannot be empty" };
        }
        else if (airport.City.Length > 100)
        {
            errors["City"] = new[] { "City cannot exceed 100 characters" };
        }

        // Validate Country
        if (string.IsNullOrWhiteSpace(airport.Country))
        {
            errors["Country"] = new[] { "Country is required and cannot be empty" };
        }
        else if (airport.Country.Length > 100)
        {
            errors["Country"] = new[] { "Country cannot exceed 100 characters" };
        }

        if (errors.Count > 0)
        {
            var errorMessage = $"Airport validation failed with {errors.Count} error(s)";
            throw new AirportValidationException(errorMessage, errors);
        }
    }
}
