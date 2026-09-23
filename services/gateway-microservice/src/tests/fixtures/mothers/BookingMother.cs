using core.domain;
using core.enums;
using tests.fixtures.builders;

namespace tests.fixtures.mothers;

/// <summary>
/// Object Mother for Booking entity
/// Provides predefined, reusable test data for common scenarios
/// Uses BookingBuilder for consistent object creation
/// </summary>
public static class BookingMother
{
    private static readonly BookingBuilder _defaultBuilder = new BookingBuilder();

    /// <summary>
    /// Creates a valid Booking with all required fields
    /// </summary>
    public static Booking CreateValidBooking()
    {
        return _defaultBuilder
            .WithId(1)
            .WithBookingReference("BK100")
            .WithBookingUid(Guid.NewGuid())
            .WithCustomerName("John Doe")
            .WithCustomerEmail("john.doe@example.com")
            .WithCustomerPhone("+79001234567")
            .WithTotalPrice(30000)
            .WithConfirmedStatus()
            .WithCardPayment()
            .WithPaymentTransactionId("TXN123456")
            .WithBookingDate(DateTime.UtcNow.AddDays(-1))
            .Build();
    }

    /// <summary>
    /// Creates a Booking with minimal data
    /// </summary>
    public static Booking CreateMinimalBooking()
    {
        return _defaultBuilder
            .WithId(2)
            .WithBookingReference("BK200")
            .WithBookingUid(Guid.NewGuid())
            .WithCustomerName("A")
            .WithCustomerEmail("a@b.com")
            .WithCustomerPhone("123")
            .WithTotalPrice(1000)
            .WithConfirmedStatus()
            .WithCashPayment()
            .WithBookingDate(DateTime.UtcNow)
            .Build();
    }

    /// <summary>
    /// Creates a Cancelled Booking
    /// </summary>
    public static Booking CreateCancelledBooking()
    {
        return _defaultBuilder
            .WithId(3)
            .WithBookingReference("BK300")
            .WithBookingUid(Guid.NewGuid())
            .WithCustomerName("Jane Smith")
            .WithCustomerEmail("jane.smith@example.com")
            .WithCustomerPhone("+79009876543")
            .WithTotalPrice(25000)
            .WithCancelledStatus()
            .WithCardPayment()
            .WithBookingDate(DateTime.UtcNow.AddDays(-7))
            .Build();
    }

    /// <summary>
    /// Creates a Refunded Booking
    /// </summary>
    public static Booking CreateRefundedBooking()
    {
        return _defaultBuilder
            .WithId(4)
            .WithBookingReference("BK400")
            .WithBookingUid(Guid.NewGuid())
            .WithCustomerName("Ivan Petrov")
            .WithCustomerEmail("ivan.petrov@example.com")
            .WithCustomerPhone("+79112223344")
            .WithTotalPrice(50000)
            .WithRefundedStatus()
            .WithCardPayment()
            .WithPaymentTransactionId("TXN789012")
            .WithBookingDate(DateTime.UtcNow.AddDays(-14))
            .Build();
    }

    /// <summary>
    /// Creates a list of bookings for testing
    /// </summary>
    public static List<Booking> CreateBookingList(int count = 5)
    {
        var bookings = new List<Booking>();
        var statuses = new[] { BookingStatus.Confirmed, BookingStatus.Cancelled, BookingStatus.Refunded };
        var methods = new[] { PaymentMethod.Cash, PaymentMethod.CreditCard };

        for (int i = 1; i <= count; i++)
        {
            bookings.Add(_defaultBuilder
                .WithId(i)
                .WithBookingReference($"BK{i:D3}")
                .WithBookingUid(Guid.NewGuid())
                .WithCustomerName($"Customer {i}")
                .WithCustomerEmail($"customer{i}@example.com")
                .WithCustomerPhone($"+7900{i:D7}")
                .WithTotalPrice(10000 + i * 5000)
                .WithStatus(statuses[(i - 1) % statuses.Length])
                .WithPaymentMethod(methods[(i - 1) % methods.Length])
                .WithPaymentTransactionId($"TXN{i:D6}")
                .WithBookingDate(DateTime.UtcNow.AddDays(-i))
                .Build());
        }
        return bookings;
    }
}
