using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionReservasHotelAPI.Database.Entities;

[Table("rooms", Schema = "dbo")]
public class RoomEntity : BaseEntity
{
    [Range(1, int.MaxValue)]
    [Column("number_room")]
    public int NumberRoom { get; set; }

    [RegularExpression("^(SENCILLA|DOBLE|SUITE)$")]
    [Column("type_room")]
    public string TypeRoom { get; set; }

    [Range(1, double.MaxValue)]
    [Column("price_night")]
    public double PriceNight { get; set; }

    [Column("hotel_id")]
    public Guid HotelId { get; set; }

    [Column("image_url")]
    public string ImageUrl { get; set; }

    [ForeignKey(nameof(HotelId))]
    public virtual HotelEntity Hotel { get; set; }

    public virtual IEnumerable<RoomReservationEntity> Reservations { get; set; }
}
