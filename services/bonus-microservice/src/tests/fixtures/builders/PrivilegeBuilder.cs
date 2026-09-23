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
    private string _username = "john.doe";
    private PrivilegeStatus _status = PrivilegeStatus.BRONZE;
    private int _balance = 1000;

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
    /// Sets the Privilege Status
    /// </summary>
    public PrivilegeBuilder WithStatus(PrivilegeStatus status)
    {
        _status = status;
        return this;
    }

    /// <summary>
    /// Sets the Privilege Status to BRONZE
    /// </summary>
    public PrivilegeBuilder WithBronzeStatus()
    {
        _status = PrivilegeStatus.BRONZE;
        return this;
    }

    /// <summary>
    /// Sets the Privilege Status to SILVER
    /// </summary>
    public PrivilegeBuilder WithSilverStatus()
    {
        _status = PrivilegeStatus.SILVER;
        return this;
    }

    /// <summary>
    /// Sets the Privilege Status to GOLD
    /// </summary>
    public PrivilegeBuilder WithGoldStatus()
    {
        _status = PrivilegeStatus.GOLD;
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
    /// Sets the Balance to zero
    /// </summary>
    public PrivilegeBuilder WithZeroBalance()
    {
        _balance = 0;
        return this;
    }

    /// <summary>
    /// Sets the Balance to negative (invalid)
    /// </summary>
    public PrivilegeBuilder WithNegativeBalance()
    {
        _balance = -1000;
        return this;
    }

    /// <summary>
    /// Builds the Privilege object with current configuration
    /// </summary>
    /// <returns>Privilege entity with configured properties</returns>
    public Privilege Build()
    {
        return new Privilege
        {
            Id = _id,
            Username = _username,
            Status = _status,
            Balance = _balance
        };
    }

    /// <summary>
    /// Builds a list of Privilege objects
    /// </summary>
    /// <param name="count">Number of privileges to build</param>
    /// <param name="incrementIds">Whether to increment Ids for each privilege</param>
    /// <returns>List of Privilege entities</returns>
    public List<Privilege> BuildList(int count, bool incrementIds = true)
    {
        var privileges = new List<Privilege>();
        
        for (int i = 0; i < count; i++)
        {
            var privilege = new Privilege
            {
                Id = incrementIds ? _id + i : _id,
                Username = $"{_username}{i}",
                Status = _status,
                Balance = _balance
            };
            privileges.Add(privilege);
        }

        return privileges;
    }

    /// <summary>
    /// Creates a Privilege with random Id
    /// </summary>
    public Privilege BuildWithRandomId()
    {
        return new Privilege
        {
            Id = new Random().Next(1, int.MaxValue),
            Username = _username,
            Status = _status,
            Balance = _balance
        };
    }
}
