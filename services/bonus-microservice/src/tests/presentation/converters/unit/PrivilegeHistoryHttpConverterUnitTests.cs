using core.domain;
using core.enums;
using presentation.converters.http;
using presentation.dto.http;
using presentation.dto.http.PrivilegeHistory;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

namespace tests.presentation.converters.unit;

/// <summary>
/// Unit tests for PrivilegeHistoryHttpConverter
/// Using London-style testing (pure unit tests, no dependencies to mock)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For ToDTO/ToDomain:
/// - EP1: Valid history with all fields (normal case)
/// - EP2: History with minimal data
/// - EP3: History with maximum values
/// - EP4: Round-trip conversion preserves data
/// 
/// For ToCreateDTO/ToCreateDomain:
/// - EP1: Valid history to create DTO
/// - EP2: Create DTO should have Id = 0
/// - EP3: Create DTO round-trip preserves data
/// 
/// For ToUpdateDTO/ToUpdateDomain:
/// - EP1: Valid history to update DTO
/// - EP2: Update DTO preserves ID
/// - EP3: UpdateDomain merges nullable properties correctly
/// 
/// For ToListDTO:
/// - EP1: Convert list of histories
/// - EP2: Empty list handling
/// 
/// Total: 12 unit tests (all should pass)
/// </summary>
public class PrivilegeHistoryHttpConverterUnitTests
{
    #region ToDTO / ToDomain Tests

    /// <summary>
    /// EP1: Valid history with all fields - should convert correctly
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_ValidHistoryWithAllFields_ShouldConvertCorrectly()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();

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
    [Unit]
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
    /// EP3: History with maximum values - should handle correctly
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_HistoryWithMaxValues_ShouldConvertCorrectly()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateMaxBalanceDiffHistory();

        // Act
        var dto = PrivilegeHistoryHttpConverter.ToDTO(history);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(history.Id, dto.Id);
        Assert.Equal(history.BalanceDiff, dto.BalanceDiff);
        Assert.Equal(int.MaxValue, dto.BalanceDiff);
    }

    /// <summary>
    /// EP4: Round-trip conversion preserves data
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_ToDomain_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var original = PrivilegeHistoryMother.CreateValidHistory();

        // Act
        var dto = PrivilegeHistoryHttpConverter.ToDTO(original);
        var domain = PrivilegeHistoryHttpConverter.ToDomain(dto);

        // Assert
        Assert.Equal(original.Id, domain.Id);
        Assert.Equal(original.PrivilegeId, domain.PrivilegeId);
        Assert.Equal(original.TicketUid, domain.TicketUid);
        Assert.Equal(original.DateTime, domain.DateTime);
        Assert.Equal(original.BalanceDiff, domain.BalanceDiff);
        Assert.Equal(original.OperationType, domain.OperationType);
    }

    #endregion

    #region ToCreateDTO / ToCreateDomain Tests

    /// <summary>
    /// EP1: Valid history to create DTO - should convert correctly
    /// </summary>
    [Fact]
    [Unit]
    public void ToCreateDTO_ValidHistory_ShouldConvertCorrectly()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();

        // Act
        var dto = PrivilegeHistoryHttpConverter.ToCreateDTO(history);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(history.PrivilegeId, dto.PrivilegeId);
        Assert.Equal(history.TicketUid, dto.TicketUid);
        Assert.Equal(history.DateTime, dto.DateTime);
        Assert.Equal(history.BalanceDiff, dto.BalanceDiffNegative);
        Assert.Equal((int)history.OperationType, dto.OperationType);
    }

    /// <summary>
    /// EP2: Create DTO should have Id = 0
    /// </summary>
    [Fact]
    [Unit]
    public void ToCreateDomain_CreateDto_ShouldHaveIdZero()
    {
        // Arrange
        var dto = new CreatePrivilegeHistoryDTO(1, Guid.NewGuid(), DateTime.UtcNow, -100, 1);

        // Act
        var domain = PrivilegeHistoryHttpConverter.ToCreateDomain(dto);

        // Assert
        Assert.Equal(0, domain.Id);
        Assert.Equal(dto.PrivilegeId, domain.PrivilegeId);
        Assert.Equal(dto.TicketUid, domain.TicketUid);
        Assert.Equal(dto.DateTime, domain.DateTime);
        Assert.Equal(dto.BalanceDiffNegative, domain.BalanceDiff);
        Assert.Equal((core.enums.OperationType)dto.OperationType, domain.OperationType);
    }

    /// <summary>
    /// EP3: Create DTO round-trip preserves data
    /// </summary>
    [Fact]
    [Unit]
    public void ToCreateDTO_ToCreateDomain_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var original = PrivilegeHistoryMother.CreateValidHistory();

        // Act
        var dto = PrivilegeHistoryHttpConverter.ToCreateDTO(original);
        var domain = PrivilegeHistoryHttpConverter.ToCreateDomain(dto);

        // Assert
        Assert.Equal(original.PrivilegeId, domain.PrivilegeId);
        Assert.Equal(original.TicketUid, domain.TicketUid);
        Assert.Equal(original.DateTime, domain.DateTime);
        Assert.Equal(original.BalanceDiff, domain.BalanceDiff);
        Assert.Equal(original.OperationType, domain.OperationType);
    }

    #endregion

    #region ToUpdateDTO / ToUpdateDomain Tests

    /// <summary>
    /// EP1: Valid history to update DTO - should convert correctly
    /// </summary>
    [Fact]
    [Unit]
    public void ToUpdateDTO_ValidHistory_ShouldConvertCorrectly()
    {
        // Arrange
        var history = PrivilegeHistoryMother.CreateValidHistory();

        // Act
        var dto = PrivilegeHistoryHttpConverter.ToUpdateDTO(history);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(history.Id, dto.Id);
        Assert.Equal(history.PrivilegeId, dto.PrivilegeId);
        Assert.Equal(history.TicketUid, dto.TicketUid);
        Assert.Equal(history.DateTime, dto.DateTime);
        Assert.Equal(history.BalanceDiff, dto.BalanceDiffNegative);
        Assert.Equal((int)history.OperationType, dto.OperationType);
    }

    /// <summary>
    /// EP2: Update DTO preserves ID
    /// </summary>
    [Fact]
    [Unit]
    public void ToUpdateDomain_UpdateDto_ShouldPreserveId()
    {
        // Arrange
        var existing = PrivilegeHistoryMother.CreateValidHistory();
        var dto = new UpdatePrivilegeHistoryDTO(existing.Id, 2, Guid.NewGuid(), DateTime.UtcNow, -200, 1);

        // Act
        var updated = PrivilegeHistoryHttpConverter.ToUpdateDomain(dto, existing);

        // Assert
        Assert.Equal(existing.Id, updated.Id);
        Assert.Equal(2, updated.PrivilegeId);
        Assert.Equal(-200, updated.BalanceDiff);
        Assert.Equal(OperationType.DEBIT_THE_ACCOUNT, updated.OperationType);
    }

    /// <summary>
    /// EP3: UpdateDomain merges nullable properties correctly
    /// </summary>
    [Fact]
    [Unit]
    public void ToUpdateDomain_WithPartialUpdate_ShouldMergeCorrectly()
    {
        // Arrange
        var existing = PrivilegeHistoryMother.CreateValidHistory();
        var originalBalanceDiff = existing.BalanceDiff;
        var dto = new UpdatePrivilegeHistoryDTO(existing.Id, null, null, null, null, 1); // Only update OperationType

        // Act
        var updated = PrivilegeHistoryHttpConverter.ToUpdateDomain(dto, existing);

        // Assert
        Assert.Equal(existing.PrivilegeId, updated.PrivilegeId); // Unchanged
        Assert.Equal(existing.TicketUid, updated.TicketUid); // Unchanged
        Assert.Equal(existing.DateTime, updated.DateTime); // Unchanged
        Assert.Equal(originalBalanceDiff, updated.BalanceDiff); // Unchanged
        Assert.Equal(OperationType.DEBIT_THE_ACCOUNT, updated.OperationType); // Updated
    }

    #endregion

    #region List Conversion Tests

    /// <summary>
    /// EP1: Convert list of histories
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_ListOfHistories_ShouldConvertAll()
    {
        // Arrange
        var histories = PrivilegeHistoryMother.CreateHistoryList(5);

        // Act
        var dtos = PrivilegeHistoryHttpConverter.ToDTO(histories);

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
    [Unit]
    public void ToDTO_EmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var histories = new List<PrivilegeHistory>();

        // Act
        var dtos = PrivilegeHistoryHttpConverter.ToDTO(histories);

        // Assert
        Assert.NotNull(dtos);
        Assert.Empty(dtos);
    }

    /// <summary>
    /// EP3: Convert list from DTOs to domain
    /// </summary>
    [Fact]
    [Unit]
    public void ToDomain_ListOfDTOs_ShouldConvertAll()
    {
        // Arrange
        var histories = PrivilegeHistoryMother.CreateHistoryList(3);
        var dtos = PrivilegeHistoryHttpConverter.ToDTO(histories);

        // Act
        var domainList = PrivilegeHistoryHttpConverter.ToDomain(dtos);

        // Assert
        Assert.NotNull(domainList);
        Assert.Equal(3, domainList.Count);
        for (int i = 0; i < histories.Count; i++)
        {
            Assert.Equal(histories[i].Id, domainList[i].Id);
            Assert.Equal(histories[i].PrivilegeId, domainList[i].PrivilegeId);
        }
    }

    #endregion
}
