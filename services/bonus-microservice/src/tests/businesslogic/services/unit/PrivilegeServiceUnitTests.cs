using core.domain;
using core.enums;
using core.exceptions.businesslogic.services;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.repositories;
using businesslogic.services;
using Moq;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

using RepositoryPrivilegeAlreadyExistsException = core.exceptions.dataaccess.repositories.PrivilegeAlreadyExistsException;
using RepositoryPrivilegeNotFoundException = core.exceptions.dataaccess.repositories.PrivilegeNotFoundException;

namespace tests.businesslogic.services.unit;

/// <summary>
/// Unit tests for PrivilegeService
/// Using London-style testing with Mocks (Moq)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetByIdAsync(int id):
/// - EP1: Valid ID, Privilege exists (normal case)
/// - EP2: Valid ID, Privilege does not exist (PrivilegeNotFoundException)
/// - EP3: Invalid ID (<= 0) (PrivilegeValidationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For GetAllAsync(PrivilegeFilter? filter):
/// - EP1: No filter, returns all privileges
/// - EP2: With filter, returns filtered privileges
/// - EP3: Repository throws exception (BaseServiceException)
/// 
/// For CreateAsync(Privilege privilege):
/// - EP1: Valid privilege, creation successful (normal case)
/// - EP2: Invalid privilege (PrivilegeValidationException)
/// - EP3: Privilege already exists (PrivilegeBusinessRuleViolationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For UpdateAsync(Privilege privilege):
/// - EP1: Valid privilege, Privilege exists, update successful (normal case)
/// - EP2: Privilege does not exist (PrivilegeNotFoundException)
/// - EP3: Invalid privilege (PrivilegeValidationException)
/// - EP4: Invalid ID (PrivilegeValidationException)
/// - EP5: Repository throws exception (BaseServiceException)
/// 
/// For DeleteAsync(int id):
/// - EP1: Valid ID, Privilege exists, deletion successful (normal case)
/// - EP2: Valid ID, Privilege does not exist (PrivilegeNotFoundException)
/// - EP3: Invalid ID (<= 0) (PrivilegeValidationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For ExistsAsync(int id):
/// - EP1: Valid ID, Privilege exists (returns true)
/// - EP2: Valid ID, Privilege does not exist (returns false)
/// - EP3: Invalid ID (<= 0) (PrivilegeValidationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For GetCountAsync(PrivilegeFilter? filter):
/// - EP1: No filter, returns correct count
/// - EP2: With filter, returns filtered count
/// - EP3: Repository throws exception (BaseServiceException)
/// 
/// For CreditBalanceAsync(int privilegeId, int amount, Guid ticketUid):
/// - EP1: Valid parameters, sufficient balance, credit successful (normal case)
/// - EP2: Invalid privilege ID (PrivilegeValidationException)
/// - EP3: Invalid amount (<= 0) (PrivilegeValidationException)
/// - EP4: Privilege not found (PrivilegeNotFoundException)
/// - EP5: Repository error (BaseServiceException)
/// 
/// For DebitBalanceAsync(int privilegeId, int amount, Guid ticketUid):
/// - EP1: Valid parameters, sufficient balance, debit successful (normal case)
/// - EP2: Insufficient balance (PrivilegeBusinessRuleViolationException)
/// - EP3: Invalid privilege ID (PrivilegeValidationException)
/// - EP4: Invalid amount (<= 0) (PrivilegeValidationException)
/// - EP5: Privilege not found (PrivilegeNotFoundException)
/// - EP6: Repository error (BaseServiceException)
/// 
/// For GetMaxDebitAmountAsync(int privilegeId):
/// - EP1: Valid ID, returns current balance
/// - EP2: Invalid ID (PrivilegeValidationException)
/// - EP3: Privilege not found (PrivilegeNotFoundException)
/// - EP4: Repository error (BaseServiceException)
/// 
/// Total: 38 unit tests (all should pass)
/// </summary>
public class PrivilegeServiceUnitTests
{
    private readonly Mock<IPrivilegeRepository> _mockPrivilegeRepository;
    private readonly Mock<IPrivilegeHistoryRepository> _mockPrivilegeHistoryRepository;
    private readonly IPrivilegeService _service;

    public PrivilegeServiceUnitTests()
    {
        // Arrange - Setup mock repositories
        _mockPrivilegeRepository = new Mock<IPrivilegeRepository>();
        _mockPrivilegeHistoryRepository = new Mock<IPrivilegeHistoryRepository>();
        _service = new PrivilegeService(_mockPrivilegeRepository.Object, _mockPrivilegeHistoryRepository.Object);
    }

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Valid ID, Privilege exists - should return Privilege
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_ValidId_PrivilegeExists_ShouldReturnPrivilege()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        _mockPrivilegeRepository.Setup(r => r.GetByIdAsync(privilege.Id)).ReturnsAsync(privilege);

        // Act
        var result = await _service.GetByIdAsync(privilege.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(privilege.Id, result.Id);
        Assert.Equal(privilege.Username, result.Username);
        _mockPrivilegeRepository.Verify(r => r.GetByIdAsync(privilege.Id), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Privilege does not exist - should throw PrivilegeNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_ValidId_PrivilegeNotFound_ShouldThrowPrivilegeNotFoundException()
    {
        // Arrange
        var privilegeId = 999;
        _mockPrivilegeRepository.Setup(r => r.GetByIdAsync(privilegeId)).ThrowsAsync(new RepositoryPrivilegeNotFoundException(privilegeId));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeNotFoundException>(
            () => _service.GetByIdAsync(privilegeId)
        );
        Assert.Equal(privilegeId, exception.PrivilegeId);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw PrivilegeValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [Unit]
    public async Task GetByIdAsync_InvalidId_ShouldThrowPrivilegeValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid Privilege ID", exception.Message);
    }

    /// <summary>
    /// EP4: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var privilegeId = 1;
        _mockPrivilegeRepository.Setup(r => r.GetByIdAsync(privilegeId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.GetByIdAsync(privilegeId)
        );
        Assert.Contains($"Failed to get Privilege with ID {privilegeId}", exception.Message);
    }

    #endregion

    #region GetAllAsync Tests

    /// <summary>
    /// EP1: No filter, returns all privileges - should return list of privileges
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllAsync_NoFilter_ShouldReturnAllPrivileges()
    {
        // Arrange
        var privileges = PrivilegeMother.CreatePrivilegeList(5);
        _mockPrivilegeRepository.Setup(r => r.GetAllAsync(null)).ReturnsAsync(privileges);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        _mockPrivilegeRepository.Verify(r => r.GetAllAsync(null), Times.Once);
    }

    /// <summary>
    /// EP2: With filter, returns filtered privileges - should pass filter to repository
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllAsync_WithFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var filter = new PrivilegeFilter { Status = PrivilegeStatus.GOLD };
        var filteredPrivileges = new List<Privilege> { PrivilegeMother.CreateValidPrivilege() };
        _mockPrivilegeRepository.Setup(r => r.GetAllAsync(filter)).ReturnsAsync(filteredPrivileges);

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        _mockPrivilegeRepository.Verify(r => r.GetAllAsync(filter), Times.Once);
    }

    /// <summary>
    /// EP3: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        _mockPrivilegeRepository.Setup(r => r.GetAllAsync(null))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.GetAllAsync()
        );
        Assert.Contains("Failed to get all Privileges", exception.Message);
    }

    #endregion

    #region CreateAsync Tests

    /// <summary>
    /// EP1: Valid privilege, creation successful - should return created privilege
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_ValidPrivilege_ShouldCreatePrivilege()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        _mockPrivilegeRepository.Setup(r => r.CreateAsync(privilege)).ReturnsAsync(privilege);

        // Act
        var result = await _service.CreateAsync(privilege);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(privilege.Username, result.Username);
        _mockPrivilegeRepository.Verify(r => r.CreateAsync(privilege), Times.Once);
    }

    /// <summary>
    /// EP2: Invalid privilege (null) - should throw PrivilegeValidationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_NullPrivilege_ShouldThrowPrivilegeValidationException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.CreateAsync(null!)
        );
        Assert.Contains("Privilege cannot be null", exception.Message);
    }

    /// <summary>
    /// EP3: Invalid privilege (empty username) - should throw PrivilegeValidationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_EmptyUsername_ShouldThrowPrivilegeValidationException()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Username = "";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.CreateAsync(privilege)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Privilege already exists - should throw PrivilegeBusinessRuleViolationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_PrivilegeAlreadyExists_ShouldThrowPrivilegeBusinessRuleViolationException()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        _mockPrivilegeRepository.Setup(r => r.CreateAsync(privilege))
            .ThrowsAsync(new RepositoryPrivilegeAlreadyExistsException(privilege.Id));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeBusinessRuleViolationException>(
            () => _service.CreateAsync(privilege)
        );
        Assert.Contains("Privilege", exception.Message);
    }

    /// <summary>
    /// EP5: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        _mockPrivilegeRepository.Setup(r => r.CreateAsync(privilege))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.CreateAsync(privilege)
        );
        Assert.Contains("Failed to create Privilege", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// EP1: Valid privilege, Privilege exists, update successful - should return updated privilege
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_ValidPrivilege_PrivilegeExists_ShouldUpdatePrivilege()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        _mockPrivilegeRepository.Setup(r => r.ExistsAsync(privilege.Id)).ReturnsAsync(true);
        _mockPrivilegeRepository.Setup(r => r.UpdateAsync(privilege)).ReturnsAsync(privilege);

        // Act
        var result = await _service.UpdateAsync(privilege);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(privilege.Id, result.Id);
        _mockPrivilegeRepository.Verify(r => r.ExistsAsync(privilege.Id), Times.Once);
        _mockPrivilegeRepository.Verify(r => r.UpdateAsync(privilege), Times.Once);
    }

    /// <summary>
    /// EP2: Privilege does not exist - should throw PrivilegeNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_PrivilegeNotFound_ShouldThrowPrivilegeNotFoundException()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        _mockPrivilegeRepository.Setup(r => r.ExistsAsync(privilege.Id)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeNotFoundException>(
            () => _service.UpdateAsync(privilege)
        );
        Assert.Equal(privilege.Id, exception.PrivilegeId);
    }

    /// <summary>
    /// EP3: Invalid privilege (negative balance) - should throw PrivilegeValidationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_NegativeBalance_ShouldThrowPrivilegeValidationException()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Balance = -100;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.UpdateAsync(privilege)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Invalid ID (<= 0) - should throw PrivilegeValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Unit]
    public async Task UpdateAsync_InvalidId_ShouldThrowPrivilegeValidationException(int invalidId)
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Id = invalidId;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.UpdateAsync(privilege)
        );
        Assert.Contains("Invalid Privilege ID", exception.Message);
    }

    /// <summary>
    /// EP5: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        _mockPrivilegeRepository.Setup(r => r.ExistsAsync(privilege.Id)).ReturnsAsync(true);
        _mockPrivilegeRepository.Setup(r => r.UpdateAsync(privilege))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.UpdateAsync(privilege)
        );
        Assert.Contains($"Failed to update Privilege with ID {privilege.Id}", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Valid ID, Privilege exists, deletion successful - should return true
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_ValidId_PrivilegeExists_ShouldDeletePrivilege()
    {
        // Arrange
        var privilegeId = 1;
        _mockPrivilegeRepository.Setup(r => r.ExistsAsync(privilegeId)).ReturnsAsync(true);
        _mockPrivilegeRepository.Setup(r => r.DeleteAsync(privilegeId)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(privilegeId);

        // Assert
        Assert.True(result);
        _mockPrivilegeRepository.Verify(r => r.ExistsAsync(privilegeId), Times.Once);
        _mockPrivilegeRepository.Verify(r => r.DeleteAsync(privilegeId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Privilege does not exist - should throw PrivilegeNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_ValidId_PrivilegeNotFound_ShouldThrowPrivilegeNotFoundException()
    {
        // Arrange
        var privilegeId = 999;
        _mockPrivilegeRepository.Setup(r => r.ExistsAsync(privilegeId)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeNotFoundException>(
            () => _service.DeleteAsync(privilegeId)
        );
        Assert.Equal(privilegeId, exception.PrivilegeId);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw PrivilegeValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Unit]
    public async Task DeleteAsync_InvalidId_ShouldThrowPrivilegeValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid Privilege ID", exception.Message);
    }

    /// <summary>
    /// EP4: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var privilegeId = 1;
        _mockPrivilegeRepository.Setup(r => r.ExistsAsync(privilegeId)).ReturnsAsync(true);
        _mockPrivilegeRepository.Setup(r => r.DeleteAsync(privilegeId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.DeleteAsync(privilegeId)
        );
        Assert.Contains($"Failed to delete Privilege with ID {privilegeId}", exception.Message);
    }

    #endregion

    #region ExistsAsync Tests

    /// <summary>
    /// EP1: Valid ID, Privilege exists - should return true
    /// </summary>
    [Fact]
    [Unit]
    public async Task ExistsAsync_ValidId_PrivilegeExists_ShouldReturnTrue()
    {
        // Arrange
        var privilegeId = 1;
        _mockPrivilegeRepository.Setup(r => r.ExistsAsync(privilegeId)).ReturnsAsync(true);

        // Act
        var result = await _service.ExistsAsync(privilegeId);

        // Assert
        Assert.True(result);
        _mockPrivilegeRepository.Verify(r => r.ExistsAsync(privilegeId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, Privilege does not exist - should return false
    /// </summary>
    [Fact]
    [Unit]
    public async Task ExistsAsync_ValidId_PrivilegeNotFound_ShouldReturnFalse()
    {
        // Arrange
        var privilegeId = 999;
        _mockPrivilegeRepository.Setup(r => r.ExistsAsync(privilegeId)).ReturnsAsync(false);

        // Act
        var result = await _service.ExistsAsync(privilegeId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw PrivilegeValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Unit]
    public async Task ExistsAsync_InvalidId_ShouldThrowPrivilegeValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid Privilege ID", exception.Message);
    }

    /// <summary>
    /// EP4: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task ExistsAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var privilegeId = 1;
        _mockPrivilegeRepository.Setup(r => r.ExistsAsync(privilegeId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.ExistsAsync(privilegeId)
        );
        Assert.Contains($"Failed to check existence of Privilege with ID {privilegeId}", exception.Message);
    }

    #endregion

    #region GetCountAsync Tests

    /// <summary>
    /// EP1: No filter, returns correct count - should return count from repository
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetCountAsync_NoFilter_ShouldReturnCorrectCount()
    {
        // Arrange
        var expectedCount = 42;
        _mockPrivilegeRepository.Setup(r => r.GetCountAsync(null)).ReturnsAsync(expectedCount);

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(expectedCount, result);
        _mockPrivilegeRepository.Verify(r => r.GetCountAsync(null), Times.Once);
    }

    /// <summary>
    /// EP2: With filter, returns filtered count - should pass filter to repository
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetCountAsync_WithFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var filter = new PrivilegeFilter { Status = PrivilegeStatus.SILVER };
        var expectedCount = 5;
        _mockPrivilegeRepository.Setup(r => r.GetCountAsync(filter)).ReturnsAsync(expectedCount);

        // Act
        var result = await _service.GetCountAsync(filter);

        // Assert
        Assert.Equal(expectedCount, result);
        _mockPrivilegeRepository.Verify(r => r.GetCountAsync(filter), Times.Once);
    }

    /// <summary>
    /// EP3: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetCountAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        _mockPrivilegeRepository.Setup(r => r.GetCountAsync(null))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.GetCountAsync()
        );
        Assert.Contains("Failed to get Privilege count", exception.Message);
    }

    #endregion

    #region CreditBalanceAsync Tests

    /// <summary>
    /// EP1: Valid parameters, credit successful - should update balance and create history
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreditBalanceAsync_ValidParameters_ShouldCreditBalance()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        var initialBalance = privilege.Balance;
        var creditAmount = 500;
        var ticketUid = Guid.NewGuid();

        _mockPrivilegeRepository.Setup(r => r.GetByIdAsync(privilege.Id)).ReturnsAsync(privilege);
        _mockPrivilegeRepository.Setup(r => r.UpdateAsync(It.IsAny<Privilege>())).ReturnsAsync(privilege);
        _mockPrivilegeRepository.Setup(r => r.ExistsAsync(privilege.Id)).ReturnsAsync(true);
        _mockPrivilegeHistoryRepository.Setup(r => r.CreateAsync(It.IsAny<PrivilegeHistory>())).ReturnsAsync(new PrivilegeHistory());

        // Act
        var result = await _service.CreditBalanceAsync(privilege.Id, creditAmount, ticketUid);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(initialBalance + creditAmount, result.Balance);
        _mockPrivilegeHistoryRepository.Verify(r => r.CreateAsync(It.IsAny<PrivilegeHistory>()), Times.Once);
    }

    /// <summary>
    /// EP2: Invalid privilege ID - should throw PrivilegeValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Unit]
    public async Task CreditBalanceAsync_InvalidPrivilegeId_ShouldThrowPrivilegeValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.CreditBalanceAsync(invalidId, 100, Guid.NewGuid())
        );
        Assert.Contains("Invalid Privilege ID", exception.Message);
    }

    /// <summary>
    /// EP3: Invalid amount (<= 0) - should throw PrivilegeValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    [Unit]
    public async Task CreditBalanceAsync_InvalidAmount_ShouldThrowPrivilegeValidationException(int invalidAmount)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.CreditBalanceAsync(1, invalidAmount, Guid.NewGuid())
        );
        Assert.Contains("Invalid amount", exception.Message);
    }

    /// <summary>
    /// EP4: Privilege not found - should throw PrivilegeNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreditBalanceAsync_PrivilegeNotFound_ShouldThrowPrivilegeNotFoundException()
    {
        // Arrange
        var privilegeId = 999;
        _mockPrivilegeRepository.Setup(r => r.GetByIdAsync(privilegeId))
            .ThrowsAsync(new RepositoryPrivilegeNotFoundException(privilegeId));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeNotFoundException>(
            () => _service.CreditBalanceAsync(privilegeId, 100, Guid.NewGuid())
        );
        Assert.Equal(privilegeId, exception.PrivilegeId);
    }

    /// <summary>
    /// EP5: Repository error - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreditBalanceAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        _mockPrivilegeRepository.Setup(r => r.GetByIdAsync(1))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.CreditBalanceAsync(1, 100, Guid.NewGuid())
        );
        Assert.Contains("Failed to get Privilege", exception.Message);
    }

    #endregion

    #region DebitBalanceAsync Tests

    /// <summary>
    /// EP1: Valid parameters, sufficient balance - should debit balance and create history
    /// </summary>
    [Fact]
    [Unit]
    public async Task DebitBalanceAsync_ValidParameters_ShouldDebitBalance()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Balance = 1000;
        var debitAmount = 300;
        var ticketUid = Guid.NewGuid();

        _mockPrivilegeRepository.Setup(r => r.GetByIdAsync(privilege.Id)).ReturnsAsync(privilege);
        _mockPrivilegeRepository.Setup(r => r.UpdateAsync(It.IsAny<Privilege>())).ReturnsAsync(privilege);
        _mockPrivilegeRepository.Setup(r => r.ExistsAsync(privilege.Id)).ReturnsAsync(true);
        _mockPrivilegeHistoryRepository.Setup(r => r.CreateAsync(It.IsAny<PrivilegeHistory>())).ReturnsAsync(new PrivilegeHistory());

        // Act
        var result = await _service.DebitBalanceAsync(privilege.Id, debitAmount, ticketUid);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1000 - debitAmount, result.Balance);
        _mockPrivilegeHistoryRepository.Verify(r => r.CreateAsync(It.IsAny<PrivilegeHistory>()), Times.Once);
    }

    /// <summary>
    /// EP2: Insufficient balance - should throw PrivilegeBusinessRuleViolationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task DebitBalanceAsync_InsufficientBalance_ShouldThrowPrivilegeBusinessRuleViolationException()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Balance = 100;
        var debitAmount = 500;

        _mockPrivilegeRepository.Setup(r => r.GetByIdAsync(privilege.Id)).ReturnsAsync(privilege);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeBusinessRuleViolationException>(
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
    [Unit]
    public async Task DebitBalanceAsync_InvalidPrivilegeId_ShouldThrowPrivilegeValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.DebitBalanceAsync(invalidId, 100, Guid.NewGuid())
        );
        Assert.Contains("Invalid Privilege ID", exception.Message);
    }

    /// <summary>
    /// EP4: Invalid amount (<= 0) - should throw PrivilegeValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    [Unit]
    public async Task DebitBalanceAsync_InvalidAmount_ShouldThrowPrivilegeValidationException(int invalidAmount)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.DebitBalanceAsync(1, invalidAmount, Guid.NewGuid())
        );
        Assert.Contains("Invalid amount", exception.Message);
    }

    /// <summary>
    /// EP5: Privilege not found - should throw PrivilegeNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task DebitBalanceAsync_PrivilegeNotFound_ShouldThrowPrivilegeNotFoundException()
    {
        // Arrange
        var privilegeId = 999;
        _mockPrivilegeRepository.Setup(r => r.GetByIdAsync(privilegeId))
            .ThrowsAsync(new RepositoryPrivilegeNotFoundException(privilegeId));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeNotFoundException>(
            () => _service.DebitBalanceAsync(privilegeId, 100, Guid.NewGuid())
        );
        Assert.Equal(privilegeId, exception.PrivilegeId);
    }

    /// <summary>
    /// EP6: Repository error - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task DebitBalanceAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        _mockPrivilegeRepository.Setup(r => r.GetByIdAsync(1))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.DebitBalanceAsync(1, 100, Guid.NewGuid())
        );
        Assert.Contains("Failed to get Privilege", exception.Message);
    }

    #endregion

    #region GetMaxDebitAmountAsync Tests

    /// <summary>
    /// EP1: Valid ID, returns current balance
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetMaxDebitAmountAsync_ValidId_ShouldReturnCurrentBalance()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Balance = 2500;
        _mockPrivilegeRepository.Setup(r => r.GetByIdAsync(privilege.Id)).ReturnsAsync(privilege);

        // Act
        var result = await _service.GetMaxDebitAmountAsync(privilege.Id);

        // Assert
        Assert.Equal(2500, result);
    }

    /// <summary>
    /// EP2: Invalid ID - should throw PrivilegeValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Unit]
    public async Task GetMaxDebitAmountAsync_InvalidId_ShouldThrowPrivilegeValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.GetMaxDebitAmountAsync(invalidId)
        );
        Assert.Contains("Invalid Privilege ID", exception.Message);
    }

    /// <summary>
    /// EP3: Privilege not found - should throw PrivilegeNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetMaxDebitAmountAsync_PrivilegeNotFound_ShouldThrowPrivilegeNotFoundException()
    {
        // Arrange
        var privilegeId = 999;
        _mockPrivilegeRepository.Setup(r => r.GetByIdAsync(privilegeId))
            .ThrowsAsync(new RepositoryPrivilegeNotFoundException(privilegeId));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeNotFoundException>(
            () => _service.GetMaxDebitAmountAsync(privilegeId)
        );
        Assert.Equal(privilegeId, exception.PrivilegeId);
    }

    /// <summary>
    /// EP4: Repository error - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetMaxDebitAmountAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        _mockPrivilegeRepository.Setup(r => r.GetByIdAsync(1))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.GetMaxDebitAmountAsync(1)
        );
        Assert.Contains("Failed to get Privilege", exception.Message);
    }

    #endregion
}
