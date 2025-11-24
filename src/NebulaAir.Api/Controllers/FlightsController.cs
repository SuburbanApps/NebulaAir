using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NebulaAir.Api.Dtos;
using NebulaAir.Infrastructure;

namespace NebulaAir.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class FlightsController : ControllerBase
{
    private readonly NebulaAirDbContext _db;

    public FlightsController(NebulaAirDbContext db)
    {
        _db = db;
    }

    // GET api/v1/flights
    // Parámetros opcionales: origin, destination, date
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FlightDto>>> GetFlights(
        [FromQuery] string? origin = null,
        [FromQuery] string? destination = null,
        [FromQuery] DateTime? date = null)
    {
        // 1) Empezamos con todos los vuelos
        var query = _db.Flights.AsQueryable();

        // 2) Filtro por origen (si se envía)
        if (!string.IsNullOrWhiteSpace(origin))
        {
            query = query.Where(f => f.Origin == origin);
        }

        // 3) Filtro por destino (si se envía)
        if (!string.IsNullOrWhiteSpace(destination))
        {
            query = query.Where(f => f.Destination == destination);
        }

        // 4) Filtro por fecha (si se envía)
        if (date.HasValue)
        {
            var dayStart = date.Value.Date;
            var dayEnd = dayStart.AddDays(1);

            query = query.Where(f =>
                f.DepartureTime >= dayStart &&
                f.DepartureTime < dayEnd);
        }

        // 5) Ordenamos por hora de salida
        var flights = await query
            .OrderBy(f => f.DepartureTime)
            .ToListAsync();

        // 6) Mapeamos a DTO
        var result = flights.Select(f => new FlightDto
        {
            Id = f.Id,
            Code = f.Code,
            Origin = f.Origin,
            Destination = f.Destination,
            DepartureTime = f.DepartureTime,
            ArrivalTime = f.ArrivalTime,
            Price = f.Price
        });

        return Ok(result);
    }
}
