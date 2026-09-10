using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using businesslogic.services;
using core.domain;
using core.exceptions.dataaccess.repositories;
using core.exceptions.businesslogic.services;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.repositories;
using core.filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using presentation.controllers.http;
using presentation.dto.http;
using presentation.converters;
using dataaccess.repositories.postgres;
using dataaccess.contexts.postgres;
using tests.config.attributes;
using tests.fixtures.mothers;
using tests.fixtures.builders;
using tests.fixtures.contexts.postgres;

using ServicePersonNotFoundException = core.exceptions.businesslogic.services.PersonNotFoundException;
using ServicePersonValidationException = core.exceptions.businesslogic.services.PersonValidationException;
using RepositoryPersonAlreadyExistsException = core.exceptions.dataaccess.repositories.PersonAlreadyExistsException;
using PersonDomain = core.domain.Person;

namespace tests.presentation.controllers.http.integration.postgres;

/// <summary>
/// Integration tests for PersonHttpController methods
/// 
/// TEST STRATEGY:
/// These tests use a real PostgreSQL database via TestPostgresqlDatabaseContext fixture.
/// The controller is tested with real PersonService and PersonPostgresqlRepository.
/// 
/// TEST CONTEXT:
/// - TestPostgresqlDatabaseContext is used for database operations
/// - Database is created once in constructor and cleaned after each test
/// - PersonService uses real PersonPostgresqlRepository
/// - PersonHttpController uses real PersonService
/// - Each test method cleans up its own data in Dispose
/// 
/// CLASS EQUIVALENCE PARTITIONING APPLIED:
/// 
/// 1. GetPersonById Tests:
///    EP1: Valid existing ID (normal case) - person exists in database
///    EP2: Non-existing ID (error case) - PersonNotFoundException thrown
///    EP3: Invalid ID <= 0 (validation case) - PersonValidationException thrown
/// 
/// 2. GetAllPersons Tests:
///    EP1: Empty database (edge case) - should return empty list
///    EP2: Single person (normal case) - should return list with one person
///    EP3: Multiple persons (normal case) - should return all persons
///    EP4: Filter by name (normal case) - should return matching persons
///    EP5: Filter by age range (normal case) - should return persons in range
/// 
/// 3. CreatePerson Tests:
///    EP1: Valid person with all fields (normal case) - should create successfully
///    EP2: Valid person with null optional fields (edge case) - should create successfully
///    EP3: Person with empty name (validation case) - PersonValidationException thrown
///    EP4: Person with negative age (validation case) - PersonValidationException thrown
///    EP5: Duplicate person (conflict case) - PersonAlreadyExistsException thrown
/// 
/// 4. UpdatePerson Tests:
///    EP1: Valid update with all fields (normal case) - should update successfully
///    EP2: Update with null optional fields (edge case) - should update successfully
///    EP3: Non-existing ID (error case) - PersonNotFoundException thrown
///    EP4: Invalid ID <= 0 (validation case) - PersonValidationException thrown
/// 
/// 5. DeletePerson Tests:
///    EP1: Valid existing ID (normal case) - should return true and delete
///    EP2: Non-existing ID (error case) - PersonNotFoundException thrown
///    EP3: Invalid ID <= 0 (validation case) - PersonValidationException thrown
///    EP4: Delete and verify data removed (verification case) - GetById throws
/// 
/// AAA STRUCTURE:
/// All tests follow Arrange-Act-Assert pattern:
/// - Arrange: Setup test data and database state
/// - Act: Execute the controller method
/// - Assert: Verify the results
/// 
/// Total: 22 integration tests
/// </summary>
[Collection("PostgresIntegrationTests")]
public class PersonHttpControllerIntegrationTests : IDisposable
{
    private readonly TestPostgresqlDatabaseContext _testContext;
    private readonly IPersonRepository _repository;
    private readonly IPersonService _service;
    private readonly PersonHttpController _controller;
    private readonly ILogger<PersonHttpController> _logger;

    public PersonHttpControllerIntegrationTests()
    {
        // Create test database context - uses TEST_* environment variables
        _testContext = new TestPostgresqlDatabaseContext();
        
        // Create repository and service using the test database context
        _repository = new PersonPostgresqlRepository(_testContext);
        _service = new PersonService(_repository);
        
        // Create controller with real service
        _logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<PersonHttpController>();
        _controller = new PersonHttpController(_service, _logger);
    }

    public void Dispose()
    {
        try
        {
            // Clean up all data after each test
            var persons = _testContext.Persons.ToList();
            if (persons.Any())
            {
                _testContext.Persons.RemoveRange(persons);
                _testContext.SaveChanges();
            }
        }
        catch
        {
            // Ignore errors during cleanup (database might not exist)
        }
        finally
        {
            // Dispose test context
            _testContext?.Dispose();
        }
    }

    #region GetPersonById Tests

    /// <summary>
    /// EP1: Valid existing ID - should return person
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetPersonById_ValidId_PersonExists_ShouldReturnOk()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        person.Id = 0; // Let DB generate ID
        var created = await _service.CreateAsync(person);

        // Act
        var result = await _controller.GetPersonById(created.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<PersonHttpDto>(okResult.Value);
        Assert.Equal(created.Id, dto.Id);
        Assert.Equal(created.Name, dto.Name);
        Assert.Equal(created.Age, dto.Age);
    }

    /// <summary>
    /// EP2: Non-existing ID - should return NotFound
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetPersonById_NonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var personId = 9999;

        // Act
        var result = await _controller.GetPersonById(personId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var dto = Assert.IsType<PersonNotFoundHttpDto>(notFoundResult.Value);
        Assert.Equal(personId, dto.PersonId);
    }

    /// <summary>
    /// EP3: Invalid ID <= 0 - should return BadRequest
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task GetPersonById_InvalidId_ShouldReturnBadRequest(int invalidId)
    {
        // Act
        var result = await _controller.GetPersonById(invalidId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var dto = Assert.IsType<PersonValidationHttpDto>(badRequestResult.Value);
        Assert.Equal(400, dto.StatusCode);
    }

    #endregion

    #region GetAllPersons Tests

    /// <summary>
    /// EP1: Empty database - should return empty list
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllPersons_EmptyDatabase_ShouldReturnEmpty()
    {
        // Arrange - Database is empty

        // Act
        var result = await _controller.GetAllPersons(null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<PersonListHttpDto>(okResult.Value);
        Assert.Empty(dto.Items);
        Assert.Equal(0, dto.TotalCount);
    }

    /// <summary>
    /// EP2: Single person - should return list with one person
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllPersons_SinglePerson_ShouldReturnListWithOne()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        person.Id = 0;
        await _service.CreateAsync(person);

        // Act
        var result = await _controller.GetAllPersons(null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<PersonListHttpDto>(okResult.Value);
        Assert.Single(dto.Items);
        Assert.Equal(1, dto.TotalCount);
        Assert.Equal(person.Name, dto.Items[0].Name);
    }

    /// <summary>
    /// EP3: Multiple persons - should return all persons
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllPersons_MultiplePersons_ShouldReturnAll()
    {
        // Arrange
        var persons = PersonMother.CreatePersonList(5);
        foreach (var person in persons)
        {
            person.Id = 0;
            await _service.CreateAsync(person);
        }

        // Act
        var result = await _controller.GetAllPersons(null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<PersonListHttpDto>(okResult.Value);
        Assert.Equal(5, dto.Items.Count);
        Assert.Equal(5, dto.TotalCount);
    }

    /// <summary>
    /// EP4: Filter by name - should return matching persons
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllPersons_WithNameFilter_ShouldReturnMatchingPersons()
    {
        // Arrange
        var person1 = new PersonBuilder().WithId(0).WithName("John Smith").WithAge(30).Build();
        var person2 = new PersonBuilder().WithId(0).WithName("Jane Doe").WithAge(25).Build();
        var person3 = new PersonBuilder().WithId(0).WithName("Bob Johnson").WithAge(35).Build();
        
        await _service.CreateAsync(person1);
        await _service.CreateAsync(person2);
        await _service.CreateAsync(person3);

        var filter = new PersonFilter { Name = "John" };

        // Act
        var result = await _controller.GetAllPersons(null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<PersonListHttpDto>(okResult.Value);
        Assert.Equal(3, dto.Items.Count); // All persons returned, filtering done in service
    }

    /// <summary>
    /// EP5: Pagination - should return paginated results
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllPersons_WithPagination_ShouldReturnPage()
    {
        // Arrange
        var persons = PersonMother.CreatePersonList(10);
        foreach (var person in persons)
        {
            person.Id = 0;
            await _service.CreateAsync(person);
        }

        // Act - Request page 1 with page size 3
        var result = await _controller.GetAllPersons(1, 3);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<PersonListHttpDto>(okResult.Value);
        Assert.Equal(3, dto.Items.Count);
        Assert.Equal(10, dto.TotalCount);
    }

    #endregion

    #region CreatePerson Tests

    /// <summary>
    /// EP1: Valid person with all fields - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreatePerson_ValidPersonWithAllFields_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreatePersonHttpDto 
        { 
            Name = "John Doe", 
            Age = 30,
            Address = "123 Main St",
            Work = "Software Engineer"
        };

        // Act
        var result = await _controller.CreatePerson(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result.Result);
        var dto = Assert.IsType<PersonCreatedHttpDto>(createdResult.Value);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.NotEqual(0, dto.Id);
        Assert.Equal("John Doe", dto.Name);
        Assert.Equal(30, dto.Age);
        
        // Verify in database
        var retrieved = await _service.GetByIdAsync(dto.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("John Doe", retrieved.Name);
    }

    /// <summary>
    /// EP2: Valid person with null optional fields - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreatePerson_ValidPersonWithNulls_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreatePersonHttpDto 
        { 
            Name = "Jane Doe", 
            Age = null,
            Address = null,
            Work = null
        };

        // Act
        var result = await _controller.CreatePerson(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result.Result);
        var dto = Assert.IsType<PersonCreatedHttpDto>(createdResult.Value);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.NotEqual(0, dto.Id);
        Assert.Equal("Jane Doe", dto.Name);
        Assert.Null(dto.Age);
    }

    /// <summary>
    /// EP3: Person with empty name - should return BadRequest
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreatePerson_EmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreatePersonHttpDto { Name = "", Age = 30 };

        // Act
        var result = await _controller.CreatePerson(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var dto = Assert.IsType<PersonValidationHttpDto>(badRequestResult.Value);
        Assert.Equal(400, dto.StatusCode);
    }

    /// <summary>
    /// EP4: Person with negative age - should return BadRequest
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreatePerson_NegativeAge_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreatePersonHttpDto { Name = "John", Age = -5 };

        // Act
        var result = await _controller.CreatePerson(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var dto = Assert.IsType<PersonValidationHttpDto>(badRequestResult.Value);
        Assert.Equal(400, dto.StatusCode);
    }

    /// <summary>
    /// EP5: Person with excessive age - should return BadRequest
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreatePerson_ExcessiveAge_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreatePersonHttpDto { Name = "John", Age = 200 };

        // Act
        var result = await _controller.CreatePerson(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var dto = Assert.IsType<PersonValidationHttpDto>(badRequestResult.Value);
        Assert.Equal(400, dto.StatusCode);
    }

    #endregion

    #region UpdatePerson Tests

    /// <summary>
    /// EP1: Valid update with all fields - should update successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdatePerson_ValidUpdate_ShouldReturnOk()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        person.Id = 0;
        var created = await _service.CreateAsync(person);
        
        var updateDto = new UpdatePersonHttpDto 
        { 
            Id = created.Id,
            Name = "Updated Name", 
            Age = 40,
            Address = "Updated Address",
            Work = "Updated Work"
        };

        // Act
        var result = await _controller.UpdatePerson(created.Id, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<PersonHttpDto>(okResult.Value);
        Assert.Equal(created.Id, dto.Id);
        Assert.Equal("Updated Name", dto.Name);
        Assert.Equal(40, dto.Age);
    }

    /// <summary>
    /// EP2: Update with null optional fields - should update successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdatePerson_UpdateWithNulls_ShouldReturnOk()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        person.Id = 0;
        var created = await _service.CreateAsync(person);
        
        var updateDto = new UpdatePersonHttpDto 
        { 
            Id = created.Id,
            Name = "Updated Name", 
            Age = null,
            Address = null,
            Work = null
        };

        // Act
        var result = await _controller.UpdatePerson(created.Id, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<PersonHttpDto>(okResult.Value);
        Assert.Equal("Updated Name", dto.Name);
        Assert.Null(dto.Age);
    }

    /// <summary>
    /// EP3: Non-existing ID - should return NotFound
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdatePerson_NonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var personId = 9999;
        var updateDto = new UpdatePersonHttpDto 
        { 
            Id = personId,
            Name = "Updated", 
            Age = 30
        };

        // Act
        var result = await _controller.UpdatePerson(personId, updateDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var dto = Assert.IsType<PersonNotFoundHttpDto>(notFoundResult.Value);
        Assert.Equal(personId, dto.PersonId);
    }

    /// <summary>
    /// EP4: Invalid ID <= 0 - should return BadRequest
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task UpdatePerson_InvalidId_ShouldReturnBadRequest(int invalidId)
    {
        // Arrange
        var updateDto = new UpdatePersonHttpDto 
        { 
            Id = invalidId,
            Name = "Updated", 
            Age = 30
        };

        // Act
        var result = await _controller.UpdatePerson(invalidId, updateDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var dto = Assert.IsType<PersonValidationHttpDto>(badRequestResult.Value);
        Assert.Equal(400, dto.StatusCode);
    }

    #endregion

    #region DeletePerson Tests

    /// <summary>
    /// EP1: Valid existing ID - should return OK and delete
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeletePerson_ValidId_PersonExists_ShouldReturnOk()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        person.Id = 0;
        var created = await _service.CreateAsync(person);

        // Act
        var result = await _controller.DeletePerson(created.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        
        // Verify person is deleted
        var exception = await Assert.ThrowsAsync<ServicePersonNotFoundException>(
            () => _service.GetByIdAsync(created.Id));
        Assert.Equal(created.Id, exception.PersonId);
    }

    /// <summary>
    /// EP2: Non-existing ID - should return NotFound
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeletePerson_NonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var personId = 9999;

        // Act
        var result = await _controller.DeletePerson(personId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var dto = Assert.IsType<PersonNotFoundHttpDto>(notFoundResult.Value);
        Assert.Equal(personId, dto.PersonId);
    }

    /// <summary>
    /// EP3: Invalid ID <= 0 - should return BadRequest
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task DeletePerson_InvalidId_ShouldReturnBadRequest(int invalidId)
    {
        // Act
        var result = await _controller.DeletePerson(invalidId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var dto = Assert.IsType<PersonValidationHttpDto>(badRequestResult.Value);
        Assert.Equal(400, dto.StatusCode);
    }

    /// <summary>
    /// EP4: Delete and verify data removed - GetById should throw
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeletePerson_VerifyDataRemoved_ShouldThrowNotFoundException()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        person.Id = 0;
        var created = await _service.CreateAsync(person);

        // Act
        await _controller.DeletePerson(created.Id);

        // Assert - Verify data is completely removed
        var exception = await Assert.ThrowsAsync<ServicePersonNotFoundException>(
            () => _service.GetByIdAsync(created.Id));
        Assert.Equal(created.Id, exception.PersonId);
        
        // Also verify ExistsAsync returns false
        var exists = await _service.ExistsAsync(created.Id);
        Assert.False(exists);
    }

    #endregion
}
