namespace NebulaAir.Domain.Entities;

public class Flight
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;        // NA1234
    public string Origin { get; set; } = null!;      // MAD
    public string Destination { get; set; } = null!; // LON
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal Price { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
