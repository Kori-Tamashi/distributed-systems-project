using presentation.converters.http;
using presentation.dto.http.PrivilegeHistory;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

namespace tests.presentation.converters.unit.http;

/// <summary>
/// Unit tests for presentation PrivilegeHistoryHttpConverter
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For ToDTO/ToDomain:
/// - EP1: Valid privilege history with all fields (normal case)
/// - EP2: Privilege history with minimal data
/// - EP3: Round-trip conversion preserves data
/// 
/// For ToCreateDTO/ToCreateDomain:
/// - EP1: Valid privilege history to create DTO
/// - EP2: Create DTO round-trip preserves data
/// 
/// For ToUpdateDTO/ToUpdateDomain:
/// - EP1: Valid privilege history to update DTO
/// - EP2: UpdateDomain handles nullable fields correctly
/// 
/// For ToListDTO:
/// - EP1: Convert list of privilege histories
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
    /// EP2: Privilege history with minimal data - should handle correctly
    /// </summary>
    [Fact]
    public void ToDTO_MinimalPrivilegeHistory_ShouldConvertCorrectly()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateMinimalHistory();

        // Act
        var dto = PrivilegeHistoryHttpConverter.ToDTO(history);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(history.Id, dto.Id);
        Assert.Equal(history.PrivilegeId, dto.PrivilegeId);
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

    #region ToCreateDTO / ToCreateDomain Tests

    /// <summary>
    /// EP1: CreateDTO should convert to domain correctly
    /// </summary>
    [Fact]
    public void ToCreateDTO_ValidPrivilegeHistory_ShouldConvertCorrectly()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidCreditHistory();

        // Act
        var createDto = PrivilegeHistoryHttpConverter.ToCreateDTO(history);

        // Assert
        Assert.NotNull(createDto);
        Assert.Equal(history.PrivilegeId, createDto.PrivilegeId);
        Assert.Equal(history.TicketUid, createDto.TicketUid);
        Assert.Equal(history.BalanceDiff, createDto.BalanceDiff);
        Assert.Equal((int)history.OperationType, createDto.OperationType);
    }

    /// <summary>
    /// EP2: CreateDTO round-trip should preserve data
    /// </summary>
    [Fact]
    public void ToCreateDomain_CreatePrivilegeHistoryDTO_RoundTripShouldPreserveData()
    {
        // Arrange
        var originalHistory = PrivilegeHistoryMother.CreateValidCreditHistory();
        var createDto = new CreatePrivilegeHistoryDTO
        {
            PrivilegeId = originalHistory.PrivilegeId,
            TicketUid = originalHistory.TicketUid,
            DateTime = originalHistory.DateTime,
            BalanceDiff = originalHistory.BalanceDiff,
            OperationType = (int)originalHistory.OperationType
        };

        // Act
        var history = PrivilegeHistoryHttpConverter.ToCreateDomain(createDto);

        // Assert
        Assert.Equal(originalHistory.PrivilegeId, history.PrivilegeId);
        Assert.Equal(originalHistory.TicketUid, history.TicketUid);
        Assert.Equal(originalHistory.BalanceDiff, history.BalanceDiff);
    }

    #endregion

    #region ToUpdateDTO / ToUpdateDomain Tests

    /// <summary>
    /// EP1: UpdateDTO should convert to domain correctly
    /// </summary>
    [Fact]
    public void ToUpdateDTO_ValidPrivilegeHistory_ShouldConvertCorrectly()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidCreditHistory();

        // Act
        var updateDto = PrivilegeHistoryHttpConverter.ToUpdateDTO(history);

        // Assert
        Assert.NotNull(updateDto);
        Assert.Equal(history.PrivilegeId, updateDto.PrivilegeId);
        Assert.Equal(history.TicketUid, updateDto.TicketUid);
        Assert.Equal(history.BalanceDiff, updateDto.BalanceDiff);
    }

    /// <summary>
    /// EP2: UpdateDomain handles nullable fields correctly
    /// </summary>
    [Fact]
    public void ToUpdateDomain_UpdatePrivilegeHistoryDTO_NullFields_ShouldKeepExistingValues()
    {
        // Arrange
        var existingHistory = PrivilegeHistoryMother.CreateValidCreditHistory();
        var updateDto = new UpdatePrivilegeHistoryDTO
        {
            BalanceDiff = 500
            // Other fields are null
        };

        // Act
        var updatedHistory = PrivilegeHistoryHttpConverter.ToUpdateDomain(updateDto, existingHistory);

        // Assert
        Assert.Equal(500, updatedHistory.BalanceDiff);
        Assert.Equal(existingHistory.PrivilegeId, updatedHistory.PrivilegeId);
        Assert.Equal(existingHistory.TicketUid, updatedHistory.TicketUid);
    }

    #endregion

    #region ToListDTO Tests

    /// <summary>
    /// EP1: Convert list of privilege histories
    /// </summary>
    [Fact]
    public void ToDTO_ListOfPrivilegeHistories_ShouldConvertAll()
    {
        // Arrange
        var histories = PrivilegeHistoryMother.CreateHistoryList(3);

        // Act
        var dtos = PrivilegeHistoryHttpConverter.ToDTO(histories).ToList();

        // Assert
        Assert.NotNull(dtos);
        Assert.Equal(3, dtos.Count);
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
        var histories = new List<core.domain.PrivilegeHistory>();

        // Act
        var dtos = PrivilegeHistoryHttpConverter.ToDTO(histories).ToList();

        // Assert
        Assert.NotNull(dtos);
        Assert.Empty(dtos);
    }

    #endregion
}
