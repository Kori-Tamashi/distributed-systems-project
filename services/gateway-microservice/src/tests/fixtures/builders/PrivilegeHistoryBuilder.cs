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
    private int _balanceDiff = 1000;
    private OperationType _operationType = OperationType.FILL_IN_BALANCE;

    /// <summary>
    /// Sets the Id
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
    /// Sets the Balance Diff (positive for credit, negative for debit)
    /// </summary>
    public PrivilegeHistoryBuilder WithBalanceDiff(int balanceDiff)
    {
        _balanceDiff = balanceDiff;
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
    /// Sets FILL_IN_BALANCE operation
    /// </summary>
    public PrivilegeHistoryBuilder WithFillBalanceOperation()
    {
        _operationType = OperationType.FILL_IN_BALANCE;
        return this;
    }

    /// <summary>
    /// Sets DEBIT_THE_ACCOUNT operation
    /// </summary>
    public PrivilegeHistoryBuilder WithDebitOperation()
    {
        _operationType = OperationType.DEBIT_THE_ACCOUNT;
        return this;
    }

    /// <summary>
    /// Builds the PrivilegeHistory object
    /// </summary>
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
}
