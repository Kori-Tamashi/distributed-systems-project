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

using RepositoryPrivilegeHistoryAlreadyExistsException = core.exceptions.dataaccess.repositories.PrivilegeHistoryAlreadyExistsException;
using RepositoryPrivilegeHistoryNotFoundException = core.exceptions.dataaccess.repositories.PrivilegeHistoryNotFoundException;

namespace tests.businesslogic.services.unit;

/// <summary>
/// Unit tests for PrivilegeHistoryService
/// Using London-style testing with Mocks (Moq)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For GetByIdAsync(int id):
/// - EP1: Valid ID, PrivilegeHistory exists (normal case)
/// - EP2: Valid ID, PrivilegeHistory does not exist (PrivilegeHistoryNotFoundException)
/// - EP3: Invalid ID (<= 0) (PrivilegeHistoryValidationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For GetAllAsync(PrivilegeHistoryFilter? filter):
/// - EP1: No filter, returns all history records
/// - EP2: With filter, returns filtered history records
/// - EP3: Repository throws exception (BaseServiceException)
/// 
/// For CreateAsync(PrivilegeHistory history):
/// - EP1: Valid history, creation successful (normal case)
/// - EP2: Invalid history (PrivilegeHistoryValidationException)
/// - EP3: History already exists (PrivilegeHistoryBusinessRuleViolationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For UpdateAsync(PrivilegeHistory history):
/// - EP1: Valid history, History exists, update successful (normal case)
/// - EP2: History does not exist (PrivilegeHistoryNotFoundException)
/// - EP3: Invalid history (PrivilegeHistoryValidationException)
/// - EP4: Invalid ID (PrivilegeHistoryValidationException)
/// - EP5: Repository throws exception (BaseServiceException)
/// 
/// For DeleteAsync(int id):
/// - EP1: Valid ID, History exists, deletion successful (normal case)
/// - EP2: Valid ID, History does not exist (PrivilegeHistoryNotFoundException)
/// - EP3: Invalid ID (<= 0) (PrivilegeHistoryValidationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For ExistsAsync(int id):
/// - EP1: Valid ID, History exists (returns true)
/// - EP2: Valid ID, History does not exist (returns false)
/// - EP3: Invalid ID (<= 0) (PrivilegeHistoryValidationException)
/// - EP4: Repository throws exception (BaseServiceException)
/// 
/// For GetCountAsync(PrivilegeHistoryFilter? filter):
/// - EP1: No filter, returns correct count
/// - EP2: With filter, returns filtered count
/// - EP3: Repository throws exception (BaseServiceException)
/// 
/// Total: 27 unit tests (all should pass)
/// </summary>
public class PrivilegeHistoryServiceUnitTests
{
    private readonly Mock<IPrivilegeHistoryRepository> _mockPrivilegeHistoryRepository;
    private readonly IPrivilegeHistoryService _service;

    public PrivilegeHistoryServiceUnitTests()
    {
        // Arrange - Setup mock repository
        _mockPrivilegeHistoryRepository = new Mock<IPrivilegeHistoryRepository>();
        _service = new PrivilegeHistoryService(_mockPrivilegeHistoryRepository.Object);
    }

    #region GetByIdAsync Tests

    /// <summary>
    /// EP1: Valid ID, PrivilegeHistory exists - should return PrivilegeHistory
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_ValidId_HistoryExists_ShouldReturnHistory()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        _mockPrivilegeHistoryRepository.Setup(r => r.GetByIdAsync(history.Id)).ReturnsAsync(history);

        // Act
        var result = await _service.GetByIdAsync(history.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(history.Id, result.Id);
        Assert.Equal(history.PrivilegeId, result.PrivilegeId);
        _mockPrivilegeHistoryRepository.Verify(r => r.GetByIdAsync(history.Id), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, PrivilegeHistory does not exist - should throw PrivilegeHistoryNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_ValidId_HistoryNotFound_ShouldThrowPrivilegeHistoryNotFoundException()
    {
        // Arrange
        var historyId = 999;
        _mockPrivilegeHistoryRepository.Setup(r => r.GetByIdAsync(historyId)).ThrowsAsync(new RepositoryPrivilegeHistoryNotFoundException(historyId));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryNotFoundException>(
            () => _service.GetByIdAsync(historyId)
        );
        Assert.Equal(historyId, exception.HistoryId);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw PrivilegeHistoryValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [Unit]
    public async Task GetByIdAsync_InvalidId_ShouldThrowPrivilegeHistoryValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryValidationException>(
            () => _service.GetByIdAsync(invalidId)
        );
        Assert.Contains("Invalid PrivilegeHistory ID", exception.Message);
    }

    /// <summary>
    /// EP4: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetByIdAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var historyId = 1;
        _mockPrivilegeHistoryRepository.Setup(r => r.GetByIdAsync(historyId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.GetByIdAsync(historyId)
        );
        Assert.Contains($"Failed to get PrivilegeHistory with ID {historyId}", exception.Message);
    }

    #endregion

    #region GetAllAsync Tests

    /// <summary>
    /// EP1: No filter, returns all history records - should return list of history records
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllAsync_NoFilter_ShouldReturnAllHistoryRecords()
    {
        // Arrange
        var histories = PrivilegeHistoryMother.CreateHistoryList(5);
        _mockPrivilegeHistoryRepository.Setup(r => r.GetAllAsync(null)).ReturnsAsync(histories);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        _mockPrivilegeHistoryRepository.Verify(r => r.GetAllAsync(null), Times.Once);
    }

    /// <summary>
    /// EP2: With filter, returns filtered history records - should pass filter to repository
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllAsync_WithFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var filter = new PrivilegeHistoryFilter { PrivilegeId = 1 };
        var filteredHistories = new List<PrivilegeHistory> { PrivilegeHistoryMother.CreateValidHistory() };
        _mockPrivilegeHistoryRepository.Setup(r => r.GetAllAsync(filter)).ReturnsAsync(filteredHistories);

        // Act
        var result = await _service.GetAllAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        _mockPrivilegeHistoryRepository.Verify(r => r.GetAllAsync(filter), Times.Once);
    }

    /// <summary>
    /// EP3: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetAllAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        _mockPrivilegeHistoryRepository.Setup(r => r.GetAllAsync(null))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.GetAllAsync()
        );
        Assert.Contains("Failed to get all PrivilegeHistory records", exception.Message);
    }

    #endregion

    #region CreateAsync Tests

    /// <summary>
    /// EP1: Valid history, creation successful - should return created history
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_ValidHistory_ShouldCreateHistory()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        _mockPrivilegeHistoryRepository.Setup(r => r.CreateAsync(history)).ReturnsAsync(history);

        // Act
        var result = await _service.CreateAsync(history);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(history.PrivilegeId, result.PrivilegeId);
        _mockPrivilegeHistoryRepository.Verify(r => r.CreateAsync(history), Times.Once);
    }

    /// <summary>
    /// EP2: Invalid history (null) - should throw PrivilegeHistoryValidationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_NullHistory_ShouldThrowPrivilegeHistoryValidationException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryValidationException>(
            () => _service.CreateAsync(null!)
        );
        Assert.Contains("PrivilegeHistory cannot be null", exception.Message);
    }

    /// <summary>
    /// EP3: Invalid history (zero BalanceDiff) - should throw PrivilegeHistoryValidationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_ZeroBalanceDiff_ShouldThrowPrivilegeHistoryValidationException()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        history.BalanceDiff = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryValidationException>(
            () => _service.CreateAsync(history)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: History already exists - should throw PrivilegeHistoryBusinessRuleViolationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_HistoryAlreadyExists_ShouldThrowPrivilegeHistoryBusinessRuleViolationException()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        _mockPrivilegeHistoryRepository.Setup(r => r.CreateAsync(history))
            .ThrowsAsync(new RepositoryPrivilegeHistoryAlreadyExistsException(history.Id));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryBusinessRuleViolationException>(
            () => _service.CreateAsync(history)
        );
        Assert.Contains("PrivilegeHistory", exception.Message);
    }

    /// <summary>
    /// EP5: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task CreateAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        _mockPrivilegeHistoryRepository.Setup(r => r.CreateAsync(history))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.CreateAsync(history)
        );
        Assert.Contains("Failed to create PrivilegeHistory", exception.Message);
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// EP1: Valid history, History exists, update successful - should return updated history
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_ValidHistory_HistoryExists_ShouldUpdateHistory()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        _mockPrivilegeHistoryRepository.Setup(r => r.ExistsAsync(history.Id)).ReturnsAsync(true);
        _mockPrivilegeHistoryRepository.Setup(r => r.UpdateAsync(history)).ReturnsAsync(history);

        // Act
        var result = await _service.UpdateAsync(history);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(history.Id, result.Id);
        _mockPrivilegeHistoryRepository.Verify(r => r.ExistsAsync(history.Id), Times.Once);
        _mockPrivilegeHistoryRepository.Verify(r => r.UpdateAsync(history), Times.Once);
    }

    /// <summary>
    /// EP2: History does not exist - should throw PrivilegeHistoryNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_HistoryNotFound_ShouldThrowPrivilegeHistoryNotFoundException()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        _mockPrivilegeHistoryRepository.Setup(r => r.ExistsAsync(history.Id)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryNotFoundException>(
            () => _service.UpdateAsync(history)
        );
        Assert.Equal(history.Id, exception.HistoryId);
    }

    /// <summary>
    /// EP3: Invalid history (negative PrivilegeId) - should throw PrivilegeHistoryValidationException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_NegativePrivilegeId_ShouldThrowPrivilegeHistoryValidationException()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        history.PrivilegeId = -1;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryValidationException>(
            () => _service.UpdateAsync(history)
        );
        Assert.Contains("validation failed", exception.Message);
    }

    /// <summary>
    /// EP4: Invalid ID (<= 0) - should throw PrivilegeHistoryValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Unit]
    public async Task UpdateAsync_InvalidId_ShouldThrowPrivilegeHistoryValidationException(int invalidId)
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        history.Id = invalidId;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryValidationException>(
            () => _service.UpdateAsync(history)
        );
        Assert.Contains("Invalid PrivilegeHistory ID", exception.Message);
    }

    /// <summary>
    /// EP5: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task UpdateAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();
        _mockPrivilegeHistoryRepository.Setup(r => r.ExistsAsync(history.Id)).ReturnsAsync(true);
        _mockPrivilegeHistoryRepository.Setup(r => r.UpdateAsync(history))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.UpdateAsync(history)
        );
        Assert.Contains($"Failed to update PrivilegeHistory with ID {history.Id}", exception.Message);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// EP1: Valid ID, History exists, deletion successful - should return true
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_ValidId_HistoryExists_ShouldDeleteHistory()
    {
        // Arrange
        var historyId = 1;
        _mockPrivilegeHistoryRepository.Setup(r => r.ExistsAsync(historyId)).ReturnsAsync(true);
        _mockPrivilegeHistoryRepository.Setup(r => r.DeleteAsync(historyId)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(historyId);

        // Assert
        Assert.True(result);
        _mockPrivilegeHistoryRepository.Verify(r => r.ExistsAsync(historyId), Times.Once);
        _mockPrivilegeHistoryRepository.Verify(r => r.DeleteAsync(historyId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, History does not exist - should throw PrivilegeHistoryNotFoundException
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_ValidId_HistoryNotFound_ShouldThrowPrivilegeHistoryNotFoundException()
    {
        // Arrange
        var historyId = 999;
        _mockPrivilegeHistoryRepository.Setup(r => r.ExistsAsync(historyId)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryNotFoundException>(
            () => _service.DeleteAsync(historyId)
        );
        Assert.Equal(historyId, exception.HistoryId);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw PrivilegeHistoryValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Unit]
    public async Task DeleteAsync_InvalidId_ShouldThrowPrivilegeHistoryValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryValidationException>(
            () => _service.DeleteAsync(invalidId)
        );
        Assert.Contains("Invalid PrivilegeHistory ID", exception.Message);
    }

    /// <summary>
    /// EP4: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task DeleteAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var historyId = 1;
        _mockPrivilegeHistoryRepository.Setup(r => r.ExistsAsync(historyId)).ReturnsAsync(true);
        _mockPrivilegeHistoryRepository.Setup(r => r.DeleteAsync(historyId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.DeleteAsync(historyId)
        );
        Assert.Contains($"Failed to delete PrivilegeHistory with ID {historyId}", exception.Message);
    }

    #endregion

    #region ExistsAsync Tests

    /// <summary>
    /// EP1: Valid ID, History exists - should return true
    /// </summary>
    [Fact]
    [Unit]
    public async Task ExistsAsync_ValidId_HistoryExists_ShouldReturnTrue()
    {
        // Arrange
        var historyId = 1;
        _mockPrivilegeHistoryRepository.Setup(r => r.ExistsAsync(historyId)).ReturnsAsync(true);

        // Act
        var result = await _service.ExistsAsync(historyId);

        // Assert
        Assert.True(result);
        _mockPrivilegeHistoryRepository.Verify(r => r.ExistsAsync(historyId), Times.Once);
    }

    /// <summary>
    /// EP2: Valid ID, History does not exist - should return false
    /// </summary>
    [Fact]
    [Unit]
    public async Task ExistsAsync_ValidId_HistoryNotFound_ShouldReturnFalse()
    {
        // Arrange
        var historyId = 999;
        _mockPrivilegeHistoryRepository.Setup(r => r.ExistsAsync(historyId)).ReturnsAsync(false);

        // Act
        var result = await _service.ExistsAsync(historyId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// EP3: Invalid ID (<= 0) - should throw PrivilegeHistoryValidationException
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Unit]
    public async Task ExistsAsync_InvalidId_ShouldThrowPrivilegeHistoryValidationException(int invalidId)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<PrivilegeHistoryValidationException>(
            () => _service.ExistsAsync(invalidId)
        );
        Assert.Contains("Invalid PrivilegeHistory ID", exception.Message);
    }

    /// <summary>
    /// EP4: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task ExistsAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        var historyId = 1;
        _mockPrivilegeHistoryRepository.Setup(r => r.ExistsAsync(historyId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.ExistsAsync(historyId)
        );
        Assert.Contains($"Failed to check existence of PrivilegeHistory with ID {historyId}", exception.Message);
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
        _mockPrivilegeHistoryRepository.Setup(r => r.GetCountAsync(null)).ReturnsAsync(expectedCount);

        // Act
        var result = await _service.GetCountAsync();

        // Assert
        Assert.Equal(expectedCount, result);
        _mockPrivilegeHistoryRepository.Verify(r => r.GetCountAsync(null), Times.Once);
    }

    /// <summary>
    /// EP2: With filter, returns filtered count - should pass filter to repository
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetCountAsync_WithFilter_ShouldPassFilterToRepository()
    {
        // Arrange
        var filter = new PrivilegeHistoryFilter { PrivilegeId = 5 };
        var expectedCount = 10;
        _mockPrivilegeHistoryRepository.Setup(r => r.GetCountAsync(filter)).ReturnsAsync(expectedCount);

        // Act
        var result = await _service.GetCountAsync(filter);

        // Assert
        Assert.Equal(expectedCount, result);
        _mockPrivilegeHistoryRepository.Verify(r => r.GetCountAsync(filter), Times.Once);
    }

    /// <summary>
    /// EP3: Repository throws exception - should throw BaseServiceException
    /// </summary>
    [Fact]
    [Unit]
    public async Task GetCountAsync_RepositoryError_ShouldThrowBaseServiceException()
    {
        // Arrange
        _mockPrivilegeHistoryRepository.Setup(r => r.GetCountAsync(null))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BaseServiceException>(
            () => _service.GetCountAsync()
        );
        Assert.Contains("Failed to get PrivilegeHistory count", exception.Message);
    }

    #endregion
}
