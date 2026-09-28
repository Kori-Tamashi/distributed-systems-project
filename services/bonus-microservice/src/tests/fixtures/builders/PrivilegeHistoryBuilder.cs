using core.domain;
using core.enums;

namespace tests.fixtures.builders;

/// <summary>
/// Test Data Builder for PrivilegeHistory entity
/// Allows fluent construction of PrivilegeHistory objects for testing
/// </summary>
public class PrivilegeHistoryBuilder
{
    private int _id = 1;
    private int _privilegeId = 1;
    private Guid _ticketUid = Guid.NewGuid();
    private DateTime _dateTime = DateTime.UtcNow;
    private int _balanceDiff = 500;
    private OperationType _operationType = OperationType.FILL_IN_BALANCE;

    /// <summary>
    /// Sets the PrivilegeHistory Id
    /// </summary>
    public PrivilegeHistoryBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the Privilege Id
    /// </summary>
    public PrivilegeHistoryBuilder WithPrivilegeId(int privilegeId)
    {
        _privilegeId = privilegeId;
        return this;
    }

    /// <summary>
    /// Sets the Ticket UID
    /// </summary>
    public PrivilegeHistoryBuilder WithTicketUid(Guid ticketUid)
    {
        _ticketUid = ticketUid;
        return this;
    }

    /// <summary>
    /// Sets the DateTime
    /// </summary>
    public PrivilegeHistoryBuilder WithDateTime(DateTime dateTime)
    {
        _dateTime = dateTime;
        return this;
    }

    /// <summary>
    /// Sets the DateTime to future
    /// </summary>
    public PrivilegeHistoryBuilder WithFutureDateTime()
    {
        _dateTime = DateTime.UtcNow.AddDays(1);
        return this;
    }

    /// <summary>
    /// Sets the DateTime to past
    /// </summary>
    public PrivilegeHistoryBuilder WithPastDateTime()
    {
        _dateTime = DateTime.UtcNow.AddDays(-1);
        return this;
    }

    /// <summary>
    /// Sets the Balance Diff
    /// </summary>
    public PrivilegeHistoryBuilder WithBalanceDiff(int balanceDiff)
    {
        _balanceDiff = balanceDiff;
        return this;
    }

    /// <summary>
    /// Sets the Balance Diff to positive (credit)
    /// </summary>
    public PrivilegeHistoryBuilder WithPositiveBalanceDiff()
    {
        _balanceDiff = 500;
        return this;
    }

    /// <summary>
    /// Sets the Balance Diff to negative (debit)
    /// </summary>
    public PrivilegeHistoryBuilder WithNegativeBalanceDiff()
    {
        _balanceDiff = -500;
        return this;
    }

    /// <summary>
    /// Sets the Operation Type
    /// </summary>
    public PrivilegeHistoryBuilder WithOperationType(OperationType operationType)
    {
        _operationType = operationType;
        return this;
    }

    /// <summary>
    /// Sets the Operation Type to FILL_IN_BALANCE
    /// </summary>
    public PrivilegeHistoryBuilder WithFillInBalance()
    {
        _operationType = OperationType.FILL_IN_BALANCE;
        return this;
    }

    /// <summary>
    /// Sets the Operation Type to DEBIT_THE_ACCOUNT
    /// </summary>
    public PrivilegeHistoryBuilder WithDebitTheAccount()
    {
        _operationType = OperationType.DEBIT_THE_ACCOUNT;
        return this;
    }

    /// <summary>
    /// Builds the PrivilegeHistory object with current configuration
    /// </summary>
    /// <returns>PrivilegeHistory entity with configured properties</returns>
    public PrivilegeHistory Build()
    {
        return new PrivilegeHistory
        {
            Id = _id,
            PrivilegeId = _privilegeId,
            TicketUid = _ticketUid,
            DateTime = _dateTime,
            BalanceDiff = _balanceDiff,
            OperationType = _operationType
        };
    }

    /// <summary>
    /// Builds a list of PrivilegeHistory objects
    /// </summary>
    /// <param name="count">Number of history records to build</param>
    /// <param name="incrementIds">Whether to increment Ids for each record</param>
    /// <returns>List of PrivilegeHistory entities</returns>
    public List<PrivilegeHistory> BuildList(int count, bool incrementIds = true)
    {
        var histories = new List<PrivilegeHistory>();
        
        for (int i = 0; i < count; i++)
        {
            var history = new PrivilegeHistory
            {
                Id = incrementIds ? _id + i : _id,
                PrivilegeId = _privilegeId,
                TicketUid = Guid.NewGuid(),
                DateTime = _dateTime.AddMinutes(i),
                BalanceDiff = _balanceDiff,
                OperationType = _operationType
            };
            histories.Add(history);
        }

        return histories;
    }

    /// <summary>
    /// Creates a PrivilegeHistory with random Id
    /// </summary>
    public PrivilegeHistory BuildWithRandomId()
    {
        return new PrivilegeHistory
        {
            Id = new Random().Next(1, int.MaxValue),
            PrivilegeId = _privilegeId,
            TicketUid = Guid.NewGuid(),
            DateTime = _dateTime,
            BalanceDiff = _balanceDiff,
            OperationType = _operationType
        };
    }
}
