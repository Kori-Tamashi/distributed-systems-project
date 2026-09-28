using core.domain;
using core.enums;

namespace tests.fixtures.builders;

/// <summary>
/// Test Data Builder for Privilege entity
/// Allows fluent construction of Privilege objects for testing
/// </summary>
public class PrivilegeBuilder
{
    private int _id = 1;
    private string _username = "john_doe";
    private int _balance = 10000;
    private PrivilegeStatus _status = PrivilegeStatus.BRONZE;

    /// <summary>
    /// Sets the Privilege Id
    /// </summary>
    public PrivilegeBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the Username
    /// </summary>
    public PrivilegeBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }

    /// <summary>
    /// Sets the Balance
    /// </summary>
    public PrivilegeBuilder WithBalance(int balance)
    {
        _balance = balance;
        return this;
    }

    /// <summary>
    /// Sets the Status
    /// </summary>
    public PrivilegeBuilder WithStatus(PrivilegeStatus status)
    {
        _status = status;
        return this;
    }

    /// <summary>
    /// Sets Bronze Status
    /// </summary>
    public PrivilegeBuilder WithBronzeStatus()
    {
        _status = PrivilegeStatus.BRONZE;
        return this;
    }

    /// <summary>
    /// Sets Silver Status
    /// </summary>
    public PrivilegeBuilder WithSilverStatus()
    {
        _status = PrivilegeStatus.SILVER;
        return this;
    }

    /// <summary>
    /// Sets Gold Status
    /// </summary>
    public PrivilegeBuilder WithGoldStatus()
    {
        _status = PrivilegeStatus.GOLD;
        return this;
    }

    /// <summary>
    /// Builds the Privilege object
    /// </summary>
    public Privilege Build()
    {
        return new Privilege
        {
            Id = _id,
            Username = _username,
            Balance = _balance,
            Status = _status
        };
    }
}
