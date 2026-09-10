using core.exceptions.businesslogic.services;
using core.exceptions.dataaccess.repositories;
using core.interfaces.businesslogic.services;
using Microsoft.AspNetCore.Mvc;
using presentation.converters;
using presentation.dto.http;
using presentation.exceptions.http;

using RepositoryPersonNotFoundException = core.exceptions.dataaccess.repositories.PersonNotFoundException;
using ServicePersonNotFoundException = core.exceptions.businesslogic.services.PersonNotFoundException;
using ServicePersonValidationException = core.exceptions.businesslogic.services.PersonValidationException;
using ServicePersonBusinessRuleViolationException = core.exceptions.businesslogic.services.PersonBusinessRuleViolationException;
using RepositoryPersonAlreadyExistsException = core.exceptions.dataaccess.repositories.PersonAlreadyExistsException;
using HttpPersonValidationException = presentation.exceptions.http.HttpPersonValidationException;
using HttpPersonBusinessRuleViolationException = presentation.exceptions.http.HttpPersonBusinessRuleViolationException;

// Keep original exception names for catch blocks
using PersonValidationException = core.exceptions.businesslogic.services.PersonValidationException;
using PersonBusinessRuleViolationException = core.exceptions.businesslogic.services.PersonBusinessRuleViolationException;

namespace presentation.controllers.http;

/// <summary>
/// HTTP Controller for Person CRUD operations
/// Implements RESTful API endpoints for managing Person entities
/// </summary>
[ApiController]
[Route("api/v1/persons")]
[Produces("application/json")]
[Consumes("application/json")]
public class PersonHttpController : ControllerBase
{
    private readonly IPersonService _personService;
    private readonly ILogger<PersonHttpController> _logger;

    /// <summary>
    /// Initializes a new instance of the PersonHttpController
    /// </summary>
    /// <param name="personService">The Person business logic service</param>
    /// <param name="logger">The logger for the controller</param>
    public PersonHttpController(
        IPersonService personService,
        ILogger<PersonHttpController> logger)
    {
        _personService = personService ?? throw new ArgumentNullException(nameof(personService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a Person by their unique identifier
    /// </summary>
    /// <param name="personId">The unique identifier of the Person</param>
    /// <returns>Person data with HTTP 200 OK</returns>
    /// <response code="200">Returns the Person</response>
    /// <response code="404">Person not found</response>
    /// <response code="500">Server error</response>
    [HttpGet("{personId}")]
    [ProducesResponseType(typeof(PersonHttpDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PersonNotFoundHttpDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PersonValidationHttpDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PersonHttpDto>> GetPersonById(int personId)
    {
        try
        {
            _logger.LogDebug("Getting person by ID: {PersonId}", personId);
            
            var person = await _personService.GetByIdAsync(personId);
            var dto = PersonConverter.ToDTO(person);
            
            _logger.LogInformation("Person retrieved successfully: {PersonId}", personId);
            return Ok(dto);
        }
        catch (ServicePersonNotFoundException ex)
        {
            _logger.LogWarning(ex, "Person not found: {PersonId}", personId);
            var notFoundDto = PersonConverter.ToNotFoundDTO(personId);
            return NotFound(notFoundDto);
        }
        catch (ServicePersonValidationException ex)
        {
            _logger.LogWarning(ex, "Person validation failed for ID: {PersonId}", personId);
            var validationErrors = ex.Data.Count > 0 
                ? (Dictionary<string, string[]>)ex.Data 
                : new Dictionary<string, string[]>();
            var validationDto = PersonConverter.ToValidationDTO(validationErrors, ex.Message);
            return BadRequest(validationDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting person by ID: {PersonId}", personId);
            throw new HttpPersonServiceException(
                new core.exceptions.businesslogic.services.BaseServiceException(
                    "An error occurred while retrieving the person", ex));
        }
    }

    /// <summary>
    /// Gets all Persons
    /// </summary>
    /// <returns>List of all Persons with HTTP 200 OK</returns>
    /// <response code="200">Returns the list of Persons</response>
    /// <response code="500">Server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(PersonListHttpDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PersonListHttpDto>> GetAllPersons(
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        try
        {
            _logger.LogDebug("Getting all persons");
            
            var persons = await _personService.GetAllAsync();
            var totalCount = persons.Count;
            
            // Apply pagination if requested
            if (page.HasValue && pageSize.HasValue && pageSize.Value > 0)
            {
                var skip = (page.Value - 1) * pageSize.Value;
                persons = persons.Skip(skip).Take(pageSize.Value).ToList();
            }
            
            var listDto = PersonConverter.ToListDTO(
                persons,
                totalCount,
                page ?? 1,
                pageSize ?? totalCount);
            
            _logger.LogInformation("Retrieved {Count} persons", persons.Count);
            return Ok(listDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all persons");
            throw new HttpPersonServiceException(
                new core.exceptions.businesslogic.services.BaseServiceException(
                    "An error occurred while retrieving persons", ex));
        }
    }

    /// <summary>
    /// Creates a new Person
    /// </summary>
    /// <param name="createDto">The Person data to create</param>
    /// <returns>Created Person with HTTP 201 Created and Location header</returns>
    /// <response code="201">Person created successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="409">Person already exists</response>
    /// <response code="500">Server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(PersonCreatedHttpDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(PersonValidationHttpDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PersonAlreadyExistsHttpDto), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PersonCreatedHttpDto>> CreatePerson(
        [FromBody] CreatePersonHttpDto createDto)
    {
        try
        {
            _logger.LogDebug("Creating new person");
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for person creation");
                var validationErrors = new Dictionary<string, List<string>>();
                foreach (var keyValuePair in ModelState)
                {
                    var key = keyValuePair.Key ?? "Unknown";
                    foreach (var error in keyValuePair.Value.Errors)
                    {
                        if (!validationErrors.ContainsKey(key))
                        {
                            validationErrors[key] = new List<string>();
                        }
                        validationErrors[key].Add(error.ErrorMessage);
                    }
                }
                
                // Convert to string[] for DTO
                var stringArrayErrors = validationErrors.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToArray());
                
                var validationDto = PersonConverter.ToValidationDTO(
                    stringArrayErrors,
                    "Person validation failed");
                return BadRequest(validationDto);
            }
            
            // Convert DTO to domain entity
            var person = PersonConverter.ToCreateDomain(createDto);
            
            // Create the person
            var createdPerson = await _personService.CreateAsync(person);
            
            // Build location URI (handle null Request in tests)
            var scheme = HttpContext?.Request?.Scheme ?? "http";
            var host = HttpContext?.Request?.Host.ToString() ?? "localhost";
            var location = $"{scheme}://{host}/api/v1/persons/{createdPerson.Id}";
            
            var createdDto = PersonConverter.ToCreatedDTO(createdPerson, location);
            
            _logger.LogInformation("Person created successfully with ID: {PersonId}", createdPerson.Id);
            return Created(location, createdDto);
        }
        catch (PersonValidationException ex)
        {
            _logger.LogWarning(ex, "Person validation failed");
            var validationErrors = ex.Data.Count > 0 
                ? (Dictionary<string, string[]>)ex.Data 
                : new Dictionary<string, string[]>();
            var validationDto = PersonConverter.ToValidationDTO(validationErrors, ex.Message);
            return BadRequest(validationDto);
        }
        catch (RepositoryPersonAlreadyExistsException ex)
        {
            _logger.LogWarning(ex, "Person already exists: {PersonId}", ex.PersonId);
            var alreadyExistsDto = new presentation.dto.http.PersonAlreadyExistsHttpDto(
                ex.PersonId,
                "Person already exists");
            return Conflict(alreadyExistsDto);
        }
        catch (PersonBusinessRuleViolationException ex)
        {
            _logger.LogWarning(ex, "Person business rule violated");
            var ruleException = new HttpPersonBusinessRuleViolationException(ex);
            return UnprocessableEntity(ruleException.ToProblemDetails());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating person: {Error}", ex.Message);
            throw new HttpPersonServiceException(
                new core.exceptions.businesslogic.services.BaseServiceException(
                    "An error occurred while creating the person"),
                ex);
        }
    }

    /// <summary>
    /// Updates an existing Person
    /// </summary>
    /// <param name="personId">The unique identifier of the Person to update</param>
    /// <param name="updateDto">The updated Person data</param>
    /// <returns>Updated Person with HTTP 200 OK</returns>
    /// <response code="200">Person updated successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="404">Person not found</response>
    /// <response code="409">Person already exists</response>
    /// <response code="500">Server error</response>
    [HttpPatch("{personId}")]
    [ProducesResponseType(typeof(PersonHttpDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PersonValidationHttpDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PersonNotFoundHttpDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PersonAlreadyExistsHttpDto), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PersonHttpDto>> UpdatePerson(
        int personId,
        [FromBody] UpdatePersonHttpDto updateDto)
    {
        try
        {
            _logger.LogDebug("Updating person with ID: {PersonId}", personId);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for person update");
                var validationErrors = new Dictionary<string, List<string>>();
                foreach (var keyValuePair in ModelState)
                {
                    var key = keyValuePair.Key ?? "Unknown";
                    foreach (var error in keyValuePair.Value.Errors)
                    {
                        if (!validationErrors.ContainsKey(key))
                        {
                            validationErrors[key] = new List<string>();
                        }
                        validationErrors[key].Add(error.ErrorMessage);
                    }
                }
                
                // Convert to string[] for DTO
                var stringArrayErrors = validationErrors.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToArray());
                
                var validationDto = PersonConverter.ToValidationDTO(
                    stringArrayErrors,
                    "Person validation failed");
                return BadRequest(validationDto);
            }
            
            // Ensure IDs match
            if (personId != updateDto.Id)
            {
                _logger.LogWarning("ID mismatch: route ID {RouteId} != body ID {BodyId}", personId, updateDto.Id);
                var validationDto = PersonConverter.ToValidationDTO(
                    new Dictionary<string, string[]> { { "Id", new[] { "ID in URL must match ID in request body" } } },
                    "ID mismatch");
                return BadRequest(validationDto);
            }
            
            // Convert DTO to domain entity
            var person = PersonConverter.ToUpdateDomain(updateDto);
            
            // Update the person
            var updatedPerson = await _personService.UpdateAsync(person);
            var dto = PersonConverter.ToDTO(updatedPerson);
            
            _logger.LogInformation("Person updated successfully: {PersonId}", personId);
            return Ok(dto);
        }
        catch (ServicePersonNotFoundException ex)
        {
            _logger.LogWarning(ex, "Person not found for update: {PersonId}", personId);
            var notFoundDto = PersonConverter.ToNotFoundDTO(personId);
            return NotFound(notFoundDto);
        }
        catch (ServicePersonValidationException ex)
        {
            _logger.LogWarning(ex, "Person validation failed during update");
            var validationErrors = ex.Data.Count > 0 
                ? (Dictionary<string, string[]>)ex.Data 
                : new Dictionary<string, string[]>();
            var validationDto = PersonConverter.ToValidationDTO(validationErrors, ex.Message);
            return BadRequest(validationDto);
        }
        catch (ServicePersonBusinessRuleViolationException ex)
        {
            _logger.LogWarning(ex, "Person business rule violated during update");
            var ruleException = new HttpPersonBusinessRuleViolationException(ex);
            return UnprocessableEntity(ruleException.ToProblemDetails());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating person: {PersonId}", personId);
            throw new HttpPersonServiceException(
                new core.exceptions.businesslogic.services.BaseServiceException(
                    "An error occurred while updating the person", ex));
        }
    }

    /// <summary>
    /// Deletes a Person by their unique identifier
    /// </summary>
    /// <param name="personId">The unique identifier of the Person to delete</param>
    /// <returns>HTTP 200 OK with confirmation</returns>
    /// <response code="200">Person deleted successfully</response>
    /// <response code="404">Person not found</response>
    /// <response code="500">Server error</response>
    [HttpDelete("{personId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PersonNotFoundHttpDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PersonValidationHttpDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeletePerson(int personId)
    {
        try
        {
            _logger.LogDebug("Deleting person with ID: {PersonId}", personId);
            
            var deleted = await _personService.DeleteAsync(personId);
            
            if (!deleted)
            {
                _logger.LogWarning("Person deletion failed: {PersonId}", personId);
                var notFoundDto = PersonConverter.ToNotFoundDTO(personId);
                return NotFound(notFoundDto);
            }
            
            _logger.LogInformation("Person deleted successfully: {PersonId}", personId);
            return Ok(new { message = $"Person with ID {personId} was deleted successfully" });
        }
        catch (ServicePersonNotFoundException ex)
        {
            _logger.LogWarning(ex, "Person not found for deletion: {PersonId}", personId);
            var notFoundDto = PersonConverter.ToNotFoundDTO(personId);
            return NotFound(notFoundDto);
        }
        catch (ServicePersonValidationException ex)
        {
            _logger.LogWarning(ex, "Person validation failed for ID: {PersonId}", personId);
            var validationErrors = ex.Data.Count > 0 
                ? (Dictionary<string, string[]>)ex.Data 
                : new Dictionary<string, string[]>();
            var validationDto = PersonConverter.ToValidationDTO(validationErrors, ex.Message);
            return BadRequest(validationDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting person: {PersonId} - {Error}", personId, ex.Message);
            throw new HttpPersonServiceException(
                new core.exceptions.businesslogic.services.BaseServiceException(
                    "An error occurred while deleting the person"),
                ex);
        }
    }
}
