namespace NebulaAir.Api.Dtos;

public class CreateBookingRequest
{
    public int FlightId { get; set; }
    public string PassengerName { get; set; } = null!;
    public string PassengerEmail { get; set; } = null!;
}
