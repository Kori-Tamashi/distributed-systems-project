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
            .WithBookingUid(Guid.NewGuid())
            .WithBookingReference("ABC123")
            .WithCustomerName("John Doe")
            .WithCustomerEmail("john.doe@example.com")
            .WithCustomerPhone("+79001234567")
            .WithTotalPrice(45000)
            .WithPastBookingDate()
            .WithConfirmedStatus()
            .WithCreditCardPayment()
            .WithPaymentTransactionId("TXN123456")
            .Build();
    }

    /// <summary>
    /// Creates a Booking with minimal data (only required fields)
    /// </summary>
    public static Booking CreateMinimalBooking()
    {
        return _defaultBuilder
            .WithId(2)
            .WithBookingUid(Guid.NewGuid())
            .WithBookingReference("A1")
            .WithCustomerName("A")
            .WithCustomerEmail("a@b.com")
            .WithCustomerPhone("123")
            .WithTotalPrice(1000)
            .WithBookingDate(DateTime.UtcNow)
            .WithConfirmedStatus()
            .WithCashPayment()
            .WithNoPaymentTransactionId()
            .Build();
    }

    /// <summary>
    /// Creates a Booking with PayPal payment
    /// </summary>
    public static Booking CreateBookingWithPayPal()
    {
        return _defaultBuilder
            .WithId(3)
            .WithBookingUid(Guid.NewGuid())
            .WithBookingReference("PAY789")
            .WithCustomerName("Jane Smith")
            .WithCustomerEmail("jane.smith@example.com")
            .WithCustomerPhone("+79009876543")
            .WithTotalPrice(60000)
            .WithPastBookingDate()
            .WithConfirmedStatus()
            .WithPayPalPayment()
            .WithPaymentTransactionId("PAYPAL987654")
            .Build();
    }

    /// <summary>
    /// Creates a Booking with Cash payment
    /// </summary>
    public static Booking CreateBookingWithCash()
    {
        return _defaultBuilder
            .WithId(4)
            .WithBookingUid(Guid.NewGuid())
            .WithBookingReference("CASH456")
            .WithCustomerName("Ivan Petrov")
            .WithCustomerEmail("ivan.petrov@example.com")
            .WithCustomerPhone("+79112223344")
            .WithTotalPrice(30000)
            .WithPastBookingDate()
            .WithConfirmedStatus()
            .WithCashPayment()
            .WithNoPaymentTransactionId()
            .Build();
    }

    /// <summary>
    /// Creates a Booking with zero price
    /// </summary>
    public static Booking CreateBookingWithZeroPrice()
    {
        return _defaultBuilder
            .WithId(5)
            .WithBookingUid(Guid.NewGuid())
            .WithBookingReference("FREE123")
            .WithCustomerName("Free Booking")
            .WithCustomerEmail("free@example.com")
            .WithCustomerPhone("+79000000000")
            .WithZeroPrice()
            .WithPastBookingDate()
            .WithConfirmedStatus()
            .WithCreditCardPayment()
            .WithPaymentTransactionId("TXN000000")
            .Build();
    }

    /// <summary>
    /// Creates a Booking with negative price (invalid)
    /// </summary>
    public static Booking CreateBookingWithNegativePrice()
    {
        return _defaultBuilder
            .WithId(6)
            .WithBookingUid(Guid.NewGuid())
            .WithBookingReference("INVALID")
            .WithCustomerName("Invalid Booking")
            .WithCustomerEmail("invalid@example.com")
            .WithCustomerPhone("+79000000000")
            .WithNegativePrice()
            .WithPastBookingDate()
            .WithConfirmedStatus()
            .WithCreditCardPayment()
            .WithPaymentTransactionId("TXN000001")
            .Build();
    }

    /// <summary>
    /// Creates a Booking in the past (invalid for booking)
    /// </summary>
    public static Booking CreatePastBooking()
    {
        return _defaultBuilder
            .WithId(7)
            .WithBookingUid(Guid.NewGuid())
            .WithBookingReference("PAST123")
            .WithCustomerName("Past Customer")
            .WithCustomerEmail("past@example.com")
            .WithCustomerPhone("+79000000000")
            .WithTotalPrice(25000)
            .WithPastBookingDate()
            .WithConfirmedStatus()
            .WithCreditCardPayment()
            .WithPaymentTransactionId("TXN000002")
            .Build();
    }

    /// <summary>
    /// Creates a Cancelled Booking
    /// </summary>
    public static Booking CreateCancelledBooking()
    {
        return _defaultBuilder
            .WithId(8)
            .WithBookingUid(Guid.NewGuid())
            .WithBookingReference("CANCEL1")
            .WithCustomerName("Cancelled Customer")
            .WithCustomerEmail("cancelled@example.com")
            .WithCustomerPhone("+79000000000")
            .WithTotalPrice(20000)
            .WithPastBookingDate()
            .WithCancelledStatus()
            .WithCreditCardPayment()
            .WithPaymentTransactionId("TXN000003")
            .Build();
    }

    /// <summary>
    /// Creates a Refunded Booking
    /// </summary>
    public static Booking CreateRefundedBooking()
    {
        return _defaultBuilder
            .WithId(9)
            .WithBookingUid(Guid.NewGuid())
            .WithBookingReference("REFUND1")
            .WithCustomerName("Refunded Customer")
            .WithCustomerEmail("refunded@example.com")
            .WithCustomerPhone("+79000000000")
            .WithTotalPrice(22000)
            .WithPastBookingDate()
            .WithRefundedStatus()
            .WithCreditCardPayment()
            .WithPaymentTransactionId("TXN000004")
            .Build();
    }

    /// <summary>
    /// Creates a Booking with maximum price
    /// </summary>
    public static Booking CreateBookingWithMaxPrice()
    {
        return _defaultBuilder
            .WithId(10)
            .WithBookingUid(Guid.NewGuid())
            .WithBookingReference("VIP123")
            .WithCustomerName("VIP Customer")
            .WithCustomerEmail("vip@example.com")
            .WithCustomerPhone("+79000000000")
            .WithTotalPrice(int.MaxValue)
            .WithPastBookingDate()
            .WithConfirmedStatus()
            .WithCreditCardPayment()
            .WithPaymentTransactionId("TXN999999")
            .Build();
    }

    /// <summary>
    /// Creates a Booking with maximum customer name length
    /// </summary>
    public static Booking CreateBookingWithMaxCustomerName()
    {
        return _defaultBuilder
            .WithId(11)
            .WithBookingUid(Guid.NewGuid())
            .WithBookingReference("MAX123")
            .WithCustomerName(new string('A', 255))
            .WithCustomerEmail("max@example.com")
            .WithCustomerPhone("+79000000000")
            .WithTotalPrice(15000)
            .WithPastBookingDate()
            .WithConfirmedStatus()
            .WithCreditCardPayment()
            .WithPaymentTransactionId("TXN000005")
            .Build();
    }

    /// <summary>
    /// Creates a list of valid Bookings for collection tests
    /// </summary>
    public static List<Booking> CreateBookingList(int count = 5)
    {
        var builder = new BookingBuilder();
        var bookings = builder.BuildList(count, incrementIds: true);
        // Ensure all bookings have past booking dates
        foreach (var booking in bookings)
        {
            booking.BookingDate = DateTime.UtcNow.AddDays(-1);
        }
        return bookings;
    }

    /// <summary>
    /// Creates a Booking with unique random Id
    /// </summary>
    public static Booking CreateBookingWithRandomId()
    {
        return new BookingBuilder()
            .WithCustomerName("Random Customer")
            .WithCustomerEmail("random@example.com")
            .WithTotalPrice(12000)
            .BuildWithRandomId();
    }
}
