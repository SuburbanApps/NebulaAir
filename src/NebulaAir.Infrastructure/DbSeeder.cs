using NebulaAir.Domain.Entities;

namespace NebulaAir.Infrastructure;

public static class DbSeeder
{
    public static async Task SeedAsync(NebulaAirDbContext context)
    {
        if (!context.Flights.Any())
        {
            var flights = new List<Flight>
            {
                new Flight
                {
                    Code = "NA1001",
                    Origin = "MAD",
                    Destination = "LON",
                    DepartureTime = new DateTime(2025, 11, 25, 9, 0, 0),
                    ArrivalTime = new DateTime(2025, 11, 25, 11, 0, 0),
                    Price = 120.50m
                },
                new Flight
                {
                    Code = "NA1002",
                    Origin = "MAD",
                    Destination = "LON",
                    DepartureTime = new DateTime(2025, 11, 25, 16, 0, 0),
                    ArrivalTime = new DateTime(2025, 11, 25, 18, 0, 0),
                    Price = 150.00m
                }
            };

            context.Flights.AddRange(flights);
            await context.SaveChangesAsync();
        }
    }
}
