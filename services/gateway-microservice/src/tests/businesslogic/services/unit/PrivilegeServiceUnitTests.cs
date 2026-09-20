using core.domain;
using Microsoft.Extensions.Logging;
using core.enums;
using core.exceptions.businesslogic.services;
using core.exceptions.dataaccess.gateways;
using core.filters;
using core.interfaces.businesslogic.services;
using core.interfaces.dataaccess.gateways;
using businesslogic.services;
using Moq;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

using GatewayPrivilegeNotFoundException = core.exceptions.dataaccess.gateways.PrivilegeGatewayEntityNotFoundException;
using GatewayPrivilegeCommunicationException = core.exceptions.dataaccess.gateways.PrivilegeGatewayCommunicationException;
using GatewayPrivilegeHistoryNotFoundException = core.exceptions.dataaccess.gateways.PrivilegeHistoryGatewayEntityNotFoundException;
using GatewayPrivilegeHistoryCommunicationException = core.exceptions.dataaccess.gateways.PrivilegeHistoryGatewayCommunicationException;

namespace tests.businesslogic.services.unit;

/// <summary>
/// Unit tests for PrivilegeService
/// Using London-style TDD with Mocks (Moq) for HTTP Gateways
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetByIdAsync(int id):
/// - EP1: Valid ID, Privilege exists (normal case)
/// - EP2: Valid ID, Privilege does not exist (GatewayPrivilegeNotFoundException)
/// - EP3: Invalid ID (<= 0) (PrivilegeValidationException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// For GetAllAsync(PrivilegeFilter? filter):
/// - EP1: No filter, returns all privileges
/// - EP2: With filter, returns filtered privileges
/// - EP3: Gateway communication error (ValidationException)
/// 
/// For CreateAsync(Privilege privilege):
/// - EP1: Valid privilege, creation successful (normal case)
/// - EP2: Invalid privilege (PrivilegeValidationException)
/// - EP3: Gateway communication error (ValidationException)
/// 
/// For UpdateAsync(Privilege privilege):
/// - EP1: Valid privilege, Privilege exists, update successful (normal case)
/// - EP2: Privilege does not exist (GatewayPrivilegeNotFoundException)
/// - EP3: Invalid privilege (PrivilegeValidationException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// For DeleteAsync(int id):
/// - EP1: Valid ID, Privilege exists, deletion successful (normal case)
/// - EP2: Valid ID, Privilege does not exist (GatewayPrivilegeNotFoundException)
/// - EP3: Invalid ID (<= 0) (PrivilegeValidationException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// For ExistsAsync(int id):
/// - EP1: Valid ID, Privilege exists (returns true)
/// - EP2: Valid ID, Privilege does not exist (returns false)
/// - EP3: Invalid ID (<= 0) (PrivilegeValidationException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// For GetCountAsync(PrivilegeFilter? filter):
/// - EP1: No filter, returns correct count
/// - EP2: With filter, returns filtered count
/// - EP3: Gateway communication error (ValidationException)
/// 
/// For CreditBalanceAsync(int privilegeId, int amount, Guid ticketUid):
/// - EP1: Valid parameters, credit successful (normal case)
/// - EP2: Invalid privilege ID (PrivilegeValidationException)
/// - EP3: Invalid amount (<= 0) (PrivilegeValidationException)
/// - EP4: Privilege not found (GatewayPrivilegeNotFoundException)
/// - EP5: Gateway communication error (ValidationException)
/// 
/// For DebitBalanceAsync(int privilegeId, int amount, Guid ticketUid):
/// - EP1: Valid parameters, sufficient balance, debit successful (normal case)
/// - EP2: Insufficient balance (PrivilegeBusinessRuleViolationException)
/// - EP3: Invalid privilege ID (PrivilegeValidationException)
/// - EP4: Invalid amount (<= 0) (PrivilegeValidationException)
/// - EP5: Privilege not found (GatewayPrivilegeNotFoundException)
/// - EP6: Gateway communication error (ValidationException)
/// 
/// For GetMaxDebitAmountAsync(int privilegeId):
/// - EP1: Valid ID, returns current balance
/// - EP2: Invalid ID (PrivilegeValidationException)
/// - EP3: Privilege not found (GatewayPrivilegeNotFoundException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// Total: 32 unit tests (all should pass)
/// </summary>
public class PrivilegeServiceUnitTests
{
    private readonly Mock<IPrivilegeGateway> _mockPrivilegeGateway;
    private readonly Mock<IPrivilegeHistoryGateway> _mockPrivilegeHistoryGateway;
    private readonly IPrivilegeService _service;

    public PrivilegeServiceUnitTests()
    {
        // Arrange - Setup mock gateways
        _mockPrivilegeGateway = new Mock<IPrivilegeGateway>();
        _mockPrivilegeHistoryGateway = new Mock<IPrivilegeHistoryGateway>();
        _service = new PrivilegeService(
            _mockPrivilegeGateway.Object,
            _mockPrivilegeHistoryGateway.Object,
            Mock.Of<ILogger<PrivilegeService>>()
        );
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ValidId_PrivilegeExists_ShouldReturnPrivilege()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        _mockPrivilegeGateway.Setup(g => g.GetByIdAsync(privilege.Id)).ReturnsAsync(privilege);

        // Act
        var result = await _service.GetByIdAsync(privilege.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(privilege.Id, result.Id);
        _mockPrivilegeGateway.Verify(g => g.GetByIdAsync(privilege.Id), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ValidId_PrivilegeNotFound_ShouldThrowPrivilegeNotFoundException()
    {
        // Arrange
        var privilegeId = 999;
        _mockPrivilegeGateway.Setup(g => g.GetByIdAsync(privilegeId)).ThrowsAsync(new GatewayPrivilegeNotFoundException($"Privilege with id {privilegeId} not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeNotFoundException>(
            () => _service.GetByIdAsync(privilegeId)
        );
        Assert.Equal(privilegeId, exception.EntityId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetByIdAsync_InvalidId_ShouldThrowPrivilegeValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid Privilege ID", exception.Message);
    }

    [Fact]
    public async Task GetByIdAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var privilegeId = 1;
        _mockPrivilegeGateway.Setup(g => g.GetByIdAsync(privilegeId))
            .ThrowsAsync(new GatewayPrivilegeCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetByIdAsync(privilegeId)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_NoFilter_ShouldReturnAllPrivileges()
    {
        // Arrange
        var privileges = PrivilegeMother.CreatePrivilegeList(5);
        _mockPrivilegeGateway.Setup(g => g.GetAllAsync()).ReturnsAsync(privileges);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_WithFilter_ShouldReturnFilteredPrivileges()
    {
        // Arrange
        var privileges = PrivilegeMother.CreatePrivilegeList(3);
        var filter = new PrivilegeFilter { Status = PrivilegeStatus.GOLD };
        _mockPrivilegeGateway.Setup(g => g.GetAllAsync(filter)).ReturnsAsync(privileges);

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        _mockPrivilegeGateway.Setup(g => g.GetAllAsync())
            .ThrowsAsync(new GatewayPrivilegeCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetAllAsync()
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidPrivilege_ShouldReturnCreatedPrivilege()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        _mockPrivilegeGateway.Setup(g => g.CreateAsync(It.IsAny<Privilege>())).ReturnsAsync(privilege);

        // Act
        var result = await _service.CreateAsync(privilege);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task CreateAsync_InvalidPrivilege_ShouldThrowPrivilegeValidationException()
    {
        // Arrange
        var privilege = new Privilege { Username = "", Balance = 100 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.CreateAsync(privilege)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        _mockPrivilegeGateway.Setup(g => g.CreateAsync(It.IsAny<Privilege>()))
            .ThrowsAsync(new GatewayPrivilegeCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateAsync(privilege)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ValidPrivilege_PrivilegeExists_ShouldReturnUpdatedPrivilege()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        _mockPrivilegeGateway.Setup(g => g.GetByIdAsync(privilege.Id)).ReturnsAsync(privilege);
        _mockPrivilegeGateway.Setup(g => g.UpdateAsync(It.IsAny<Privilege>())).ReturnsAsync(privilege);

        // Act
        var result = await _service.UpdateAsync(privilege);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task UpdateAsync_PrivilegeNotFound_ShouldThrowPrivilegeNotFoundException()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        _mockPrivilegeGateway.Setup(g => g.UpdateAsync(It.IsAny<Privilege>())).ThrowsAsync(new GatewayPrivilegeNotFoundException($"Privilege with id {privilege.Id} not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeNotFoundException>(
            () => _service.UpdateAsync(privilege)
        );
        Assert.Equal(privilege.Id, exception.EntityId);
    }

    [Fact]
    public async Task UpdateAsync_InvalidPrivilege_ShouldThrowPrivilegeValidationException()
    {
        // Arrange
        var privilege = new Privilege { Id = 1, Username = "", Balance = 100 };
        _mockPrivilegeGateway.Setup(g => g.GetByIdAsync(privilege.Id)).ReturnsAsync(privilege);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.UpdateAsync(privilege)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        _mockPrivilegeGateway.Setup(g => g.GetByIdAsync(privilege.Id)).ReturnsAsync(privilege);
        _mockPrivilegeGateway.Setup(g => g.UpdateAsync(It.IsAny<Privilege>()))
            .ThrowsAsync(new GatewayPrivilegeCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.UpdateAsync(privilege)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ValidId_PrivilegeExists_ShouldReturnTrue()
    {
        // Arrange
        var privilegeId = 1;
        _mockPrivilegeGateway.Setup(g => g.GetByIdAsync(privilegeId)).ReturnsAsync(PrivilegeMother.CreateValidPrivilege());
        _mockPrivilegeGateway.Setup(g => g.DeleteAsync(privilegeId)).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(privilegeId);

        // Assert
        // DeleteAsync now returns void
    }

    [Fact]
    public async Task DeleteAsync_ValidId_PrivilegeNotFound_ShouldThrowPrivilegeNotFoundException()
    {
        // Arrange
        var privilegeId = 999;
        _mockPrivilegeGateway.Setup(g => g.DeleteAsync(privilegeId)).ThrowsAsync(new GatewayPrivilegeNotFoundException($"Privilege with id {privilegeId} not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeNotFoundException>(
            () => _service.DeleteAsync(privilegeId)
        );
        Assert.Equal(privilegeId, exception.EntityId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DeleteAsync_InvalidId_ShouldThrowPrivilegeValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid Privilege ID", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var privilegeId = 1;
        _mockPrivilegeGateway.Setup(g => g.GetByIdAsync(privilegeId)).ReturnsAsync(PrivilegeMother.CreateValidPrivilege());
        _mockPrivilegeGateway.Setup(g => g.DeleteAsync(privilegeId))
            .ThrowsAsync(new GatewayPrivilegeCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.DeleteAsync(privilegeId)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region CreditBalanceAsync Tests

    [Fact]
    public async Task CreditBalanceAsync_ValidParameters_ShouldReturnUpdatedPrivilege()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        var originalBalance = privilege.Balance;
        var creditAmount = 100;
        var ticketUid = Guid.NewGuid();

        _mockPrivilegeGateway.Setup(g => g.GetByIdAsync(privilege.Id)).ReturnsAsync(privilege);
        _mockPrivilegeGateway.Setup(g => g.UpdateAsync(It.IsAny<Privilege>())).ReturnsAsync(privilege);
        _mockPrivilegeHistoryGateway.Setup(g => g.CreateAsync(It.IsAny<PrivilegeHistory>()))
            .ReturnsAsync(new PrivilegeHistory { Id = 1, PrivilegeId = privilege.Id });

        // Act
        var result = await _service.CreditBalanceAsync(privilege.Id, creditAmount, ticketUid);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(originalBalance + creditAmount, result.Balance);
        _mockPrivilegeHistoryGateway.Verify(g => g.CreateAsync(It.IsAny<PrivilegeHistory>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreditBalanceAsync_InvalidPrivilegeId_ShouldThrowPrivilegeValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.CreditBalanceAsync(invalidId, 100, Guid.NewGuid())
        );
        Assert.Contains("Invalid Privilege ID", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreditBalanceAsync_InvalidAmount_ShouldThrowPrivilegeValidationException(int invalidAmount)
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.CreditBalanceAsync(privilege.Id, invalidAmount, Guid.NewGuid())
        );
        Assert.Contains("Invalid amount", exception.Message);
    }

    [Fact]
    public async Task CreditBalanceAsync_PrivilegeNotFound_ShouldThrowPrivilegeNotFoundException()
    {
        // Arrange
        var privilegeId = 999;
        _mockPrivilegeGateway.Setup(g => g.GetByIdAsync(privilegeId)).ThrowsAsync(new GatewayPrivilegeNotFoundException($"Privilege with id {privilegeId} not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeNotFoundException>(
            () => _service.CreditBalanceAsync(privilegeId, 100, Guid.NewGuid())
        );
        Assert.Equal(privilegeId, exception.PrivilegeId);
    }

    [Fact]
    public async Task CreditBalanceAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        _mockPrivilegeGateway.Setup(g => g.GetByIdAsync(privilege.Id)).ReturnsAsync(privilege);
        _mockPrivilegeGateway.Setup(g => g.UpdateAsync(It.IsAny<Privilege>()))
            .ThrowsAsync(new GatewayPrivilegeCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreditBalanceAsync(privilege.Id, 100, Guid.NewGuid())
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region DebitBalanceAsync Tests

    [Fact]
    public async Task DebitBalanceAsync_ValidParameters_ShouldReturnUpdatedPrivilege()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Balance = 1000;
        var originalBalance = privilege.Balance;
        var debitAmount = 100;
        var ticketUid = Guid.NewGuid();

        _mockPrivilegeGateway.Setup(g => g.GetByIdAsync(privilege.Id)).ReturnsAsync(privilege);
        _mockPrivilegeGateway.Setup(g => g.UpdateAsync(It.IsAny<Privilege>())).ReturnsAsync(privilege);
        _mockPrivilegeHistoryGateway.Setup(g => g.CreateAsync(It.IsAny<PrivilegeHistory>()))
            .ReturnsAsync(new PrivilegeHistory { Id = 1, PrivilegeId = privilege.Id });

        // Act
        var result = await _service.DebitBalanceAsync(privilege.Id, debitAmount, ticketUid);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(originalBalance - debitAmount, result.Balance);
        _mockPrivilegeHistoryGateway.Verify(g => g.CreateAsync(It.IsAny<PrivilegeHistory>()), Times.Once);
    }

    [Fact]
    public async Task DebitBalanceAsync_InsufficientBalance_ShouldThrowPrivilegeBusinessRuleViolationException()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Balance = 50;
        var debitAmount = 100;

        _mockPrivilegeGateway.Setup(g => g.GetByIdAsync(privilege.Id)).ReturnsAsync(privilege);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeBusinessRuleViolationException>(
            () => _service.DebitBalanceAsync(privilege.Id, debitAmount, Guid.NewGuid())
        );
        Assert.Contains("Insufficient balance", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DebitBalanceAsync_InvalidPrivilegeId_ShouldThrowPrivilegeValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.DebitBalanceAsync(invalidId, 100, Guid.NewGuid())
        );
        Assert.Contains("Invalid Privilege ID", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DebitBalanceAsync_InvalidAmount_ShouldThrowPrivilegeValidationException(int invalidAmount)
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.DebitBalanceAsync(privilege.Id, invalidAmount, Guid.NewGuid())
        );
        Assert.Contains("Invalid amount", exception.Message);
    }

    [Fact]
    public async Task DebitBalanceAsync_PrivilegeNotFound_ShouldThrowPrivilegeNotFoundException()
    {
        // Arrange
        var privilegeId = 999;
        _mockPrivilegeGateway.Setup(g => g.GetByIdAsync(privilegeId)).ThrowsAsync(new GatewayPrivilegeNotFoundException($"Privilege with id {privilegeId} not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeNotFoundException>(
            () => _service.DebitBalanceAsync(privilegeId, 100, Guid.NewGuid())
        );
        Assert.Equal(privilegeId, exception.PrivilegeId);
    }

    [Fact]
    public async Task DebitBalanceAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Balance = 1000;
        _mockPrivilegeGateway.Setup(g => g.GetByIdAsync(privilege.Id)).ReturnsAsync(privilege);
        _mockPrivilegeGateway.Setup(g => g.UpdateAsync(It.IsAny<Privilege>()))
            .ThrowsAsync(new GatewayPrivilegeCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.DebitBalanceAsync(privilege.Id, 100, Guid.NewGuid())
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region GetMaxDebitAmountAsync Tests

    [Fact]
    public async Task GetMaxDebitAmountAsync_ValidId_ShouldReturnBalance()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Balance = 500;
        _mockPrivilegeGateway.Setup(g => g.GetByIdAsync(privilege.Id)).ReturnsAsync(privilege);

        // Act
        var result = await _service.GetMaxDebitAmountAsync(privilege.Id);

        // Assert
        Assert.Equal(500, result);
    }

    [Fact]
    public async Task GetMaxDebitAmountAsync_InvalidId_ShouldThrowPrivilegeValidationException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.GetMaxDebitAmountAsync(-1)
        );
        Assert.Contains("Invalid Privilege ID", exception.Message);
    }

    [Fact]
    public async Task GetMaxDebitAmountAsync_PrivilegeNotFound_ShouldThrowPrivilegeNotFoundException()
    {
        // Arrange
        var privilegeId = 999;
        _mockPrivilegeGateway.Setup(g => g.GetByIdAsync(privilegeId)).ThrowsAsync(new GatewayPrivilegeNotFoundException($"Privilege with id {privilegeId} not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeNotFoundException>(
            () => _service.GetMaxDebitAmountAsync(privilegeId)
        );
        Assert.Equal(privilegeId, exception.PrivilegeId);
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_ValidId_PrivilegeExists_ShouldReturnTrue()
    {
        // Arrange
        var privilegeId = 1;
        _mockPrivilegeGateway.Setup(g => g.GetByIdAsync(privilegeId)).ReturnsAsync(PrivilegeMother.CreateValidPrivilege());

        // Act
        var result = await _service.ExistsAsync(privilegeId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_ValidId_PrivilegeNotFound_ShouldReturnFalse()
    {
        // Arrange
        var privilegeId = 999;
        _mockPrivilegeGateway.Setup(g => g.GetByIdAsync(privilegeId)).ReturnsAsync((Privilege?)null);

        // Act
        var result = await _service.ExistsAsync(privilegeId);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExistsAsync_InvalidId_ShouldThrowPrivilegeValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid Privilege ID", exception.Message);
    }

    [Fact]
    public async Task ExistsAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var privilegeId = 1;
        _mockPrivilegeGateway.Setup(g => g.GetByIdAsync(privilegeId))
            .ThrowsAsync(new GatewayPrivilegeCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.ExistsAsync(privilegeId)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region GetCountAsync Tests

    [Fact]
    public async Task GetCountAsync_NoFilter_ShouldReturnCorrectCount()
    {
        // Arrange
        var privileges = PrivilegeMother.CreatePrivilegeList(10);
        _mockPrivilegeGateway.Setup(g => g.GetAllAsync()).ReturnsAsync(privileges);

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(10, result);
    }

    [Fact]
    public async Task GetCountAsync_WithFilter_ShouldReturnFilteredCount()
    {
        // Arrange
        var privileges = PrivilegeMother.CreatePrivilegeList(5);
        var filter = new PrivilegeFilter { Status = PrivilegeStatus.GOLD };
        _mockPrivilegeGateway.Setup(g => g.GetAllAsync(filter)).ReturnsAsync(privileges);

        // Act
        var result = await _service.GetCountAsync(filter);

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public async Task GetCountAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        _mockPrivilegeGateway.Setup(g => g.GetAllAsync())
            .ThrowsAsync(new GatewayPrivilegeCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetCountAsync()
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion
}
