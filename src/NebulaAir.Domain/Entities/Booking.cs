namespace NebulaAir.Domain.Entities;

public class Booking
{
    public int Id { get; set; }

    public int FlightId { get; set; }
    public Flight Flight { get; set; } = null!;

    public string PassengerName { get; set; } = null!;
    public string PassengerEmail { get; set; } = null!;
    public string Status { get; set; } = "PENDIENTE";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
