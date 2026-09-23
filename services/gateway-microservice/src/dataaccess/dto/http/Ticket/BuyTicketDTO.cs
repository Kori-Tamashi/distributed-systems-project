namespace dataaccess.dto.http.Ticket;

/// <summary>
/// DTO для покупки билета (SAGA-сценарий)
/// Соответствует контракту ЛР3: Flight Booking System
/// </summary>
public class BuyTicketDTO
{
    /// <summary>
    /// Номер рейса (например, "AFL031")
    /// </summary>
    public string FlightNumber { get; set; } = string.Empty;

    /// <summary>
    /// Имя пассажира
    /// </summary>
    public string PassengerName { get; set; } = string.Empty;

    /// <summary>
    /// Email пассажира
    /// </summary>
    public string PassengerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Телефон пассажира
    /// </summary>
    public string PassengerPhone { get; set; } = string.Empty;

    /// <summary>
    /// Номер места
    /// </summary>
    public string SeatNumber { get; set; } = string.Empty;

    /// <summary>
    /// Класс билета (0 = Economy, 1 = Business, 2 = First)
    /// </summary>
    public int Class { get; set; }

    /// <summary>
    /// Цена билета в рублях
    /// </summary>
    public int Price { get; set; }

    /// <summary>
    /// Дата и время бронирования (UTC)
    /// </summary>
    public DateTime BookingDate { get; set; }

    /// <summary>
    /// Оплачено со счёта бонусов (true) или деньгами (false)
    /// </summary>
    public bool PaidFromBalance { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public BuyTicketDTO()
    {
    }

    /// <summary>
    /// Constructor with required fields
    /// </summary>
    public BuyTicketDTO(
        string flightNumber,
        string passengerName,
        string passengerEmail,
        string seatNumber,
        int @class,
        int price,
        DateTime bookingDate,
        bool paidFromBalance)
    {
        FlightNumber = flightNumber;
        PassengerName = passengerName;
        PassengerEmail = passengerEmail;
        SeatNumber = seatNumber;
        Class = @class;
        Price = price;
        BookingDate = bookingDate;
        PaidFromBalance = paidFromBalance;
    }
}
