using core.domain;
using core.exceptions.businesslogic.services;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.repositories;

using RepositoryPersonNotFoundException = core.exceptions.dataaccess.repositories.PersonNotFoundException;
using RepositoryPersonAlreadyExistsException = core.exceptions.dataaccess.repositories.PersonAlreadyExistsException;

namespace businesslogic.services;

/// <summary>
/// Service implementation for Person business logic operations
/// Provides high-level operations with validation and business rules
/// </summary>
public class PersonService : IPersonService
{
    private readonly IPersonRepository _personRepository;

    /// <summary>
    /// Initializes a new instance of PersonService
    /// </summary>
    /// <param name="personRepository">The Person repository for data access</param>
    public PersonService(IPersonRepository personRepository)
    {
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
    }

    /// <inheritdoc/>
    public async Task<Person> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new PersonValidationException($"Invalid Person ID: {id}. ID must be positive.");
        }

        try
        {
            var person = await _personRepository.GetByIdAsync(id);
            return person ?? throw new PersonNotFoundException(id);
        }
        catch (RepositoryPersonNotFoundException)
        {
            throw new PersonNotFoundException(id);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to get Person with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<List<Person>> GetAllAsync(PersonFilter? filter = null)
    {
        try
        {
            return await _personRepository.GetAllAsync(filter);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException("Failed to get all Persons", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Person> CreateAsync(Person person)
    {
        ValidatePerson(person);

        try
        {
            var createdPerson = await _personRepository.CreateAsync(person);
            return createdPerson;
        }
        catch (PersonValidationException)
        {
            throw;
        }
        catch (RepositoryPersonAlreadyExistsException)
        {
            throw new PersonBusinessRuleViolationException(
                "UniqueConstraint", 
                $"Person with ID {person.Id} already exists");
        }
        catch (Exception ex)
        {
            throw new BaseServiceException("Failed to create Person", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Person> UpdateAsync(Person person)
    {
        if (person.Id <= 0)
        {
            throw new PersonValidationException($"Invalid Person ID: {person.Id}. ID must be positive.");
        }

        ValidatePerson(person);

        // Check if person exists before updating (outside try-catch)
        var exists = await _personRepository.ExistsAsync(person.Id);
        if (!exists)
        {
            throw new PersonNotFoundException(person.Id);
        }

        try
        {
            var updatedPerson = await _personRepository.UpdateAsync(person);
            return updatedPerson;
        }
        catch (RepositoryPersonNotFoundException)
        {
            throw new PersonNotFoundException(person.Id);
        }
        catch (PersonValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to update Person with ID {person.Id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            throw new PersonValidationException($"Invalid Person ID: {id}. ID must be positive.");
        }

        // Check if person exists before deleting (outside try-catch)
        var exists = await _personRepository.ExistsAsync(id);
        if (!exists)
        {
            throw new PersonNotFoundException(id);
        }

        try
        {
            return await _personRepository.DeleteAsync(id);
        }
        catch (RepositoryPersonNotFoundException)
        {
            throw new PersonNotFoundException(id);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to delete Person with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(int id)
    {
        if (id <= 0)
        {
            throw new PersonValidationException($"Invalid Person ID: {id}. ID must be positive.");
        }

        try
        {
            return await _personRepository.ExistsAsync(id);
        }
        catch (Exception ex)
        {
            throw new BaseServiceException($"Failed to check existence of Person with ID {id}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetCountAsync()
    {
        try
        {
            return await _personRepository.GetCountAsync();
        }
        catch (Exception ex)
        {
            throw new BaseServiceException("Failed to get Person count", ex);
        }
    }

    /// <summary>
    /// Validates Person entity for business rules
    /// </summary>
    /// <param name="person">The Person to validate</param>
    /// <exception cref="PersonValidationException">Thrown when validation fails</exception>
    private void ValidatePerson(Person person)
    {
        if (person == null)
        {
            throw new PersonValidationException("Person cannot be null");
        }

        var errors = new Dictionary<string, string[]>();

        // Validate Name
        if (string.IsNullOrWhiteSpace(person.Name))
        {
            errors["Name"] = new[] { "Name is required and cannot be empty" };
        }
        else if (person.Name.Length > 200)
        {
            errors["Name"] = new[] { "Name cannot exceed 200 characters" };
        }

        // Validate Age
        if (person.Age.HasValue)
        {
            if (person.Age < 0)
            {
                errors["Age"] = new[] { "Age cannot be negative" };
            }
            else if (person.Age > 150)
            {
                errors["Age"] = new[] { "Age cannot exceed 150" };
            }
        }

        // Validate Address length (if provided)
        if (!string.IsNullOrEmpty(person.Address) && person.Address.Length > 500)
        {
            errors["Address"] = new[] { "Address cannot exceed 500 characters" };
        }

        // Validate Work length (if provided)
        if (!string.IsNullOrEmpty(person.Work) && person.Work.Length > 200)
        {
            errors["Work"] = new[] { "Work cannot exceed 200 characters" };
        }

        if (errors.Count > 0)
        {
            var errorMessage = $"Person validation failed with {errors.Count} error(s)";
            throw new PersonValidationException(errorMessage, errors);
        }
    }
}
