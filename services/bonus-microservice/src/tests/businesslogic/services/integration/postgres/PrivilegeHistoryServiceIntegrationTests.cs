using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using businesslogic.services;
using core.domain;
using core.enums;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.repositories;
using dataaccess.contexts.postgres;
using dataaccess.repositories.postgres;
using tests.config.attributes;
using tests.fixtures.builders;
using tests.fixtures.contexts.postgres;
using tests.fixtures.mothers;
using Xunit;

using ServicePrivilegeHistoryNotFoundException = core.exceptions.businesslogic.services.PrivilegeHistoryNotFoundException;
using ServicePrivilegeHistoryValidationException = core.exceptions.businesslogic.services.PrivilegeHistoryValidationException;
using PrivilegeHistoryDomain = core.domain.PrivilegeHistory;

namespace tests.businesslogic.services.integration.postgres;

[Collection("PostgresIntegrationTests")]
/// <summary>
/// Integration tests for PrivilegeHistoryService
/// 
/// TEST STRATEGY:
/// These tests use a real PostgreSQL database via TestPostgresDatabaseContext fixture.
/// Tests are isolated by using a dedicated test database (test_privileges).
/// PrivilegeHistoryService is tested with real PrivilegeHistoryPostgresqlRepository.
/// 
/// TEST CONTEXT:
/// - TestPostgresDatabaseContext is used for database operations
/// - Database is created once in constructor and cleaned after each test
/// - Each test method cleans up its own data in Dispose
/// - Tests share the same database but are isolated by cleanup
/// 
/// CLASS EQUIVALENCE PARTITIONING APPLIED:
/// 
/// 1. GetByIdAsync Tests:
///    EP1: Valid existing ID (normal case) - history exists in database
///    EP2: Non-existing ID (error case) - PrivilegeHistoryNotFoundException thrown
///    EP3: Invalid ID <= 0 (validation case) - PrivilegeHistoryValidationException thrown
/// 
/// 2. GetAllAsync Tests:
///    EP1: Empty database (edge case) - should return empty list
///    EP2: Single history record (normal case) - should return list with one record
///    EP3: Multiple history records (normal case) - should return all records
///    EP4: Filter by privilege ID (normal case) - should return matching records
///    EP5: Filter with no matches (edge case) - should return empty list
/// 
/// 3. CreateAsync Tests:
///    EP1: Valid history with all fields (normal case) - should create successfully
///    EP2: Valid history with minimal data (edge case) - should create successfully
///    EP3: History with zero BalanceDiff (validation case) - PrivilegeHistoryValidationException thrown
///    EP4: History with negative PrivilegeId (validation case) - PrivilegeHistoryValidationException thrown
///    EP5: History with future datetime (validation case) - PrivilegeHistoryValidationException thrown
/// 
/// 4. UpdateAsync Tests:
///    EP1: Valid update with all fields (normal case) - should update successfully
///    EP2: Non-existing ID (error case) - PrivilegeHistoryNotFoundException thrown
///    EP3: Invalid BalanceDiff (validation case) - PrivilegeHistoryValidationException thrown
///    EP4: Invalid ID <= 0 (validation case) - PrivilegeHistoryValidationException thrown
/// 
/// 5. DeleteAsync Tests:
///    EP1: Valid existing ID (normal case) - should return true and delete
///    EP2: Non-existing ID (error case) - PrivilegeHistoryNotFoundException thrown
///    EP3: Invalid ID <= 0 (validation case) - PrivilegeHistoryValidationException thrown
///    EP4: Delete and verify data removed (verification case) - GetById throws
/// 
/// 6. ExistsAsync Tests:
///    EP1: Existing ID (normal case) - should return true
///    EP2: Non-existing ID (edge case) - should return false
///    EP3: ID after deletion (verification case) - should return false
///    EP4: Invalid ID <= 0 (validation case) - PrivilegeHistoryValidationException thrown
/// 
/// 7. GetCountAsync Tests:
///    EP1: Empty database (edge case) - should return 0
///    EP2: Single history record (normal case) - should return 1
///    EP3: Multiple history records (normal case) - should return correct count
/// 
/// AAA STRUCTURE:
/// All tests follow Arrange-Act-Assert pattern:
/// - Arrange: Setup test data and database state
/// - Act: Execute the method under test
/// - Assert: Verify the results
/// 
/// Total: 27 integration tests
/// </summary>
public class PrivilegeHistoryServiceIntegrationTests : IDisposable
{
    private readonly TestPostgresDatabaseContext _testContext;
    private readonly IPrivilegeHistoryRepository _privilegeHistoryRepository;
    private readonly IPrivilegeHistoryService _service;

    static PrivilegeHistoryServiceIntegrationTests()
    {
        // Load .env file to get TEST_* environment variables
    }

    public PrivilegeHistoryServiceIntegrationTests()
    {
        // Create test database context - this uses TEST_* environment variables
        _testContext = new TestPostgresDatabaseContext();
        _testContext.EnsureDatabaseDeleted(); // Create tables automatically
        _testContext.EnsureDatabaseCreated();
        
        // Create repositories and service using the test database context
        _privilegeHistoryRepository = new PrivilegeHistoryPostgresqlRepository(_testContext);
        _service = new PrivilegeHistoryService(_privilegeHistoryRepository);
    }

    public void Dispose()
    {
        try
        {
            // Clean up all data
            var histories = _testContext.PrivilegeHistories.ToList();
            if (histories.Any())
            {
                _testContext.PrivilegeHistories.RemoveRange(histories);
            }
            
            _testContext.SaveChanges();
        }
        catch
        {
            // Ignore errors during cleanup (database might not exist)
        }
        finally
        {
            _testContext.Dispose();
        }
    }

    /// <summary>
    /// Clean up all data from database before each test
    /// </summary>
    private void CleanupDatabase()
    {
        try
        {
            var histories = _testContext.PrivilegeHistories.ToList();
            if (histories.Any())
            {
                _testContext.PrivilegeHistories.RemoveRange(histories);
            }
            
            _testContext.SaveChanges();
        }
        catch
        {
            // Ignore errors during cleanup
        }
    }

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Valid existing ID - should return history
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_ValidId_HistoryExists_ShouldReturnHistory()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        await _privilegeHistoryRepository.CreateAsync(history);

        // Act
        var result = await _service.GetByIdAsync(history.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(history.Id, result.Id);
        Assert.Equal(history.PrivilegeId, result.PrivilegeId);
        Assert.Equal(history.BalanceDiff, result.BalanceDiff);
    }

    /// <summary>
    /// EP2: Non-existing ID - should throw PrivilegeHistoryNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_NonExistingId_ShouldThrowPrivilegeHistoryNotFoundException()
    {
        // Arrange
        var nonExistingId = 9999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeHistoryNotFoundException>(
            () => _service.GetByIdAsync(nonExistingId)
        );
        Assert.Equal(nonExistingId, exception.HistoryId);
    }

    /// <summary>
    /// EP3: Invalid ID <= 0 - should throw PrivilegeHistoryValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [Integration]
    public async Task GetByIdAsync_InvalidId_ShouldThrowPrivilegeHistoryValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeHistoryValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid PrivilegeHistory ID", exception.Message);
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
        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// EP2: Single history record - should return list with one record
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_SingleHistory_ShouldReturnListWithOneHistory()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        await _privilegeHistoryRepository.CreateAsync(history);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(history.Id, result[0].Id);
    }

    /// <summary>
    /// EP3: Multiple history records - should return all records
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_MultipleHistories_ShouldReturnAllHistories()
    {
        // Arrange
        var histories = PrivilegeHistoryMother.CreateHistoryList(5);
        foreach (var h in histories)
        {
            await _privilegeHistoryRepository.CreateAsync(h);
        }

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
    }

    /// <summary>
    /// EP4: Filter by privilege ID - should return matching records
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_FilterByPrivilegeId_ShouldReturnMatchingHistories()
    {
        // Clean up before test to ensure isolation
        CleanupDatabase();
        
        // Arrange - use unique PrivilegeId values
        var history1 = new PrivilegeHistoryBuilder()
            .WithId(0)
            .WithPrivilegeId(100)
            .WithTicketUid(Guid.NewGuid())
            .WithPositiveBalanceDiff()
            .WithFillInBalance()
            .Build();
        await _privilegeHistoryRepository.CreateAsync(history1);

        var history2 = new PrivilegeHistoryBuilder()
            .WithId(0)
            .WithPrivilegeId(200)
            .WithTicketUid(Guid.NewGuid())
            .WithPositiveBalanceDiff()
            .WithFillInBalance()
            .Build();
        await _privilegeHistoryRepository.CreateAsync(history2);

        var filter = new PrivilegeHistoryFilter { PrivilegeId = 100 };

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(100, result[0].PrivilegeId);
    }

    /// <summary>
    /// EP5: Filter with no matches - should return empty list
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_FilterWithNoMatches_ShouldReturnEmptyList()
    {
        // Clean up before test to ensure isolation
        CleanupDatabase();
        
        // Arrange - use unique PrivilegeId
        var history = new PrivilegeHistoryBuilder()
            .WithId(0)
            .WithPrivilegeId(100)
            .WithTicketUid(Guid.NewGuid())
            .WithPositiveBalanceDiff()
            .WithFillInBalance()
            .Build();
        await _privilegeHistoryRepository.CreateAsync(history);

        var filter = new PrivilegeHistoryFilter { PrivilegeId = 999 };

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region CreateAsync Tests

    /// <summary>
    /// EP1: Valid history with all fields - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_ValidHistory_ShouldCreateHistory()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();

        // Act
        var result = await _service.CreateAsync(history);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal(history.PrivilegeId, result.PrivilegeId);
        Assert.Equal(history.BalanceDiff, result.BalanceDiff);
    }

    /// <summary>
    /// EP2: Valid history with minimal data - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_MinimalHistory_ShouldCreateHistory()
    {
        // Arrange
        var history = new PrivilegeHistory
        {
            PrivilegeId = 1,
            TicketUid = Guid.NewGuid(),
            DateTime = DateTime.UtcNow,
            BalanceDiff = 100,
            OperationType = OperationType.FILL_IN_BALANCE
        };

        // Act
        var result = await _service.CreateAsync(history);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal(1, result.PrivilegeId);
    }

    /// <summary>
    /// EP3: Zero BalanceDiff - should throw PrivilegeHistoryValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_ZeroBalanceDiff_ShouldThrowPrivilegeHistoryValidationException()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        history.BalanceDiff = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeHistoryValidationException>(
            () => _service.CreateAsync(history)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Negative PrivilegeId - should throw PrivilegeHistoryValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_NegativePrivilegeId_ShouldThrowPrivilegeHistoryValidationException()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        history.PrivilegeId = -1;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeHistoryValidationException>(
            () => _service.CreateAsync(history)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP5: Future datetime - should throw PrivilegeHistoryValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_FutureDateTime_ShouldThrowPrivilegeHistoryValidationException()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        history.DateTime = DateTime.UtcNow.AddDays(1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeHistoryValidationException>(
            () => _service.CreateAsync(history)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// EP1: Valid update with all fields - should update successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_ValidUpdate_ShouldUpdateHistory()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        await _privilegeHistoryRepository.CreateAsync(history);
        
        history.BalanceDiff = 500;
        history.OperationType = OperationType.DEBIT_THE_ACCOUNT;

        // Act
        var result = await _service.UpdateAsync(history);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(500, result.BalanceDiff);
        Assert.Equal(OperationType.DEBIT_THE_ACCOUNT, result.OperationType);
    }

    /// <summary>
    /// EP2: Non-existing ID - should throw PrivilegeHistoryNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_NonExistingId_ShouldThrowPrivilegeHistoryNotFoundException()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        history.Id = 9999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeHistoryNotFoundException>(
            () => _service.UpdateAsync(history)
        );
        Assert.Equal(history.Id, exception.HistoryId);
    }

    /// <summary>
    /// EP3: Invalid BalanceDiff (zero) - should throw PrivilegeHistoryValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_InvalidBalanceDiff_ShouldThrowPrivilegeHistoryValidationException()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        await _privilegeHistoryRepository.CreateAsync(history);
        history.BalanceDiff = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeHistoryValidationException>(
            () => _service.UpdateAsync(history)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Invalid ID <= 0 - should throw PrivilegeHistoryValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_InvalidId_ShouldThrowPrivilegeHistoryValidationException()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        history.Id = -1;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeHistoryValidationException>(
            () => _service.UpdateAsync(history)
        );
        Assert.Contains("Invalid PrivilegeHistory ID", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Valid existing ID - should return true and delete
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_ValidId_ShouldDeleteHistory()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        await _privilegeHistoryRepository.CreateAsync(history);

        // Act
        var result = await _service.DeleteAsync(history.Id);

        // Assert
        Assert.True(result);
        
        // Verify it's deleted
        var exception = await Assert.ThrowsAsync<ServicePrivilegeHistoryNotFoundException>(
            () => _service.GetByIdAsync(history.Id)
        );
        Assert.Equal(history.Id, exception.HistoryId);
    }

    /// <summary>
    /// EP2: Non-existing ID - should throw PrivilegeHistoryNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_NonExistingId_ShouldThrowPrivilegeHistoryNotFoundException()
    {
        // Arrange
        var nonExistingId = 9999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeHistoryNotFoundException>(
            () => _service.DeleteAsync(nonExistingId)
        );
        Assert.Equal(nonExistingId, exception.HistoryId);
    }

    /// <summary>
    /// EP3: Invalid ID <= 0 - should throw PrivilegeHistoryValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task DeleteAsync_InvalidId_ShouldThrowPrivilegeHistoryValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeHistoryValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid PrivilegeHistory ID", exception.Message);
    }

    #endregion

    #region ExistsAsync Tests

    /// <summary>
    /// EP1: Existing ID - should return true
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_ExistingId_ShouldReturnTrue()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        await _privilegeHistoryRepository.CreateAsync(history);

        // Act
        var result = await _service.ExistsAsync(history.Id);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// EP2: Non-existing ID - should return false
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_NonExistingId_ShouldReturnFalse()
    {
        // Arrange
        var nonExistingId = 9999;

        // Act
        var result = await _service.ExistsAsync(nonExistingId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP3: ID after deletion - should return false
    /// </summary>
    [Fact]
    [Integration]
    public async Task ExistsAsync_IdAfterDeletion_ShouldReturnFalse()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        await _privilegeHistoryRepository.CreateAsync(history);
        await _service.DeleteAsync(history.Id);

        // Act
        var result = await _service.ExistsAsync(history.Id);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP4: Invalid ID <= 0 - should throw PrivilegeHistoryValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task ExistsAsync_InvalidId_ShouldThrowPrivilegeHistoryValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeHistoryValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid PrivilegeHistory ID", exception.Message);
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
        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(0, result);
    }

    /// <summary>
    /// EP2: Single history record - should return 1
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetCountAsync_SingleHistory_ShouldReturnOne()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        await _privilegeHistoryRepository.CreateAsync(history);

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(1, result);
    }

    /// <summary>
    /// EP3: Multiple history records - should return correct count
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetCountAsync_MultipleHistories_ShouldReturnCorrectCount()
    {
        // Arrange
        var histories = PrivilegeHistoryMother.CreateHistoryList(7);
        foreach (var h in histories)
        {
            await _privilegeHistoryRepository.CreateAsync(h);
        }

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(7, result);
    }

    #endregion
}
