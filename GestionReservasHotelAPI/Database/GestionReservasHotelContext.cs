using GestionReservasHotelAPI.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionReservasHotelAPI.Database;

public class GestionReservasHotelContext : DbContext
{
    public GestionReservasHotelContext(DbContextOptions options) : base(options)
    {
        
    }

    public DbSet<HotelEntity> Hotels { get; set; }
    public DbSet<ReservationEntity> Reservations { get; set; }
    public DbSet<RoomEntity> Rooms { get; set; }
    public DbSet<AdditionalServiceEntity> AdditionalServices { get; set; }
    public DbSet<AdditionalServiceReservationEntity> AdditionalServiceReservations { get; set; }
    public DbSet<RoomReservationEntity> RoomReservations { get; set; }
}
