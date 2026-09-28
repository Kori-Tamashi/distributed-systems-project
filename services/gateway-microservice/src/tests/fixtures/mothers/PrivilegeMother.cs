using core.domain;
using core.enums;
using tests.fixtures.builders;

namespace tests.fixtures.mothers;

/// <summary>
/// Object Mother for Privilege entity
/// Provides predefined, reusable test data for common scenarios
/// Uses PrivilegeBuilder for consistent object creation
/// </summary>
public static class PrivilegeMother
{
    private static readonly PrivilegeBuilder _defaultBuilder = new PrivilegeBuilder();

    /// <summary>
    /// Creates a valid Privilege with all required fields
    /// </summary>
    public static Privilege CreateValidPrivilege()
    {
        return _defaultBuilder
            .WithId(1)
            .WithUsername("john_doe")
            .WithBalance(10000)
            .WithBronzeStatus()
            .Build();
    }

    /// <summary>
    /// Creates a Privilege with minimal data
    /// </summary>
    public static Privilege CreateMinimalPrivilege()
    {
        return _defaultBuilder
            .WithId(2)
            .WithUsername("a")
            .WithBalance(0)
            .WithBronzeStatus()
            .Build();
    }

    /// <summary>
    /// Creates a Silver Privilege
    /// </summary>
    public static Privilege CreateSilverPrivilege()
    {
        return _defaultBuilder
            .WithId(3)
            .WithUsername("jane_smith")
            .WithBalance(25000)
            .WithSilverStatus()
            .Build();
    }

    /// <summary>
    /// Creates a Gold Privilege
    /// </summary>
    public static Privilege CreateGoldPrivilege()
    {
        return _defaultBuilder
            .WithId(4)
            .WithUsername("ivan_petrov")
            .WithBalance(50000)
            .WithGoldStatus()
            .Build();
    }

    /// <summary>
    /// Creates a Privilege with zero balance
    /// </summary>
    public static Privilege CreateZeroBalancePrivilege()
    {
        return _defaultBuilder
            .WithId(5)
            .WithUsername("new_user")
            .WithBalance(0)
            .WithBronzeStatus()
            .Build();
    }

    /// <summary>
    /// Creates a list of privileges for testing
    /// </summary>
    public static List<Privilege> CreatePrivilegeList(int count = 5)
    {
        var privileges = new List<Privilege>();
        var statuses = new[] { PrivilegeStatus.BRONZE, PrivilegeStatus.SILVER, PrivilegeStatus.GOLD };

        for (int i = 1; i <= count; i++)
        {
            privileges.Add(_defaultBuilder
                .WithId(i)
                .WithUsername($"user{i}")
                .WithBalance(5000 + i * 5000)
                .WithStatus(statuses[(i - 1) % statuses.Length])
                .Build());
        }
        return privileges;
    }
}
