using System.Linq.Expressions;
using core.domain;
using core.exceptions.dataaccess.repositories;
using core.filters;
using core.interfaces.dataaccess.repositories;
using dataaccess.converters.postgres;
using dataaccess.contexts.postgres;
using dataaccess.models.postgres;
using Microsoft.EntityFrameworkCore;

using PersonDomain = core.domain.Person;
using PersonPostgresqlModel = dataaccess.models.postgres.PersonPostgresqlModel;

namespace dataaccess.repositories.postgres;

/// <summary>
/// PostgreSQL implementation of IPersonRepository
/// Provides data access operations for Person entities
/// </summary>
public class PersonPostgresqlRepository : IPersonRepository
{
    private readonly PostgresqlDatabaseContext _context;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="context">PostgreSQL database context</param>
    public PersonPostgresqlRepository(PostgresqlDatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets a Person by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Person</param>
    /// <returns>The Person entity if found</returns>
    /// <exception cref="PersonNotFoundException">Thrown when Person with specified id is not found</exception>
    /// <exception cref="PersonDatabaseException">Thrown when database operation fails</exception>
    public async Task<PersonDomain> GetByIdAsync(int id)
    {
        try
        {
            var model = await _context.Persons.FindAsync(id)
                ?? throw new PersonNotFoundException(id);

            return PersonPostgresqlConverter.ToDomain(model);
        }
        catch (PersonNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new PersonDatabaseException($"Failed to get Person by id {id}", ex);
        }
    }

    /// <summary>
    /// Gets all Person entities
    /// </summary>
    /// <param name="filter">Optional filter criteria for querying persons (null returns all)</param>
    /// <returns>List of Person entities matching the filter or all persons if filter is null</returns>
    /// <exception cref="PersonDatabaseException">Thrown when database operation fails</exception>
    public async Task<List<PersonDomain>> GetAllAsync(PersonFilter? filter = null)
    {
        try
        {
            var query = _context.Persons.AsQueryable();

            // Apply filter if provided
            if (filter != null)
            {
                query = ApplyFilter(query, filter);
            }

            var models = await query.ToListAsync();
            return PersonPostgresqlConverter.ToDomainList(models);
        }
        catch (Exception ex)
        {
            throw new PersonDatabaseException("Failed to get all Persons", ex);
        }
    }

    /// <summary>
    /// Creates a new Person
    /// </summary>
    /// <param name="person">The Person entity to create</param>
    /// <returns>The created Person entity with generated Id</returns>
    /// <exception cref="PersonAlreadyExistsException">Thrown when Person with same id already exists</exception>
    /// <exception cref="PersonDatabaseException">Thrown when database operation fails</exception>
    public async Task<PersonDomain> CreateAsync(PersonDomain person)
    {
        try
        {
            // Check if Person already exists
            if (await ExistsAsync(person.Id))
            {
                throw new PersonAlreadyExistsException(person.Id);
            }

            var model = PersonPostgresqlConverter.ToModel(person);
            _context.Persons.Add(model);
            await _context.SaveChangesAsync();

            // Return updated entity with generated Id
            return await GetByIdAsync(model.Id);
        }
        catch (PersonAlreadyExistsException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new PersonDatabaseException("Failed to create Person", ex);
        }
    }

    /// <summary>
    /// Updates an existing Person
    /// </summary>
    /// <param name="person">The Person entity to update</param>
    /// <returns>The updated Person entity</returns>
    /// <exception cref="PersonNotFoundException">Thrown when Person with specified id is not found</exception>
    /// <exception cref="PersonDatabaseException">Thrown when database operation fails</exception>
    public async Task<PersonDomain> UpdateAsync(PersonDomain person)
    {
        try
        {
            var existingModel = await _context.Persons.FindAsync(person.Id)
                ?? throw new PersonNotFoundException(person.Id);

            // Update properties
            existingModel.Name = person.Name;
            existingModel.Age = person.Age;
            existingModel.Address = person.Address;
            existingModel.Work = person.Work;

            await _context.SaveChangesAsync();

            return PersonPostgresqlConverter.ToDomain(existingModel);
        }
        catch (PersonNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new PersonDatabaseException($"Failed to update Person with id {person.Id}", ex);
        }
    }

    /// <summary>
    /// Deletes a Person by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the Person to delete</param>
    /// <returns>True if Person was deleted, false if not found</returns>
    /// <exception cref="PersonDatabaseException">Thrown when database operation fails</exception>
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var model = await _context.Persons.FindAsync(id);

            if (model == null)
            {
                return false;
            }

            _context.Persons.Remove(model);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new PersonDatabaseException($"Failed to delete Person with id {id}", ex);
        }
    }

    /// <summary>
    /// Checks if a Person exists by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier to check</param>
    /// <returns>True if Person exists, false otherwise</returns>
    /// <exception cref="PersonDatabaseException">Thrown when database operation fails</exception>
    public async Task<bool> ExistsAsync(int id)
    {
        try
        {
            return await _context.Persons.AnyAsync(p => p.Id == id);
        }
        catch (Exception ex)
        {
            throw new PersonDatabaseException($"Failed to check if Person with id {id} exists", ex);
        }
    }

    /// <summary>
    /// Gets the total count of Person entities
    /// </summary>
    /// <returns>Total number of Person entities</returns>
    /// <exception cref="PersonDatabaseException">Thrown when database operation fails</exception>
    public async Task<int> GetCountAsync()
    {
        try
        {
            return await _context.Persons.CountAsync();
        }
        catch (Exception ex)
        {
            throw new PersonDatabaseException("Failed to get Persons count", ex);
        }
    }

    /// <summary>
    /// Applies filter criteria to the query
    /// </summary>
    /// <param name="query">Base query</param>
    /// <param name="filter">Filter criteria</param>
    /// <returns>Filtered query</returns>
    private IQueryable<PersonPostgresqlModel> ApplyFilter(
        IQueryable<PersonPostgresqlModel> query,
        PersonFilter filter)
    {
        if (filter == null)
            return query;

        // Filter by Name (case-insensitive partial match)
        if (!string.IsNullOrEmpty(filter.Name))
        {
            var name = filter.Name.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(name));
        }

        // Filter by MinAge
        if (filter.MinAge.HasValue)
        {
            query = query.Where(p => p.Age >= filter.MinAge.Value);
        }

        // Filter by MaxAge
        if (filter.MaxAge.HasValue)
        {
            query = query.Where(p => p.Age <= filter.MaxAge.Value);
        }

        // Filter by Address (case-insensitive partial match)
        if (!string.IsNullOrEmpty(filter.Address))
        {
            var address = filter.Address.ToLower();
            query = query.Where(p => p.Address != null && p.Address.ToLower().Contains(address));
        }

        // Filter by Work (case-insensitive partial match)
        if (!string.IsNullOrEmpty(filter.Work))
        {
            var work = filter.Work.ToLower();
            query = query.Where(p => p.Work != null && p.Work.ToLower().Contains(work));
        }

        return query;
    }
}
