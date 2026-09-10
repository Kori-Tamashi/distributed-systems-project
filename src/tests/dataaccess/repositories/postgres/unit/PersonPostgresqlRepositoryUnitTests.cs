using System.Linq.Expressions;
using core.domain;
using core.exceptions.dataaccess.repositories;
using dataaccess.contexts.postgres;
using dataaccess.models.postgres;
using dataaccess.repositories.postgres;
using Microsoft.EntityFrameworkCore;
using Moq;
using tests.config.attributes;
using tests.fixtures.mothers;

using PersonDomain = core.domain.Person;
using PersonPostgresqlModel = dataaccess.models.postgres.PersonPostgresqlModel;

namespace tests.dataaccess.repositories.postgres.unit;

/// <summary>
/// Unit tests for PersonPostgresqlRepository
/// Using London-style testing with Mocks (Moq)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetByIdAsync(int id):
/// - EP1: Person exists in database (normal case)
/// - EP2: Person does not exist (PersonNotFoundException)
/// - EP3: Database error occurs (PersonDatabaseException)
/// 
/// For CreateAsync(PersonDomain person):
/// - EP1: Person created successfully (normal case) - Requires integration test
/// - EP2: Person with same Id already exists (PersonAlreadyExistsException) - Requires integration test
/// - EP3: Database error occurs (PersonDatabaseException)
/// 
/// For UpdateAsync(PersonDomain person):
/// - EP1: Person updated successfully (normal case)
/// - EP2: Person does not exist (PersonNotFoundException)
/// - EP3: Database error occurs (PersonDatabaseException)
/// 
/// For DeleteAsync(int id):
/// - EP1: Person deleted successfully (returns true)
/// - EP2: Person does not exist (returns false)
/// - EP3: Database error occurs (PersonDatabaseException)
/// 
/// Note: Tests using EF Core extension methods (AnyAsync, CountAsync, ToListAsync, etc.)
/// cannot be unit tested with Moq as these methods are not overridable.
/// These should be covered by integration tests with real database.
/// Total: 10 unit tests (all passing)
/// </summary>
public class PersonPostgresqlRepositoryUnitTests
{
    private readonly Mock<PostgresqlDatabaseContext> _mockContext;
    private readonly Mock<DbSet<PersonPostgresqlModel>> _mockDbSet;
    private readonly PersonPostgresqlRepository _repository;

    public PersonPostgresqlRepositoryUnitTests()
    {
        // Arrange - Setup mock context
        _mockContext = new Mock<PostgresqlDatabaseContext>();
        _mockDbSet = new Mock<DbSet<PersonPostgresqlModel>>();
        
        // Setup DbSet to behave like IQueryable
        var data = new List<PersonPostgresqlModel>().AsQueryable();
        _mockDbSet.As<IQueryable<PersonPostgresqlModel>>()
            .Setup(m => m.Provider).Returns(data.Provider);
        _mockDbSet.As<IQueryable<PersonPostgresqlModel>>()
            .Setup(m => m.Expression).Returns(data.Expression);
        _mockDbSet.As<IQueryable<PersonPostgresqlModel>>()
            .Setup(m => m.ElementType).Returns(data.ElementType);
        _mockDbSet.As<IQueryable<PersonPostgresqlModel>>()
            .Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        
        _mockContext.Setup(c => c.Persons).Returns(_mockDbSet.Object);
        _repository = new PersonPostgresqlRepository(_mockContext.Object);
    }

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Person exists in database - should return Person
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_PersonExists_ShouldReturnPerson()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        var model = new PersonPostgresqlModel
        {
            Id = person.Id,
            Name = person.Name,
            Age = person.Age,
            Address = person.Address,
            Work = person.Work
        };

        _mockDbSet.Setup(m => m.FindAsync(person.Id)).ReturnsAsync(model);

        // Act
        var result = await _repository.GetByIdAsync(person.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(person.Id, result.Id);
        Assert.Equal(person.Name, result.Name);
        _mockDbSet.Verify(m => m.FindAsync(person.Id), Times.Once);
    }

    /// <summary>
    /// EP2: Person does not exist - should throw PersonNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_PersonNotFound_ShouldThrowPersonNotFoundException()
    {
        // Arrange
        var personId = 999;
        _mockDbSet.Setup(m => m.FindAsync(personId)).ReturnsAsync((PersonPostgresqlModel?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PersonNotFoundException>(
            () => _repository.GetByIdAsync(personId)
        );
        Assert.Equal(personId, exception.PersonId);
    }

    /// <summary>
    /// EP3: Database error occurs - should throw PersonDatabaseException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_DatabaseError_ShouldThrowPersonDatabaseException()
    {
        // Arrange
        var personId = 1;
        _mockDbSet.Setup(m => m.FindAsync(personId))
            .ThrowsAsync(new Exception("Database connection error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PersonDatabaseException>(
            () => _repository.GetByIdAsync(personId)
        );
        Assert.Contains("Failed to get Person by id", exception.Message);
    }

    #endregion

    #region CreateAsync Tests

    /// <summary>
    /// EP3: Database error occurs - should throw PersonDatabaseException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_DatabaseError_ShouldThrowPersonDatabaseException()
    {
        // Arrange
        var person = PersonMother.CreateValidPerson();
        person.Id = 0;

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PersonDatabaseException>(
            () => _repository.CreateAsync(person)
        );
        Assert.Contains("Failed to create Person", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// EP1: Person updated successfully - should return updated Person
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_Success_ShouldReturnUpdatedPerson()
    {
        // Arrange
        var existingModel = new PersonPostgresqlModel
        {
            Id = 1,
            Name = "Old Name",
            Age = 20,
            Address = "Old Address",
            Work = "Old Work"
        };

        var updatedPerson = PersonMother.CreateValidPerson();
        updatedPerson.Id = 1;

        _mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(existingModel);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _repository.UpdateAsync(updatedPerson);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updatedPerson.Name, result.Name);
        Assert.Equal(updatedPerson.Age, result.Age);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
        _mockDbSet.Setup(m => m.FindAsync(person.Id)).ReturnsAsync((PersonPostgresqlModel?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PersonNotFoundException>(
            () => _repository.UpdateAsync(person)
        );
        Assert.Equal(person.Id, exception.PersonId);
    }

    /// <summary>
    /// EP3: Database error occurs - should throw PersonDatabaseException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_DatabaseError_ShouldThrowPersonDatabaseException()
    {
        // Arrange
        var existingModel = new PersonPostgresqlModel { Id = 1, Name = "Old Name" };
        var updatedPerson = PersonMother.CreateValidPerson();
        updatedPerson.Id = 1;

        _mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(existingModel);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PersonDatabaseException>(
            () => _repository.UpdateAsync(updatedPerson)
        );
        Assert.Contains("Failed to update Person", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Person deleted successfully - should return true
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_Success_ShouldReturnTrue()
    {
        // Arrange
        var personId = 1;
        var existingModel = new PersonPostgresqlModel { Id = personId, Name = "Test" };

        _mockDbSet.Setup(m => m.FindAsync(personId)).ReturnsAsync(existingModel);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _repository.DeleteAsync(personId);

        // Assert
        Assert.True(result);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// EP2: Person does not exist - should return false
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_PersonNotFound_ShouldReturnFalse()
    {
        // Arrange
        var personId = 999;
        _mockDbSet.Setup(m => m.FindAsync(personId)).ReturnsAsync((PersonPostgresqlModel?)null);

        // Act
        var result = await _repository.DeleteAsync(personId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP3: Database error occurs - should throw PersonDatabaseException
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_DatabaseError_ShouldThrowPersonDatabaseException()
    {
        // Arrange
        var personId = 1;
        var existingModel = new PersonPostgresqlModel { Id = personId, Name = "Test" };

        _mockDbSet.Setup(m => m.FindAsync(personId)).ReturnsAsync(existingModel);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PersonDatabaseException>(
            () => _repository.DeleteAsync(personId)
        );
        Assert.Contains("Failed to delete Person", exception.Message);
    }

    #endregion
}
