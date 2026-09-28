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
            .WithUsername("john.doe")
            .WithBronzeStatus()
            .WithBalance(1000)
            .Build();
    }

    /// <summary>
    /// Creates a Privilege with minimal data (only required fields)
    /// </summary>
    public static Privilege CreateMinimalPrivilege()
    {
        return _defaultBuilder
            .WithId(2)
            .WithUsername("a")
            .WithBronzeStatus()
            .WithBalance(0)
            .Build();
    }

    /// <summary>
    /// Creates a Privilege with SILVER status
    /// </summary>
    public static Privilege CreateSilverPrivilege()
    {
        return _defaultBuilder
            .WithId(3)
            .WithUsername("jane.smith")
            .WithSilverStatus()
            .WithBalance(5000)
            .Build();
    }

    /// <summary>
    /// Creates a Privilege with GOLD status
    /// </summary>
    public static Privilege CreateGoldPrivilege()
    {
        return _defaultBuilder
            .WithId(4)
            .WithUsername("ivan.petrov")
            .WithGoldStatus()
            .WithBalance(10000)
            .Build();
    }

    /// <summary>
    /// Creates a Privilege with zero balance
    /// </summary>
    public static Privilege CreatePrivilegeWithZeroBalance()
    {
        return _defaultBuilder
            .WithId(5)
            .WithUsername("zero.balance")
            .WithBronzeStatus()
            .WithZeroBalance()
            .Build();
    }

    /// <summary>
    /// Creates a Privilege with negative balance (invalid)
    /// </summary>
    public static Privilege CreatePrivilegeWithNegativeBalance()
    {
        return _defaultBuilder
            .WithId(6)
            .WithUsername("negative.balance")
            .WithBronzeStatus()
            .WithNegativeBalance()
            .Build();
    }

    /// <summary>
    /// Creates a Privilege with maximum balance
    /// </summary>
    public static Privilege CreatePrivilegeWithMaxBalance()
    {
        return _defaultBuilder
            .WithId(7)
            .WithUsername("max.balance")
            .WithGoldStatus()
            .WithBalance(int.MaxValue)
            .Build();
    }

    /// <summary>
    /// Creates a Privilege with maximum username length
    /// </summary>
    public static Privilege CreatePrivilegeWithMaxUsername()
    {
        return _defaultBuilder
            .WithId(8)
            .WithUsername(new string('A', 80))
            .WithBronzeStatus()
            .WithBalance(1000)
            .Build();
    }

    /// <summary>
    /// Creates a list of valid Privileges for collection tests
    /// </summary>
    public static List<Privilege> CreatePrivilegeList(int count = 5)
    {
        return new PrivilegeBuilder().BuildList(count, incrementIds: true);
    }

    /// <summary>
    /// Creates a Privilege with unique random Id
    /// </summary>
    public static Privilege CreatePrivilegeWithRandomId()
    {
        return new PrivilegeBuilder()
            .WithUsername("random.user")
            .WithBronzeStatus()
            .BuildWithRandomId();
    }
}
