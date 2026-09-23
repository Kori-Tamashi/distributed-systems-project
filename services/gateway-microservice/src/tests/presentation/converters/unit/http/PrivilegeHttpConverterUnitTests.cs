using presentation.converters.http;
using presentation.dto.http.Privilege;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

namespace tests.presentation.converters.unit.http;

/// <summary>
/// Unit tests for presentation PrivilegeHttpConverter
/// AAA Structure: Arrange - Act - Assert
/// 
/// CLASS EQUIVALENCE PARTITIONING:
/// 
/// For ToDTO/ToDomain:
/// - EP1: Valid privilege with all fields (normal case)
/// - EP2: Privilege with minimal data
/// - EP3: Round-trip conversion preserves data
/// 
/// For ToCreateDTO/ToCreateDomain:
/// - EP1: Valid privilege to create DTO
/// - EP2: Create DTO round-trip preserves data
/// 
/// For ToUpdateDTO/ToUpdateDomain:
/// - EP1: Valid privilege to update DTO
/// - EP2: UpdateDomain handles nullable fields correctly
/// 
/// For ToListDTO:
/// - EP1: Convert list of privileges
/// - EP2: Empty list handling
/// 
/// Total: 10 unit tests (all should pass)
/// </summary>
public class PrivilegeHttpConverterUnitTests
{
    #region ToDTO / ToDomain Tests

    /// <summary>
    /// EP1: Valid privilege with all fields - should convert correctly
    /// </summary>
    [Fact]
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
    }

    /// <summary>
    /// EP3: Round-trip conversion preserves all data
    /// </summary>
    [Fact]
    public void ToDTO_ToDomain_RoundTripShouldPreserveData()
    {
        // Arrange
        var originalPrivilege = PrivilegeMother.CreateValidPrivilege();

        // Act
        var dto = PrivilegeHttpConverter.ToDTO(originalPrivilege);
        var convertedPrivilege = PrivilegeHttpConverter.ToDomain(dto);

        // Assert
        Assert.Equal(originalPrivilege.Id, convertedPrivilege.Id);
        Assert.Equal(originalPrivilege.Username, convertedPrivilege.Username);
        Assert.Equal(originalPrivilege.Status, convertedPrivilege.Status);
        Assert.Equal(originalPrivilege.Balance, convertedPrivilege.Balance);
    }

    #endregion

    #region ToCreateDTO / ToCreateDomain Tests

    /// <summary>
    /// EP1: CreateDTO should convert to domain correctly
    /// </summary>
    [Fact]
    public void ToCreateDTO_ValidPrivilege_ShouldConvertCorrectly()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();

        // Act
        var createDto = PrivilegeHttpConverter.ToCreateDTO(privilege);

        // Assert
        Assert.NotNull(createDto);
        Assert.Equal(privilege.Username, createDto.Username);
        Assert.Equal((int)privilege.Status, createDto.Status);
        Assert.Equal(privilege.Balance, createDto.Balance);
    }

    /// <summary>
    /// EP2: CreateDTO round-trip should preserve data
    /// </summary>
    [Fact]
    public void ToCreateDomain_CreatePrivilegeDTO_RoundTripShouldPreserveData()
    {
        // Arrange
        var originalPrivilege = PrivilegeMother.CreateValidPrivilege();
        var createDto = new CreatePrivilegeDTO
        {
            Username = originalPrivilege.Username,
            Status = (int)originalPrivilege.Status,
            Balance = originalPrivilege.Balance
        };

        // Act
        var privilege = PrivilegeHttpConverter.ToCreateDomain(createDto);

        // Assert
        Assert.Equal(originalPrivilege.Username, privilege.Username);
        Assert.Equal(originalPrivilege.Status, privilege.Status);
        Assert.Equal(originalPrivilege.Balance, privilege.Balance);
    }

    #endregion

    #region ToUpdateDTO / ToUpdateDomain Tests

    /// <summary>
    /// EP1: UpdateDTO should convert to domain correctly
    /// </summary>
    [Fact]
    public void ToUpdateDTO_ValidPrivilege_ShouldConvertCorrectly()
    {
        // Arrange
        var privilege = PrivilegeMother.CreateValidPrivilege();

        // Act
        var updateDto = PrivilegeHttpConverter.ToUpdateDTO(privilege);

        // Assert
        Assert.NotNull(updateDto);
        Assert.Equal(privilege.Username, updateDto.Username);
        Assert.Equal((int)privilege.Status, updateDto.Status);
        Assert.Equal(privilege.Balance, updateDto.Balance);
    }

    /// <summary>
    /// EP2: UpdateDomain handles nullable fields correctly
    /// </summary>
    [Fact]
    public void ToUpdateDomain_UpdatePrivilegeDTO_NullFields_ShouldKeepExistingValues()
    {
        // Arrange
        var existingPrivilege = PrivilegeMother.CreateValidPrivilege();
        var updateDto = new UpdatePrivilegeDTO
        {
            Username = "UpdatedUsername"
            // Status and Balance are null
        };

        // Act
        var updatedPrivilege = PrivilegeHttpConverter.ToUpdateDomain(updateDto, existingPrivilege);

        // Assert
        Assert.Equal("UpdatedUsername", updatedPrivilege.Username);
        Assert.Equal(existingPrivilege.Status, updatedPrivilege.Status);
        Assert.Equal(existingPrivilege.Balance, updatedPrivilege.Balance);
    }

    #endregion

    #region ToListDTO Tests

    /// <summary>
    /// EP1: Convert list of privileges
    /// </summary>
    [Fact]
    public void ToDTO_ListOfPrivileges_ShouldConvertAll()
    {
        // Arrange
        var privileges = PrivilegeMother.CreatePrivilegeList(3);

        // Act
        var dtos = PrivilegeHttpConverter.ToDTO(privileges).ToList();

        // Assert
        Assert.NotNull(dtos);
        Assert.Equal(3, dtos.Count);
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
    public void ToDTO_EmptyList_ShouldReturnEmptyCollection()
    {
        // Arrange
        var privileges = new List<core.domain.Privilege>();

        // Act
        var dtos = PrivilegeHttpConverter.ToDTO(privileges).ToList();

        // Assert
        Assert.NotNull(dtos);
        Assert.Empty(dtos);
    }

    #endregion
}
