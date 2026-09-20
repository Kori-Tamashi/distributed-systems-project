using core.domain;
using core.enums;
using tests.fixtures.builders;

namespace tests.fixtures.mothers;

/// <summary>
/// Object Mother for PrivilegeHistory entity
/// Provides predefined, reusable test data for common scenarios
/// Uses PrivilegeHistoryBuilder for consistent object creation
/// </summary>
public static class PrivilegeHistoryMother
{
    private static readonly PrivilegeHistoryBuilder _defaultBuilder = new PrivilegeHistoryBuilder();

    /// <summary>
    /// Creates a valid PrivilegeHistory with credit operation
    /// </summary>
    public static PrivilegeHistory CreateValidCreditHistory()
    {
        return _defaultBuilder
            .WithId(1)
            .WithPrivilegeId(1)
            .WithTicketUid(Guid.NewGuid())
            .WithBalanceDiff(5000)
            .WithFillBalanceOperation()
            .WithDateTime(DateTime.UtcNow.AddDays(-1))
            .Build();
    }

    /// <summary>
    /// Creates a PrivilegeHistory with debit operation
    /// </summary>
    public static PrivilegeHistory CreateValidDebitHistory()
    {
        return _defaultBuilder
            .WithId(2)
            .WithPrivilegeId(1)
            .WithTicketUid(Guid.NewGuid())
            .WithBalanceDiff(-2000)
            .WithDebitOperation()
            .WithDateTime(DateTime.UtcNow.AddDays(-2))
            .Build();
    }

    /// <summary>
    /// Creates a PrivilegeHistory with minimal data
    /// </summary>
    public static PrivilegeHistory CreateMinimalHistory()
    {
        return _defaultBuilder
            .WithId(3)
            .WithPrivilegeId(1)
            .WithTicketUid(Guid.Empty)
            .WithBalanceDiff(1)
            .WithFillBalanceOperation()
            .WithDateTime(DateTime.UtcNow)
            .Build();
    }

    /// <summary>
    /// Creates a list of privilege histories for testing
    /// </summary>
    public static List<PrivilegeHistory> CreateHistoryList(int count = 5)
    {
        var histories = new List<PrivilegeHistory>();
        var operations = new[] { OperationType.FILL_IN_BALANCE, OperationType.DEBIT_THE_ACCOUNT };

        for (int i = 1; i <= count; i++)
        {
            var isCredit = i % 2 == 0;
            histories.Add(_defaultBuilder
                .WithId(i)
                .WithPrivilegeId(i % 3 + 1)
                .WithTicketUid(Guid.NewGuid())
                .WithBalanceDiff(isCredit ? 1000 + i * 500 : -(500 + i * 200))
                .WithOperationType(isCredit ? OperationType.FILL_IN_BALANCE : OperationType.DEBIT_THE_ACCOUNT)
                .WithDateTime(DateTime.UtcNow.AddDays(-i))
                .Build());
        }
        return histories;
    }
}
