using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace presentation.dto.http.Ticket;

/// <summary>
/// Request DTO for purchasing a ticket (per lab2-template v1 spec)
/// </summary>
public class BuyTicketRequest
{
    [Required(ErrorMessage = "Flight number is required")]
    [JsonPropertyName("flightNumber")]
    public string FlightNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Price is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Price must be positive")]
    [JsonPropertyName("price")]
    public int Price { get; set; }

    [JsonPropertyName("paidFromBalance")]
    public bool PaidFromBalance { get; set; }
}

/// <summary>
/// Response DTO for ticket purchase (per lab2-template v1 spec)
/// </summary>
public class BuyTicketResponse
{
    [JsonPropertyName("ticketUid")]
    public Guid TicketUid { get; set; }

    [JsonPropertyName("paidByBonuses")]
    public int PaidByBonuses { get; set; }

    [JsonPropertyName("paidByMoney")]
    public int PaidByMoney { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("flightNumber")]
    public string FlightNumber { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public int Price { get; set; }
}
