using presentation.converters.http;
using presentation.dto.http.User;
using tests.config.attributes;
using tests.fixtures.mothers;
using Xunit;

namespace tests.presentation.converters.unit.http;

/// <summary>
/// Unit tests for UserHttpConverter (per lab2-template v1 spec)
/// AAA Structure: Arrange - Act - Assert
/// </summary>
public class UserHttpConverterUnitTests
{
    #region ToDTO Tests

    /// <summary>
    /// EP1: Valid user with privilege and tickets - should convert correctly
    /// </summary>
    [Fact]
    public void ToDTO_ValidUserWithPrivilegeAndTickets_ShouldConvertCorrectly()
    {
        // Arrange
        var username = "testuser";
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Username = username;
        var tickets = TicketMother.CreateTicketList(3);
        tickets[0].Username = username;
        tickets[1].Username = username;

        // Act
        var userInfo = UserHttpConverter.ToDTO(username, privilege, tickets);

        // Assert
        Assert.NotNull(userInfo);
        Assert.Equal(username, userInfo.Username);
        Assert.NotNull(userInfo.PrivilegeInfo);
        Assert.Equal(privilege.Username, userInfo.PrivilegeInfo.Username);
        Assert.Equal(privilege.Status.ToString(), userInfo.PrivilegeInfo.Status);
        Assert.Equal(privilege.Balance, userInfo.PrivilegeInfo.Balance);
        Assert.NotNull(userInfo.Tickets);
        Assert.Equal(3, userInfo.Tickets.Count);
        Assert.Equal(tickets[0].Id, userInfo.Tickets[0].Id);
        Assert.Equal(tickets[0].Username, userInfo.Tickets[0].Username);
    }

    /// <summary>
    /// EP2: User with privilege but no tickets - should handle empty list
    /// </summary>
    [Fact]
    public void ToDTO_UserWithPrivilegeButNoTickets_ShouldReturnEmptyTicketList()
    {
        // Arrange
        var username = "novice_user";
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Username = username;
        var tickets = new List<core.domain.Ticket>();

        // Act
        var userInfo = UserHttpConverter.ToDTO(username, privilege, tickets);

        // Assert
        Assert.NotNull(userInfo);
        Assert.Equal(username, userInfo.Username);
        Assert.NotNull(userInfo.PrivilegeInfo);
        Assert.Equal(privilege.Username, userInfo.PrivilegeInfo.Username);
        Assert.NotNull(userInfo.Tickets);
        Assert.Empty(userInfo.Tickets);
    }

    /// <summary>
    /// EP3: User with tickets but no privilege (null privilege) - should handle null
    /// </summary>
    [Fact]
    public void ToDTO_UserWithTicketsButNullPrivilege_ShouldHandleNullPrivilege()
    {
        // Arrange
        var username = "guest_user";
        core.domain.Privilege? privilege = null;
        var tickets = TicketMother.CreateTicketList(2);
        tickets[0].Username = username;

        // Act
        var userInfo = UserHttpConverter.ToDTO(username, privilege, tickets);

        // Assert
        Assert.NotNull(userInfo);
        Assert.Equal(username, userInfo.Username);
        Assert.Null(userInfo.PrivilegeInfo);
        Assert.NotNull(userInfo.Tickets);
        Assert.Equal(2, userInfo.Tickets.Count);
    }

    /// <summary>
    /// EP4: User with empty ticket list and valid privilege
    /// </summary>
    [Fact]
    public void ToDTO_UserWithEmptyTicketListAndValidPrivilege_ShouldConvertCorrectly()
    {
        // Arrange
        var username = "new_user";
        var privilege = PrivilegeMother.CreateValidPrivilege();
        privilege.Username = username;
        privilege.Balance = 0;
        var tickets = new List<core.domain.Ticket>();

        // Act
        var userInfo = UserHttpConverter.ToDTO(username, privilege, tickets);

        // Assert
        Assert.NotNull(userInfo);
        Assert.Equal(username, userInfo.Username);
        Assert.NotNull(userInfo.PrivilegeInfo);
        Assert.Equal(username, userInfo.PrivilegeInfo.Username);
        Assert.Equal(0, userInfo.PrivilegeInfo.Balance);
        Assert.Empty(userInfo.Tickets);
    }

    #endregion
}
