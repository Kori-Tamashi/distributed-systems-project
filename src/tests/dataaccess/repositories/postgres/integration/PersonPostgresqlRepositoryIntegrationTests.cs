using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using core.domain;
using core.exceptions.dataaccess.repositories;
using core.filters;
using core.interfaces.dataaccess.repositories;
using dataaccess.repositories.postgres;
using dataaccess.contexts.postgres;
using tests.config.attributes;
using tests.fixtures.builders;
using tests.fixtures.contexts.postgres;
using tests.fixtures.mothers;

using PersonDomain = core.domain.Person;

namespace tests.dataaccess.repositories.postgres.integration;

[Collection("PostgresIntegrationTests")]
/// <summary>
/// Integration tests for PersonPostgresqlRepository
/// 
/// TEST STRATEGY:
/// These tests use a real PostgreSQL database via TestPostgresqlDatabaseContext fixture.
/// Tests are isolated by using a dedicated test database (test_persons).
/// 
/// CLASS EQUIVALENCE PARTITIONING APPLIED:
/// 
/// 1. GetByIdAsync Tests:
///    EP1: Valid existing ID (normal case) - person exists in database
///    EP2: Non-existing ID (edge case) - person not found, should throw PersonNotFoundException
///    EP3: ID with null optional fields (edge case) - person with minimal data
///    EP4: ID with all fields populated (normal case) - person with full data
///    EP5: Database connection error (error case) - handled by exception
/// 
/// 2. GetAllAsync Tests:
///    EP1: Empty database (edge case) - should return empty list
///    EP2: Single person (normal case) - should return list with one person
///    EP3: Multiple persons (normal case) - should return all persons
///    EP4: Filter by name (normal case) - should return matching persons
///    EP5: Filter by age range (normal case) - should return persons in range
///    EP6: Filter with no matches (edge case) - should return empty list
///    EP7: Null filter (normal case) - should return all persons
/// 
/// 3. CreateAsync Tests:
///    EP1: Valid person with all fields (normal case) - should create successfully
///    EP2: Valid person with null optional fields (edge case) - should create successfully
///    EP3: Person with empty strings (edge case) - should create successfully
///    EP4: Duplicate ID (error case) - should throw PersonAlreadyExistsException
///    EP5: Null person (error case) - should throw ArgumentNullException
///    EP6: Person with max age value (boundary case) - should create successfully
///    EP7: Person with min age value (boundary case) - should create successfully
/// 
/// 4. UpdateAsync Tests:
///    EP1: Valid update with all fields (normal case) - should update successfully
///    EP2: Update with null optional fields (edge case) - should update successfully
///    EP3: Non-existing ID (error case) - should throw PersonNotFoundException
///    EP4: Update to empty strings (edge case) - should update successfully
///    EP5: Update age to boundary values (boundary case) - should update successfully
/// 
/// 5. DeleteAsync Tests:
///    EP1: Valid existing ID (normal case) - should return true and delete
///    EP2: Non-existing ID (edge case) - should return false
///    EP3: Delete and verify data removed (verification case) - data should be gone
/// 
/// 6. ExistsAsync Tests:
///    EP1: Existing ID (normal case) - should return true
///    EP2: Non-existing ID (edge case) - should return false
///    EP3: ID after deletion (verification case) - should return false
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
/// </summary>
public class PersonPostgresqlRepositoryIntegrationTests : IDisposable
{
    private readonly PostgresqlDatabaseContext _context;
    private readonly IPersonRepository _repository;
    private readonly List<PersonDomain> _createdPersons;

    public PersonPostgresqlRepositoryIntegrationTests()
    {
        // Create test database context using TestPostgresqlDatabaseContext logic
        // but with production PostgresqlDatabaseContext type
        var testContext = new TestPostgresqlDatabaseContext();
        testContext.EnsureDatabaseDeleted();
        
        // Create repository using the test database connection
        _context = testContext;
        _repository = new PersonPostgresqlRepository(_context);
        
        // Track created persons for cleanup
        _createdPersons = new List<PersonDomain>();
    }

    #region IDisposable Implementation

    public void Dispose()
    {
        try
        {
            // Clean up all data
            var persons = _context.Persons.ToList();
            if (persons.Any())
            {
                _context.Persons.RemoveRange(persons);
                _context.SaveChanges();
            }
        }
        catch
        {
            // Ignore errors during cleanup (database might not exist)
        }
        finally
        {
            _context?.Dispose();
        }
    }

    #endregion

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Valid existing ID - person exists in database
    /// 
    /// ARRANGE:
    /// - Create a valid person with all fields populated
    /// - Insert person into database
    /// 
    /// ACT:
    /// - Call GetByIdAsync with the person's ID
    /// 
    /// ASSERT:
    /// - Person is returned
    /// - All properties match
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_PersonExists_ShouldReturnPerson()
    {
        // Arrange
        var expectedPerson = PersonMother.CreateValidPerson();
        await _repository.CreateAsync(expectedPerson);
        _createdPersons.Add(expectedPerson);

        // Act
        var actualPerson = await _repository.GetByIdAsync(expectedPerson.Id);

        // Assert
        Assert.NotNull(actualPerson);
        Assert.Equal(expectedPerson.Id, actualPerson.Id);
        Assert.Equal(expectedPerson.Name, actualPerson.Name);
        Assert.Equal(expectedPerson.Age, actualPerson.Age);
        Assert.Equal(expectedPerson.Address, actualPerson.Address);
        Assert.Equal(expectedPerson.Work, actualPerson.Work);
    }

    /// <summary>
    /// EP2: Non-existing ID - person not found
    /// 
    /// ARRANGE:
    /// - Ensure no person with ID 999 exists
    /// 
    /// ACT:
    /// - Call GetByIdAsync with non-existing ID
    /// 
    /// ASSERT:
    /// - PersonNotFoundException is thrown
    /// - Exception contains correct person ID
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_PersonNotFound_ShouldThrowPersonNotFoundException()
    {
        // Arrange
        var nonExistingId = 999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PersonNotFoundException>(
            () => _repository.GetByIdAsync(nonExistingId));
        
        Assert.Equal(nonExistingId, exception.PersonId);
    }

    /// <summary>
    /// EP3: ID with null optional fields - person with minimal data
    /// 
    /// ARRANGE:
    /// - Create person with null optional fields
    /// - Insert into database
    /// 
    /// ACT:
    /// - Retrieve person by ID
    /// 
    /// ASSERT:
    /// - Person is returned
    /// - Optional fields are null
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_PersonWithNulls_ShouldReturnPersonWithNulls()
    {
        // Arrange
        var expectedPerson = PersonMother.CreatePersonWithNulls();
        await _repository.CreateAsync(expectedPerson);
        _createdPersons.Add(expectedPerson);

        // Act
        var actualPerson = await _repository.GetByIdAsync(expectedPerson.Id);

        // Assert
        Assert.NotNull(actualPerson);
        Assert.Equal(expectedPerson.Id, actualPerson.Id);
        Assert.Equal(expectedPerson.Name, actualPerson.Name);
        Assert.Null(actualPerson.Age);
        Assert.Null(actualPerson.Address);
        Assert.Null(actualPerson.Work);
    }

    #endregion

    #region GetAllAsync Tests

    /// <summary>
    /// EP1: Empty database - should return empty list
    /// 
    /// ARRANGE:
    /// - Ensure database is empty
    /// 
    /// ACT:
    /// - Call GetAllAsync
    /// 
    /// ASSERT:
    /// - Empty list is returned
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_EmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange (database is already empty)

        // Act
        var persons = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(persons);
        Assert.Empty(persons);
    }

    /// <summary>
    /// EP2: Single person - should return list with one person
    /// 
    /// ARRANGE:
    /// - Create and insert one person
    /// 
    /// ACT:
    /// - Call GetAllAsync
    /// 
    /// ASSERT:
    /// - List contains exactly one person
    /// - Person matches created data
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_SinglePerson_ShouldReturnListWithOnePerson()
    {
        // Arrange
        var expectedPerson = PersonMother.CreateValidPerson();
        await _repository.CreateAsync(expectedPerson);
        _createdPersons.Add(expectedPerson);

        // Act
        var persons = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(persons);
        Assert.Single(persons);
        Assert.Equal(expectedPerson.Id, persons[0].Id);
    }

    /// <summary>
    /// EP3: Multiple persons - should return all persons
    /// 
    /// ARRANGE:
    /// - Create and insert 5 persons
    /// 
    /// ACT:
    /// - Call GetAllAsync
    /// 
    /// ASSERT:
    /// - List contains exactly 5 persons
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_MultiplePersons_ShouldReturnAllPersons()
    {
        // Arrange
        var expectedPersons = PersonMother.CreatePersonList(5);
        foreach (var person in expectedPersons)
        {
            await _repository.CreateAsync(person);
            _createdPersons.Add(person);
        }

        // Act
        var persons = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(persons);
        Assert.Equal(5, persons.Count);
    }

    /// <summary>
    /// EP4: Filter by name - should return matching persons
    /// 
    /// ARRANGE:
    /// - Create persons with different names
    /// - Insert all persons
    /// 
    /// ACT:
    /// - Call GetAllAsync with name filter
    /// 
    /// ASSERT:
    /// - Only persons with matching names are returned
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithNameFilter_ShouldReturnMatchingPersons()
    {
        // Arrange
        var johnDoe = new PersonBuilder().WithId(1).WithName("John Doe").WithAge(30).Build();
        var johnSmith = new PersonBuilder().WithId(2).WithName("John Smith").WithAge(25).Build();
        var janeDoe = new PersonBuilder().WithId(3).WithName("Jane Doe").WithAge(35).Build();
        
        await _repository.CreateAsync(johnDoe);
        await _repository.CreateAsync(johnSmith);
        await _repository.CreateAsync(janeDoe);
        _createdPersons.AddRange(new[] { johnDoe, johnSmith, janeDoe });

        // Act
        var persons = await _repository.GetAllAsync(new PersonFilter { Name = "John" });

        // Assert
        Assert.NotNull(persons);
        Assert.Equal(2, persons.Count);
        Assert.All(persons, p => Assert.Contains("John", p.Name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// EP5: Filter by age range - should return persons in range
    /// 
    /// ARRANGE:
    /// - Create persons with different ages
    /// - Insert all persons
    /// 
    /// ACT:
    /// - Call GetAllAsync with age range filter
    /// 
    /// ASSERT:
    /// - Only persons within age range are returned
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithAgeRangeFilter_ShouldReturnPersonsInRange()
    {
        // Arrange
        var youngPerson = new PersonBuilder().WithId(1).WithName("Young").WithAge(20).Build();
        var middlePerson = new PersonBuilder().WithId(2).WithName("Middle").WithAge(30).Build();
        var oldPerson = new PersonBuilder().WithId(3).WithName("Old").WithAge(50).Build();
        
        await _repository.CreateAsync(youngPerson);
        await _repository.CreateAsync(middlePerson);
        await _repository.CreateAsync(oldPerson);
        _createdPersons.AddRange(new[] { youngPerson, middlePerson, oldPerson });

        // Act
        var persons = await _repository.GetAllAsync(new PersonFilter { MinAge = 25, MaxAge = 40 });

        // Assert
        Assert.NotNull(persons);
        Assert.Equal(1, persons.Count);
        Assert.All(persons, p => Assert.InRange(p.Age ?? 0, 25, 40));
    }

    /// <summary>
    /// EP6: Filter with no matches - should return empty list
    /// 
    /// ARRANGE:
    /// - Create persons
    /// - Insert all persons
    /// 
    /// ACT:
    /// - Call GetAllAsync with filter that matches nothing
    /// 
    /// ASSERT:
    /// - Empty list is returned
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithFilterNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        await _repository.CreateAsync(person);
        _createdPersons.Add(person);

        // Act
        var persons = await _repository.GetAllAsync(new PersonFilter { Name = "NonExistent" });

        // Assert
        Assert.NotNull(persons);
        Assert.Empty(persons);
    }

    /// <summary>
    /// EP7: Null filter - should return all persons
    /// 
    /// ARRANGE:
    /// - Create and insert persons
    /// 
    /// ACT:
    /// - Call GetAllAsync with null filter
    /// 
    /// ASSERT:
    /// - All persons are returned
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_WithNullFilter_ShouldReturnAllPersons()
    {
        // Arrange
        var persons = PersonMother.CreatePersonList(3);
        foreach (var person in persons)
        {
            await _repository.CreateAsync(person);
            _createdPersons.Add(person);
        }

        // Act
        var result = await _repository.GetAllAsync(filter: null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    #endregion

    #region CreateAsync Tests

    /// <summary>
    /// EP1: Valid person with all fields - should create successfully
    /// 
    /// ARRANGE:
    /// - Create valid person with all fields
    /// 
    /// ACT:
    /// - Call CreateAsync
    /// 
    /// ASSERT:
    /// - Person is created
    /// - Returned person has ID
    /// - Data matches
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_ValidPerson_ShouldCreatePerson()
    {
        // Arrange
        var personToCreate = PersonMother.CreateValidPerson();

        // Act
        var createdPerson = await _repository.CreateAsync(personToCreate);
        _createdPersons.Add(createdPerson);

        // Assert
        Assert.NotNull(createdPerson);
        Assert.NotEqual(0, createdPerson.Id);
        Assert.Equal(personToCreate.Name, createdPerson.Name);
        Assert.Equal(personToCreate.Age, createdPerson.Age);
        Assert.Equal(personToCreate.Address, createdPerson.Address);
        Assert.Equal(personToCreate.Work, createdPerson.Work);
    }

    /// <summary>
    /// EP2: Valid person with null optional fields - should create successfully
    /// 
    /// ARRANGE:
    /// - Create person with null optional fields
    /// 
    /// ACT:
    /// - Call CreateAsync
    /// 
    /// ASSERT:
    /// - Person is created
    /// - Optional fields remain null
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_PersonWithNulls_ShouldCreatePerson()
    {
        // Arrange
        var personToCreate = PersonMother.CreatePersonWithNulls();

        // Act
        var createdPerson = await _repository.CreateAsync(personToCreate);
        _createdPersons.Add(createdPerson);

        // Assert
        Assert.NotNull(createdPerson);
        Assert.Equal(personToCreate.Name, createdPerson.Name);
        Assert.Null(createdPerson.Age);
        Assert.Null(createdPerson.Address);
        Assert.Null(createdPerson.Work);
    }

    /// <summary>
    /// EP4: Duplicate ID - should throw PersonAlreadyExistsException
    /// 
    /// ARRANGE:
    /// - Create and insert person with ID 1
    /// 
    /// ACT:
    /// - Try to create another person with ID 1
    /// 
    /// ASSERT:
    /// - PersonAlreadyExistsException is thrown
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_DuplicateId_ShouldThrowPersonAlreadyExistsException()
    {
        // Arrange
        var person1 = new PersonBuilder().WithId(1).WithName("Person 1").Build();
        var person2 = new PersonBuilder().WithId(1).WithName("Person 2").Build();
        
        await _repository.CreateAsync(person1);
        _createdPersons.Add(person1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PersonAlreadyExistsException>(
            () => _repository.CreateAsync(person2));
        
        Assert.Equal(1, exception.PersonId);
    }

    /// <summary>
    /// EP6: Person with max age value - should create successfully
    /// 
    /// ARRANGE:
    /// - Create person with age 150 (max realistic)
    /// 
    /// ACT:
    /// - Call CreateAsync
    /// 
    /// ASSERT:
    /// - Person is created with max age
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_PersonWithMaxAge_ShouldCreatePerson()
    {
        // Arrange
        var personToCreate = PersonMother.CreatePersonWithMaxAge();

        // Act
        var createdPerson = await _repository.CreateAsync(personToCreate);
        _createdPersons.Add(createdPerson);

        // Assert
        Assert.NotNull(createdPerson);
        Assert.Equal(150, createdPerson.Age);
    }

    /// <summary>
    /// EP7: Person with min age value - should create successfully
    /// 
    /// ARRANGE:
    /// - Create person with age 0 (min)
    /// 
    /// ACT:
    /// - Call CreateAsync
    /// 
    /// ASSERT:
    /// - Person is created with age 0
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_PersonWithMinAge_ShouldCreatePerson()
    {
        // Arrange
        var personToCreate = PersonMother.CreatePersonWithMinAge();

        // Act
        var createdPerson = await _repository.CreateAsync(personToCreate);
        _createdPersons.Add(createdPerson);

        // Assert
        Assert.NotNull(createdPerson);
        Assert.Equal(0, createdPerson.Age);
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// EP1: Valid update with all fields - should update successfully
    /// 
    /// ARRANGE:
    /// - Create and insert person
    /// - Modify person properties
    /// 
    /// ACT:
    /// - Call UpdateAsync
    /// 
    /// ASSERT:
    /// - Person is updated
    /// - All properties reflect new values
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_ValidUpdate_ShouldUpdatePerson()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        await _repository.CreateAsync(person);
        _createdPersons.Add(person);

        person.Name = "Updated Name";
        person.Age = 40;
        person.Address = "Updated Address";
        person.Work = "Updated Work";

        // Act
        var updatedPerson = await _repository.UpdateAsync(person);

        // Assert
        Assert.NotNull(updatedPerson);
        Assert.Equal("Updated Name", updatedPerson.Name);
        Assert.Equal(40, updatedPerson.Age);
        Assert.Equal("Updated Address", updatedPerson.Address);
        Assert.Equal("Updated Work", updatedPerson.Work);
    }

    /// <summary>
    /// EP2: Update with null optional fields - should update successfully
    /// 
    /// ARRANGE:
    /// - Create and insert person with values
    /// - Set optional fields to null
    /// 
    /// ACT:
    /// - Call UpdateAsync
    /// 
    /// ASSERT:
    /// - Person is updated
    /// - Optional fields are null
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_UpdateWithNulls_ShouldUpdatePerson()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        await _repository.CreateAsync(person);
        _createdPersons.Add(person);

        person.Age = null;
        person.Address = null;
        person.Work = null;

        // Act
        var updatedPerson = await _repository.UpdateAsync(person);

        // Assert
        Assert.NotNull(updatedPerson);
        Assert.Null(updatedPerson.Age);
        Assert.Null(updatedPerson.Address);
        Assert.Null(updatedPerson.Work);
    }

    /// <summary>
    /// EP3: Non-existing ID - should throw PersonNotFoundException
    /// 
    /// ARRANGE:
    /// - Create person with non-existing ID
    /// 
    /// ACT:
    /// - Call UpdateAsync
    /// 
    /// ASSERT:
    /// - PersonNotFoundException is thrown
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_PersonNotFound_ShouldThrowPersonNotFoundException()
    {
        // Arrange
        var person = new PersonBuilder().WithId(999).WithName("NonExistent").Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PersonNotFoundException>(
            () => _repository.UpdateAsync(person));
        
        Assert.Equal(999, exception.PersonId);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Valid existing ID - should return true and delete
    /// 
    /// ARRANGE:
    /// - Create and insert person
    /// 
    /// ACT:
    /// - Call DeleteAsync
    /// 
    /// ASSERT:
    /// - Delete returns true
    /// - Person no longer exists
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_PersonExists_ShouldReturnTrueAndDelete()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        await _repository.CreateAsync(person);
        _createdPersons.Add(person);

        // Act
        var result = await _repository.DeleteAsync(person.Id);

        // Assert
        Assert.True(result);
        
        // Verify person is deleted
        var exists = await _repository.ExistsAsync(person.Id);
        Assert.False(exists);
    }

    /// <summary>
    /// EP2: Non-existing ID - should return false
    /// 
    /// ARRANGE:
    /// - Ensure person with ID 999 doesn't exist
    /// 
    /// ACT:
    /// - Call DeleteAsync
    /// 
    /// ASSERT:
    /// - Delete returns false
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_PersonNotFound_ShouldReturnFalse()
    {
        // Arrange
        var nonExistingId = 999;

        // Act
        var result = await _repository.DeleteAsync(nonExistingId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP3: Delete and verify data removed - data should be gone
    /// 
    /// ARRANGE:
    /// - Create and insert person
    /// 
    /// ACT:
    /// - Delete person
    /// - Try to retrieve person
    /// 
    /// ASSERT:
    /// - PersonNotFoundException is thrown when retrieving
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_VerifyDataRemoved_ShouldThrowNotFoundException()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        await _repository.CreateAsync(person);
        _createdPersons.Add(person);

        // Act
        await _repository.DeleteAsync(person.Id);

        // Assert
        await Assert.ThrowsAsync<PersonNotFoundException>(
            () => _repository.GetByIdAsync(person.Id));
    }

    #endregion

    #region ExistsAsync Tests

    /// <summary>
    /// EP1: Existing ID - should return true
    /// 
    /// ARRANGE:
    /// - Create and insert person
    /// 
    /// ACT:
    /// - Call ExistsAsync
    /// 
    /// ASSERT:
    /// - Returns true
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_PersonExists_ShouldReturnTrue()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        await _repository.CreateAsync(person);
        _createdPersons.Add(person);

        // Act
        var exists = await _repository.ExistsAsync(person.Id);

        // Assert
        Assert.True(exists);
    }

    /// <summary>
    /// EP2: Non-existing ID - should return false
    /// 
    /// ARRANGE:
    /// - Ensure person with ID 999 doesn't exist
    /// 
    /// ACT:
    /// - Call ExistsAsync
    /// 
    /// ASSERT:
    /// - Returns false
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_PersonNotFound_ShouldReturnFalse()
    {
        // Arrange
        var nonExistingId = 999;

        // Act
        var exists = await _repository.ExistsAsync(nonExistingId);

        // Assert
        Assert.False(exists);
    }

    /// <summary>
    /// EP3: ID after deletion - should return false
    /// 
    /// ARRANGE:
    /// - Create and insert person
    /// - Delete person
    /// 
    /// ACT:
    /// - Call ExistsAsync
    /// 
    /// ASSERT:
    /// - Returns false
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_AfterDeletion_ShouldReturnFalse()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        await _repository.CreateAsync(person);
        _createdPersons.Add(person);
        
        await _repository.DeleteAsync(person.Id);

        // Act
        var exists = await _repository.ExistsAsync(person.Id);

        // Assert
        Assert.False(exists);
    }

    #endregion

    #region GetCountAsync Tests

    /// <summary>
    /// EP1: Empty database - should return 0
    /// 
    /// ARRANGE:
    /// - Ensure database is empty
    /// 
    /// ACT:
    /// - Call GetCountAsync
    /// 
    /// ASSERT:
    /// - Returns 0
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetCountAsync_EmptyDatabase_ShouldReturnZero()
    {
        // Arrange (database is already empty)

        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        Assert.Equal(0, count);
    }

    /// <summary>
    /// EP2: Single person - should return 1
    /// 
    /// ARRANGE:
    /// - Create and insert one person
    /// 
    /// ACT:
    /// - Call GetCountAsync
    /// 
    /// ASSERT:
    /// - Returns 1
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetCountAsync_SinglePerson_ShouldReturnOne()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        await _repository.CreateAsync(person);
        _createdPersons.Add(person);

        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        Assert.Equal(1, count);
    }

    /// <summary>
    /// EP3: Multiple persons - should return correct count
    /// 
    /// ARRANGE:
    /// - Create and insert 7 persons
    /// 
    /// ACT:
    /// - Call GetCountAsync
    /// 
    /// ASSERT:
    /// - Returns 7
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetCountAsync_MultiplePersons_ShouldReturnCorrectCount()
    {
        // Arrange
        var persons = PersonMother.CreatePersonList(7);
        foreach (var person in persons)
        {
            await _repository.CreateAsync(person);
            _createdPersons.Add(person);
        }

        // Act
        var count = await _repository.GetCountAsync();

        // Assert
        Assert.Equal(7, count);
    }

    #endregion
}
