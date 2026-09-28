using core.domain;
using core.enums;
using presentation.converters.http;
using presentation.dto.http;
using presentation.dto.http.Privilege;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

namespace tests.presentation.converters.unit;

/// <summary>
/// Unit tests for PrivilegeHttpConverter
/// Using London-style testing (pure unit tests, no dependencies to mock)
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For ToDTO/ToDomain:
/// - EP1: Valid privilege with all fields (normal case)
/// - EP2: Privilege with minimal data
/// - EP3: Privilege with maximum values
/// - EP4: Round-trip conversion preserves data
/// 
/// For ToCreateDTO/ToCreateDomain:
/// - EP1: Valid privilege to create DTO
/// - EP2: Create DTO should have Id = 0
/// - EP3: Create DTO round-trip preserves data
/// 
/// For ToUpdateDTO/ToUpdateDomain:
/// - EP1: Valid privilege to update DTO
/// - EP2: Update DTO preserves ID
/// - EP3: UpdateDomain merges nullable properties correctly
/// 
/// For ToListDTO:
/// - EP1: Convert list of privileges
/// - EP2: Empty list handling
/// 
/// Total: 12 unit tests (all should pass)
/// </summary>
public class PrivilegeHttpConverterUnitTests
{
    #region ToDTO / ToDomain Tests

    /// <summary>
    /// EP1: Valid privilege with all fields - should convert correctly
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_ValidPrivilegeWithAllFields_ShouldConvertCorrectly()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();

        // Act
        var dto = PrivilegeHttpConverter.ToDTO(privilege);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(privilege.Id, dto.Id);
        Assert.Equal(privilege.Username, dto.Username);
        Assert.Equal((int)privilege.Status, dto.Status);
        Assert.Equal(privilege.Balance, dto.Balance);
    }

    /// <summary>
    /// EP2: Privilege with minimal data - should handle correctly
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_MinimalPrivilege_ShouldConvertCorrectly()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateMinimalPrivilege();

        // Act
        var dto = PrivilegeHttpConverter.ToDTO(privilege);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(privilege.Id, dto.Id);
        Assert.Equal(privilege.Username, dto.Username);
        Assert.Equal(privilege.Balance, dto.Balance);
    }

    /// <summary>
    /// EP3: Privilege with maximum values - should handle correctly
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_PrivilegeWithMaxValues_ShouldConvertCorrectly()
    {
        // Arrange
        var privilege = PrivilegeMother.CreatePrivilegeWithMaxBalance();

        // Act
        var dto = PrivilegeHttpConverter.ToDTO(privilege);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(privilege.Id, dto.Id);
        Assert.Equal(privilege.Balance, dto.Balance);
        Assert.Equal(int.MaxValue, dto.Balance);
    }

    /// <summary>
    /// EP4: Round-trip conversion preserves data
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_ToDomain_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var original = PrivilegeMother.CreateValidPrivilege();

        // Act
        var dto = PrivilegeHttpConverter.ToDTO(original);
        var domain = PrivilegeHttpConverter.ToDomain(dto);

        // Assert
        Assert.Equal(original.Id, domain.Id);
        Assert.Equal(original.Username, domain.Username);
        Assert.Equal(original.Status, domain.Status);
        Assert.Equal(original.Balance, domain.Balance);
    }

    #endregion

    #region ToCreateDTO / ToCreateDomain Tests

    /// <summary>
    /// EP1: Valid privilege to create DTO - should convert correctly
    /// </summary>
    [Fact]
    [Unit]
    public void ToCreateDTO_ValidPrivilege_ShouldConvertCorrectly()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();

        // Act
        var dto = PrivilegeHttpConverter.ToCreateDTO(privilege);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(privilege.Username, dto.Username);
        Assert.Equal((int)privilege.Status, dto.Status);
        Assert.Equal(privilege.Balance, dto.Balance);
    }

    /// <summary>
    /// EP2: Create DTO should have Id = 0
    /// </summary>
    [Fact]
    [Unit]
    public void ToCreateDomain_CreateDto_ShouldHaveIdZero()
    {
        // Arrange
        var dto = new CreatePrivilegeDTO("test.user", 0, 1000);

        // Act
        var domain = PrivilegeHttpConverter.ToCreateDomain(dto);

        // Assert
        Assert.Equal(0, domain.Id);
        Assert.Equal(dto.Username, domain.Username);
        Assert.Equal((core.enums.PrivilegeStatus)dto.Status, domain.Status);
        Assert.Equal(dto.Balance, domain.Balance);
    }

    /// <summary>
    /// EP3: Create DTO round-trip preserves data
    /// </summary>
    [Fact]
    [Unit]
    public void ToCreateDTO_ToCreateDomain_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var original = PrivilegeMother.CreateValidPrivilege();

        // Act
        var dto = PrivilegeHttpConverter.ToCreateDTO(original);
        var domain = PrivilegeHttpConverter.ToCreateDomain(dto);

        // Assert
        Assert.Equal(original.Username, domain.Username);
        Assert.Equal(original.Status, domain.Status);
        Assert.Equal(original.Balance, domain.Balance);
    }

    #endregion

    #region ToUpdateDTO / ToUpdateDomain Tests

    /// <summary>
    /// EP1: Valid privilege to update DTO - should convert correctly
    /// </summary>
    [Fact]
    [Unit]
    public void ToUpdateDTO_ValidPrivilege_ShouldConvertCorrectly()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();

        // Act
        var dto = PrivilegeHttpConverter.ToUpdateDTO(privilege);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(privilege.Id, dto.Id);
        Assert.Equal(privilege.Username, dto.Username);
        Assert.Equal((int)privilege.Status, dto.Status);
        Assert.Equal(privilege.Balance, dto.Balance);
    }

    /// <summary>
    /// EP2: Update DTO preserves ID
    /// </summary>
    [Fact]
    [Unit]
    public void ToUpdateDomain_UpdateDto_ShouldPreserveId()
    {
        // Arrange
        var existing = PrivilegeMother.CreateValidPrivilege();
        var dto = new UpdatePrivilegeDTO(existing.Id, "updated.user", 1, 2000);

        // Act
        var updated = PrivilegeHttpConverter.ToUpdateDomain(dto, existing);

        // Assert
        Assert.Equal(existing.Id, updated.Id);
        Assert.Equal("updated.user", updated.Username);
        Assert.Equal(PrivilegeStatus.SILVER, updated.Status);
        Assert.Equal(2000, updated.Balance);
    }

    /// <summary>
    /// EP3: UpdateDomain merges nullable properties correctly
    /// </summary>
    [Fact]
    [Unit]
    public void ToUpdateDomain_WithPartialUpdate_ShouldMergeCorrectly()
    {
        // Arrange
        var existing = PrivilegeMother.CreateValidPrivilege();
        var dto = new UpdatePrivilegeDTO(existing.Id, null, 1, null); // Only update status

        // Act
        var updated = PrivilegeHttpConverter.ToUpdateDomain(dto, existing);

        // Assert
        Assert.Equal(existing.Username, updated.Username); // Unchanged
        Assert.Equal(PrivilegeStatus.SILVER, updated.Status); // Updated
        Assert.Equal(existing.Balance, updated.Balance); // Unchanged
    }

    #endregion

    #region List Conversion Tests

    /// <summary>
    /// EP1: Convert list of privileges
    /// </summary>
    [Fact]
    [Unit]
    public void ToDTO_ListOfPrivileges_ShouldConvertAll()
    {
        // Arrange
        var privileges = PrivilegeMother.CreatePrivilegeList(5);

        // Act
        var dtos = PrivilegeHttpConverter.ToDTO(privileges);

        // Assert
        Assert.NotNull(dtos);
        Assert.Equal(5, dtos.Count);
        for (int i = 0; i < privileges.Count; i++)
        {
            Assert.Equal(privileges[i].Id, dtos[i].Id);
            Assert.Equal(privileges[i].Username, dtos[i].Username);
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
        var privileges = new List<Privilege>();

        // Act
        var dtos = PrivilegeHttpConverter.ToDTO(privileges);

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
        var privileges = PrivilegeMother.CreatePrivilegeList(3);
        var dtos = PrivilegeHttpConverter.ToDTO(privileges);

        // Act
        var domainList = PrivilegeHttpConverter.ToDomain(dtos);

        // Assert
        Assert.NotNull(domainList);
        Assert.Equal(3, domainList.Count);
        for (int i = 0; i < privileges.Count; i++)
        {
            Assert.Equal(privileges[i].Id, domainList[i].Id);
            Assert.Equal(privileges[i].Username, domainList[i].Username);
        }
    }

    #endregion
}
