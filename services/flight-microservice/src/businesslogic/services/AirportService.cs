using core.domain;
using core.exceptions.businesslogic.services;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.repositories;

using RepositoryAirportNotFoundException = core.exceptions.dataaccess.repositories.AirportNotFoundException;
using RepositoryAirportAlreadyExistsException = core.exceptions.dataaccess.repositories.AirportAlreadyExistsException;

namespace businesslogic.services;

/// <summary>
/// Service implementation for Airport business logic operations
/// Provides high-level operations with validation and business rules
/// </summary>
public class AirportService : IAirportService
{
    private readonly IAirportRepository _airportRepository;

    /// <summary>
    /// Initializes a new instance of AirportService
    /// </summary>
    /// <param name="airportRepository">The Airport repository for data access</param>
    public AirportService(IAirportRepository airportRepository)
    {
        _airportRepository = airportRepository ?? throw new ArgumentNullException(nameof(airportRepository));
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
            var airport = await _airportRepository.GetByIdAsync(id);
            return airport ?? throw new AirportNotFoundException(id);
        }
        catch (RepositoryAirportNotFoundException)
        {
            throw new AirportNotFoundException(id);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to get Airport with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<List<Airport>> GetAllAsync(AirportFilter? filter = null)
    {
        try
        {
            return await _airportRepository.GetAllAsync(filter);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException("Failed to get all Airports", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Airport> CreateAsync(Airport airport)
    {
        ValidateAirport(airport);

        try
        {
            var createdAirport = await _airportRepository.CreateAsync(airport);
            return createdAirport;
        }
        catch (AirportValidationException)
        {
            throw;
        }
        catch (RepositoryAirportAlreadyExistsException)
        {
            throw new AirportBusinessRuleViolationException(
                "UniqueConstraint", 
                $"Airport with ID {airport.Id} already exists");
        }
        catch (Exception ex)
        {
            throw new BaseServiceException("Failed to create Airport", ex);
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

        // Check if airport exists before updating (outside try-catch)
        var exists = await _airportRepository.ExistsAsync(airport.Id);
        if (!exists)
        {
            throw new AirportNotFoundException(airport.Id);
        }

        try
        {
            var updatedAirport = await _airportRepository.UpdateAsync(airport);
            return updatedAirport;
        }
        catch (RepositoryAirportNotFoundException)
        {
            throw new AirportNotFoundException(airport.Id);
        }
        catch (AirportValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to update Airport with ID {airport.Id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            throw new AirportValidationException($"Invalid Airport ID: {id}. ID must be positive.");
        }

        // Check if airport exists before deleting (outside try-catch)
        var exists = await _airportRepository.ExistsAsync(id);
        if (!exists)
        {
            throw new AirportNotFoundException(id);
        }

        try
        {
            return await _airportRepository.DeleteAsync(id);
        }
        catch (RepositoryAirportNotFoundException)
        {
            throw new AirportNotFoundException(id);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to delete Airport with ID {id}", ex);
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
            return await _airportRepository.ExistsAsync(id);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to check existence of Airport with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetCountAsync(AirportFilter? filter = null)
    {
        try
        {
            return await _airportRepository.GetCountAsync(filter);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException("Failed to get Airport count", ex);
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

        // Validate Name
        if (string.IsNullOrWhiteSpace(airport.Name))
        {
            errors["Name"] = new[] { "Name is required and cannot be empty" };
        }
        else if (airport.Name.Length > 200)
        {
            errors["Name"] = new[] { "Name cannot exceed 200 characters" };
        }

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
