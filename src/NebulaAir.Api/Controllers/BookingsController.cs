using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NebulaAir.Api.Dtos;
using NebulaAir.Domain.Entities;
using NebulaAir.Infrastructure;

namespace NebulaAir.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly NebulaAirDbContext _db;

    public BookingsController(NebulaAirDbContext db)
    {
        _db = db;
    }

    // GET api/v1/bookings
    // Devuelve TODAS las reservas
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetAllBookings()
    {
        var bookings = await _db.Bookings
            .Include(b => b.Flight)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        var result = bookings.Select(b => new BookingDto
        {
            Id = b.Id,
            FlightId = b.FlightId,
            FlightCode = b.Flight.Code,
            PassengerName = b.PassengerName,
            PassengerEmail = b.PassengerEmail,
            Status = b.Status,
            CreatedAt = b.CreatedAt
        });

        return Ok(result);
    }

    // POST api/v1/bookings
    [HttpPost]
    public async Task<ActionResult<BookingDto>> CreateBooking([FromBody] CreateBookingRequest request)
    {
        // 1) Validar el vuelo
        var flight = await _db.Flights.FindAsync(request.FlightId);
        if (flight == null)
        {
            return NotFound($"No se encontró el vuelo con id {request.FlightId}");
        }

        // 2) Validar datos básicos del pasajero
        if (string.IsNullOrWhiteSpace(request.PassengerName) ||
            string.IsNullOrWhiteSpace(request.PassengerEmail))
        {
            return BadRequest("PassengerName y PassengerEmail son obligatorios.");
        }

        // 3) Crear la reserva
        var booking = new Booking
        {
            FlightId = request.FlightId,
            PassengerName = request.PassengerName,
            PassengerEmail = request.PassengerEmail,
            Status = "PENDIENTE",
            CreatedAt = DateTime.UtcNow
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        // 4) Mapear a DTO
        var dto = new BookingDto
        {
            Id = booking.Id,
            FlightId = booking.FlightId,
            FlightCode = flight.Code,
            PassengerName = booking.PassengerName,
            PassengerEmail = booking.PassengerEmail,
            Status = booking.Status,
            CreatedAt = booking.CreatedAt
        };

        // 201 Created con la ruta de GetBookingById
        return CreatedAtAction(nameof(GetBookingById), new { id = booking.Id }, dto);
    }

    // GET api/v1/bookings/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingDto>> GetBookingById(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.Flight)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
        {
            return NotFound();
        }

        var dto = new BookingDto
        {
            Id = booking.Id,
            FlightId = booking.FlightId,
            FlightCode = booking.Flight.Code,
            PassengerName = booking.PassengerName,
            PassengerEmail = booking.PassengerEmail,
            Status = booking.Status,
            CreatedAt = booking.CreatedAt
        };

        return Ok(dto);
    }
}
