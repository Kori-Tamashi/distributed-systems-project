using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using businesslogic.services;
using core.domain;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.repositories;
using dataaccess.contexts.postgres;
using dataaccess.repositories.postgres;
using tests.config.attributes;
using tests.fixtures.builders;
using tests.fixtures.contexts.postgres;
using tests.fixtures.mothers;

using ServicePersonNotFoundException = core.exceptions.businesslogic.services.PersonNotFoundException;
using ServicePersonValidationException = core.exceptions.businesslogic.services.PersonValidationException;
using PersonDomain = core.domain.Person;

namespace tests.businesslogic.services.integration.postgres;

[Collection("PostgresIntegrationTests")]
/// <summary>
/// Integration tests for PersonService
/// 
/// TEST STRATEGY:
/// These tests use a real PostgreSQL database via TestPostgresqlDatabaseContext fixture.
/// Tests are isolated by using a dedicated test database (test_persons).
/// PersonService is tested with real PersonPostgresqlRepository using TestPostgresqlDatabaseContext.
/// 
/// TEST CONTEXT:
/// - TestPostgresqlDatabaseContext is used for database operations
/// - Database is created once in constructor and cleaned after each test
/// - Each test method cleans up its own data in Dispose
/// - Tests share the same database but are isolated by cleanup
/// 
/// CLASS EQUIVALENCE PARTITIONING APPLIED:
/// 
/// 1. GetByIdAsync Tests:
///    EP1: Valid existing ID (normal case) - person exists in database
///    EP2: Non-existing ID (error case) - PersonNotFoundException thrown
///    EP3: Invalid ID <= 0 (validation case) - PersonValidationException thrown
/// 
/// 2. GetAllAsync Tests:
///    EP1: Empty database (edge case) - should return empty list
///    EP2: Single person (normal case) - should return list with one person
///    EP3: Multiple persons (normal case) - should return all persons
///    EP4: Filter by name (normal case) - should return matching persons
///    EP5: Filter by age range (normal case) - should return persons in range
///    EP6: Filter with no matches (edge case) - should return empty list
/// 
/// 3. CreateAsync Tests:
///    EP1: Valid person with all fields (normal case) - should create successfully
///    EP2: Valid person with null optional fields (edge case) - should create successfully
///    EP3: Person with empty name (validation case) - PersonValidationException thrown
///    EP4: Person with negative age (validation case) - PersonValidationException thrown
///    EP5: Person with age > 150 (validation case) - PersonValidationException thrown
/// 
/// 4. UpdateAsync Tests:
///    EP1: Valid update with all fields (normal case) - should update successfully
///    EP2: Update with null optional fields (edge case) - should update successfully
///    EP3: Non-existing ID (error case) - PersonNotFoundException thrown
///    EP4: Invalid ID <= 0 (validation case) - PersonValidationException thrown
///    EP5: Update invalid data (validation case) - PersonValidationException thrown
/// 
/// 5. DeleteAsync Tests:
///    EP1: Valid existing ID (normal case) - should return true and delete
///    EP2: Non-existing ID (error case) - PersonNotFoundException thrown
///    EP3: Invalid ID <= 0 (validation case) - PersonValidationException thrown
///    EP4: Delete and verify data removed (verification case) - GetById throws
/// 
/// 6. ExistsAsync Tests:
///    EP1: Existing ID (normal case) - should return true
///    EP2: Non-existing ID (edge case) - should return false
///    EP3: ID after deletion (verification case) - should return false
///    EP4: Invalid ID <= 0 (validation case) - PersonValidationException thrown
/// 
/// 7. GetCountAsync Tests:
///    EP1: Empty database (edge case) - should return 0
///    EP2: Single person (normal case) - should return 1
///    EP3: Multiple persons (normal case) - should return correct count
/// 
/// AAA STRUCTURE:
/// All tests follow Arrange-Act-Assert pattern:
/// - Arrange: Setup test data and database state
/// - Act: Execute the method under test
/// - Assert: Verify the results
/// 
/// Total: 34 integration tests
/// </summary>
public class PersonServiceIntegrationTests : IDisposable
{
    private readonly TestPostgresqlDatabaseContext _testContext;
    private readonly IPersonRepository _repository;
    private readonly IPersonService _service;

    public PersonServiceIntegrationTests()
    {
        // Create test database context - this uses TEST_* environment variables
        _testContext = new TestPostgresqlDatabaseContext();
        
        // Create repository and service using the test database context
        _repository = new PersonPostgresqlRepository(_testContext);
        _service = new PersonService(_repository);
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

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Valid existing ID - should return person
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_ValidId_PersonExists_ShouldReturnPerson()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        person.Id = 0; // Let DB generate ID
        var created = await _service.CreateAsync(person);

        // Act
        var result = await _service.GetByIdAsync(created.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal(created.Name, result.Name);
        Assert.Equal(created.Age, result.Age);
    }

    /// <summary>
    /// EP2: Non-existing ID - should throw ServicePersonNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_NonExistingId_ShouldThrowPersonNotFoundException()
    {
        // Arrange
        var nonExistentId = 9999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePersonNotFoundException>(
            () => _service.GetByIdAsync(nonExistentId)
        );
        Assert.Equal(nonExistentId, exception.PersonId);
    }

    /// <summary>
    /// EP3: Invalid ID <= 0 - should throw ServicePersonValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task GetByIdAsync_InvalidId_ShouldThrowPersonValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePersonValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid Person ID", exception.Message);
    }

    #endregion

    #region GetAllAsync Tests

    /// <summary>
    /// EP1: Empty database - should return empty list
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_EmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange - Database is empty

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// EP2: Single person - should return list with one person
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_SinglePerson_ShouldReturnListWithOnePerson()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        person.Id = 0;
        await _service.CreateAsync(person);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(person.Name, result[0].Name);
    }

    /// <summary>
    /// EP3: Multiple persons - should return all persons
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_MultiplePersons_ShouldReturnAllPersons()
    {
        // Arrange
        var persons = PersonMother.CreatePersonList(5);
        foreach (var person in persons)
        {
            person.Id = 0;
            await _service.CreateAsync(person);
        }

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
    }

    /// <summary>
    /// EP4: Filter by name - should return matching persons
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithNameFilter_ShouldReturnMatchingPersons()
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
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Contains("John", p.Name));
    }

    /// <summary>
    /// EP5: Filter by age range - should return persons in range
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithAgeRangeFilter_ShouldReturnPersonsInRange()
    {
        // Arrange
        var youngPerson = new PersonBuilder().WithId(0).WithName("Young").WithAge(20).Build();
        var adultPerson = new PersonBuilder().WithId(0).WithName("Adult").WithAge(30).Build();
        var seniorPerson = new PersonBuilder().WithId(0).WithName("Senior").WithAge(60).Build();
        
        await _service.CreateAsync(youngPerson);
        await _service.CreateAsync(adultPerson);
        await _service.CreateAsync(seniorPerson);

        var filter = new PersonFilter { MinAge = 25, MaxAge = 40 };

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result); // Only adultPerson (30) should match
        Assert.Equal(30, result[0].Age);
        Assert.All(result, p => Assert.InRange(p.Age ?? 0, 25, 40));
    }

    /// <summary>
    /// EP6: Filter with no matches - should return empty list
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithFilterNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        person.Id = 0;
        await _service.CreateAsync(person);

        var filter = new PersonFilter { Name = "NonExistent" };

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region CreateAsync Tests

    /// <summary>
    /// EP1: Valid person with all fields - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_ValidPersonWithAllFields_ShouldCreatePerson()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        person.Id = 0;

        // Act
        var result = await _service.CreateAsync(person);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal(person.Name, result.Name);
        Assert.Equal(person.Age, result.Age);
        Assert.Equal(person.Address, result.Address);
        Assert.Equal(person.Work, result.Work);
        
        // Verify in database
        var retrieved = await _service.GetByIdAsync(result.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(result.Id, retrieved.Id);
    }

    /// <summary>
    /// EP2: Valid person with null optional fields - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_ValidPersonWithNulls_ShouldCreatePerson()
    {
        // Arrange
        var person = PersonMother.CreatePersonWithNulls();
        person.Id = 0;

        // Act
        var result = await _service.CreateAsync(person);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal(person.Name, result.Name);
        Assert.Null(result.Age);
        Assert.Null(result.Address);
        Assert.Null(result.Work);
    }

    /// <summary>
    /// EP3: Person with empty name - should throw ServicePersonValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_EmptyName_ShouldThrowPersonValidationException()
    {
        // Arrange
        var person = PersonMother.CreateInvalidPersonWithEmptyName();
        person.Id = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePersonValidationException>(
            () => _service.CreateAsync(person)
        );
        Assert.Contains("Person validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Person with negative age - should throw ServicePersonValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_NegativeAge_ShouldThrowPersonValidationException()
    {
        // Arrange
        var person = PersonMother.CreateInvalidPersonWithNegativeAge();
        person.Id = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePersonValidationException>(
            () => _service.CreateAsync(person)
        );
        Assert.Contains("Person validation failed", exception.Message);
    }

    /// <summary>
    /// EP5: Person with age > 150 - should throw ServicePersonValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_ExcessiveAge_ShouldThrowPersonValidationException()
    {
        // Arrange
        var person = PersonMother.CreateInvalidPersonWithExcessiveAge();
        person.Id = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePersonValidationException>(
            () => _service.CreateAsync(person)
        );
        Assert.Contains("Person validation failed", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// EP1: Valid update with all fields - should update successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_ValidUpdate_ShouldUpdatePerson()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        person.Id = 0;
        var created = await _service.CreateAsync(person);
        
        var updatedPerson = new Person
        {
            Id = created.Id,
            Name = "Updated Name",
            Age = 40,
            Address = "Updated Address",
            Work = "Updated Work"
        };

        // Act
        var result = await _service.UpdateAsync(updatedPerson);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal(40, result.Age);
        Assert.Equal("Updated Address", result.Address);
        Assert.Equal("Updated Work", result.Work);
    }

    /// <summary>
    /// EP2: Update with null optional fields - should update successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_UpdateWithNulls_ShouldUpdatePerson()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        person.Id = 0;
        var created = await _service.CreateAsync(person);
        
        var updatedPerson = new Person
        {
            Id = created.Id,
            Name = "Updated Name",
            Age = null,
            Address = null,
            Work = null
        };

        // Act
        var result = await _service.UpdateAsync(updatedPerson);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Null(result.Age);
        Assert.Null(result.Address);
        Assert.Null(result.Work);
    }

    /// <summary>
    /// EP3: Non-existing ID - should throw PersonNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_PersonNotFound_ShouldThrowPersonNotFoundException()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        person.Id = 9999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePersonNotFoundException>(
            () => _service.UpdateAsync(person)
        );
        Assert.Equal(person.Id, exception.PersonId);
    }

    /// <summary>
    /// EP4: Invalid ID <= 0 - should throw ServicePersonValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task UpdateAsync_InvalidId_ShouldThrowPersonValidationException(int invalidId)
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        person.Id = invalidId;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePersonValidationException>(
            () => _service.UpdateAsync(person)
        );
        Assert.Contains("Invalid Person ID", exception.Message);
    }

    /// <summary>
    /// EP5: Update invalid data - should throw ServicePersonValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_InvalidData_ShouldThrowPersonValidationException()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        person.Id = 0;
        var created = await _service.CreateAsync(person);
        
        var invalidPerson = new Person
        {
            Id = created.Id,
            Name = "", // Invalid: empty name
            Age = -5  // Invalid: negative age
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePersonValidationException>(
            () => _service.UpdateAsync(invalidPerson)
        );
        Assert.Contains("Person validation failed", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Valid existing ID - should return true and delete
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_PersonExists_ShouldReturnTrueAndDelete()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        person.Id = 0;
        var created = await _service.CreateAsync(person);

        // Act
        var result = await _service.DeleteAsync(created.Id);

        // Assert
        Assert.True(result);
        
        // Verify person is deleted
        var exception = await Assert.ThrowsAsync<ServicePersonNotFoundException>(
            () => _service.GetByIdAsync(created.Id)
        );
        Assert.Equal(created.Id, exception.PersonId);
    }

    /// <summary>
    /// EP2: Non-existing ID - should throw ServicePersonNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_PersonNotFound_ShouldThrowPersonNotFoundException()
    {
        // Arrange
        var nonExistentId = 9999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePersonNotFoundException>(
            () => _service.DeleteAsync(nonExistentId)
        );
        Assert.Equal(nonExistentId, exception.PersonId);
    }

    /// <summary>
    /// EP3: Invalid ID <= 0 - should throw ServicePersonValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task DeleteAsync_InvalidId_ShouldThrowPersonValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePersonValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid Person ID", exception.Message);
    }

    /// <summary>
    /// EP4: Delete and verify data removed - GetById should throw
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_VerifyDataRemoved_ShouldThrowNotFoundException()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        person.Id = 0;
        var created = await _service.CreateAsync(person);

        // Act
        await _service.DeleteAsync(created.Id);

        // Assert - Verify data is completely removed
        var exception = await Assert.ThrowsAsync<ServicePersonNotFoundException>(
            () => _service.GetByIdAsync(created.Id)
        );
        Assert.Equal(created.Id, exception.PersonId);
        
        // Also verify ExistsAsync returns false
        var exists = await _service.ExistsAsync(created.Id);
        Assert.False(exists);
    }

    #endregion

    #region ExistsAsync Tests

    /// <summary>
    /// EP1: Existing ID - should return true
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_PersonExists_ShouldReturnTrue()
    {
        // Arrange
        var person = new PersonBuilder().WithId(0).WithName("Test Person").WithAge(30).Build();
        var created = await _service.CreateAsync(person);

        // Act
        var result = await _service.ExistsAsync(created.Id);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// EP2: Non-existing ID - should return false
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_PersonNotFound_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = 9999;

        // Act
        var result = await _service.ExistsAsync(nonExistentId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP3: ID after deletion - should return false
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_AfterDeletion_ShouldReturnFalse()
    {
        // Arrange
        var person = new PersonBuilder().WithId(0).WithName("Test Person").WithAge(30).Build();
        var created = await _service.CreateAsync(person);
        await _service.DeleteAsync(created.Id);

        // Act
        var result = await _service.ExistsAsync(created.Id);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP4: Invalid ID <= 0 - should throw PersonValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task ExistsAsync_InvalidId_ShouldThrowPersonValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePersonValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid Person ID", exception.Message);
    }

    #endregion

    #region GetCountAsync Tests

    /// <summary>
    /// EP1: Empty database - should return 0
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetCountAsync_EmptyDatabase_ShouldReturnZero()
    {
        // Arrange - Database is empty

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(0, result);
    }

    /// <summary>
    /// EP2: Single person - should return 1
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetCountAsync_SinglePerson_ShouldReturnOne()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        person.Id = 0;
        await _service.CreateAsync(person);

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(1, result);
    }

    /// <summary>
    /// EP3: Multiple persons - should return correct count
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetCountAsync_MultiplePersons_ShouldReturnCorrectCount()
    {
        // Arrange
        var persons = PersonMother.CreatePersonList(7);
        foreach (var person in persons)
        {
            person.Id = 0;
            await _service.CreateAsync(person);
        }

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(7, result);
    }

    #endregion
}
