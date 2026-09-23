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
    private Guid _bookingUid = Guid.NewGuid();
    private string _bookingReference = "ABC123";
    private string _customerName = "John Doe";
    private string _customerEmail = "john.doe@example.com";
    private string _customerPhone = "+79001234567";
    private int _totalPrice = 45000;
    private DateTime _bookingDate = DateTime.UtcNow;
    private BookingStatus _status = BookingStatus.Confirmed;
    private PaymentMethod _paymentMethod = PaymentMethod.CreditCard;
    private string? _paymentTransactionId = "TXN123456";

    /// <summary>
    /// Sets the Booking Id
    /// </summary>
    public BookingBuilder WithId(int id)
    {
        _id = id;
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
    /// Sets the Booking Reference
    /// </summary>
    public BookingBuilder WithBookingReference(string bookingReference)
    {
        _bookingReference = bookingReference;
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
    /// Sets the Total Price
    /// </summary>
    public BookingBuilder WithTotalPrice(int totalPrice)
    {
        _totalPrice = totalPrice;
        return this;
    }

    /// <summary>
    /// Sets the Total Price to zero
    /// </summary>
    public BookingBuilder WithZeroPrice()
    {
        _totalPrice = 0;
        return this;
    }

    /// <summary>
    /// Sets the Total Price to negative (invalid)
    /// </summary>
    public BookingBuilder WithNegativePrice()
    {
        _totalPrice = -1000;
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
    /// Sets the Booking Date to future
    /// </summary>
    public BookingBuilder WithFutureBookingDate()
    {
        _bookingDate = DateTime.UtcNow.AddDays(1);
        return this;
    }

    /// <summary>
    /// Sets the Booking Date to past
    /// </summary>
    public BookingBuilder WithPastBookingDate()
    {
        _bookingDate = DateTime.UtcNow.AddDays(-1);
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
    /// Sets the Booking Status to Confirmed
    /// </summary>
    public BookingBuilder WithConfirmedStatus()
    {
        _status = BookingStatus.Confirmed;
        return this;
    }

    /// <summary>
    /// Sets the Booking Status to Cancelled
    /// </summary>
    public BookingBuilder WithCancelledStatus()
    {
        _status = BookingStatus.Cancelled;
        return this;
    }

    /// <summary>
    /// Sets the Booking Status to Refunded
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
    /// Sets the Payment Method to CreditCard
    /// </summary>
    public BookingBuilder WithCreditCardPayment()
    {
        _paymentMethod = PaymentMethod.CreditCard;
        return this;
    }

    /// <summary>
    /// Sets the Payment Method to PayPal
    /// </summary>
    public BookingBuilder WithPayPalPayment()
    {
        _paymentMethod = PaymentMethod.PayPal;
        return this;
    }

    /// <summary>
    /// Sets the Payment Method to Cash
    /// </summary>
    public BookingBuilder WithCashPayment()
    {
        _paymentMethod = PaymentMethod.Cash;
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
    /// Sets the Payment Transaction ID to null
    /// </summary>
    public BookingBuilder WithNoPaymentTransactionId()
    {
        _paymentTransactionId = null;
        return this;
    }

    /// <summary>
    /// Builds the Booking object with current configuration
    /// </summary>
    /// <returns>Booking entity with configured properties</returns>
    public Booking Build()
    {
        return new Booking
        {
            Id = _id,
            BookingUid = _bookingUid,
            BookingReference = _bookingReference,
            CustomerName = _customerName,
            CustomerEmail = _customerEmail,
            CustomerPhone = _customerPhone,
            TotalPrice = _totalPrice,
            BookingDate = _bookingDate,
            Status = _status,
            PaymentMethod = _paymentMethod,
            PaymentTransactionId = _paymentTransactionId
        };
    }

    /// <summary>
    /// Builds a list of Booking objects
    /// </summary>
    /// <param name="count">Number of bookings to build</param>
    /// <param name="incrementIds">Whether to increment Ids for each booking</param>
    /// <returns>List of Booking entities</returns>
    public List<Booking> BuildList(int count, bool incrementIds = true)
    {
        var bookings = new List<Booking>();
        
        for (int i = 0; i < count; i++)
        {
            var booking = new Booking
            {
                Id = incrementIds ? _id + i : _id,
                BookingUid = Guid.NewGuid(),
                BookingReference = $"{_bookingReference}{i}",
                CustomerName = $"{_customerName} {i}",
                CustomerEmail = $"booking{i}@example.com",
                CustomerPhone = _customerPhone,
                TotalPrice = _totalPrice + (i * 1000),
                BookingDate = _bookingDate.AddMinutes(i),
                Status = _status,
                PaymentMethod = _paymentMethod,
                PaymentTransactionId = _paymentTransactionId
            };
            bookings.Add(booking);
        }

        return bookings;
    }

    /// <summary>
    /// Creates a Booking with random Id
    /// </summary>
    public Booking BuildWithRandomId()
    {
        return new Booking
        {
            Id = new Random().Next(1, int.MaxValue),
            BookingUid = Guid.NewGuid(),
            BookingReference = _bookingReference,
            CustomerName = _customerName,
            CustomerEmail = _customerEmail,
            CustomerPhone = _customerPhone,
            TotalPrice = _totalPrice,
            BookingDate = _bookingDate,
            Status = _status,
            PaymentMethod = _paymentMethod,
            PaymentTransactionId = _paymentTransactionId
        };
    }
}
