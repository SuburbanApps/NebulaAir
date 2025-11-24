namespace NebulaAir.Api.Dtos;

public class BookingDto
{
    public int Id { get; set; }
    public int FlightId { get; set; }
    public string FlightCode { get; set; } = null!;
    public string PassengerName { get; set; } = null!;
    public string PassengerEmail { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
