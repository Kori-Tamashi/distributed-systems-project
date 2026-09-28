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
    /// Creates a valid PrivilegeHistory with all required fields
    /// </summary>
    public static PrivilegeHistory CreateValidHistory()
    {
        return _defaultBuilder
            .WithId(1)
            .WithPrivilegeId(1)
            .WithTicketUid(Guid.NewGuid())
            .WithPastDateTime()
            .WithPositiveBalanceDiff()
            .WithFillInBalance()
            .Build();
    }

    /// <summary>
    /// Creates a PrivilegeHistory with minimal data
    /// </summary>
    public static PrivilegeHistory CreateMinimalHistory()
    {
        return _defaultBuilder
            .WithId(2)
            .WithPrivilegeId(1)
            .WithTicketUid(Guid.NewGuid())
            .WithDateTime(DateTime.UtcNow)
            .WithBalanceDiff(1)
            .WithFillInBalance()
            .Build();
    }

    /// <summary>
    /// Creates a PrivilegeHistory with FILL_IN_BALANCE operation (credit)
    /// </summary>
    public static PrivilegeHistory CreateCreditHistory()
    {
        return _defaultBuilder
            .WithId(3)
            .WithPrivilegeId(1)
            .WithTicketUid(Guid.NewGuid())
            .WithPastDateTime()
            .WithPositiveBalanceDiff()
            .WithFillInBalance()
            .Build();
    }

    /// <summary>
    /// Creates a PrivilegeHistory with DEBIT_THE_ACCOUNT operation (debit)
    /// </summary>
    public static PrivilegeHistory CreateDebitHistory()
    {
        return _defaultBuilder
            .WithId(4)
            .WithPrivilegeId(1)
            .WithTicketUid(Guid.NewGuid())
            .WithPastDateTime()
            .WithNegativeBalanceDiff()
            .WithDebitTheAccount()
            .Build();
    }

    /// <summary>
    /// Creates a PrivilegeHistory with large balance diff
    /// </summary>
    public static PrivilegeHistory CreateLargeBalanceDiffHistory()
    {
        return _defaultBuilder
            .WithId(5)
            .WithPrivilegeId(1)
            .WithTicketUid(Guid.NewGuid())
            .WithPastDateTime()
            .WithBalanceDiff(100000)
            .WithFillInBalance()
            .Build();
    }

    /// <summary>
    /// Creates a PrivilegeHistory with future datetime (invalid)
    /// </summary>
    public static PrivilegeHistory CreateFutureHistory()
    {
        return _defaultBuilder
            .WithId(6)
            .WithPrivilegeId(1)
            .WithTicketUid(Guid.NewGuid())
            .WithFutureDateTime()
            .WithPositiveBalanceDiff()
            .WithFillInBalance()
            .Build();
    }

    /// <summary>
    /// Creates a PrivilegeHistory with maximum balance diff
    /// </summary>
    public static PrivilegeHistory CreateMaxBalanceDiffHistory()
    {
        return _defaultBuilder
            .WithId(7)
            .WithPrivilegeId(1)
            .WithTicketUid(Guid.NewGuid())
            .WithPastDateTime()
            .WithBalanceDiff(int.MaxValue)
            .WithFillInBalance()
            .Build();
    }

    /// <summary>
    /// Creates a list of valid PrivilegeHistory records for collection tests
    /// </summary>
    public static List<PrivilegeHistory> CreateHistoryList(int count = 5)
    {
        return new PrivilegeHistoryBuilder().BuildList(count, incrementIds: true);
    }

    /// <summary>
    /// Creates a PrivilegeHistory with unique random Id
    /// </summary>
    public static PrivilegeHistory CreateHistoryWithRandomId()
    {
        return new PrivilegeHistoryBuilder()
            .WithPrivilegeId(1)
            .WithFillInBalance()
            .BuildWithRandomId();
    }
}
