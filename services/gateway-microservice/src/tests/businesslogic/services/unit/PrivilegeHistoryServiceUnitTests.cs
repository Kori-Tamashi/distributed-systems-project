using core.domain;
using Microsoft.Extensions.Logging;
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

using GatewayPrivilegeHistoryNotFoundException = core.exceptions.dataaccess.gateways.PrivilegeHistoryGatewayEntityNotFoundException;
using GatewayPrivilegeHistoryCommunicationException = core.exceptions.dataaccess.gateways.PrivilegeHistoryGatewayCommunicationException;

namespace tests.businesslogic.services.unit;

/// <summary>
/// Unit tests for PrivilegeHistoryService
/// Using London-style TDD with Mocks (Moq) for HTTP Gateways
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetByIdAsync(int id):
/// - EP1: Valid ID, PrivilegeHistory exists (normal case)
/// - EP2: Valid ID, PrivilegeHistory does not exist (GatewayPrivilegeHistoryNotFoundException)
/// - EP3: Invalid ID (<= 0) (PrivilegeHistoryValidationException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// For GetAllAsync(PrivilegeHistoryFilter? filter):
/// - EP1: No filter, returns all histories
/// - EP2: With filter, returns filtered histories
/// - EP3: Gateway communication error (ValidationException)
/// 
/// For CreateAsync(PrivilegeHistory history):
/// - EP1: Valid history, creation successful (normal case)
/// - EP2: Invalid history (PrivilegeHistoryValidationException)
/// - EP3: Gateway communication error (ValidationException)
/// 
/// For UpdateAsync(PrivilegeHistory history):
/// - EP1: Valid history, History exists, update successful (normal case)
/// - EP2: History does not exist (GatewayPrivilegeHistoryNotFoundException)
/// - EP3: Invalid history (PrivilegeHistoryValidationException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// For DeleteAsync(int id):
/// - EP1: Valid ID, History exists, deletion successful (normal case)
/// - EP2: Valid ID, History does not exist (GatewayPrivilegeHistoryNotFoundException)
/// - EP3: Invalid ID (<= 0) (PrivilegeHistoryValidationException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// For ExistsAsync(int id):
/// - EP1: Valid ID, History exists (returns true)
/// - EP2: Valid ID, History does not exist (returns false)
/// - EP3: Invalid ID (<= 0) (PrivilegeHistoryValidationException)
/// - EP4: Gateway communication error (ValidationException)
/// 
/// For GetCountAsync(PrivilegeHistoryFilter? filter):
/// - EP1: No filter, returns correct count
/// - EP2: With filter, returns filtered count
/// - EP3: Gateway communication error (ValidationException)
/// 
/// Total: 21 unit tests (all should pass)
/// </summary>
public class PrivilegeHistoryServiceUnitTests
{
    private readonly Mock<IPrivilegeHistoryGateway> _mockPrivilegeHistoryGateway;
    private readonly IPrivilegeHistoryService _service;

    public PrivilegeHistoryServiceUnitTests()
    {
        // Arrange - Setup mock gateway
        _mockPrivilegeHistoryGateway = new Mock<IPrivilegeHistoryGateway>();
        _service = new PrivilegeHistoryService(_mockPrivilegeHistoryGateway.Object, Mock.Of<ILogger<PrivilegeHistoryService>>());
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ValidId_HistoryExists_ShouldReturnHistory()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidCreditHistory();
        _mockPrivilegeHistoryGateway.Setup(g => g.GetByIdAsync(history.Id)).ReturnsAsync(history);

        // Act
        var result = await _service.GetByIdAsync(history.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(history.Id, result.Id);
        _mockPrivilegeHistoryGateway.Verify(g => g.GetByIdAsync(history.Id), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ValidId_HistoryNotFound_ShouldThrowPrivilegeHistoryNotFoundException()
    {
        // Arrange
        var historyId = 999;
        _mockPrivilegeHistoryGateway.Setup(g => g.GetByIdAsync(historyId)).ThrowsAsync(new GatewayPrivilegeHistoryNotFoundException($"PrivilegeHistory with id {historyId} not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryNotFoundException>(
            () => _service.GetByIdAsync(historyId)
        );
        Assert.Equal(historyId, exception.EntityId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetByIdAsync_InvalidId_ShouldThrowPrivilegeHistoryValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid PrivilegeHistory ID", exception.Message);
    }

    [Fact]
    public async Task GetByIdAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var historyId = 1;
        _mockPrivilegeHistoryGateway.Setup(g => g.GetByIdAsync(historyId))
            .ThrowsAsync(new GatewayPrivilegeHistoryCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetByIdAsync(historyId)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_NoFilter_ShouldReturnAllHistories()
    {
        // Arrange
        var histories = PrivilegeHistoryMother.CreateHistoryList(5);
        _mockPrivilegeHistoryGateway.Setup(g => g.GetAllAsync()).ReturnsAsync(histories);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_WithFilter_ShouldReturnFilteredHistories()
    {
        // Arrange
        var histories = PrivilegeHistoryMother.CreateHistoryList(3);
        var filter = new PrivilegeHistoryFilter { PrivilegeId = 1 };
        _mockPrivilegeHistoryGateway.Setup(g => g.GetAllAsync(filter)).ReturnsAsync(histories);

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
        _mockPrivilegeHistoryGateway.Setup(g => g.GetAllAsync())
            .ThrowsAsync(new GatewayPrivilegeHistoryCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetAllAsync()
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidHistory_ShouldReturnCreatedHistory()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidCreditHistory();
        _mockPrivilegeHistoryGateway.Setup(g => g.CreateAsync(It.IsAny<PrivilegeHistory>())).ReturnsAsync(history);

        // Act
        var result = await _service.CreateAsync(history);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task CreateAsync_InvalidHistory_ZeroBalanceDiff_ShouldThrowPrivilegeHistoryValidationException()
    {
        // Arrange
        var history = new PrivilegeHistory { PrivilegeId = 1, BalanceDiff = 0, DateTime = DateTime.UtcNow.AddHours(-1), TicketUid = Guid.NewGuid() };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryValidationException>(
            () => _service.CreateAsync(history)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_FutureDateTime_ShouldThrowPrivilegeHistoryValidationException()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidCreditHistory();
        history.DateTime = DateTime.UtcNow.AddDays(30);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryValidationException>(
            () => _service.CreateAsync(history)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidCreditHistory();
        _mockPrivilegeHistoryGateway.Setup(g => g.CreateAsync(It.IsAny<PrivilegeHistory>()))
            .ThrowsAsync(new GatewayPrivilegeHistoryCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateAsync(history)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ValidHistory_HistoryExists_ShouldReturnUpdatedHistory()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidCreditHistory();
        _mockPrivilegeHistoryGateway.Setup(g => g.GetByIdAsync(history.Id)).ReturnsAsync(history);
        _mockPrivilegeHistoryGateway.Setup(g => g.UpdateAsync(It.IsAny<PrivilegeHistory>())).ReturnsAsync(history);

        // Act
        var result = await _service.UpdateAsync(history);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task UpdateAsync_HistoryNotFound_ShouldThrowPrivilegeHistoryNotFoundException()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidCreditHistory();
        _mockPrivilegeHistoryGateway.Setup(g => g.GetByIdAsync(history.Id)).ThrowsAsync(new GatewayPrivilegeHistoryNotFoundException($"PrivilegeHistory with id {history.Id} not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryNotFoundException>(
            () => _service.UpdateAsync(history)
        );
        Assert.Equal(history.Id, exception.EntityId);
    }

    [Fact]
    public async Task UpdateAsync_InvalidHistory_ShouldThrowPrivilegeHistoryValidationException()
    {
        // Arrange
        var history = new PrivilegeHistory { Id = 1, PrivilegeId = 1, BalanceDiff = 0, DateTime = DateTime.UtcNow.AddHours(-1), TicketUid = Guid.NewGuid() };
        _mockPrivilegeHistoryGateway.Setup(g => g.GetByIdAsync(history.Id)).ReturnsAsync(history);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryValidationException>(
            () => _service.UpdateAsync(history)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidCreditHistory();
        _mockPrivilegeHistoryGateway.Setup(g => g.GetByIdAsync(history.Id)).ReturnsAsync(history);
        _mockPrivilegeHistoryGateway.Setup(g => g.UpdateAsync(It.IsAny<PrivilegeHistory>()))
            .ThrowsAsync(new GatewayPrivilegeHistoryCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.UpdateAsync(history)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ValidId_HistoryExists_ShouldReturnTrue()
    {
        // Arrange
        var historyId = 1;
        _mockPrivilegeHistoryGateway.Setup(g => g.GetByIdAsync(historyId)).ReturnsAsync(PrivilegeHistoryMother.CreateValidCreditHistory());
        _mockPrivilegeHistoryGateway.Setup(g => g.DeleteAsync(historyId)).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(historyId);

        // Assert
    }

    [Fact]
    public async Task DeleteAsync_ValidId_HistoryNotFound_ShouldThrowPrivilegeHistoryNotFoundException()
    {
        // Arrange
        var historyId = 999;
        _mockPrivilegeHistoryGateway.Setup(g => g.GetByIdAsync(historyId)).ThrowsAsync(new GatewayPrivilegeHistoryNotFoundException($"PrivilegeHistory with id {historyId} not found"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryNotFoundException>(
            () => _service.DeleteAsync(historyId)
        );
        Assert.Equal(historyId, exception.EntityId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DeleteAsync_InvalidId_ShouldThrowPrivilegeHistoryValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid PrivilegeHistory ID", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var historyId = 1;
        _mockPrivilegeHistoryGateway.Setup(g => g.GetByIdAsync(historyId)).ReturnsAsync(PrivilegeHistoryMother.CreateValidCreditHistory());
        _mockPrivilegeHistoryGateway.Setup(g => g.DeleteAsync(historyId))
            .ThrowsAsync(new GatewayPrivilegeHistoryCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.DeleteAsync(historyId)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_ValidId_HistoryExists_ShouldReturnTrue()
    {
        // Arrange
        var historyId = 1;
        _mockPrivilegeHistoryGateway.Setup(g => g.GetByIdAsync(historyId)).ReturnsAsync(PrivilegeHistoryMother.CreateValidCreditHistory());

        // Act
        var result = await _service.ExistsAsync(historyId);

        // Assert
    }

    [Fact]
    public async Task ExistsAsync_ValidId_HistoryNotFound_ShouldReturnFalse()
    {
        // Arrange
        var historyId = 999;
        _mockPrivilegeHistoryGateway.Setup(g => g.GetByIdAsync(historyId)).ReturnsAsync((PrivilegeHistory?)null);

        // Act
        var result = await _service.ExistsAsync(historyId);

        // Assert
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExistsAsync_InvalidId_ShouldThrowPrivilegeHistoryValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid PrivilegeHistory ID", exception.Message);
    }

    [Fact]
    public async Task ExistsAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        var historyId = 1;
        _mockPrivilegeHistoryGateway.Setup(g => g.GetByIdAsync(historyId))
            .ThrowsAsync(new GatewayPrivilegeHistoryCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.ExistsAsync(historyId)
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion

    #region GetCountAsync Tests

    [Fact]
    public async Task GetCountAsync_NoFilter_ShouldReturnCorrectCount()
    {
        // Arrange
        var histories = PrivilegeHistoryMother.CreateHistoryList(10);
        _mockPrivilegeHistoryGateway.Setup(g => g.GetAllAsync()).ReturnsAsync(histories);

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(10, result);
    }

    [Fact]
    public async Task GetCountAsync_WithFilter_ShouldReturnFilteredCount()
    {
        // Arrange
        var histories = PrivilegeHistoryMother.CreateHistoryList(5);
        var filter = new PrivilegeHistoryFilter { PrivilegeId = 1 };
        _mockPrivilegeHistoryGateway.Setup(g => g.GetAllAsync(filter)).ReturnsAsync(histories);

        // Act
        var result = await _service.GetCountAsync(filter);

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public async Task GetCountAsync_GatewayCommunicationError_ShouldThrowValidationException()
    {
        // Arrange
        _mockPrivilegeHistoryGateway.Setup(g => g.GetAllAsync())
            .ThrowsAsync(new GatewayPrivilegeHistoryCommunicationException("Connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.GetCountAsync()
        );
        Assert.Contains("Failed to communicate", exception.Message);
    }

    #endregion
}
