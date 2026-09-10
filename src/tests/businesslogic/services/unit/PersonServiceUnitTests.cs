using core.domain;
using core.exceptions.businesslogic.services;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.repositories;
using businesslogic.services;
using Moq;
using tests.config.attributes;
using tests.fixtures.mothers;

using RepositoryPersonAlreadyExistsException = core.exceptions.dataaccess.repositories.PersonAlreadyExistsException;
using RepositoryPersonNotFoundException = core.exceptions.dataaccess.repositories.PersonNotFoundException;

namespace tests.businesslogic.services.unit;

/// <summary>
/// Unit tests for PersonService
/// Using London-style testing with Mocks (Moq)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetByIdAsync(int id):
/// - EP1: Valid ID, Person exists (normal case)
/// - EP2: Valid ID, Person does not exist (PersonNotFoundException)
/// - EP3: Invalid ID (<= 0) (PersonValidationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For GetAllAsync(PersonFilter? filter):
/// - EP1: No filter, returns all persons
/// - EP2: With filter, returns filtered persons
/// - EP3: Repository throws exception (BaseServiceException)
/// 
/// For CreateAsync(Person person):
/// - EP1: Valid person, creation successful (normal case)
/// - EP2: Invalid person (PersonValidationException)
/// - EP3: Person already exists (PersonBusinessRuleViolationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For UpdateAsync(Person person):
/// - EP1: Valid person, Person exists, update successful (normal case)
/// - EP2: Person does not exist (PersonNotFoundException)
/// - EP3: Invalid person (PersonValidationException)
/// - EP4: Invalid ID (PersonValidationException)
/// - EP5: Repository throws exception (BaseServiceException)
/// 
/// For DeleteAsync(int id):
/// - EP1: Valid ID, Person exists, deletion successful (normal case)
/// - EP2: Valid ID, Person does not exist (PersonNotFoundException)
/// - EP3: Invalid ID (<= 0) (PersonValidationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For ExistsAsync(int id):
/// - EP1: Valid ID, Person exists (returns true)
/// - EP2: Valid ID, Person does not exist (returns false)
/// - EP3: Invalid ID (<= 0) (PersonValidationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For GetCountAsync():
/// - EP1: Returns correct count
/// - EP2: Repository throws exception (BaseServiceException)
/// 
/// Total: 25 unit tests (all should pass)
/// </summary>
public class PersonServiceUnitTests
{
    private readonly Mock<IPersonRepository> _mockRepository;
    private readonly IPersonService _service;

    public PersonServiceUnitTests()
    {
        // Arrange - Setup mock repository
        _mockRepository = new Mock<IPersonRepository>();
        _service = new PersonService(_mockRepository.Object);
    }

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Valid ID, Person exists - should return Person
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_ValidId_PersonExists_ShouldReturnPerson()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        _mockRepository.Setup(r => r.GetByIdAsync(person.Id)).ReturnsAsync(person);

        // Act
        var result = await _service.GetByIdAsync(person.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(person.Id, result.Id);
        Assert.Equal(person.Name, result.Name);
        _mockRepository.Verify(r => r.GetByIdAsync(person.Id), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Person does not exist - should throw PersonNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_ValidId_PersonNotFound_ShouldThrowPersonNotFoundException()
    {
        // Arrange
        var personId = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(personId)).ThrowsAsync(new RepositoryPersonNotFoundException(personId));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PersonNotFoundException>(
            () => _service.GetByIdAsync(personId)
        );
        Assert.Equal(personId, exception.PersonId);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw PersonValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [Unit]
    public async Task GetByIdAsync_InvalidId_ShouldThrowPersonValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PersonValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid Person ID", exception.Message);
    }

    /// <summary>
    /// EP4: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var personId = 1;
        _mockRepository.Setup(r => r.GetByIdAsync(personId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.GetByIdAsync(personId)
        );
        Assert.Contains($"Failed to get Person with ID {personId}", exception.Message);
    }

    #endregion

    #region GetAllAsync Tests

    /// <summary>
    /// EP1: No filter, returns all persons - should return list of persons
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllAsync_NoFilter_ShouldReturnAllPersons()
    {
        // Arrange
        var persons = PersonMother.CreatePersonList(5);
        _mockRepository.Setup(r => r.GetAllAsync(null)).ReturnsAsync(persons);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        _mockRepository.Verify(r => r.GetAllAsync(null), Times.Once);
    }

    /// <summary>
    /// EP2: With filter, returns filtered persons - should pass filter to repository
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllAsync_WithFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var filter = new core.filters.PersonFilter { Name = "John" };
        var filteredPersons = new List<Person> { PersonMother.CreateValidPerson() };
        _mockRepository.Setup(r => r.GetAllAsync(filter)).ReturnsAsync(filteredPersons);

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        _mockRepository.Verify(r => r.GetAllAsync(filter), Times.Once);
    }

    /// <summary>
    /// EP3: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync(null))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.GetAllAsync()
        );
        Assert.Contains("Failed to get all Persons", exception.Message);
    }

    #endregion

    #region CreateAsync Tests

    /// <summary>
    /// EP1: Valid person, creation successful - should return created person
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_ValidPerson_ShouldReturnCreatedPerson()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        person.Id = 0; // Id will be generated
        _mockRepository.Setup(r => r.CreateAsync(person)).ReturnsAsync(person);

        // Act
        var result = await _service.CreateAsync(person);

        // Assert
        Assert.NotNull(result);
        _mockRepository.Verify(r => r.CreateAsync(person), Times.Once);
    }

    /// <summary>
    /// EP2: Invalid person (empty name) - should throw PersonValidationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_InvalidPerson_EmptyName_ShouldThrowPersonValidationException()
    {
        // Arrange
        var person = PersonMother.CreateInvalidPersonWithEmptyName();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PersonValidationException>(
            () => _service.CreateAsync(person)
        );
        Assert.Contains("Person validation failed", exception.Message);
    }

    /// <summary>
    /// EP3: Invalid person (negative age) - should throw PersonValidationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_InvalidPerson_NegativeAge_ShouldThrowPersonValidationException()
    {
        // Arrange
        var person = PersonMother.CreateInvalidPersonWithNegativeAge();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PersonValidationException>(
            () => _service.CreateAsync(person)
        );
        Assert.Contains("Person validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Person already exists - should throw PersonBusinessRuleViolationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_PersonAlreadyExists_ShouldThrowPersonBusinessRuleViolationException()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        _mockRepository.Setup(r => r.CreateAsync(person))
            .ThrowsAsync(new RepositoryPersonAlreadyExistsException(person.Id));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PersonBusinessRuleViolationException>(
            () => _service.CreateAsync(person)
        );
        Assert.Contains("UniqueConstraint", exception.RuleName);
        Assert.Contains("already exists", exception.Message);
    }

    /// <summary>
    /// EP5: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        person.Id = 0;
        _mockRepository.Setup(r => r.CreateAsync(person))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.CreateAsync(person)
        );
        Assert.Contains("Failed to create Person", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// EP1: Valid person, Person exists, update successful - should return updated person
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_ValidPerson_PersonExists_ShouldReturnUpdatedPerson()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        _mockRepository.Setup(r => r.ExistsAsync(person.Id)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.UpdateAsync(person)).ReturnsAsync(person);

        // Act
        var result = await _service.UpdateAsync(person);

        // Assert
        Assert.NotNull(result);
        _mockRepository.Verify(r => r.ExistsAsync(person.Id), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(person), Times.Once);
    }

    /// <summary>
    /// EP2: Person does not exist - should throw PersonNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_PersonNotFound_ShouldThrowPersonNotFoundException()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        _mockRepository.Setup(r => r.ExistsAsync(person.Id)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PersonNotFoundException>(
            () => _service.UpdateAsync(person)
        );
        Assert.Equal(person.Id, exception.PersonId);
    }

    /// <summary>
    /// EP3: Invalid person (empty name) - should throw PersonValidationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_InvalidPerson_EmptyName_ShouldThrowPersonValidationException()
    {
        // Arrange
        var person = PersonMother.CreateInvalidPersonWithEmptyName();
        _mockRepository.Setup(r => r.ExistsAsync(person.Id)).ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PersonValidationException>(
            () => _service.UpdateAsync(person)
        );
        Assert.Contains("Person validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Invalid ID (<= 0) - should throw PersonValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Unit]
    public async Task UpdateAsync_InvalidId_ShouldThrowPersonValidationException(int invalidId)
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        person.Id = invalidId;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PersonValidationException>(
            () => _service.UpdateAsync(person)
        );
        Assert.Contains("Invalid Person ID", exception.Message);
    }

    /// <summary>
    /// EP5: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        _mockRepository.Setup(r => r.ExistsAsync(person.Id)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.UpdateAsync(person))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.UpdateAsync(person)
        );
        Assert.Contains($"Failed to update Person with ID {person.Id}", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Valid ID, Person exists, deletion successful - should return true
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_ValidId_PersonExists_ShouldReturnTrue()
    {
        // Arrange
        var personId = 1;
        _mockRepository.Setup(r => r.ExistsAsync(personId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DeleteAsync(personId)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(personId);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.ExistsAsync(personId), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(personId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Person does not exist - should throw PersonNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_ValidId_PersonNotFound_ShouldThrowPersonNotFoundException()
    {
        // Arrange
        var personId = 999;
        _mockRepository.Setup(r => r.ExistsAsync(personId)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PersonNotFoundException>(
            () => _service.DeleteAsync(personId)
        );
        Assert.Equal(personId, exception.PersonId);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw PersonValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Unit]
    public async Task DeleteAsync_InvalidId_ShouldThrowPersonValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PersonValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid Person ID", exception.Message);
    }

    /// <summary>
    /// EP4: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var personId = 1;
        _mockRepository.Setup(r => r.ExistsAsync(personId)).ReturnsAsync(true);
        _mockRepository.Setup(r => r.DeleteAsync(personId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.DeleteAsync(personId)
        );
        Assert.Contains($"Failed to delete Person with ID {personId}", exception.Message);
    }

    #endregion

    #region ExistsAsync Tests

    /// <summary>
    /// EP1: Valid ID, Person exists - should return true
    /// </summary>
    [Fact]
    [Unit]
    public async Task ExistsAsync_ValidId_PersonExists_ShouldReturnTrue()
    {
        // Arrange
        var personId = 1;
        _mockRepository.Setup(r => r.ExistsAsync(personId)).ReturnsAsync(true);

        // Act
        var result = await _service.ExistsAsync(personId);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.ExistsAsync(personId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Person does not exist - should return false
    /// </summary>
    [Fact]
    [Unit]
    public async Task ExistsAsync_ValidId_PersonNotFound_ShouldReturnFalse()
    {
        // Arrange
        var personId = 999;
        _mockRepository.Setup(r => r.ExistsAsync(personId)).ReturnsAsync(false);

        // Act
        var result = await _service.ExistsAsync(personId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw PersonValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Unit]
    public async Task ExistsAsync_InvalidId_ShouldThrowPersonValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PersonValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid Person ID", exception.Message);
    }

    /// <summary>
    /// EP4: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task ExistsAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var personId = 1;
        _mockRepository.Setup(r => r.ExistsAsync(personId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.ExistsAsync(personId)
        );
        Assert.Contains($"Failed to check existence of Person with ID {personId}", exception.Message);
    }

    #endregion

    #region GetCountAsync Tests

    /// <summary>
    /// EP1: Returns correct count - should return count from repository
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetCountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var expectedCount = 42;
        _mockRepository.Setup(r => r.GetCountAsync()).ReturnsAsync(expectedCount);

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(expectedCount, result);
        _mockRepository.Verify(r => r.GetCountAsync(), Times.Once);
    }

    /// <summary>
    /// EP2: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetCountAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetCountAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.GetCountAsync()
        );
        Assert.Contains("Failed to get Person count", exception.Message);
    }

    #endregion
}
