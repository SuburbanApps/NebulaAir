using Microsoft.EntityFrameworkCore;
using NebulaAir.Domain.Entities;

namespace NebulaAir.Infrastructure;

public class NebulaAirDbContext : DbContext
{
    public NebulaAirDbContext(DbContextOptions<NebulaAirDbContext> options)
        : base(options)
    {
    }

    public DbSet<Flight> Flights => Set<Flight>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de Flight
        modelBuilder.Entity<Flight>(entity =>
        {
            entity.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(x => x.Origin)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.Destination)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.Price)
                .HasColumnType("decimal(10,2)");
        });

        // Configuración de Booking
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.Property(x => x.PassengerName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.PassengerEmail)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasOne(x => x.Flight)
                .WithMany(f => f.Bookings)
                .HasForeignKey(x => x.FlightId);
        });
    }
}
