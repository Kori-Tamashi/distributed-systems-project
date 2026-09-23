using core.domain;
using core.enums;
using dataaccess.converters.http;
using dataaccess.dto.http;
using dataaccess.dto.http.PrivilegeHistory;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

namespace tests.dataaccess.converters.unit.http;

/// <summary>
/// Unit tests for PrivilegeHistoryHttpConverter
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For ToDTO/ToDomain:
/// - EP1: Valid privilege history with all fields (normal case)
/// - EP2: History with minimal data
/// - EP3: Round-trip conversion preserves data
/// 
/// For ToCreateDTO/ToCreateDomain:
/// - EP1: Valid history to create DTO
/// - EP2: Create DTO round-trip preserves data
/// 
/// For ToUpdateDTO/ToUpdateDomain:
/// - EP1: Valid history to update DTO
/// - EP2: UpdateDomain handles nullable fields correctly
/// 
/// For ToListDTO:
/// - EP1: Convert list of histories
/// - EP2: Empty list handling
/// 
/// Total: 10 unit tests (all should pass)
/// </summary>
public class PrivilegeHistoryHttpConverterUnitTests
{
    #region ToDTO / ToDomain Tests

    /// <summary>
    /// EP1: Valid privilege history with all fields - should convert correctly
    /// </summary>
    [Fact]
    public void ToDTO_ValidPrivilegeHistoryWithAllFields_ShouldConvertCorrectly()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidCreditHistory();

        // Act
        var dto = PrivilegeHistoryHttpConverter.ToDTO(history);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(history.Id, dto.Id);
        Assert.Equal(history.PrivilegeId, dto.PrivilegeId);
        Assert.Equal(history.TicketUid, dto.TicketUid);
        Assert.Equal(history.DateTime, dto.DateTime);
        Assert.Equal(history.BalanceDiff, dto.BalanceDiff);
        Assert.Equal((int)history.OperationType, dto.OperationType);
    }

    /// <summary>
    /// EP2: History with minimal data - should handle correctly
    /// </summary>
    [Fact]
    public void ToDTO_MinimalHistory_ShouldConvertCorrectly()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateMinimalHistory();

        // Act
        var dto = PrivilegeHistoryHttpConverter.ToDTO(history);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(history.Id, dto.Id);
        Assert.Equal(history.PrivilegeId, dto.PrivilegeId);
        Assert.Equal(history.BalanceDiff, dto.BalanceDiff);
    }

    /// <summary>
    /// EP3: Round-trip conversion preserves all data
    /// </summary>
    [Fact]
    public void ToDTO_ToDomain_RoundTripShouldPreserveData()
    {
        // Arrange
        var originalHistory = PrivilegeHistoryMother.CreateValidCreditHistory();

        // Act
        var dto = PrivilegeHistoryHttpConverter.ToDTO(originalHistory);
        var convertedHistory = PrivilegeHistoryHttpConverter.ToDomain(dto);

        // Assert
        Assert.Equal(originalHistory.Id, convertedHistory.Id);
        Assert.Equal(originalHistory.PrivilegeId, convertedHistory.PrivilegeId);
        Assert.Equal(originalHistory.TicketUid, convertedHistory.TicketUid);
        Assert.Equal(originalHistory.DateTime, convertedHistory.DateTime);
        Assert.Equal(originalHistory.BalanceDiff, convertedHistory.BalanceDiff);
        Assert.Equal(originalHistory.OperationType, convertedHistory.OperationType);
    }

    #endregion

    #region ToDomain(CreateDTO) Tests

    /// <summary>
    /// EP1: CreateDTO should convert to domain correctly
    /// </summary>
    [Fact]
    public void ToDomain_CreatePrivilegeHistoryDTO_ShouldConvertCorrectly()
    {
        // Arrange
        var createDto = new CreatePrivilegeHistoryDTO
        {
            PrivilegeId = 1,
            TicketUid = Guid.NewGuid(),
            DateTime = DateTime.UtcNow,
            BalanceDiffNegative = 5000,
            OperationType = (int)OperationType.FILL_IN_BALANCE
        };

        // Act
        var history = PrivilegeHistoryHttpConverter.ToDomain(createDto);

        // Assert
        Assert.Equal(createDto.PrivilegeId, history.PrivilegeId);
        Assert.Equal(createDto.TicketUid, history.TicketUid);
        Assert.Equal(createDto.BalanceDiffNegative, history.BalanceDiff);
        Assert.Equal((OperationType)createDto.OperationType, history.OperationType);
    }

    /// <summary>
    /// EP2: CreateDTO round-trip should preserve data
    /// </summary>
    [Fact]
    public void ToDomain_CreatePrivilegeHistoryDTO_RoundTripShouldPreserveData()
    {
        // Arrange
        var originalHistory = PrivilegeHistoryMother.CreateValidCreditHistory();
        var createDto = new CreatePrivilegeHistoryDTO
        {
            PrivilegeId = originalHistory.PrivilegeId,
            TicketUid = originalHistory.TicketUid,
            DateTime = originalHistory.DateTime,
            BalanceDiffNegative = originalHistory.BalanceDiff,
            OperationType = (int)originalHistory.OperationType
        };

        // Act
        var history = PrivilegeHistoryHttpConverter.ToDomain(createDto);

        // Assert
        Assert.Equal(originalHistory.PrivilegeId, history.PrivilegeId);
        Assert.Equal(originalHistory.TicketUid, history.TicketUid);
        Assert.Equal(originalHistory.BalanceDiff, history.BalanceDiff);
        Assert.Equal(originalHistory.OperationType, history.OperationType);
    }

    #endregion

    #region ToDomain(UpdateDTO) Tests

    /// <summary>
    /// EP1: UpdateDTO should convert to domain correctly
    /// </summary>
    [Fact]
    public void ToDomain_UpdatePrivilegeHistoryDTO_ShouldConvertCorrectly()
    {
        // Arrange
        var updateDto = new UpdatePrivilegeHistoryDTO(1)
        {
            BalanceDiffNegative = 10000,
            OperationType = (int)OperationType.DEBIT_THE_ACCOUNT
        };

        // Act
        var history = PrivilegeHistoryHttpConverter.ToDomain(updateDto);

        // Assert
        Assert.Equal(1, history.Id);
        Assert.Equal(10000, history.BalanceDiff);
        Assert.Equal(OperationType.DEBIT_THE_ACCOUNT, history.OperationType);
    }

    /// <summary>
    /// EP2: UpdateDTO with nullable fields should handle nulls correctly
    /// </summary>
    [Fact]
    public void ToDomain_UpdatePrivilegeHistoryDTO_NullFields_ShouldUseDefaults()
    {
        // Arrange
        var updateDto = new UpdatePrivilegeHistoryDTO(1);

        // Act
        var history = PrivilegeHistoryHttpConverter.ToDomain(updateDto);

        // Assert
        Assert.Equal(1, history.Id);
        Assert.Equal(0, history.BalanceDiff);
        Assert.Equal(OperationType.FILL_IN_BALANCE, history.OperationType);
    }

    #endregion

    #region ToListDTO Tests

    /// <summary>
    /// EP1: Convert list of histories
    /// </summary>
    [Fact]
    public void ToDTO_ListOfHistories_ShouldConvertAll()
    {
        // Arrange
        var histories = PrivilegeHistoryMother.CreateHistoryList(5);

        // Act
        var dtos = PrivilegeHistoryHttpConverter.ToDTO(histories).ToList();

        // Assert
        Assert.NotNull(dtos);
        Assert.Equal(5, dtos.Count);
        for (int i = 0; i < histories.Count; i++)
        {
            Assert.Equal(histories[i].Id, dtos[i].Id);
            Assert.Equal(histories[i].PrivilegeId, dtos[i].PrivilegeId);
        }
    }

    /// <summary>
    /// EP2: Empty list handling
    /// </summary>
    [Fact]
    public void ToDTO_EmptyList_ShouldReturnEmptyCollection()
    {
        // Arrange
        var histories = new List<PrivilegeHistory>();

        // Act
        var dtos = PrivilegeHistoryHttpConverter.ToDTO(histories).ToList();

        // Assert
        Assert.NotNull(dtos);
        Assert.Empty(dtos);
    }

    #endregion
}
