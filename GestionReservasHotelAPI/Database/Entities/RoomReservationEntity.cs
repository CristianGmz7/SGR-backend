using System.ComponentModel.DataAnnotations.Schema;

namespace GestionReservasHotelAPI.Database.Entities;

[Table("rooms_reservations", Schema = "dbo")]
public class RoomReservationEntity : BaseEntity
{
    [Column("room_id")]
    public Guid RoomId { get; set; }


    [ForeignKey(nameof(RoomId))]
    public virtual RoomEntity Room { get; set; }


    [Column("reservation_id")]
    public Guid ReservationId { get; set; }

    [ForeignKey(nameof(ReservationId))]
    public virtual ReservationEntity Reservation { get; set; }
}
