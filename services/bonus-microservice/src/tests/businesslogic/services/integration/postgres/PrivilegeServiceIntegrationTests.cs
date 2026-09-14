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
using DotNetEnv;
using Xunit;

using ServicePrivilegeNotFoundException = core.exceptions.businesslogic.services.PrivilegeNotFoundException;
using ServicePrivilegeValidationException = core.exceptions.businesslogic.services.PrivilegeValidationException;
using PrivilegeDomain = core.domain.Privilege;

namespace tests.businesslogic.services.integration.postgres;

[Collection("PostgresIntegrationTests")]
/// <summary>
/// Integration tests for PrivilegeService
/// 
/// TEST STRATEGY:
/// These tests use a real PostgreSQL database via TestPostgresDatabaseContext fixture.
/// Tests are isolated by using a dedicated test database (test_privileges).
/// PrivilegeService is tested with real PrivilegePostgresqlRepository.
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
///    EP1: Valid existing ID (normal case) - privilege exists in database
///    EP2: Non-existing ID (error case) - PrivilegeNotFoundException thrown
///    EP3: Invalid ID <= 0 (validation case) - PrivilegeValidationException thrown
/// 
/// 2. GetAllAsync Tests:
///    EP1: Empty database (edge case) - should return empty list
///    EP2: Single privilege (normal case) - should return list with one privilege
///    EP3: Multiple privileges (normal case) - should return all privileges
///    EP4: Filter by status (normal case) - should return matching privileges
///    EP5: Filter with no matches (edge case) - should return empty list
/// 
/// 3. CreateAsync Tests:
///    EP1: Valid privilege with all fields (normal case) - should create successfully
///    EP2: Valid privilege with minimal data (edge case) - should create successfully
///    EP3: Privilege with empty username (validation case) - PrivilegeValidationException thrown
///    EP4: Privilege with negative balance (validation case) - PrivilegeValidationException thrown
///    EP5: Privilege with too long username (validation case) - PrivilegeValidationException thrown
/// 
/// 4. UpdateAsync Tests:
///    EP1: Valid update with all fields (normal case) - should update successfully
///    EP2: Non-existing ID (error case) - PrivilegeNotFoundException thrown
///    EP3: Invalid username (validation case) - PrivilegeValidationException thrown
///    EP4: Invalid ID <= 0 (validation case) - PrivilegeValidationException thrown
/// 
/// 5. DeleteAsync Tests:
///    EP1: Valid existing ID (normal case) - should return true and delete
///    EP2: Non-existing ID (error case) - PrivilegeNotFoundException thrown
///    EP3: Invalid ID <= 0 (validation case) - PrivilegeValidationException thrown
///    EP4: Delete and verify data removed (verification case) - GetById throws
/// 
/// 6. ExistsAsync Tests:
///    EP1: Existing ID (normal case) - should return true
///    EP2: Non-existing ID (edge case) - should return false
///    EP3: ID after deletion (verification case) - should return false
///    EP4: Invalid ID <= 0 (validation case) - PrivilegeValidationException thrown
/// 
/// 7. GetCountAsync Tests:
///    EP1: Empty database (edge case) - should return 0
///    EP2: Single privilege (normal case) - should return 1
///    EP3: Multiple privileges (normal case) - should return correct count
/// 
/// 8. CreditBalanceAsync Tests:
///    EP1: Valid credit operation (normal case) - balance increases, history created
///    EP2: Invalid privilege ID (validation case) - PrivilegeValidationException thrown
///    EP3: Invalid amount <= 0 (validation case) - PrivilegeValidationException thrown
///    EP4: Non-existing privilege (error case) - PrivilegeNotFoundException thrown
/// 
/// 9. DebitBalanceAsync Tests:
///    EP1: Valid debit with sufficient balance (normal case) - balance decreases, history created
///    EP2: Insufficient balance (business rule case) - PrivilegeBusinessRuleViolationException thrown
///    EP3: Invalid privilege ID (validation case) - PrivilegeValidationException thrown
///    EP4: Invalid amount <= 0 (validation case) - PrivilegeValidationException thrown
/// 
/// 10. GetMaxDebitAmountAsync Tests:
///     EP1: Valid existing ID (normal case) - should return current balance
///     EP2: Non-existing ID (error case) - PrivilegeNotFoundException thrown
///     EP3: Invalid ID <= 0 (validation case) - PrivilegeValidationException thrown
/// 
/// AAA STRUCTURE:
/// All tests follow Arrange-Act-Assert pattern:
/// - Arrange: Setup test data and database state
/// - Act: Execute the method under test
/// - Assert: Verify the results
/// 
/// Total: 38 integration tests
/// </summary>
public class PrivilegeServiceIntegrationTests : IDisposable
{
    private readonly TestPostgresDatabaseContext _testContext;
    private readonly IPrivilegeRepository _privilegeRepository;
    private readonly IPrivilegeHistoryRepository _privilegeHistoryRepository;
    private readonly IPrivilegeService _service;

    static PrivilegeServiceIntegrationTests()
    {
        // Load .env file to get TEST_* environment variables
        DotNetEnv.Env.Load();
    }

    public PrivilegeServiceIntegrationTests()
    {
        // Create test database context - this uses TEST_* environment variables
        _testContext = new TestPostgresDatabaseContext();
        _testContext.EnsureDatabaseDeleted(); // Create tables automatically
        _testContext.EnsureDatabaseCreated();
        
        // Create repositories and service using the test database context
        _privilegeRepository = new PrivilegePostgresqlRepository(_testContext);
        _privilegeHistoryRepository = new PrivilegeHistoryPostgresqlRepository(_testContext);
        _service = new PrivilegeService(_privilegeRepository, _privilegeHistoryRepository);
    }

    public void Dispose()
    {
        try
        {
            // Clean up all data
            var privileges = _testContext.Privileges.ToList();
            if (privileges.Any())
            {
                _testContext.Privileges.RemoveRange(privileges);
            }
            
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
            
            var privileges = _testContext.Privileges.ToList();
            if (privileges.Any())
            {
                _testContext.Privileges.RemoveRange(privileges);
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
    /// EP1: Valid existing ID - should return privilege
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_ValidId_PrivilegeExists_ShouldReturnPrivilege()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        await _privilegeRepository.CreateAsync(privilege);

        // Act
        var result = await _service.GetByIdAsync(privilege.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(privilege.Id, result.Id);
        Assert.Equal(privilege.Username, result.Username);
        Assert.Equal(privilege.Balance, result.Balance);
    }

    /// <summary>
    /// EP2: Non-existing ID - should throw PrivilegeNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetByIdAsync_NonExistingId_ShouldThrowPrivilegeNotFoundException()
    {
        // Arrange
        var nonExistingId = 9999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeNotFoundException>(
            () => _service.GetByIdAsync(nonExistingId)
        );
        Assert.Equal(nonExistingId, exception.PrivilegeId);
    }

    /// <summary>
    /// EP3: Invalid ID <= 0 - should throw PrivilegeValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [Integration]
    public async Task GetByIdAsync_InvalidId_ShouldThrowPrivilegeValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid Privilege ID", exception.Message);
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
    /// EP2: Single privilege - should return list with one privilege
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_SinglePrivilege_ShouldReturnListWithOnePrivilege()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        await _privilegeRepository.CreateAsync(privilege);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(privilege.Id, result[0].Id);
    }

    /// <summary>
    /// EP3: Multiple privileges - should return all privileges
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_MultiplePrivileges_ShouldReturnAllPrivileges()
    {
        // Arrange
        var privileges = PrivilegeMother.CreatePrivilegeList(5);
        foreach (var p in privileges)
        {
            await _privilegeRepository.CreateAsync(p);
        }

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
    }

    /// <summary>
    /// EP4: Filter by status - should return matching privileges
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetAllAsync_FilterByStatus_ShouldReturnMatchingPrivileges()
    {
        // Clean up before test to ensure isolation
        CleanupDatabase();
        
        // Arrange - use unique IDs to avoid conflicts
        var goldPrivilege = new PrivilegeBuilder()
            .WithId(0)
            .WithUsername("gold.user.test1")
            .WithStatus(PrivilegeStatus.GOLD)
            .WithBalance(1000)
            .Build();
        await _privilegeRepository.CreateAsync(goldPrivilege);

        var silverPrivilege = new PrivilegeBuilder()
            .WithId(0)
            .WithUsername("silver.user.test1")
            .WithStatus(PrivilegeStatus.SILVER)
            .WithBalance(500)
            .Build();
        await _privilegeRepository.CreateAsync(silverPrivilege);

        var filter = new PrivilegeFilter { Status = PrivilegeStatus.GOLD };

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(PrivilegeStatus.GOLD, result[0].Status);
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
        
        // Arrange - use unique username to avoid conflicts
        var privilege = new PrivilegeBuilder()
            .WithId(0)
            .WithUsername("bronze.user.test1")
            .WithStatus(PrivilegeStatus.BRONZE)
            .WithBalance(100)
            .Build();
        await _privilegeRepository.CreateAsync(privilege);

        var filter = new PrivilegeFilter { Status = PrivilegeStatus.GOLD };

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region CreateAsync Tests

    /// <summary>
    /// EP1: Valid privilege with all fields - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_ValidPrivilege_ShouldCreatePrivilege()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();

        // Act
        var result = await _service.CreateAsync(privilege);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal(privilege.Username, result.Username);
        Assert.Equal(privilege.Balance, result.Balance);
    }

    /// <summary>
    /// EP2: Valid privilege with minimal data - should create successfully
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_MinimalPrivilege_ShouldCreatePrivilege()
    {
        // Arrange
        var privilege = new Privilege
        {
            Username = "minimal_user",
            Balance = 0,
            Status = PrivilegeStatus.BRONZE
        };

        // Act
        var result = await _service.CreateAsync(privilege);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(0, result.Id);
        Assert.Equal("minimal_user", result.Username);
    }

    /// <summary>
    /// EP3: Empty username - should throw PrivilegeValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_EmptyUsername_ShouldThrowPrivilegeValidationException()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Username = "";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeValidationException>(
            () => _service.CreateAsync(privilege)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Negative balance - should throw PrivilegeValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_NegativeBalance_ShouldThrowPrivilegeValidationException()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Balance = -100;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeValidationException>(
            () => _service.CreateAsync(privilege)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP5: Too long username - should throw PrivilegeValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreateAsync_TooLongUsername_ShouldThrowPrivilegeValidationException()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Username = new string('A', 81);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeValidationException>(
            () => _service.CreateAsync(privilege)
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
    public async Task UpdateAsync_ValidUpdate_ShouldUpdatePrivilege()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        await _privilegeRepository.CreateAsync(privilege);
        
        privilege.Username = "updated_username";
        privilege.Balance = 5000;

        // Act
        var result = await _service.UpdateAsync(privilege);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("updated_username", result.Username);
        Assert.Equal(5000, result.Balance);
    }

    /// <summary>
    /// EP2: Non-existing ID - should throw PrivilegeNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_NonExistingId_ShouldThrowPrivilegeNotFoundException()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Id = 9999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeNotFoundException>(
            () => _service.UpdateAsync(privilege)
        );
        Assert.Equal(privilege.Id, exception.PrivilegeId);
    }

    /// <summary>
    /// EP3: Invalid username - should throw PrivilegeValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_InvalidUsername_ShouldThrowPrivilegeValidationException()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        await _privilegeRepository.CreateAsync(privilege);
        privilege.Username = "";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeValidationException>(
            () => _service.UpdateAsync(privilege)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Invalid ID <= 0 - should throw PrivilegeValidationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task UpdateAsync_InvalidId_ShouldThrowPrivilegeValidationException()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Id = -1;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeValidationException>(
            () => _service.UpdateAsync(privilege)
        );
        Assert.Contains("Invalid Privilege ID", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Valid existing ID - should return true and delete
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_ValidId_ShouldDeletePrivilege()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        await _privilegeRepository.CreateAsync(privilege);

        // Act
        var result = await _service.DeleteAsync(privilege.Id);

        // Assert
        Assert.True(result);
        
        // Verify it's deleted
        var exception = await Assert.ThrowsAsync<ServicePrivilegeNotFoundException>(
            () => _service.GetByIdAsync(privilege.Id)
        );
        Assert.Equal(privilege.Id, exception.PrivilegeId);
    }

    /// <summary>
    /// EP2: Non-existing ID - should throw PrivilegeNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task DeleteAsync_NonExistingId_ShouldThrowPrivilegeNotFoundException()
    {
        // Arrange
        var nonExistingId = 9999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeNotFoundException>(
            () => _service.DeleteAsync(nonExistingId)
        );
        Assert.Equal(nonExistingId, exception.PrivilegeId);
    }

    /// <summary>
    /// EP3: Invalid ID <= 0 - should throw PrivilegeValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task DeleteAsync_InvalidId_ShouldThrowPrivilegeValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid Privilege ID", exception.Message);
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
        var privilege = PrivilegeMother.CreateValidPrivilege();
        await _privilegeRepository.CreateAsync(privilege);

        // Act
        var result = await _service.ExistsAsync(privilege.Id);

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
        var privilege = PrivilegeMother.CreateValidPrivilege();
        await _privilegeRepository.CreateAsync(privilege);
        await _service.DeleteAsync(privilege.Id);

        // Act
        var result = await _service.ExistsAsync(privilege.Id);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP4: Invalid ID <= 0 - should throw PrivilegeValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task ExistsAsync_InvalidId_ShouldThrowPrivilegeValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid Privilege ID", exception.Message);
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
    /// EP2: Single privilege - should return 1
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetCountAsync_SinglePrivilege_ShouldReturnOne()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        await _privilegeRepository.CreateAsync(privilege);

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(1, result);
    }

    /// <summary>
    /// EP3: Multiple privileges - should return correct count
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetCountAsync_MultiplePrivileges_ShouldReturnCorrectCount()
    {
        // Arrange
        var privileges = PrivilegeMother.CreatePrivilegeList(7);
        foreach (var p in privileges)
        {
            await _privilegeRepository.CreateAsync(p);
        }

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(7, result);
    }

    #endregion

    #region CreditBalanceAsync Tests

    /// <summary>
    /// EP1: Valid credit operation - balance increases, history created
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreditBalanceAsync_ValidOperation_ShouldCreditBalance()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Balance = 1000;
        await _privilegeRepository.CreateAsync(privilege);
        
        var creditAmount = 500;
        var ticketUid = Guid.NewGuid();

        // Act
        var result = await _service.CreditBalanceAsync(privilege.Id, creditAmount, ticketUid);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1500, result.Balance);
        
        // Verify history was created
        var history = await _privilegeHistoryRepository.GetAllAsync(null);
        Assert.Single(history);
        Assert.Equal(creditAmount, history[0].BalanceDiff);
        Assert.Equal(OperationType.FILL_IN_BALANCE, history[0].OperationType);
    }

    /// <summary>
    /// EP2: Invalid privilege ID - should throw PrivilegeValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task CreditBalanceAsync_InvalidPrivilegeId_ShouldThrowPrivilegeValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeValidationException>(
            () => _service.CreditBalanceAsync(invalidId, 100, Guid.NewGuid())
        );
        Assert.Contains("Invalid Privilege ID", exception.Message);
    }

    /// <summary>
    /// EP3: Invalid amount <= 0 - should throw PrivilegeValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    [Integration]
    public async Task CreditBalanceAsync_InvalidAmount_ShouldThrowPrivilegeValidationException(int invalidAmount)
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        await _privilegeRepository.CreateAsync(privilege);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeValidationException>(
            () => _service.CreditBalanceAsync(privilege.Id, invalidAmount, Guid.NewGuid())
        );
        Assert.Contains("Invalid amount", exception.Message);
    }

    /// <summary>
    /// EP4: Non-existing privilege - should throw PrivilegeNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task CreditBalanceAsync_NonExistingPrivilege_ShouldThrowPrivilegeNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeNotFoundException>(
            () => _service.CreditBalanceAsync(9999, 100, Guid.NewGuid())
        );
        Assert.Equal(9999, exception.PrivilegeId);
    }

    #endregion

    #region DebitBalanceAsync Tests

    /// <summary>
    /// EP1: Valid debit with sufficient balance - balance decreases, history created
    /// </summary>
    [Fact]
    [Integration]
    public async Task DebitBalanceAsync_ValidOperation_ShouldDebitBalance()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Balance = 2000;
        await _privilegeRepository.CreateAsync(privilege);
        
        var debitAmount = 700;
        var ticketUid = Guid.NewGuid();

        // Act
        var result = await _service.DebitBalanceAsync(privilege.Id, debitAmount, ticketUid);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1300, result.Balance);
        
        // Verify history was created
        var history = await _privilegeHistoryRepository.GetAllAsync(null);
        Assert.Single(history);
        Assert.Equal(-debitAmount, history[0].BalanceDiff);
        Assert.Equal(OperationType.DEBIT_THE_ACCOUNT, history[0].OperationType);
    }

    /// <summary>
    /// EP2: Insufficient balance - should throw PrivilegeBusinessRuleViolationException
    /// </summary>
    [Fact]
    [Integration]
    public async Task DebitBalanceAsync_InsufficientBalance_ShouldThrowPrivilegeBusinessRuleViolationException()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Balance = 100;
        await _privilegeRepository.CreateAsync(privilege);
        
        var debitAmount = 500;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<core.exceptions.businesslogic.services.PrivilegeBusinessRuleViolationException>(
            () => _service.DebitBalanceAsync(privilege.Id, debitAmount, Guid.NewGuid())
        );
        Assert.Contains("InsufficientBalance", exception.RuleName);
    }

    /// <summary>
    /// EP3: Invalid privilege ID - should throw PrivilegeValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task DebitBalanceAsync_InvalidPrivilegeId_ShouldThrowPrivilegeValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeValidationException>(
            () => _service.DebitBalanceAsync(invalidId, 100, Guid.NewGuid())
        );
        Assert.Contains("Invalid Privilege ID", exception.Message);
    }

    /// <summary>
    /// EP4: Invalid amount <= 0 - should throw PrivilegeValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    [Integration]
    public async Task DebitBalanceAsync_InvalidAmount_ShouldThrowPrivilegeValidationException(int invalidAmount)
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        await _privilegeRepository.CreateAsync(privilege);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeValidationException>(
            () => _service.DebitBalanceAsync(privilege.Id, invalidAmount, Guid.NewGuid())
        );
        Assert.Contains("Invalid amount", exception.Message);
    }

    #endregion

    #region GetMaxDebitAmountAsync Tests

    /// <summary>
    /// EP1: Valid existing ID - should return current balance
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetMaxDebitAmountAsync_ValidId_ShouldReturnCurrentBalance()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Balance = 3500;
        await _privilegeRepository.CreateAsync(privilege);

        // Act
        var result = await _service.GetMaxDebitAmountAsync(privilege.Id);

        // Assert
        Assert.Equal(3500, result);
    }

    /// <summary>
    /// EP2: Non-existing ID - should throw PrivilegeNotFoundException
    /// </summary>
    [Fact]
    [Integration]
    public async Task GetMaxDebitAmountAsync_NonExistingId_ShouldThrowPrivilegeNotFoundException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeNotFoundException>(
            () => _service.GetMaxDebitAmountAsync(9999)
        );
        Assert.Equal(9999, exception.PrivilegeId);
    }

    /// <summary>
    /// EP3: Invalid ID <= 0 - should throw PrivilegeValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Integration]
    public async Task GetMaxDebitAmountAsync_InvalidId_ShouldThrowPrivilegeValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ServicePrivilegeValidationException>(
            () => _service.GetMaxDebitAmountAsync(invalidId)
        );
        Assert.Contains("Invalid Privilege ID", exception.Message);
    }

    #endregion
}
