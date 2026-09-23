using core.domain;
using core.enums;

namespace tests.fixtures.builders;

/// <summary>
/// Test Data Builder for Booking entity
/// Allows fluent construction of Booking objects for testing
/// </summary>
public class BookingBuilder
{
    private int _id = 1;
    private string _bookingReference = "BK100";
    private Guid _bookingUid = Guid.NewGuid();
    private string _customerName = "John Doe";
    private string _customerEmail = "john.doe@example.com";
    private string _customerPhone = "+79001234567";
    private DateTime _bookingDate = DateTime.UtcNow;
    private int _totalPrice = 30000;
    private BookingStatus _status = BookingStatus.Confirmed;
    private PaymentMethod _paymentMethod = PaymentMethod.Cash;
    private string? _paymentTransactionId = null;

    /// <summary>
    /// Sets the Booking Id
    /// </summary>
    public BookingBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the Booking Reference
    /// </summary>
    public BookingBuilder WithBookingReference(string bookingReference)
    {
        _bookingReference = bookingReference;
        return this;
    }

    /// <summary>
    /// Sets the Booking UID
    /// </summary>
    public BookingBuilder WithBookingUid(Guid bookingUid)
    {
        _bookingUid = bookingUid;
        return this;
    }

    /// <summary>
    /// Sets the Customer Name
    /// </summary>
    public BookingBuilder WithCustomerName(string customerName)
    {
        _customerName = customerName;
        return this;
    }

    /// <summary>
    /// Sets the Customer Email
    /// </summary>
    public BookingBuilder WithCustomerEmail(string customerEmail)
    {
        _customerEmail = customerEmail;
        return this;
    }

    /// <summary>
    /// Sets the Customer Phone
    /// </summary>
    public BookingBuilder WithCustomerPhone(string customerPhone)
    {
        _customerPhone = customerPhone;
        return this;
    }

    /// <summary>
    /// Sets the Booking Date
    /// </summary>
    public BookingBuilder WithBookingDate(DateTime bookingDate)
    {
        _bookingDate = bookingDate;
        return this;
    }

    /// <summary>
    /// Sets the Total Price
    /// </summary>
    public BookingBuilder WithTotalPrice(int totalPrice)
    {
        _totalPrice = totalPrice;
        return this;
    }

    /// <summary>
    /// Sets the Booking Status
    /// </summary>
    public BookingBuilder WithStatus(BookingStatus status)
    {
        _status = status;
        return this;
    }

    /// <summary>
    /// Sets Confirmed Status
    /// </summary>
    public BookingBuilder WithConfirmedStatus()
    {
        _status = BookingStatus.Confirmed;
        return this;
    }

    /// <summary>
    /// Sets Cancelled Status
    /// </summary>
    public BookingBuilder WithCancelledStatus()
    {
        _status = BookingStatus.Cancelled;
        return this;
    }

    /// <summary>
    /// Sets Refunded Status
    /// </summary>
    public BookingBuilder WithRefundedStatus()
    {
        _status = BookingStatus.Refunded;
        return this;
    }

    /// <summary>
    /// Sets the Payment Method
    /// </summary>
    public BookingBuilder WithPaymentMethod(PaymentMethod paymentMethod)
    {
        _paymentMethod = paymentMethod;
        return this;
    }

    /// <summary>
    /// Sets Cash Payment
    /// </summary>
    public BookingBuilder WithCashPayment()
    {
        _paymentMethod = PaymentMethod.Cash;
        return this;
    }

    /// <summary>
    /// Sets Card Payment
    /// </summary>
    public BookingBuilder WithCardPayment()
    {
        _paymentMethod = PaymentMethod.CreditCard;
        return this;
    }

    /// <summary>
    /// Sets the Payment Transaction ID
    /// </summary>
    public BookingBuilder WithPaymentTransactionId(string? paymentTransactionId)
    {
        _paymentTransactionId = paymentTransactionId;
        return this;
    }

    /// <summary>
    /// Builds the Booking object
    /// </summary>
    public Booking Build()
    {
        return new Booking
        {
            Id = _id,
            BookingReference = _bookingReference,
            BookingUid = _bookingUid,
            CustomerName = _customerName,
            CustomerEmail = _customerEmail,
            CustomerPhone = _customerPhone,
            BookingDate = _bookingDate,
            TotalPrice = _totalPrice,
            Status = _status,
            PaymentMethod = _paymentMethod,
            PaymentTransactionId = _paymentTransactionId
        };
    }
}
