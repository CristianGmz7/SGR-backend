using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionReservasHotelAPI.Database.Entities;

[Table("additional_sevices", Schema = "dbo")]
public class AdditionalServiceEntity : BaseEntity
{
    [StringLength(100)]
    [Column("name")]
    public string Name { get; set; }

    [Range(1, double.MaxValue)]
    [Column("price")]
    public double Price { get; set; }

    [Column("hotel_id")]
    public Guid HotelId { get; set; }

    [ForeignKey(nameof(HotelId))]
    public virtual HotelEntity Hotel { get; set; }

    public virtual IEnumerable<AdditionalServiceReservationEntity> Reservations { get; set; }
}
