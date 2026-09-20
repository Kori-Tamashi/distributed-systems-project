using core.domain;
using core.enums;
using dataaccess.converters.http;
using dataaccess.dto.http;
using dataaccess.dto.http.Privilege;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

namespace tests.dataaccess.converters.unit.http;

/// <summary>
/// Unit tests for PrivilegeHttpConverter
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
        Assert.Equal(privilege.Balance, dto.Balance);
        Assert.Equal((int)privilege.Status, dto.Status);
    }

    /// <summary>
    /// EP2: Privilege with minimal data - should handle correctly
    /// </summary>
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
        Assert.Equal(0, dto.Balance);
    }

    /// <summary>
    /// EP3: Round-trip conversion preserves all data
    /// </summary>
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
        Assert.Equal(originalPrivilege.Balance, convertedPrivilege.Balance);
        Assert.Equal(originalPrivilege.Status, convertedPrivilege.Status);
    }

    #endregion

    #region ToDomain(CreateDTO) Tests

    /// <summary>
    /// EP1: CreateDTO should convert to domain correctly
    /// </summary>
    public void ToDomain_CreatePrivilegeDTO_ShouldConvertCorrectly()
    {
        // Arrange
        var createDto = new CreatePrivilegeDTO
        {
            Username = "test_user",
            Balance = 5000,
            Status = (int)PrivilegeStatus.BRONZE
        };

        // Act
        var privilege = PrivilegeHttpConverter.ToDomain(createDto);

        // Assert
        Assert.Equal(createDto.Username, privilege.Username);
        Assert.Equal(createDto.Balance, privilege.Balance);
        Assert.Equal((PrivilegeStatus)createDto.Status, privilege.Status);
    }

    /// <summary>
    /// EP2: CreateDTO round-trip should preserve data
    /// </summary>
    public void ToDomain_CreatePrivilegeDTO_RoundTripShouldPreserveData()
    {
        // Arrange
        var originalPrivilege = PrivilegeMother.CreateValidPrivilege();
        var createDto = new CreatePrivilegeDTO
        {
            Username = originalPrivilege.Username,
            Balance = originalPrivilege.Balance,
            Status = (int)originalPrivilege.Status
        };

        // Act
        var privilege = PrivilegeHttpConverter.ToDomain(createDto);

        // Assert
        Assert.Equal(originalPrivilege.Username, privilege.Username);
        Assert.Equal(originalPrivilege.Balance, privilege.Balance);
        Assert.Equal(originalPrivilege.Status, privilege.Status);
    }

    #endregion

    #region ToDomain(UpdateDTO) Tests

    /// <summary>
    /// EP1: UpdateDTO should convert to domain correctly
    /// </summary>
    public void ToDomain_UpdatePrivilegeDTO_ShouldConvertCorrectly()
    {
        // Arrange
        var updateDto = new UpdatePrivilegeDTO(1)
        {
            Username = "updated_user",
            Balance = 15000
        };

        // Act
        var privilege = PrivilegeHttpConverter.ToDomain(updateDto);

        // Assert
        Assert.Equal(1, privilege.Id);
        Assert.Equal("updated_user", privilege.Username);
        Assert.Equal(15000, privilege.Balance);
    }

    /// <summary>
    /// EP2: UpdateDTO with nullable fields should handle nulls correctly
    /// </summary>
    public void ToDomain_UpdatePrivilegeDTO_NullFields_ShouldUseDefaults()
    {
        // Arrange
        var updateDto = new UpdatePrivilegeDTO(1);

        // Act
        var privilege = PrivilegeHttpConverter.ToDomain(updateDto);

        // Assert
        Assert.Equal(1, privilege.Id);
        Assert.Equal(string.Empty, privilege.Username);
        Assert.Equal(0, privilege.Balance);
    }

    #endregion

    #region ToListDTO Tests

    /// <summary>
    /// EP1: Convert list of privileges
    /// </summary>
    public void ToDTO_ListOfPrivileges_ShouldConvertAll()
    {
        // Arrange
        var privileges = PrivilegeMother.CreatePrivilegeList(5);

        // Act
        var dtos = PrivilegeHttpConverter.ToDTO(privileges).ToList();

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
    public void ToDTO_EmptyList_ShouldReturnEmptyCollection()
    {
        // Arrange
        var privileges = new List<Privilege>();

        // Act
        var dtos = PrivilegeHttpConverter.ToDTO(privileges).ToList();

        // Assert
        Assert.NotNull(dtos);
        Assert.Empty(dtos);
    }

    #endregion
}
