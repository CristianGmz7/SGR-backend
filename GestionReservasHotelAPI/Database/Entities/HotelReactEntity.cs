using System.ComponentModel.DataAnnotations.Schema;

namespace GestionReservasHotelAPI.Database.Entities;

[Table("hotels_reacts", Schema = "dbo")]
public class HotelReactEntity : BaseEntity
{
    [Column("hotel_id")]
    public Guid HotelId { get; set; }

    [ForeignKey(nameof(HotelId))]
    public virtual HotelEntity Hotel { get; set; }

    [Column("user_id")]
    public string UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual UserEntity UserEntity { get; set; }

    [Column("reaction")]
    public bool Reaction { get; set; }

    public virtual UserEntity CreatedByUser { get; set; }

    public virtual UserEntity UpdatedByUser { get; set; }
}
